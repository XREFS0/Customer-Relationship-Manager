Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports EnterpriseCRM.Models
Imports EnterpriseCRM.Services
Imports EnterpriseCRM.UI.Theme

Namespace EnterpriseCRM.UI.Forms
    Public Class CustomerEditDialog
        Inherits Form

        Private _customer As Customer
        Private ReadOnly _customerService As New CustomerService()

        Private txtName As TextBox
        Private txtCompany As TextBox
        Private txtEmail As TextBox
        Private txtPhone As TextBox
        Private txtAddress As TextBox
        Private txtCity As TextBox
        Private txtCountry As TextBox
        Private cmbStatus As ComboBox
        Private txtNotes As TextBox
        Private btnSave As Button
        Private btnCancel As Button

        Public Sub New(Optional customer As Customer = Nothing)
            _customer = If(customer, New Customer())
            InitializeComponent()
            LoadCustomerData()
        End Sub

        Private Sub InitializeComponent()
            Text = If(_customer.Id = 0, "Add New Customer", $"Edit Customer - {_customer.Name}")
            Size = New Size(560, 600)
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            StartPosition = FormStartPosition.CenterParent
            Font = New Font("Segoe UI", 9.5F)
            BackColor = ThemeManager.CardBackgroundColor
            ForeColor = ThemeManager.TextPrimaryColor

            Dim pnlTop As New Panel With {.Dock = DockStyle.Top, .Height = 60, .Padding = New Padding(20, 15, 20, 0)}
            Dim lblTitle As New Label With {
                .Text = If(_customer.Id = 0, "Create Customer Profile", "Edit Customer Profile"),
                .Font = New Font("Segoe UI", 12.0F, FontStyle.Bold),
                .ForeColor = ThemeManager.TextPrimaryColor,
                .AutoSize = True
            }
            Dim lblSub As New Label With {
                .Text = "Enter customer and company details below.",
                .Font = New Font("Segoe UI", 8.5F),
                .ForeColor = ThemeManager.TextSecondaryColor,
                .Location = New Point(20, 35),
                .AutoSize = True
            }
            pnlTop.Controls.Add(lblTitle)
            pnlTop.Controls.Add(lblSub)

            Dim pnlBottom As New Panel With {.Dock = DockStyle.Bottom, .Height = 65, .Padding = New Padding(20, 12, 20, 12)}
            btnSave = New Button With {.Text = "Save Customer", .Size = New Size(130, 38), .Location = New Point(265, 14)}
            btnCancel = New Button With {.Text = "Cancel", .Size = New Size(100, 38), .Location = New Point(405, 14)}
            ThemeManager.StyleButton(btnSave, True)
            ThemeManager.StyleButton(btnCancel, False)
            AddHandler btnSave.Click, AddressOf BtnSave_Click
            AddHandler btnCancel.Click, Sub() DialogResult = DialogResult.Cancel

            pnlBottom.Controls.Add(btnSave)
            pnlBottom.Controls.Add(btnCancel)

            Dim pnlContent As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(24, 10, 24, 10)}
            Dim layout As New TableLayoutPanel With {
                .Dock = DockStyle.Fill,
                .ColumnCount = 2,
                .RowCount = 8,
                .AutoScroll = True
            }
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))
            layout.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 50.0F))

            txtName = CreateTextBox()
            txtCompany = CreateTextBox()
            txtEmail = CreateTextBox()
            txtPhone = CreateTextBox()
            txtCity = CreateTextBox()
            txtCountry = CreateTextBox()
            txtAddress = CreateTextBox()
            txtNotes = New TextBox With {.Multiline = True, .Height = 60, .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9.5F)}
            cmbStatus = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9.5F)}
            cmbStatus.Items.AddRange(New Object() {"Lead", "Prospect", "Active", "Inactive"})
            cmbStatus.SelectedIndex = 0

            layout.Controls.Add(CreateFieldPanel("Full Name *", txtName), 0, 0)
            layout.Controls.Add(CreateFieldPanel("Company Name", txtCompany), 1, 0)

            layout.Controls.Add(CreateFieldPanel("Email Address", txtEmail), 0, 1)
            layout.Controls.Add(CreateFieldPanel("Phone Number", txtPhone), 1, 1)

            layout.Controls.Add(CreateFieldPanel("City", txtCity), 0, 2)
            layout.Controls.Add(CreateFieldPanel("Country", txtCountry), 1, 2)

            layout.Controls.Add(CreateFieldPanel("Status", cmbStatus), 0, 3)
            layout.Controls.Add(CreateFieldPanel("Street Address", txtAddress), 1, 3)

            Dim pnlNotes As New Panel With {.Dock = DockStyle.Fill, .Height = 85}
            Dim lblNotes As New Label With {.Text = "Notes / Description", .Dock = DockStyle.Top, .Height = 20, .ForeColor = ThemeManager.TextSecondaryColor}
            pnlNotes.Controls.Add(txtNotes)
            pnlNotes.Controls.Add(lblNotes)
            layout.Controls.Add(pnlNotes, 0, 4)
            layout.SetColumnSpan(pnlNotes, 2)

            pnlContent.Controls.Add(layout)
            Controls.Add(pnlContent)
            Controls.Add(pnlTop)
            Controls.Add(pnlBottom)
        End Sub

        Private Function CreateTextBox() As TextBox
            Return New TextBox With {.Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9.5F)}
        End Function

        Private Function CreateFieldPanel(label As String, ctrl As Control) As Panel
            Dim pnl As New Panel With {.Dock = DockStyle.Fill, .Height = 55, .Padding = New Padding(0, 0, 8, 8)}
            Dim lbl As New Label With {.Text = label, .Dock = DockStyle.Top, .Height = 20, .ForeColor = ThemeManager.TextSecondaryColor}
            ctrl.Dock = DockStyle.Top
            pnl.Controls.Add(ctrl)
            pnl.Controls.Add(lbl)
            Return pnl
        End Function

        Private Sub LoadCustomerData()
            If _customer.Id > 0 Then
                txtName.Text = _customer.Name
                txtCompany.Text = _customer.Company
                txtEmail.Text = _customer.Email
                txtPhone.Text = _customer.Phone
                txtAddress.Text = _customer.Address
                txtCity.Text = _customer.City
                txtCountry.Text = _customer.Country
                txtNotes.Text = _customer.Notes
                cmbStatus.SelectedItem = _customer.Status.ToString()
            End If
        End Sub

        Private Sub BtnSave_Click(sender As Object, e As EventArgs)
            If String.IsNullOrWhiteSpace(txtName.Text) Then
                MessageBox.Show("Customer Name is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtName.Focus()
                Return
            End If

            _customer.Name = txtName.Text.Trim()
            _customer.Company = txtCompany.Text.Trim()
            _customer.Email = txtEmail.Text.Trim()
            _customer.Phone = txtPhone.Text.Trim()
            _customer.Address = txtAddress.Text.Trim()
            _customer.City = txtCity.Text.Trim()
            _customer.Country = txtCountry.Text.Trim()
            _customer.Notes = txtNotes.Text.Trim()
            _customer.Status = CType([Enum].Parse(GetType(CustomerStatus), cmbStatus.SelectedItem.ToString()), CustomerStatus)

            Try
                _customerService.SaveCustomer(_customer)
                DialogResult = DialogResult.OK
                Close()
            Catch ex As Exception
                MessageBox.Show($"Failed to save customer: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub
    End Class
End Namespace
