Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports EnterpriseCRM.Models
Imports EnterpriseCRM.Services
Imports EnterpriseCRM.UI.Forms
Imports EnterpriseCRM.UI.Theme

Namespace EnterpriseCRM.UI.Views
    Public Class SalesView
        Inherits UserControl

        Private ReadOnly _salesService As New SalesService()
        Private ReadOnly _reportService As New ReportExportService()

        Private dgvSales As DataGridView
        Private cmbStageFilter As ComboBox
        Private lblPipelineTotal As Label
        Private lblWonTotal As Label

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
                .Text = "Sales Pipeline & Opportunities",
                .Font = New Font("Segoe UI", 16.0F, FontStyle.Bold),
                .ForeColor = ThemeManager.TextPrimaryColor,
                .AutoSize = True
            }
            Dim lblSub As New Label With {
                .Text = "Track lead conversion, deal stages, probability, and revenue forecasting.",
                .Font = New Font("Segoe UI", 8.5F),
                .ForeColor = ThemeManager.TextSecondaryColor,
                .Location = New Point(0, 32),
                .AutoSize = True
            }

            Dim btnAdd As New Button With {.Text = "+ New Opportunity", .Size = New Size(150, 36), .Anchor = AnchorStyles.Top Or AnchorStyles.Right, .Location = New Point(pnlMain.Width - 360, 10)}
            Dim btnExport As New Button With {.Text = "Export Pipeline", .Size = New Size(130, 36), .Anchor = AnchorStyles.Top Or AnchorStyles.Right, .Location = New Point(pnlMain.Width - 190, 10)}
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
                .Padding = New Padding(12, 10, 12, 10)
            }

            Dim lblFilter As New Label With {.Text = "Stage:", .AutoSize = True, .Location = New Point(12, 16), .ForeColor = ThemeManager.TextSecondaryColor}
            cmbStageFilter = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(60, 12), .Size = New Size(150, 26)}
            cmbStageFilter.Items.AddRange(New Object() {"All Stages", "NewLead", "Contacted", "Negotiation", "Won", "Lost"})
            cmbStageFilter.SelectedIndex = 0
            AddHandler cmbStageFilter.SelectedIndexChanged, Sub() LoadSalesData()

            lblPipelineTotal = New Label With {.AutoSize = True, .Location = New Point(250, 16), .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold), .ForeColor = ThemeManager.BrandPrimary}
            lblWonTotal = New Label With {.AutoSize = True, .Location = New Point(470, 16), .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold), .ForeColor = ThemeManager.BrandSuccess}

            pnlFilter.Controls.Add(lblFilter)
            pnlFilter.Controls.Add(cmbStageFilter)
            pnlFilter.Controls.Add(lblPipelineTotal)
            pnlFilter.Controls.Add(lblWonTotal)

            Dim pnlGridCard As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = ThemeManager.CardBackgroundColor,
                .Padding = New Padding(12),
                .Margin = New Padding(0, 12, 0, 0)
            }

            dgvSales = New DataGridView With {.Dock = DockStyle.Fill}
            ThemeManager.StyleGridView(dgvSales)

            dgvSales.Columns.Add("Id", "ID")
            dgvSales.Columns("Id").Width = 60
            dgvSales.Columns.Add("Title", "Opportunity Title")
            dgvSales.Columns("Title").Width = 220
            dgvSales.Columns.Add("Customer", "Customer Account")
            dgvSales.Columns("Customer").Width = 180
            dgvSales.Columns.Add("Amount", "Deal Value ($)")
            dgvSales.Columns("Amount").Width = 120
            dgvSales.Columns.Add("Stage", "Pipeline Stage")
            dgvSales.Columns("Stage").Width = 130
            dgvSales.Columns.Add("Prob", "Win Probability")
            dgvSales.Columns("Prob").Width = 120
            dgvSales.Columns.Add("Owner", "Opportunity Owner")
            dgvSales.Columns("Owner").Width = 140
            dgvSales.Columns.Add("CloseDate", "Est. Close Date")
            dgvSales.Columns("CloseDate").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

            AddHandler dgvSales.CellDoubleClick, AddressOf DgvSales_CellDoubleClick

            Dim ctxMenu As New ContextMenuStrip()
            ctxMenu.Items.Add("Edit Opportunity", Nothing, AddressOf OnEditSale)
            
            Dim mnuQuickStage = New ToolStripMenuItem("Move Stage To...")
            mnuQuickStage.DropDownItems.Add("New Lead", Nothing, Sub() MoveStage(PipelineStage.NewLead))
            mnuQuickStage.DropDownItems.Add("Contacted", Nothing, Sub() MoveStage(PipelineStage.Contacted))
            mnuQuickStage.DropDownItems.Add("Negotiation", Nothing, Sub() MoveStage(PipelineStage.Negotiation))
            mnuQuickStage.DropDownItems.Add("Won (Closed)", Nothing, Sub() MoveStage(PipelineStage.Won))
            mnuQuickStage.DropDownItems.Add("Lost (Closed)", Nothing, Sub() MoveStage(PipelineStage.Lost))
            ctxMenu.Items.Add(mnuQuickStage)

            ctxMenu.Items.Add(New ToolStripSeparator())
            ctxMenu.Items.Add("Delete Opportunity", Nothing, AddressOf OnDeleteSale)
            dgvSales.ContextMenuStrip = ctxMenu

            pnlGridCard.Controls.Add(dgvSales)

            pnlMain.Controls.Add(pnlGridCard)
            pnlMain.Controls.Add(pnlFilter)
            pnlMain.Controls.Add(pnlHeader)

            Controls.Add(pnlMain)
        End Sub

        Public Sub LoadSalesData()
            Dim sales = _salesService.GetAllSales()
            Dim filterStage As String = If(cmbStageFilter.SelectedIndex > 0, cmbStageFilter.SelectedItem.ToString(), "")

            dgvSales.Rows.Clear()
            Dim activePipelineVal As Decimal = 0
            Dim wonVal As Decimal = 0

            For Each s In sales
                If s.Stage = PipelineStage.Won Then
                    wonVal += s.Amount
                ElseIf s.Stage <> PipelineStage.Lost Then
                    activePipelineVal += s.Amount
                End If

                If Not String.IsNullOrEmpty(filterStage) AndAlso s.Stage.ToString() <> filterStage Then Continue For

                Dim dtStr = If(s.ExpectedCloseDate.HasValue, s.ExpectedCloseDate.Value.ToString("yyyy-MM-dd"), "-")
                dgvSales.Rows.Add(s.Id, s.Title, s.CustomerName, $"${s.Amount:N2}", s.StageName, $"{s.Probability}%", s.UserName, dtStr)
            Next

            lblPipelineTotal.Text = $"Active Pipeline: ${activePipelineVal:N2}"
            lblWonTotal.Text = $"Won Deals: ${wonVal:N2}"
        End Sub

        Private Function GetSelectedSaleId() As Integer
            If dgvSales.SelectedRows.Count > 0 Then
                Return Convert.ToInt32(dgvSales.SelectedRows(0).Cells("Id").Value)
            End If
            Return 0
        End Function

        Private Sub MoveStage(newStage As PipelineStage)
            Dim id = GetSelectedSaleId()
            If id > 0 Then
                _salesService.UpdateStage(id, newStage)
                LoadSalesData()
            End If
        End Sub

        Private Sub BtnAdd_Click(sender As Object, e As EventArgs)
            Using dlg As New SaleEditDialog()
                If dlg.ShowDialog(Me) = DialogResult.OK Then
                    LoadSalesData()
                End If
            End Using
        End Sub

        Private Sub DgvSales_CellDoubleClick(sender As Object, e As DataGridViewCellEventArgs)
            If e.RowIndex >= 0 Then
                OnEditSale(sender, EventArgs.Empty)
            End If
        End Sub

        Private Sub OnEditSale(sender As Object, e As EventArgs)
            Dim id = GetSelectedSaleId()
            If id > 0 Then
                Dim sale = _salesService.GetSaleById(id)
                If sale IsNot Nothing Then
                    Using dlg As New SaleEditDialog(sale)
                        If dlg.ShowDialog(Me) = DialogResult.OK Then
                            LoadSalesData()
                        End If
                    End Using
                End If
            End If
        End Sub

        Private Sub OnDeleteSale(sender As Object, e As EventArgs)
            Dim id = GetSelectedSaleId()
            If id > 0 Then
                Dim res = MessageBox.Show("Are you sure you want to delete this opportunity?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                If res = DialogResult.Yes Then
                    _salesService.DeleteSale(id)
                    LoadSalesData()
                End If
            End If
        End Sub

        Private Sub BtnExport_Click(sender As Object, e As EventArgs)
            Using sfd As New SaveFileDialog With {
                .Filter = "CSV File (*.csv)|*.csv|All Files (*.*)|*.*",
                .FileName = $"sales_pipeline_{DateTime.Now:yyyyMMdd}.csv"
            }
                If sfd.ShowDialog() = DialogResult.OK Then
                    Dim list = _salesService.GetAllSales()
                    _reportService.ExportSalesToCsv(list, sfd.FileName)
                    MessageBox.Show("Sales pipeline exported successfully!", "Export Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            End Using
        End Sub
    End Class
End Namespace
