Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports EnterpriseCRM.Models
Imports EnterpriseCRM.Services
Imports EnterpriseCRM.UI.Controls
Imports EnterpriseCRM.UI.Theme

Namespace EnterpriseCRM.UI.Views
    Public Class DashboardView
        Inherits UserControl

        Private ReadOnly _dashboardService As New DashboardService()
        Private ReadOnly _customerService As New CustomerService()

        Private cardCustomers As StatCardControl
        Private cardLeads As StatCardControl
        Private cardWonSales As StatCardControl
        Private cardTasks As StatCardControl

        Private chartPipeline As SimpleBarChartControl
        Private dgvActivities As DataGridView

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Dock = DockStyle.Fill
            BackColor = ThemeManager.BackgroundColor
            Font = New Font("Segoe UI", 9.5F)

            Dim pnlScroll As New Panel With {
                .Dock = DockStyle.Fill,
                .AutoScroll = True,
                .Padding = New Padding(24, 20, 24, 20)
            }

            Dim pnlTitle As New Panel With {.Dock = DockStyle.Top, .Height = 50}
            Dim lblTitle As New Label With {
                .Text = "Executive Dashboard",
                .Font = New Font("Segoe UI", 16.0F, FontStyle.Bold),
                .ForeColor = ThemeManager.TextPrimaryColor,
                .AutoSize = True
            }
            Dim btnRefresh As New Button With {
                .Text = "Refresh Data",
                .Size = New Size(110, 32),
                .Anchor = AnchorStyles.Top Or AnchorStyles.Right,
                .Location = New Point(pnlScroll.Width - 140, 6)
            }
            ThemeManager.StyleButton(btnRefresh, False)
            AddHandler btnRefresh.Click, Sub() LoadDashboardData()

            pnlTitle.Controls.Add(lblTitle)
            pnlTitle.Controls.Add(btnRefresh)

            Dim flowKpi As New FlowLayoutPanel With {
                .Dock = DockStyle.Top,
                .Height = 120,
                .WrapContents = False,
                .AutoScroll = False
            }

            cardCustomers = New StatCardControl With {.Title = "Total Customers", .Value = "0", .Subtext = "Accounts Registered", .AccentColor = ThemeManager.BrandPrimary}
            cardLeads = New StatCardControl With {.Title = "Active Leads", .Value = "0", .Subtext = "In Pipeline Qualification", .AccentColor = ThemeManager.BrandWarning}
            cardWonSales = New StatCardControl With {.Title = "Total Sales Won", .Value = "$0", .Subtext = "Revenue Closed", .AccentColor = ThemeManager.BrandSuccess}
            cardTasks = New StatCardControl With {.Title = "Completed Tasks", .Value = "0", .Subtext = "Team Execution", .AccentColor = ThemeManager.BrandInfo}

            flowKpi.Controls.Add(cardCustomers)
            flowKpi.Controls.Add(cardLeads)
            flowKpi.Controls.Add(cardWonSales)
            flowKpi.Controls.Add(cardTasks)

            Dim tableMain As New TableLayoutPanel With {
                .Dock = DockStyle.Top,
                .Height = 360,
                .ColumnCount = 2,
                .RowCount = 1,
                .Margin = New Padding(0, 15, 0, 0)
            }
            tableMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 45.0F))
            tableMain.ColumnStyles.Add(New ColumnStyle(SizeType.Percent, 55.0F))

            chartPipeline = New SimpleBarChartControl With {.Dock = DockStyle.Fill, .Title = "Opportunity Pipeline Distribution"}

            Dim pnlActivitiesCard As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = ThemeManager.CardBackgroundColor,
                .Padding = New Padding(16),
                .Margin = New Padding(10, 0, 0, 0)
            }
            Dim lblActTitle As New Label With {
                .Text = "Recent Audit & Activity Log",
                .Font = New Font("Segoe UI", 11.0F, FontStyle.Bold),
                .ForeColor = ThemeManager.TextPrimaryColor,
                .Dock = DockStyle.Top,
                .Height = 30
            }
            dgvActivities = New DataGridView With {.Dock = DockStyle.Fill}
            ThemeManager.StyleGridView(dgvActivities)
            dgvActivities.Columns.Add("Time", "Time")
            dgvActivities.Columns.Add("User", "User")
            dgvActivities.Columns.Add("Action", "Action")
            dgvActivities.Columns.Add("Details", "Details")
            dgvActivities.Columns("Details").AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill

            pnlActivitiesCard.Controls.Add(dgvActivities)
            pnlActivitiesCard.Controls.Add(lblActTitle)

            tableMain.Controls.Add(chartPipeline, 0, 0)
            tableMain.Controls.Add(pnlActivitiesCard, 1, 0)

            pnlScroll.Controls.Add(tableMain)
            pnlScroll.Controls.Add(flowKpi)
            pnlScroll.Controls.Add(pnlTitle)

            Controls.Add(pnlScroll)
        End Sub

        Public Sub LoadDashboardData()
            Try
                Dim metrics = _dashboardService.GetDashboardMetrics()
                cardCustomers.Value = metrics.TotalCustomers.ToString("N0")
                cardLeads.Value = metrics.ActiveLeads.ToString("N0")
                cardWonSales.Value = $"${metrics.TotalSalesWonAmount:N0}"
                cardTasks.Value = metrics.CompletedTasks.ToString("N0")
                cardTasks.Subtext = $"{metrics.PendingTasksCount} Tasks Pending"
                cardWonSales.Subtext = $"Win Rate: {metrics.WinRatePercentage}%"

                Dim stageData = _dashboardService.GetPipelineStagesSummary()
                Dim chartItems As New List(Of SimpleBarChartControl.ChartItem)()
                Dim colors = New Color() {ThemeManager.BrandInfo, ThemeManager.BrandPrimary, ThemeManager.BrandWarning, ThemeManager.BrandSuccess, ThemeManager.BrandDanger}
                Dim idx = 0
                For Each stg In stageData
                    chartItems.Add(New SimpleBarChartControl.ChartItem With {
                        .Label = stg.StageName,
                        .Value = stg.TotalAmount,
                        .Color = colors(idx Mod colors.Length)
                    })
                    idx += 1
                Next
                chartPipeline.SetData(chartItems)

                dgvActivities.Rows.Clear()
                Dim activities = _dashboardService.GetRecentActivities(10)
                For Each act In activities
                    dgvActivities.Rows.Add(act.Timestamp.ToString("HH:mm:ss"), act.UserName, act.Action, act.Details)
                Next
            Catch ex As Exception
                AppLogger.LogException(ex, "LoadDashboardData")
            End Try
        End Sub
    End Class
End Namespace
