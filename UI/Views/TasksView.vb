Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports EnterpriseCRM.Models
Imports EnterpriseCRM.Services
Imports EnterpriseCRM.UI.Forms
Imports EnterpriseCRM.UI.Theme

Namespace EnterpriseCRM.UI.Views
    Public Class TasksView
        Inherits UserControl

        Private ReadOnly _taskService As New TaskService()

        Private dgvTasks As DataGridView
        Private cmbPriorityFilter As ComboBox
        Private cmbStatusFilter As ComboBox
        Private lblTaskMetrics As Label

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Dock = DockStyle.Fill
            BackColor = ThemeManager.BackgroundColor
            Font = New Font("Segoe UI", 9.5F)

            Dim pnlMain As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(24, 20, 24, 20)}

            Dim pnlHeader As New Panel With {.Dock = DockStyle.Top, .Height = 65}
            Dim lblTitle As New Label With {
                .Text = "Task & Activity Execution",
                .Font = New Font("Segoe UI", 16.0F, FontStyle.Bold),
                .ForeColor = ThemeManager.TextPrimaryColor,
                .AutoSize = True
            }
            Dim lblSub As New Label With {
                .Text = "Assign, track deadlines, prioritize customer follow-ups and milestones.",
                .Font = New Font("Segoe UI", 8.5F),
                .ForeColor = ThemeManager.TextSecondaryColor,
                .Location = New Point(0, 32),
                .AutoSize = True
            }

            Dim btnAdd As New Button With {.Text = "+ Create Task", .Size = New Size(140, 36), .Anchor = AnchorStyles.Top Or AnchorStyles.Right, .Location = New Point(pnlMain.Width - 160, 10)}
            ThemeManager.StyleButton(btnAdd, True)
            AddHandler btnAdd.Click, AddressOf BtnAdd_Click

            pnlHeader.Controls.Add(lblTitle)
            pnlHeader.Controls.Add(lblSub)
            pnlHeader.Controls.Add(btnAdd)

            Dim pnlFilter As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 55,
                .BackColor = ThemeManager.CardBackgroundColor,
                .Padding = New Padding(12, 10, 12, 10)
            }

            Dim lblStatus As New Label With {.Text = "Status:", .AutoSize = True, .Location = New Point(12, 16), .ForeColor = ThemeManager.TextSecondaryColor}
            cmbStatusFilter = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(65, 12), .Size = New Size(140, 26)}
            cmbStatusFilter.Items.AddRange(New Object() {"All Statuses", "Pending", "InProgress", "Completed", "Cancelled"})
            cmbStatusFilter.SelectedIndex = 0
            AddHandler cmbStatusFilter.SelectedIndexChanged, Sub() LoadTasksData()

            Dim lblPri As New Label With {.Text = "Priority:", .AutoSize = True, .Location = Point.Empty, .Left = 220, .Top = 16, .ForeColor = ThemeManager.TextSecondaryColor}
            cmbPriorityFilter = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(275, 12), .Size = New Size(130, 26)}
            cmbPriorityFilter.Items.AddRange(New Object() {"All Priorities", "Low", "Medium", "High", "Urgent"})
            cmbPriorityFilter.SelectedIndex = 0
            AddHandler cmbPriorityFilter.SelectedIndexChanged, Sub() LoadTasksData()

            lblTaskMetrics = New Label With {.AutoSize = True, .Anchor = AnchorStyles.Top Or AnchorStyles.Right, .Location = New Point(pnlFilter.Width - 180, 16), .ForeColor = ThemeManager.TextSecondaryColor}

            pnlFilter.Controls.Add(lblStatus)
            pnlFilter.Controls.Add(cmbStatusFilter)
            pnlFilter.Controls.Add(lblPri)
            pnlFilter.Controls.Add(cmbPriorityFilter)
            pnlFilter.Controls.Add(lblTaskMetrics)

            Dim pnlGridCard As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = ThemeManager.CardBackgroundColor,
                .Padding = New Padding(12),
                .Margin = New Padding(0, 12, 0, 0)
            }

            dgvTasks = New DataGridView With {.Dock = DockStyle.Fill}
            ThemeManager.StyleGridView(dgvTasks)

            dgvTasks.Columns.Add("Id", "ID")
            dgvTasks.Columns("Id").Width = 60
            dgvTasks.Columns.Add("Title", "Task Title")
            dgvTasks.Columns("Title").Width = 240
            dgvTasks.Columns.Add("Customer", "Related Customer")
            dgvTasks.Columns("Customer").Width = 180
            dgvTasks.Columns.Add("AssignedTo", "Assigned Rep")
            dgvTasks.Columns("AssignedTo").Width = 140
            dgvTasks.Columns.Add("Priority", "Priority")
            dgvTasks.Columns("Priority").Width = 100
            dgvTasks.Columns.Add("Status", "Status")
            dgvTasks.Columns("Status").Width = 110
            dgvTasks.Columns.Add("DueDate", "Due Date")
            dgvTasks.Columns("DueDate").Width = 120
            dgvTasks.Columns.Add("Desc", "Description")
            dgvTasks.Columns("Desc").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

            AddHandler dgvTasks.CellDoubleClick, AddressOf DgvTasks_CellDoubleClick

            Dim ctxMenu As New ContextMenuStrip()
            ctxMenu.Items.Add("Mark as Completed", Nothing, Sub() SetTaskStatus(TaskStatus.Completed))
            ctxMenu.Items.Add("Mark as In-Progress", Nothing, Sub() SetTaskStatus(TaskStatus.InProgress))
            ctxMenu.Items.Add(New ToolStripSeparator())
            ctxMenu.Items.Add("Edit Task", Nothing, AddressOf OnEditTask)
            ctxMenu.Items.Add("Delete Task", Nothing, AddressOf OnDeleteTask)
            dgvTasks.ContextMenuStrip = ctxMenu

            pnlGridCard.Controls.Add(dgvTasks)

            pnlMain.Controls.Add(pnlGridCard)
            pnlMain.Controls.Add(pnlFilter)
            pnlMain.Controls.Add(pnlHeader)

            Controls.Add(pnlMain)
        End Sub

        Public Sub LoadTasksData()
            Dim tasks = _taskService.GetAllTasks()
            Dim filterStat = If(cmbStatusFilter.SelectedIndex > 0, cmbStatusFilter.SelectedItem.ToString(), "")
            Dim filterPri = If(cmbPriorityFilter.SelectedIndex > 0, cmbPriorityFilter.SelectedItem.ToString(), "")

            dgvTasks.Rows.Clear()
            Dim pendingCount = 0

            For Each t In tasks
                If t.Status = TaskStatus.Pending OrElse t.Status = TaskStatus.InProgress Then
                    pendingCount += 1
                End If

                If Not String.IsNullOrEmpty(filterStat) AndAlso t.Status.ToString() <> filterStat Then Continue For
                If Not String.IsNullOrEmpty(filterPri) AndAlso t.Priority.ToString() <> filterPri Then Continue For

                Dim dtStr = If(t.DueDate.HasValue, t.DueDate.Value.ToString("yyyy-MM-dd"), "-")
                Dim custName = If(String.IsNullOrEmpty(t.CustomerName), "(None / Internal)", t.CustomerName)
                dgvTasks.Rows.Add(t.Id, t.Title, custName, t.AssignedToUserName, t.PriorityName, t.StatusName, dtStr, t.Description)
            Next

            lblTaskMetrics.Text = $"Active Tasks: {pendingCount}"
        End Sub

        Private Function GetSelectedTaskId() As Integer
            If dgvTasks.SelectedRows.Count > 0 Then
                Return Convert.ToInt32(dgvTasks.SelectedRows(0).Cells("Id").Value)
            End If
            Return 0
        End Function

        Private Sub SetTaskStatus(status As TaskStatus)
            Dim id = GetSelectedTaskId()
            If id > 0 Then
                Dim task = _taskService.GetAllTasks().Find(Function(x) x.Id = id)
                If task IsNot Nothing Then
                    task.Status = status
                    _taskService.SaveTask(task)
                    LoadTasksData()
                End If
            End If
        End Sub

        Private Sub BtnAdd_Click(sender As Object, e As EventArgs)
            Using dlg As New TaskEditDialog()
                If dlg.ShowDialog(Me) = DialogResult.OK Then
                    LoadTasksData()
                End If
            End Using
        End Sub

        Private Sub DgvTasks_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs)
            If e.RowIndex >= 0 Then
                OnEditTask(sender, EventArgs.Empty)
            End If
        End Sub

        Private Sub OnEditTask(sender As Object, e As EventArgs)
            Dim id = GetSelectedTaskId()
            If id > 0 Then
                Dim task = _taskService.GetAllTasks().Find(Function(x) x.Id = id)
                If task IsNot Nothing Then
                    Using dlg As New TaskEditDialog(task)
                        If dlg.ShowDialog(Me) = DialogResult.OK Then
                            LoadTasksData()
                        End If
                    End Using
                End If
            End If
        End Sub

        Private Sub OnDeleteTask(sender As Object, e As EventArgs)
            Dim id = GetSelectedTaskId()
            If id > 0 Then
                Dim res = MessageBox.Show("Delete this task?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If res = DialogResult.Yes Then
                    _taskService.DeleteTask(id)
                    LoadTasksData()
                End If
            End If
        End Sub
    End Class
End Namespace
