Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports EnterpriseCRM.Models
Imports EnterpriseCRM.Services
Imports EnterpriseCRM.UI.Forms
Imports EnterpriseCRM.UI.Theme

Namespace EnterpriseCRM.UI.Views
    Public Class ContactsView
        Inherits UserControl

        Private ReadOnly _contactService As New ContactService()
        Private ReadOnly _customerService As New CustomerService()

        Private dgvContacts As DataGridView
        Private cmbCustomerFilter As ComboBox
        Private cmbTypeFilter As ComboBox
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
                .Text = "Communication & Interaction History",
                .Font = New Font("Segoe UI", 16.0F, FontStyle.Bold),
                .ForeColor = ThemeManager.TextPrimaryColor,
                .AutoSize = True
            }
            Dim lblSub As New Label With {
                .Text = "Chronological audit of calls, meetings, emails, and touchpoints.",
                .Font = New Font("Segoe UI", 8.5F),
                .ForeColor = ThemeManager.TextSecondaryColor,
                .Location = New Point(0, 32),
                .AutoSize = True
            }

            Dim btnAdd As New Button With {.Text = "+ Log Interaction", .Size = New Size(140, 36), .Anchor = AnchorStyles.Top Or AnchorStyles.Right, .Location = New Point(pnlMain.Width - 160, 10)}
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

            Dim lblType As New Label With {.Text = "Type:", .AutoSize = True, .Location = New Point(12, 16), .ForeColor = ThemeManager.TextSecondaryColor}
            cmbTypeFilter = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(55, 12), .Size = New Size(130, 26)}
            cmbTypeFilter.Items.AddRange(New Object() {"All Types", "Call", "Meeting", "Email", "Note", "Message"})
            cmbTypeFilter.SelectedIndex = 0
            AddHandler cmbTypeFilter.SelectedIndexChanged, Sub() FilterContacts()

            Dim lblCust As New Label With {.Text = "Customer:", .AutoSize = True, .Location = New Point(205, 16), .ForeColor = ThemeManager.TextSecondaryColor}
            cmbCustomerFilter = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(275, 12), .Size = New Size(220, 26)}
            AddHandler cmbCustomerFilter.SelectedIndexChanged, Sub() FilterContacts()

            lblTotalRecords = New Label With {.AutoSize = True, .Anchor = AnchorStyles.Top Or AnchorStyles.Right, .Location = New Point(pnlFilter.Width - 150, 16), .ForeColor = ThemeManager.TextSecondaryColor}

            pnlFilter.Controls.Add(lblType)
            pnlFilter.Controls.Add(cmbTypeFilter)
            pnlFilter.Controls.Add(lblCust)
            pnlFilter.Controls.Add(cmbCustomerFilter)
            pnlFilter.Controls.Add(lblTotalRecords)

            Dim pnlGridCard As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = ThemeManager.CardBackgroundColor,
                .Padding = New Padding(12),
                .Margin = New Padding(0, 12, 0, 0)
            }

            dgvContacts = New DataGridView With {.Dock = DockStyle.Fill}
            ThemeManager.StyleGridView(dgvContacts)

            dgvContacts.Columns.Add("Id", "ID")
            dgvContacts.Columns("Id").Width = 60
            dgvContacts.Columns.Add("Date", "Date & Time")
            dgvContacts.Columns("Date").Width = 140
            dgvContacts.Columns.Add("Customer", "Customer Name")
            dgvContacts.Columns("Customer").Width = 180
            dgvContacts.Columns.Add("Type", "Channel")
            dgvContacts.Columns("Type").Width = 100
            dgvContacts.Columns.Add("Subject", "Subject")
            dgvContacts.Columns("Subject").Width = 220
            dgvContacts.Columns.Add("Notes", "Notes / Outcome")
            dgvContacts.Columns("Notes").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            dgvContacts.Columns.Add("User", "Representative")
            dgvContacts.Columns("User").Width = 140

            Dim ctxMenu As New ContextMenuStrip()
            ctxMenu.Items.Add("Edit Interaction", Nothing, AddressOf OnEditContact)
            ctxMenu.Items.Add("Delete Interaction", Nothing, AddressOf OnDeleteContact)
            dgvContacts.ContextMenuStrip = ctxMenu

            pnlGridCard.Controls.Add(dgvContacts)

            pnlMain.Controls.Add(pnlGridCard)
            pnlMain.Controls.Add(pnlFilter)
            pnlMain.Controls.Add(pnlHeader)

            Controls.Add(pnlMain)
        End Sub

        Public Sub LoadData()
            Dim customers = _customerService.GetAllCustomers()
            customers.Insert(0, New Customer With {.Id = 0, .Name = "All Customers"})
            cmbCustomerFilter.DisplayMember = "Name"
            cmbCustomerFilter.ValueMember = "Id"
            cmbCustomerFilter.DataSource = customers
            cmbCustomerFilter.SelectedIndex = 0

            FilterContacts()
        End Sub

        Private Sub FilterContacts()
            Dim contacts = _contactService.GetAllContacts()
            Dim selCustId As Integer = 0
            If cmbCustomerFilter.SelectedItem IsNot Nothing AndAlso TypeOf cmbCustomerFilter.SelectedItem Is Customer Then
                selCustId = DirectCast(cmbCustomerFilter.SelectedItem, Customer).Id
            End If

            Dim selType As String = If(cmbTypeFilter.SelectedIndex > 0, cmbTypeFilter.SelectedItem.ToString(), "")

            dgvContacts.Rows.Clear()
            Dim count = 0
            For Each c In contacts
                If selCustId > 0 AndAlso c.CustomerId <> selCustId Then Continue For
                If Not String.IsNullOrEmpty(selType) AndAlso c.TypeName <> selType Then Continue For

                dgvContacts.Rows.Add(c.Id, c.ContactDate.ToString("yyyy-MM-dd HH:mm"), c.CustomerName, c.TypeName, c.Subject, c.Notes, c.UserName)
                count += 1
            Next

            lblTotalRecords.Text = $"Total Logs: {count}"
        End Sub

        Private Function GetSelectedContactId() As Integer
            If dgvContacts.SelectedRows.Count > 0 Then
                Return Convert.ToInt32(dgvContacts.SelectedRows(0).Cells("Id").Value)
            End If
            Return 0
        End Function

        Private Sub BtnAdd_Click(sender As Object, e As EventArgs)
            Using dlg As New ContactEditDialog()
                If dlg.ShowDialog(Me) = DialogResult.OK Then
                    FilterContacts()
                End If
            End Using
        End Sub

        Private Sub OnEditContact(sender As Object, e As EventArgs)
            Dim id = GetSelectedContactId()
            If id > 0 Then
                Dim contact = _contactService.GetAllContacts().Find(Function(x) x.Id = id)
                If contact IsNot Nothing Then
                    Using dlg As New ContactEditDialog(contact)
                        If dlg.ShowDialog(Me) = DialogResult.OK Then
                            FilterContacts()
                        End If
                    End Using
                End If
            End If
        End Sub

        Private Sub OnDeleteContact(sender As Object, e As EventArgs)
            Dim id = GetSelectedContactId()
            If id > 0 Then
                Dim res = MessageBox.Show("Delete this interaction record?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If res = DialogResult.Yes Then
                    _contactService.DeleteContact(id)
                    FilterContacts()
                End If
            End If
        End Sub
    End Class
End Namespace
