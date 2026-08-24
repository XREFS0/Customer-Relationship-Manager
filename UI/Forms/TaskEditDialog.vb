Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports EnterpriseCRM.Data.Repositories
Imports EnterpriseCRM.Models
Imports EnterpriseCRM.Services
Imports EnterpriseCRM.UI.Theme

Namespace EnterpriseCRM.UI.Forms
    Public Class TaskEditDialog
        Inherits Form

        Private _task As CrmTask
        Private ReadOnly _taskService As New TaskService()
        Private ReadOnly _customerService As New CustomerService()
        Private ReadOnly _userRepo As New UserRepository()

        Private txtTitle As TextBox
        Private txtDesc As TextBox
        Private cmbCustomer As ComboBox
        Private cmbAssignedTo As ComboBox
        Private cmbPriority As ComboBox
        Private cmbStatus As ComboBox
        Private dtpDueDate As DateTimePicker
        Private btnSave As Button
        Private btnCancel As Button

        Public Sub New(Optional task As CrmTask = Nothing)
            _task = If(task, New CrmTask())
            InitializeComponent()
            LoadData()
        End Sub

        Private Sub InitializeComponent()
            Text = If(_task.Id = 0, "Create Task", $"Edit Task - {_task.Title}")
            Size = New Size(540, 520)
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            StartPosition = FormStartPosition.CenterParent
            Font = New Font("Segoe UI", 9.5F)
            BackColor = ThemeManager.CardBackgroundColor
            ForeColor = ThemeManager.TextPrimaryColor

            Dim pnlTop As New Panel With {.Dock = DockStyle.Top, .Height = 55, .Padding = New Padding(20, 12, 20, 0)}
            Dim lblTitle As New Label With {
                .Text = "Task Details & Assignment",
                .Font = New Font("Segoe UI", 12.0F, FontStyle.Bold),
                .ForeColor = ThemeManager.TextPrimaryColor,
                .AutoSize = True
            }
            pnlTop.Controls.Add(lblTitle)

            Dim pnlBottom As New Panel With {.Dock = DockStyle.Bottom, .Height = 60, .Padding = New Padding(20, 10, 20, 10)}
            btnSave = New Button With {.Text = "Save Task", .Size = New Size(130, 36), .Location = New Point(250, 12)}
            btnCancel = New Button With {.Text = "Cancel", .Size = New Size(100, 36), .Location = New Point(390, 12)}
            ThemeManager.StyleButton(btnSave, True)
            ThemeManager.StyleButton(btnCancel, False)
            AddHandler btnSave.Click, AddressOf BtnSave_Click
            AddHandler btnCancel.Click, Sub() DialogResult = DialogResult.Cancel

            pnlBottom.Controls.Add(btnSave)
            pnlBottom.Controls.Add(btnCancel)

            Dim pnlContent As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(20, 10, 20, 10)}
            Dim layout As New TableLayoutPanel With {.Dock = DockStyle.Fill, .ColumnCount = 2, .RowCount = 5}
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))

            txtTitle = New TextBox With {.Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9.5F)}
            cmbAssignedTo = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9.5F)}
            cmbCustomer = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9.5F)}
            dtpDueDate = New DateTimePicker With {.Format = DateTimePickerFormat.Short, .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9.5F)}
            
            cmbPriority = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9.5F)}
            cmbPriority.Items.AddRange(New Object() {"Low", "Medium", "High", "Urgent"})
            cmbPriority.SelectedIndex = 1

            cmbStatus = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9.5F)}
            cmbStatus.Items.AddRange(New Object() {"Pending", "InProgress", "Completed", "Cancelled"})
            cmbStatus.SelectedIndex = 0

            txtDesc = New TextBox With {.Multiline = True, .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9.5F)}

            layout.Controls.Add(CreateField("Task Title *", txtTitle), 0, 0)
            layout.Controls.Add(CreateField("Assigned Rep *", cmbAssignedTo), 1, 0)

            layout.Controls.Add(CreateField("Related Customer (Optional)", cmbCustomer), 0, 1)
            layout.Controls.Add(CreateField("Due Date", dtpDueDate), 1, 1)

            layout.Controls.Add(CreateField("Priority Level", cmbPriority), 0, 2)
            layout.Controls.Add(CreateField("Task Status", cmbStatus), 1, 2)

            Dim pnlNotes As New Panel With {.Dock = DockStyle.Fill, .Height = 80}
            Dim lblNotes As New Label With {.Text = "Description & Checklist", .Dock = DockStyle.Top, .Height = 20, .ForeColor = ThemeManager.TextSecondaryColor}
            pnlNotes.Controls.Add(txtDesc)
            pnlNotes.Controls.Add(lblNotes)
            layout.Controls.Add(pnlNotes, 0, 3)
            layout.SetColumnSpan(pnlNotes, 2)

            pnlContent.Controls.Add(layout)
            Controls.Add(pnlContent)
            Controls.Add(pnlTop)
            Controls.Add(pnlBottom)
        End Sub

        Private Function CreateField(label As String, ctrl As Control) As Panel
            Dim pnl As New Panel With {.Dock = DockStyle.Fill, .Height = 55, .Padding = New Padding(0, 0, 8, 8)}
            Dim lbl As New Label With {.Text = label, .Dock = DockStyle.Top, .Height = 20, .ForeColor = ThemeManager.TextSecondaryColor}
            ctrl.Dock = DockStyle.Top
            pnl.Controls.Add(ctrl)
            pnl.Controls.Add(lbl)
            Return pnl
        End Function

        Private Sub LoadData()
            Dim users = _userRepo.GetAll()
            cmbAssignedTo.DisplayMember = "FullName"
            cmbAssignedTo.ValueMember = "Id"
            cmbAssignedTo.DataSource = users

            Dim customers = _customerService.GetAllCustomers()
            customers.Insert(0, New Customer With {.Id = 0, .Name = "-- None / Internal --"})
            cmbCustomer.DisplayMember = "Name"
            cmbCustomer.ValueMember = "Id"
            cmbCustomer.DataSource = customers

            If _task.CustomerId.HasValue Then
                cmbCustomer.SelectedValue = _task.CustomerId.Value
            End If

            If _task.AssignedToUserId > 0 Then
                cmbAssignedTo.SelectedValue = _task.AssignedToUserId
            ElseIf AppLogger.CurrentUser IsNot Nothing Then
                cmbAssignedTo.SelectedValue = AppLogger.CurrentUser.Id
            End If

            If _task.Id > 0 Then
                txtTitle.Text = _task.Title
                txtDesc.Text = _task.Description
                cmbPriority.SelectedItem = _task.Priority.ToString()
                cmbStatus.SelectedItem = _task.Status.ToString()
                If _task.DueDate.HasValue Then
                    dtpDueDate.Value = _task.DueDate.Value
                End If
            End If
        End Sub

        Private Sub BtnSave_Click(sender As Object, e As EventArgs)
            If String.IsNullOrWhiteSpace(txtTitle.Text) Then
                MessageBox.Show("Task title is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtTitle.Focus()
                Return
            End If

            If cmbAssignedTo.SelectedValue Is Nothing Then
                MessageBox.Show("Please assign the task to a team member.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            _task.Title = txtTitle.Text.Trim()
            _task.Description = txtDesc.Text.Trim()
            _task.AssignedToUserId = CInt(cmbAssignedTo.SelectedValue)

            Dim selCustId = CInt(cmbCustomer.SelectedValue)
            _task.CustomerId = If(selCustId > 0, CType(selCustId, Nullable(Of Integer)), Nothing)

            _task.Priority = CType([Enum].Parse(GetType(TaskPriority), cmbPriority.SelectedItem.ToString()), TaskPriority)
            _task.Status = CType([Enum].Parse(GetType(TaskStatus), cmbStatus.SelectedItem.ToString()), TaskStatus)
            _task.DueDate = dtpDueDate.Value

            Try
                _taskService.SaveTask(_task)
                DialogResult = DialogResult.OK
                Close()
            Catch ex As Exception
                MessageBox.Show($"Failed to save task: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub
    End Class
End Namespace
