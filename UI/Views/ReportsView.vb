Imports System
Imports System.Collections.Generic
Imports System.Diagnostics
Imports System.Drawing
Imports System.Windows.Forms
Imports EnterpriseCRM.Models
Imports EnterpriseCRM.Services
Imports EnterpriseCRM.UI.Theme

Namespace EnterpriseCRM.UI.Views
    Public Class ReportsView
        Inherits UserControl

        Private ReadOnly _customerService As New CustomerService()
        Private ReadOnly _salesService As New SalesService()
        Private ReadOnly _taskService As New TaskService()
        Private ReadOnly _reportService As New ReportExportService()

        Private cmbReportType As ComboBox
        Private dtpFrom As DateTimePicker
        Private dtpTo As DateTimePicker
        Private dgvReportResult As DataGridView
        Private lblSummary As Label

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
                .Text = "Business Intelligence & Reports",
                .Font = New Font("Segoe UI", 16.0F, FontStyle.Bold),
                .ForeColor = ThemeManager.TextPrimaryColor,
                .AutoSize = True
            }
            Dim lblSub As New Label With {
                .Text = "Generate customer analyses, sales forecasts, and operational summaries.",
                .Font = New Font("Segoe UI", 8.5F),
                .ForeColor = ThemeManager.TextSecondaryColor,
                .Location = New Point(0, 32),
                .AutoSize = True
            }

            Dim btnExportCsv As New Button With {.Text = "Export to Excel/CSV", .Size = New Size(150, 36), .Anchor = AnchorStyles.Top Or AnchorStyles.Right, .Location = New Point(pnlMain.Width - 360, 10)}
            Dim btnExportHtml As New Button With {.Text = "Export Formatted Report", .Size = New Size(170, 36), .Anchor = AnchorStyles.Top Or AnchorStyles.Right, .Location = New Point(pnlMain.Width - 190, 10)}
            ThemeManager.StyleButton(btnExportCsv, False)
            ThemeManager.StyleButton(btnExportHtml, True)

            AddHandler btnExportCsv.Click, AddressOf BtnExportCsv_Click
            AddHandler btnExportHtml.Click, AddressOf BtnExportHtml_Click

            pnlHeader.Controls.Add(lblTitle)
            pnlHeader.Controls.Add(lblSub)
            pnlHeader.Controls.Add(btnExportCsv)
            pnlHeader.Controls.Add(btnExportHtml)

            Dim pnlFilter As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 55,
                .BackColor = ThemeManager.CardBackgroundColor,
                .Padding = New Padding(12, 10, 12, 10)
            }

            Dim lblType As New Label With {.Text = "Report Type:", .AutoSize = True, .Location = New Point(12, 16), .ForeColor = ThemeManager.TextSecondaryColor}
            cmbReportType = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(100, 12), .Size = New Size(220, 26)}
            cmbReportType.Items.AddRange(New Object() {"Customer Accounts Summary", "Sales Deals & Forecasts", "Task Execution & Backlog", "Opportunity Stage Breakdown"})
            cmbReportType.SelectedIndex = 0
            AddHandler cmbReportType.SelectedIndexChanged, Sub() GenerateReport()

            Dim btnRun As New Button With {.Text = "Run Report", .Location = New Point(335, 10), .Size = New Size(100, 30)}
            ThemeManager.StyleButton(btnRun, True)
            AddHandler btnRun.Click, Sub() GenerateReport()

            lblSummary = New Label With {.AutoSize = True, .Anchor = AnchorStyles.Top Or AnchorStyles.Right, .Location = New Point(pnlFilter.Width - 250, 16), .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold), .ForeColor = ThemeManager.TextPrimaryColor}

            pnlFilter.Controls.Add(lblType)
            pnlFilter.Controls.Add(cmbReportType)
            pnlFilter.Controls.Add(btnRun)
            pnlFilter.Controls.Add(lblSummary)

            Dim pnlGridCard As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = ThemeManager.CardBackgroundColor,
                .Padding = New Padding(12),
                .Margin = New Padding(0, 12, 0, 0)
            }

            dgvReportResult = New DataGridView With {.Dock = DockStyle.Fill}
            ThemeManager.StyleGridView(dgvReportResult)

            pnlGridCard.Controls.Add(dgvReportResult)

            pnlMain.Controls.Add(pnlGridCard)
            pnlMain.Controls.Add(pnlFilter)
            pnlMain.Controls.Add(pnlHeader)

            Controls.Add(pnlMain)
        End Sub

        Public Sub GenerateReport()
            dgvReportResult.Columns.Clear()
            dgvReportResult.Rows.Clear()

            Select Case cmbReportType.SelectedIndex
                Case 0
                    dgvReportResult.Columns.Add("Id", "ID")
                    dgvReportResult.Columns.Add("Name", "Name")
                    dgvReportResult.Columns.Add("Company", "Company")
                    dgvReportResult.Columns.Add("Email", "Email")
                    dgvReportResult.Columns.Add("Phone", "Phone")
                    dgvReportResult.Columns.Add("Status", "Status")
                    dgvReportResult.Columns.Add("Created", "Registered")
                    dgvReportResult.Columns("Company").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

                    Dim custs = _customerService.GetAllCustomers()
                    For Each c In custs
                        dgvReportResult.Rows.Add(c.Id, c.Name, c.Company, c.Email, c.Phone, c.StatusName, c.CreatedAt.ToString("yyyy-MM-dd"))
                    Next
                    lblSummary.Text = $"Total Customer Records: {custs.Count}"

                Case 1
                    dgvReportResult.Columns.Add("Id", "ID")
                    dgvReportResult.Columns.Add("Title", "Opportunity")
                    dgvReportResult.Columns.Add("Customer", "Account")
                    dgvReportResult.Columns.Add("Rep", "Representative")
                    dgvReportResult.Columns.Add("Amount", "Value ($)")
                    dgvReportResult.Columns.Add("Stage", "Pipeline Stage")
                    dgvReportResult.Columns.Add("Probability", "Win %")
                    dgvReportResult.Columns.Add("CloseDate", "Est. Close Date")
                    dgvReportResult.Columns("Title").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

                    Dim sales = _salesService.GetAllSales()
                    Dim totalVal As Decimal = 0
                    For Each s In sales
                        totalVal += s.Amount
                        Dim dt = If(s.ExpectedCloseDate.HasValue, s.ExpectedCloseDate.Value.ToString("yyyy-MM-dd"), "-")
                        dgvReportResult.Rows.Add(s.Id, s.Title, s.CustomerName, s.UserName, $"${s.Amount:N2}", s.StageName, $"{s.Probability}%", dt)
                    Next
                    lblSummary.Text = $"Total Pipeline Value: ${totalVal:N2}"

                Case 2
                    dgvReportResult.Columns.Add("Id", "ID")
                    dgvReportResult.Columns.Add("Title", "Task")
                    dgvReportResult.Columns.Add("Customer", "Customer")
                    dgvReportResult.Columns.Add("AssignedTo", "Assigned To")
                    dgvReportResult.Columns.Add("Priority", "Priority")
                    dgvReportResult.Columns.Add("Status", "Status")
                    dgvReportResult.Columns.Add("DueDate", "Due Date")
                    dgvReportResult.Columns("Title").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

                    Dim tasks = _taskService.GetAllTasks()
                    Dim completed = 0
                    For Each t In tasks
                        If t.Status = TaskStatus.Completed Then completed += 1
                        Dim dt = If(t.DueDate.HasValue, t.DueDate.Value.ToString("yyyy-MM-dd"), "-")
                        dgvReportResult.Rows.Add(t.Id, t.Title, t.CustomerName, t.AssignedToUserName, t.PriorityName, t.StatusName, dt)
                    Next
                    lblSummary.Text = $"Completed: {completed} / Total: {tasks.Count}"

                Case 3
                    dgvReportResult.Columns.Add("Stage", "Pipeline Stage")
                    dgvReportResult.Columns.Add("Count", "Deals Count")
                    dgvReportResult.Columns.Add("TotalVal", "Total Pipeline Value ($)")
                    dgvReportResult.Columns.Add("AvgVal", "Avg Deal Size ($)")
                    dgvReportResult.Columns("Stage").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

                    Dim dashServ As New DashboardService()
                    Dim stages = dashServ.GetPipelineStagesSummary()
                    Dim totalAmt As Decimal = 0
                    For Each stg In stages
                        totalAmt += stg.TotalAmount
                        Dim avg = If(stg.Count > 0, stg.TotalAmount / stg.Count, 0)
                        dgvReportResult.Rows.Add(stg.StageName, stg.Count, $"${stg.TotalAmount:N2}", $"${avg:N2}")
                    Next
                    lblSummary.Text = $"Cumulative Value: ${totalAmt:N2}"
            End Select
        End Sub

        Private Sub BtnExportCsv_Click(sender As Object, e As EventArgs)
            If dgvReportResult.Rows.Count = 0 Then
                MessageBox.Show("No report data available to export.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Using sfd As New SaveFileDialog With {
                .Filter = "CSV File (*.csv)|*.csv",
                .FileName = $"report_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv"
            }
                If sfd.ShowDialog() = DialogResult.OK Then
                    Dim headers As New List(Of String)()
                    For Each col As DataGridViewColumn In dgvReportResult.Columns
                        headers.Add(col.HeaderText)
                    Next

                    Dim sb As New System.Text.StringBuilder()
                    sb.AppendLine(String.Join(",", headers))

                    For Each row As DataGridViewRow In dgvReportResult.Rows
                        Dim rowCells As New List(Of String)()
                        For Each cell As DataGridViewCell In row.Cells
                            Dim val = If(cell.Value IsNot Nothing, cell.Value.ToString().Replace("""", """"""), "")
                            rowCells.Add($"""{val}""")
                        Next
                        sb.AppendLine(String.Join(",", rowCells))
                    Next

                    System.IO.File.WriteAllText(sfd.FileName, sb.ToString(), System.Text.Encoding.UTF8)
                    MessageBox.Show("Report exported successfully!", "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End Using
        End Sub

        Private Sub BtnExportHtml_Click(sender As Object, e As EventArgs)
            If dgvReportResult.Rows.Count = 0 Then
                MessageBox.Show("No report data available to export.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Return
            End If

            Using sfd As New SaveFileDialog With {
                .Filter = "HTML Document (*.html)|*.html",
                .FileName = $"report_export_{DateTime.Now:yyyyMMdd_HHmmss}.html"
            }
                If sfd.ShowDialog() = DialogResult.OK Then
                    Dim headers As New List(Of String)()
                    For Each col As DataGridViewColumn In dgvReportResult.Columns
                        headers.Add(col.HeaderText)
                    Next

                    Dim rows As New List(Of List(Of String))()
                    For Each row As DataGridViewRow In dgvReportResult.Rows
                        Dim rList As New List(Of String)()
                        For Each cell As DataGridViewCell In row.Cells
                            rList.Add(If(cell.Value IsNot Nothing, cell.Value.ToString(), ""))
                        Next
                        rows.Add(rList)
                    Next

                    _reportService.ExportHtmlReport(cmbReportType.SelectedItem.ToString(), headers, rows, sfd.FileName)
                    Dim res = MessageBox.Show("Report generated successfully! Open in browser now?", "Report Ready", MessageBoxButtons.YesNo, MessageBoxIcon.Information)
                    If res = DialogResult.Yes Then
                        Process.Start(New ProcessStartInfo(sfd.FileName) With {.UseShellExecute = True})
                    End If
                End If
            End Using
        End Sub
    End Class
End Namespace
