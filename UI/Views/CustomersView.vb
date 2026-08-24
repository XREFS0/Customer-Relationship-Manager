Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports EnterpriseCRM.Models
Imports EnterpriseCRM.Services
Imports EnterpriseCRM.UI.Forms
Imports EnterpriseCRM.UI.Theme

Namespace EnterpriseCRM.UI.Views
    Public Class CustomersView
        Inherits UserControl

        Private ReadOnly _customerService As New CustomerService()
        Private ReadOnly _reportService As New ReportExportService()

        Private dgvCustomers As DataGridView
        Private txtSearch As TextBox
        Private cmbFilterStatus As ComboBox
        Private lblTotalRecords As Label

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
                .Text = "Customer Directory",
                .Font = New Font("Segoe UI", 16.0F, FontStyle.Bold),
                .ForeColor = ThemeManager.TextPrimaryColor,
                .AutoSize = True
            }
            Dim lblSub As New Label With {
                .Text = "Manage clients, prospects, leads, and account relationships.",
                .Font = New Font("Segoe UI", 8.5F),
                .ForeColor = ThemeManager.TextSecondaryColor,
                .Location = New Point(0, 32),
                .AutoSize = True
            }

            Dim btnAdd As New Button With {.Text = "+ New Customer", .Size = New Size(140, 36), .Anchor = AnchorStyles.Top Or AnchorStyles.Right, .Location = New Point(pnlMain.Width - 420, 10)}
            Dim btnExport As New Button With {.Text = "Export to Excel/CSV", .Size = New Size(150, 36), .Anchor = AnchorStyles.Top Or AnchorStyles.Right, .Location = New Point(pnlMain.Width - 270, 10)}
            ThemeManager.StyleButton(btnAdd, True)
            ThemeManager.StyleButton(btnExport, False)

            AddHandler btnAdd.Click, AddressOf BtnAdd_Click
            AddHandler btnExport.Click, AddressOf BtnExport_Click

            pnlHeader.Controls.Add(lblTitle)
            pnlHeader.Controls.Add(lblSub)
            pnlHeader.Controls.Add(btnAdd)
            pnlHeader.Controls.Add(btnExport)

            Dim pnlFilter As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 55,
                .BackColor = ThemeManager.CardBackgroundColor,
                .Padding = New Padding(12, 10, 12, 10),
                .Margin = New Padding(0, 10, 0, 10)
            }

            Dim lblSearch As New Label With {.Text = "Search:", .AutoSize = True, .Location = New Point(12, 16), .ForeColor = ThemeManager.TextSecondaryColor}
            txtSearch = New TextBox With {.Location = New Point(70, 12), .Size = New Size(240, 26), .PlaceholderText = "Search by Name, Company, Email..."}
            AddHandler txtSearch.TextChanged, Sub() LoadCustomers()

            Dim lblStatus As New Label With {.Text = "Status:", .AutoSize = True, .Location = New Point(330, 16), .ForeColor = ThemeManager.TextSecondaryColor}
            cmbFilterStatus = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(380, 12), .Size = New Size(140, 26)}
            cmbFilterStatus.Items.AddRange(New Object() {"All Statuses", "Lead", "Prospect", "Active", "Inactive"})
            cmbFilterStatus.SelectedIndex = 0
            AddHandler cmbFilterStatus.SelectedIndexChanged, Sub() LoadCustomers()

            lblTotalRecords = New Label With {.AutoSize = True, .Anchor = AnchorStyles.Top Or AnchorStyles.Right, .Location = New Point(pnlFilter.Width - 160, 16), .ForeColor = ThemeManager.TextSecondaryColor}

            pnlFilter.Controls.Add(lblSearch)
            pnlFilter.Controls.Add(txtSearch)
            pnlFilter.Controls.Add(lblStatus)
            pnlFilter.Controls.Add(cmbFilterStatus)
            pnlFilter.Controls.Add(lblTotalRecords)

            Dim pnlGridCard As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = ThemeManager.CardBackgroundColor,
                .Padding = New Padding(12),
                .Margin = New Padding(0, 12, 0, 0)
            }

            dgvCustomers = New DataGridView With {.Dock = DockStyle.Fill}
            ThemeManager.StyleGridView(dgvCustomers)

            dgvCustomers.Columns.Add("Id", "ID")
            dgvCustomers.Columns("Id").Width = 60
            dgvCustomers.Columns.Add("Name", "Customer Name")
            dgvCustomers.Columns("Name").Width = 180
            dgvCustomers.Columns.Add("Company", "Company")
            dgvCustomers.Columns("Company").Width = 180
            dgvCustomers.Columns.Add("Email", "Email Address")
            dgvCustomers.Columns("Email").Width = 200
            dgvCustomers.Columns.Add("Phone", "Phone")
            dgvCustomers.Columns("Phone").Width = 140
            dgvCustomers.Columns.Add("City", "Location")
            dgvCustomers.Columns("City").Width = 130
            dgvCustomers.Columns.Add("Status", "Status")
            dgvCustomers.Columns("Status").Width = 100
            dgvCustomers.Columns.Add("Created", "Created Date")
            dgvCustomers.Columns("Created").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

            AddHandler dgvCustomers.CellDoubleClick, AddressOf DgvCustomers_CellDoubleClick

            Dim ctxMenu As New ContextMenuStrip()
            Dim mnuView = ctxMenu.Items.Add("View 360 Profile", Nothing, AddressOf OnViewProfile)
            Dim mnuEdit = ctxMenu.Items.Add("Edit Customer", Nothing, AddressOf OnEditCustomer)
            Dim mnuLog = ctxMenu.Items.Add("Log Interaction", Nothing, AddressOf OnLogInteraction)
            ctxMenu.Items.Add(New ToolStripSeparator())
            Dim mnuDelete = ctxMenu.Items.Add("Delete Customer", Nothing, AddressOf OnDeleteCustomer)
            dgvCustomers.ContextMenuStrip = ctxMenu

            pnlGridCard.Controls.Add(dgvCustomers)

            pnlMain.Controls.Add(pnlGridCard)
            pnlMain.Controls.Add(pnlFilter)
            pnlMain.Controls.Add(pnlHeader)

            Controls.Add(pnlMain)
        End Sub

        Public Sub LoadCustomers()
            Dim query = txtSearch.Text.Trim()
            Dim status As Nullable(Of CustomerStatus) = Nothing
            If cmbFilterStatus.SelectedIndex > 0 Then
                status = CType([Enum].Parse(GetType(CustomerStatus), cmbFilterStatus.SelectedItem.ToString()), CustomerStatus)
            End If

            Dim list = _customerService.SearchCustomers(query, status)
            dgvCustomers.Rows.Clear()
            For Each c In list
                Dim loc = If(Not String.IsNullOrEmpty(c.City), $"{c.City}, {c.Country}", c.Country)
                dgvCustomers.Rows.Add(c.Id, c.Name, c.Company, c.Email, c.Phone, loc, c.StatusName, c.CreatedAt.ToString("yyyy-MM-dd"))
            Next

            lblTotalRecords.Text = $"Records: {list.Count}"
        End Sub

        Private Function GetSelectedCustomerId() As Integer
            If dgvCustomers.SelectedRows.Count > 0 Then
                Return Convert.ToInt32(dgvCustomers.SelectedRows(0).Cells("Id").Value)
            End If
            Return 0
        End Function

        Private Sub BtnAdd_Click(sender As Object, e As EventArgs)
            Using dlg As New CustomerEditDialog()
                If dlg.ShowDialog(Me) = DialogResult.OK Then
                    LoadCustomers()
                End If
            End Using
        End Sub

        Private Sub DgvCustomers_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs)
            If e.RowIndex >= 0 Then
                Dim id = Convert.ToInt32(dgvCustomers.Rows(e.RowIndex).Cells("Id").Value)
                Using detailForm As New CustomerDetailForm(id)
                    detailForm.ShowDialog(Me)
                    LoadCustomers()
                End Using
            End If
        End Sub

        Private Sub OnViewProfile(sender As Object, e As EventArgs)
            Dim id = GetSelectedCustomerId()
            If id > 0 Then
                Using detailForm As New CustomerDetailForm(id)
                    detailForm.ShowDialog(Me)
                    LoadCustomers()
                End Using
            End If
        End Sub

        Private Sub OnEditCustomer(sender As Object, e As EventArgs)
            Dim id = GetSelectedCustomerId()
            If id > 0 Then
                Dim cust = _customerService.GetCustomerById(id)
                If cust IsNot Nothing Then
                    Using dlg As New CustomerEditDialog(cust)
                        If dlg.ShowDialog(Me) = DialogResult.OK Then
                            LoadCustomers()
                        End If
                    End Using
                End If
            End If
        End Sub

        Private Sub OnLogInteraction(sender As Object, e As EventArgs)
            Dim id = GetSelectedCustomerId()
            If id > 0 Then
                Using dlg As New ContactEditDialog(New ContactHistory With {.CustomerId = id})
                    dlg.ShowDialog(Me)
                End Using
            End If
        End Sub

        Private Sub OnDeleteCustomer(sender As Object, e As EventArgs)
            Dim id = GetSelectedCustomerId()
            If id > 0 Then
                Dim res = MessageBox.Show("Are you sure you want to delete this customer? All associated deals, tasks, and interaction records will be affected.", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                If res = DialogResult.Yes Then
                    _customerService.DeleteCustomer(id)
                    LoadCustomers()
                End If
            End If
        End Sub

        Private Sub BtnExport_Click(sender As Object, e As EventArgs)
            Using sfd As New SaveFileDialog With {
                .Filter = "CSV File (*.csv)|*.csv|All Files (*.*)|*.*",
                .FileName = $"customers_export_{DateTime.Now:yyyyMMdd}.csv"
            }
                If sfd.ShowDialog() = DialogResult.OK Then
                    Dim list = _customerService.GetAllCustomers()
                    _reportService.ExportCustomersToCsv(list, sfd.FileName)
                    MessageBox.Show("Customer data exported successfully!", "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End Using
        End Sub
    End Class
End Namespace
