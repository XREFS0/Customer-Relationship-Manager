Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports EnterpriseCRM.Models
Imports EnterpriseCRM.Services
Imports EnterpriseCRM.UI.Theme

Namespace EnterpriseCRM.UI.Forms
    Public Class ContactEditDialog
        Inherits Form

        Private _contact As ContactHistory
        Private ReadOnly _contactService As New ContactService()
        Private ReadOnly _customerService As New CustomerService()

        Private cmbCustomer As ComboBox
        Private cmbType As ComboBox
        Private txtSubject As TextBox
        Private dtpDate As DateTimePicker
        Private txtNotes As TextBox
        Private btnSave As Button
        Private btnCancel As Button

        Private _customerList As List(Of Customer)

        Public Sub New(Optional contact As ContactHistory = Nothing)
            _contact = If(contact, New ContactHistory())
            InitializeComponent()
            LoadData()
        End Sub

        Private Sub InitializeComponent()
            Text = If(_contact.Id = 0, "Log Customer Interaction", "Edit Interaction")
            Size = New Size(500, 480)
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            StartPosition = FormStartPosition.CenterParent
            Font = New Font("Segoe UI", 9.5F)
            BackColor = ThemeManager.CardBackgroundColor
            ForeColor = ThemeManager.TextPrimaryColor

            Dim pnlTop As New Panel With {.Dock = DockStyle.Top, .Height = 55, .Padding = New Padding(20, 12, 20, 0)}
            Dim lblTitle As New Label With {
                .Text = "Log Call, Meeting, or Note",
                .Font = New Font("Segoe UI", 12.0F, FontStyle.Bold),
                .ForeColor = ThemeManager.TextPrimaryColor,
                .AutoSize = True
            }
            pnlTop.Controls.Add(lblTitle)

            Dim pnlBottom As New Panel With {.Dock = DockStyle.Bottom, .Height = 60, .Padding = New Padding(20, 10, 20, 10)}
            btnSave = New Button With {.Text = "Save Interaction", .Size = New Size(140, 36), .Location = New Point(200, 12)}
            btnCancel = New Button With {.Text = "Cancel", .Size = New Size(100, 36), .Location = New Point(350, 12)}
            ThemeManager.StyleButton(btnSave, True)
            ThemeManager.StyleButton(btnCancel, False)
            AddHandler btnSave.Click, AddressOf BtnSave_Click
            AddHandler btnCancel.Click, Sub() DialogResult = DialogResult.Cancel

            pnlBottom.Controls.Add(btnSave)
            pnlBottom.Controls.Add(btnCancel)

            Dim pnlContent As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(20, 10, 20, 10)}

            cmbCustomer = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Top, .Font = New Font("Segoe UI", 9.5F)}
            cmbType = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Top, .Font = New Font("Segoe UI", 9.5F)}
            cmbType.Items.AddRange(New Object() {"Call", "Meeting", "Email", "Note", "Message"})
            cmbType.SelectedIndex = 0

            txtSubject = New TextBox With {.Dock = DockStyle.Top, .Font = New Font("Segoe UI", 9.5F)}
            dtpDate = New DateTimePicker With {.Format = DateTimePickerFormat.Custom, .CustomFormat = "yyyy-MM-dd HH:mm", .Dock = DockStyle.Top, .Font = New Font("Segoe UI", 9.5F)}
            txtNotes = New TextBox With {.Multiline = True, .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9.5F)}

            Dim pnlNotes As New Panel With {.Dock = DockStyle.Fill, .Padding = New Padding(0, 5, 0, 5)}
            Dim lblNotes As New Label With {.Text = "Summary & Discussion Notes", .Dock = DockStyle.Top, .Height = 20, .ForeColor = ThemeManager.TextSecondaryColor}
            pnlNotes.Controls.Add(txtNotes)
            pnlNotes.Controls.Add(lblNotes)

            pnlContent.Controls.Add(pnlNotes)
            pnlContent.Controls.Add(CreateField("Date & Time", dtpDate))
            pnlContent.Controls.Add(CreateField("Subject / Topic *", txtSubject))
            pnlContent.Controls.Add(CreateField("Interaction Type", cmbType))
            pnlContent.Controls.Add(CreateField("Customer *", cmbCustomer))

            Controls.Add(pnlContent)
            Controls.Add(pnlTop)
            Controls.Add(pnlBottom)
        End Sub

        Private Function CreateField(label As String, ctrl As Control) As Panel
            Dim pnl As New Panel With {.Dock = DockStyle.Top, .Height = 55, .Padding = New Padding(0, 0, 0, 8)}
            Dim lbl As New Label With {.Text = label, .Dock = DockStyle.Top, .Height = 20, .ForeColor = ThemeManager.TextSecondaryColor}
            ctrl.Dock = DockStyle.Top
            pnl.Controls.Add(ctrl)
            pnl.Controls.Add(lbl)
            Return pnl
        End Function

        Private Sub LoadData()
            _customerList = _customerService.GetAllCustomers()
            cmbCustomer.DisplayMember = "Name"
            cmbCustomer.ValueMember = "Id"
            cmbCustomer.DataSource = _customerList

            If _contact.CustomerId > 0 Then
                cmbCustomer.SelectedValue = _contact.CustomerId
            End If

            If _contact.Id > 0 Then
                txtSubject.Text = _contact.Subject
                txtNotes.Text = _contact.Notes
                dtpDate.Value = _contact.ContactDate
                cmbType.SelectedItem = _contact.ContactType.ToString()
            End If
        End Sub

        Private Sub BtnSave_Click(sender As Object, e As EventArgs)
            If cmbCustomer.SelectedValue Is Nothing Then
                MessageBox.Show("Please select a customer.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            If String.IsNullOrWhiteSpace(txtSubject.Text) Then
                MessageBox.Show("Subject is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtSubject.Focus()
                Return
            End If

            _contact.CustomerId = CInt(cmbCustomer.SelectedValue)
            _contact.UserId = If(AppLogger.CurrentUser IsNot Nothing, AppLogger.CurrentUser.Id, 1)
            _contact.ContactType = CType([Enum].Parse(GetType(ContactType), cmbType.SelectedItem.ToString()), ContactType)
            _contact.Subject = txtSubject.Text.Trim()
            _contact.Notes = txtNotes.Text.Trim()
            _contact.ContactDate = dtpDate.Value

            Try
                _contactService.SaveContact(_contact)
                DialogResult = DialogResult.OK
                Close()
            Catch ex As Exception
                MessageBox.Show($"Failed to log interaction: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub
    End Class
End Namespace
