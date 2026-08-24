Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports EnterpriseCRM.Models
Imports EnterpriseCRM.Services
Imports EnterpriseCRM.UI.Theme

Namespace EnterpriseCRM.UI.Forms
    Public Class SaleEditDialog
        Inherits Form

        Private _sale As SaleOpportunity
        Private ReadOnly _salesService As New SalesService()
        Private ReadOnly _customerService As New CustomerService()

        Private cmbCustomer As ComboBox
        Private txtTitle As TextBox
        Private numAmount As NumericUpDown
        Private cmbStage As ComboBox
        Private numProbability As NumericUpDown
        Private dtpExpectedClose As DateTimePicker
        Private txtNotes As TextBox
        Private btnSave As Button
        Private btnCancel As Button

        Private _customerList As List(Of Customer)

        Public Sub New(Optional sale As SaleOpportunity = Nothing)
            _sale = If(sale, New SaleOpportunity())
            InitializeComponent()
            LoadData()
        End Sub

        Private Sub InitializeComponent()
            Text = If(_sale.Id = 0, "New Sales Opportunity", $"Edit Opportunity - {_sale.Title}")
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
                .Text = "Sales Deal / Opportunity Information",
                .Font = New Font("Segoe UI", 12.0F, FontStyle.Bold),
                .ForeColor = ThemeManager.TextPrimaryColor,
                .AutoSize = True
            }
            pnlTop.Controls.Add(lblTitle)

            Dim pnlBottom As New Panel With {.Dock = DockStyle.Bottom, .Height = 60, .Padding = New Padding(20, 10, 20, 10)}
            btnSave = New Button With {.Text = "Save Opportunity", .Size = New Size(150, 36), .Location = New Point(230, 12)}
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

            cmbCustomer = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9.5F)}
            txtTitle = New TextBox With {.Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9.5F)}
            
            numAmount = New NumericUpDown With {.Dock = DockStyle.Fill, .Maximum = 100000000, .DecimalPlaces = 2, .ThousandsSeparator = True, .Font = New Font("Segoe UI", 9.5F)}
            numProbability = New NumericUpDown With {.Dock = DockStyle.Fill, .Maximum = 100, .Minimum = 0, .Value = 20, .Font = New Font("Segoe UI", 9.5F)}
            
            cmbStage = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9.5F)}
            cmbStage.Items.AddRange(New Object() {"NewLead", "Contacted", "Negotiation", "Won", "Lost"})
            cmbStage.SelectedIndex = 0

            dtpExpectedClose = New DateTimePicker With {.Format = DateTimePickerFormat.Short, .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9.5F)}
            txtNotes = New TextBox With {.Multiline = True, .Dock = DockStyle.Fill, .Font = New Font("Segoe UI", 9.5F)}

            layout.Controls.Add(CreateField("Deal Title *", txtTitle), 0, 0)
            layout.Controls.Add(CreateField("Customer Account *", cmbCustomer), 1, 0)

            layout.Controls.Add(CreateField("Deal Value ($)", numAmount), 0, 1)
            layout.Controls.Add(CreateField("Pipeline Stage", cmbStage), 1, 1)

            layout.Controls.Add(CreateField("Win Probability (%)", numProbability), 0, 2)
            layout.Controls.Add(CreateField("Expected Close Date", dtpExpectedClose), 1, 2)

            Dim pnlNotes As New Panel With {.Dock = DockStyle.Fill, .Height = 80}
            Dim lblNotes As New Label With {.Text = "Opportunity Notes", .Dock = DockStyle.Top, .Height = 20, .ForeColor = ThemeManager.TextSecondaryColor}
            pnlNotes.Controls.Add(txtNotes)
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
            _customerList = _customerService.GetAllCustomers()
            cmbCustomer.DisplayMember = "Name"
            cmbCustomer.ValueMember = "Id"
            cmbCustomer.DataSource = _customerList

            If _sale.CustomerId > 0 Then
                cmbCustomer.SelectedValue = _sale.CustomerId
            End If

            If _sale.Id > 0 Then
                txtTitle.Text = _sale.Title
                numAmount.Value = _sale.Amount
                cmbStage.SelectedItem = _sale.Stage.ToString()
                numProbability.Value = _sale.Probability
                If _sale.ExpectedCloseDate.HasValue Then
                    dtpExpectedClose.Value = _sale.ExpectedCloseDate.Value
                End If
                txtNotes.Text = _sale.Notes
            End If
        End Sub

        Private Sub BtnSave_Click(sender As Object, e As EventArgs)
            If cmbCustomer.SelectedValue Is Nothing Then
                MessageBox.Show("Please select a customer account.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            If String.IsNullOrWhiteSpace(txtTitle.Text) Then
                MessageBox.Show("Opportunity title is required.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                txtTitle.Focus()
                Return
            End If

            _sale.CustomerId = CInt(cmbCustomer.SelectedValue)
            _sale.UserId = If(AppLogger.CurrentUser IsNot Nothing, AppLogger.CurrentUser.Id, 1)
            _sale.Title = txtTitle.Text.Trim()
            _sale.Amount = numAmount.Value
            _sale.Stage = CType([Enum].Parse(GetType(PipelineStage), cmbStage.SelectedItem.ToString()), PipelineStage)
            _sale.Probability = CInt(numProbability.Value)
            _sale.ExpectedCloseDate = dtpExpectedClose.Value
            _sale.Notes = txtNotes.Text.Trim()

            Try
                _salesService.SaveSale(_sale)
                DialogResult = DialogResult.OK
                Close()
            Catch ex As Exception
                MessageBox.Show($"Failed to save deal: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub
    End Class
End Namespace
