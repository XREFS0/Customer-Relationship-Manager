Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports EnterpriseCRM.Models
Imports EnterpriseCRM.Services
Imports EnterpriseCRM.UI.Theme

Namespace EnterpriseCRM.UI.Forms
    Public Class CustomerDetailForm
        Inherits Form

        Private ReadOnly _customerId As Integer
        Private ReadOnly _customerService As New CustomerService()
        Private ReadOnly _contactService As New ContactService()
        Private ReadOnly _salesService As New SalesService()
        Private ReadOnly _taskService As New TaskService()

        Private _customer As Customer
        Private lblName As Label
        Private lblCompany As Label
        Private lblContactInfo As Label
        Private lblAddressInfo As Label
        Private lblStatusBadge As Label
        Private dgvContacts As DataGridView
        Private dgvSales As DataGridView
        Private dgvTasks As DataGridView

        Public Sub New(customerId As Integer)
            _customerId = customerId
            InitializeComponent()
            LoadData()
        End Sub

        Private Sub InitializeComponent()
            Text = "Customer 360 View"
            Size = New Size(950, 700)
            StartPosition = FormStartPosition.CenterParent
            Font = New Font("Segoe UI", 9.5F)
            BackColor = ThemeManager.BackgroundColor
            ForeColor = ThemeManager.TextPrimaryColor

            Dim pnlHeader As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 110,
                .BackColor = ThemeManager.CardBackgroundColor,
                .Padding = New Padding(24, 16, 24, 16)
            }

            lblName = New Label With {.Font = New Font("Segoe UI", 16.0F, FontStyle.Bold), .AutoSize = True, .Location = New Point(20, 14)}
            lblCompany = New Label With {.Font = New Font("Segoe UI", 11.0F, FontStyle.Regular), .ForeColor = ThemeManager.BrandPrimary, .AutoSize = True, .Location = New Point(22, 46)}
            lblContactInfo = New Label With {.Font = New Font("Segoe UI", 9.0F), .ForeColor = ThemeManager.TextSecondaryColor, .AutoSize = True, .Location = New Point(22, 74)}
            lblAddressInfo = New Label With {.Font = New Font("Segoe UI", 9.0F), .ForeColor = ThemeManager.TextSecondaryColor, .AutoSize = True, .Location = New Point(380, 74)}

            lblStatusBadge = New Label With {
                .AutoSize = False,
                .Size = New Size(90, 26),
                .TextAlign = ContentAlignment.MiddleCenter,
                .Font = New Font("Segoe UI", 8.5F, FontStyle.Bold),
                .BackColor = Color.FromArgb(219, 234, 254),
                .ForeColor = Color.FromArgb(30, 64, 175),
                .Anchor = AnchorStyles.Top Or AnchorStyles.Right,
                .Location = New Point(820, 20)
            }

            pnlHeader.Controls.Add(lblName)
            pnlHeader.Controls.Add(lblCompany)
            pnlHeader.Controls.Add(lblContactInfo)
            pnlHeader.Controls.Add(lblAddressInfo)
            pnlHeader.Controls.Add(lblStatusBadge)

            Dim tabCtrl As New TabControl With {
                .Dock = DockStyle.Fill,
                .Font = New Font("Segoe UI", 9.5F, FontStyle.Regular),
                .Padding = New Point(16, 8)
            }

            Dim tabContacts As New TabPage With {.Text = "  Interaction History  ", .BackColor = ThemeManager.BackgroundColor, .Padding = New Padding(12)}
            Dim pnlContactActions As New Panel With {.Dock = DockStyle.Top, .Height = 44}
            Dim btnAddContact As New Button With {.Text = "+ Log Interaction", .Size = New Size(140, 32), .Location = New Point(0, 4)}
            ThemeManager.StyleButton(btnAddContact, True)
            AddHandler btnAddContact.Click, AddressOf BtnAddContact_Click
            pnlContactActions.Controls.Add(btnAddContact)

            dgvContacts = New DataGridView With {.Dock = DockStyle.Fill}
            ThemeManager.StyleGridView(dgvContacts)
            dgvContacts.Columns.Add("Id", "ID")
            dgvContacts.Columns("Id").Visible = False
            dgvContacts.Columns.Add("Type", "Type")
            dgvContacts.Columns.Add("Subject", "Subject")
            dgvContacts.Columns.Add("Notes", "Notes")
            dgvContacts.Columns.Add("User", "Rep")
            dgvContacts.Columns.Add("Date", "Date")
            dgvContacts.Columns("Notes").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

            tabContacts.Controls.Add(dgvContacts)
            tabContacts.Controls.Add(pnlContactActions)

            Dim tabSales As New TabPage With {.Text = "  Sales Opportunities  ", .BackColor = ThemeManager.BackgroundColor, .Padding = New Padding(12)}
            Dim pnlSaleActions As New Panel With {.Dock = DockStyle.Top, .Height = 44}
            Dim btnAddSale As New Button With {.Text = "+ New Opportunity", .Size = New Size(150, 32), .Location = New Point(0, 4)}
            ThemeManager.StyleButton(btnAddSale, True)
            AddHandler btnAddSale.Click, AddressOf BtnAddSale_Click
            pnlSaleActions.Controls.Add(btnAddSale)

            dgvSales = New DataGridView With {.Dock = DockStyle.Fill}
            ThemeManager.StyleGridView(dgvSales)
            dgvSales.Columns.Add("Id", "ID")
            dgvSales.Columns("Id").Visible = False
            dgvSales.Columns.Add("Title", "Opportunity")
            dgvSales.Columns.Add("Amount", "Value ($)")
            dgvSales.Columns.Add("Stage", "Stage")
            dgvSales.Columns.Add("Prob", "Probability")
            dgvSales.Columns.Add("Rep", "Owner")
            dgvSales.Columns.Add("CloseDate", "Est. Close")
            dgvSales.Columns("Title").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

            tabSales.Controls.Add(dgvSales)
            tabSales.Controls.Add(pnlSaleActions)

            Dim tabTasks As New TabPage With {.Text = "  Assigned Tasks  ", .BackColor = ThemeManager.BackgroundColor, .Padding = New Padding(12)}
            Dim pnlTaskActions As New Panel With {.Dock = DockStyle.Top, .Height = 44}
            Dim btnAddTask As New Button With {.Text = "+ Create Task", .Size = New Size(130, 32), .Location = New Point(0, 4)}
            ThemeManager.StyleButton(btnAddTask, True)
            AddHandler btnAddTask.Click, AddressOf BtnAddTask_Click
            pnlTaskActions.Controls.Add(btnAddTask)

            dgvTasks = New DataGridView With {.Dock = DockStyle.Fill}
            ThemeManager.StyleGridView(dgvTasks)
            dgvTasks.Columns.Add("Id", "ID")
            dgvTasks.Columns("Id").Visible = False
            dgvTasks.Columns.Add("Title", "Task Title")
            dgvTasks.Columns.Add("Priority", "Priority")
            dgvTasks.Columns.Add("Status", "Status")
            dgvTasks.Columns.Add("Assigned", "Assigned To")
            dgvTasks.Columns.Add("Due", "Due Date")
            dgvTasks.Columns("Title").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

            tabTasks.Controls.Add(dgvTasks)
            tabTasks.Controls.Add(pnlTaskActions)

            tabCtrl.TabPages.Add(tabContacts)
            tabCtrl.TabPages.Add(tabSales)
            tabCtrl.TabPages.Add(tabTasks)

            Dim pnlContainer As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(20)}
            pnlContainer.Controls.Add(tabCtrl)

            Controls.Add(pnlContainer)
            Controls.Add(pnlHeader)
        End Sub

        Private Sub LoadData()
            _customer = _customerService.GetCustomerById(_customerId)
            If _customer Is Nothing Then
                MessageBox.Show("Customer not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Close()
                Return
            End If

            lblName.Text = _customer.Name
            lblCompany.Text = If(String.IsNullOrEmpty(_customer.Company), "Individual Account", _customer.Company)
            lblContactInfo.Text = $"Email: {_customer.Email}  |  Phone: {_customer.Phone}"
            lblAddressInfo.Text = $"Location: {_customer.City}, {_customer.Country}"
            lblStatusBadge.Text = _customer.StatusName.ToUpper()

            LoadContacts()
            LoadSales()
            LoadTasks()
        End Sub

        Private Sub LoadContacts()
            dgvContacts.Rows.Clear()
            Dim contacts = _contactService.GetContactsByCustomer(_customerId)
            For Each c In contacts
                dgvContacts.Rows.Add(c.Id, c.TypeName, c.Subject, c.Notes, c.UserName, c.ContactDate.ToString("yyyy-MM-dd HH:mm"))
            Next
        End Sub

        Private Sub LoadSales()
            dgvSales.Rows.Clear()
            Dim sales = _salesService.GetSalesByCustomer(_customerId)
            For Each s In sales
                Dim dtStr = If(s.ExpectedCloseDate.HasValue, s.ExpectedCloseDate.Value.ToString("yyyy-MM-dd"), "-")
                dgvSales.Rows.Add(s.Id, s.Title, $"${s.Amount:N2}", s.StageName, $"{s.Probability}%", s.UserName, dtStr)
            Next
        End Sub

        Private Sub LoadTasks()
            dgvTasks.Rows.Clear()
            Dim tasks = _taskService.GetTasksByCustomer(_customerId)
            For Each t In tasks
                Dim dtStr = If(t.DueDate.HasValue, t.DueDate.Value.ToString("yyyy-MM-dd"), "-")
                dgvTasks.Rows.Add(t.Id, t.Title, t.PriorityName, t.StatusName, t.AssignedToUserName, dtStr)
            Next
        End Sub

        Private Sub BtnAddContact_Click(sender As Object, e As EventArgs)
            Using dlg As New ContactEditDialog(New ContactHistory With {.CustomerId = _customerId})
                If dlg.ShowDialog(Me) = DialogResult.OK Then
                    LoadContacts()
                End If
            End Using
        End Sub

        Private Sub BtnAddSale_Click(sender As Object, e As EventArgs)
            Using dlg As New SaleEditDialog(New SaleOpportunity With {.CustomerId = _customerId})
                If dlg.ShowDialog(Me) = DialogResult.OK Then
                    LoadSales()
                End If
            End Using
        End Sub

        Private Sub BtnAddTask_Click(sender As Object, e As EventArgs)
            Using dlg As New TaskEditDialog(New CrmTask With {.CustomerId = _customerId})
                If dlg.ShowDialog(Me) = DialogResult.OK Then
                    LoadTasks()
                End If
            End Using
        End Sub
    End Class
End Namespace
