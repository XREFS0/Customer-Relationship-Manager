Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Windows.Forms
Imports EnterpriseCRM.Models
Imports EnterpriseCRM.Services
Imports EnterpriseCRM.UI.Forms
Imports EnterpriseCRM.UI.Theme
Imports EnterpriseCRM.UI.Views

Namespace EnterpriseCRM.UI.Forms
    Public Class MainForm
        Inherits Form

        Private pnlSidebar As Panel
        Private pnlTopHeader As Panel
        Private pnlContentHost As Panel

        Private lblAppTitle As Label
        Private lblUserInfo As Label
        Private lblUserRole As Label
        Private btnLogout As Button

        Private _navButtons As New List(Of Button)()
        Private _currentView As Control = Nothing

        Private _dashboardView As DashboardView
        Private _customersView As CustomersView
        Private _contactsView As ContactsView
        Private _salesView As SalesView
        Private _tasksView As TasksView
        Private _reportsView As ReportsView
        Private _settingsView As SettingsView

        Public Sub New()
            InitializeComponent()
            InitializeViews()
            ApplyTheme()
            NavigateTo("Dashboard")
        End Sub

        Private Sub InitializeComponent()
            Text = "Enterprise CRM - Business Operations Suite"
            Size = New Size(1280, 800)
            MinimumSize = New Size(1024, 650)
            StartPosition = FormStartPosition.CenterScreen
            Font = New Font("Segoe UI", 9.5F)
            BackColor = ThemeManager.BackgroundColor
            ForeColor = ThemeManager.TextPrimaryColor

            pnlSidebar = New Panel With {
                .Dock = DockStyle.Left,
                .Width = 220,
                .BackColor = Color.FromArgb(15, 23, 42)
            }

            Dim pnlBrandLogo As New Panel With {.Dock = DockStyle.Top, .Height = 70, .Padding = New Padding(20, 20, 20, 0)}
            Dim lblLogoTitle As New Label With {
                .Text = "ENTERPRISE CRM",
                .Font = New Font("Segoe UI", 12.5F, FontStyle.Bold),
                .ForeColor = Color.White,
                .AutoSize = True,
                .Location = New Point(18, 22)
            }
            pnlBrandLogo.Controls.Add(lblLogoTitle)
            pnlSidebar.Controls.Add(pnlBrandLogo)

            Dim navItems = New String() {"Dashboard", "Customers", "Contacts", "Sales", "Tasks", "Reports", "Settings"}
            Dim topY = 70

            For Each item In navItems
                Dim btn = CreateNavButton(item, topY)
                pnlSidebar.Controls.Add(btn)
                _navButtons.Add(btn)
                topY += 46
            Next

            pnlTopHeader = New Panel With {
                .Dock = DockStyle.Top,
                .Height = 60,
                .BackColor = Color.White,
                .Padding = New Padding(24, 12, 24, 12)
            }

            lblAppTitle = New Label With {
                .Text = "Al-Rawasi Enterprise CRM",
                .Font = New Font("Segoe UI", 12.0F, FontStyle.Bold),
                .ForeColor = Color.FromArgb(15, 23, 42),
                .AutoSize = True,
                .Location = New Point(20, 18)
            }

            Dim pnlUserArea As New Panel With {
                .Dock = DockStyle.Right,
                .Width = 320,
                .Height = 40
            }

            Dim userFullName = If(AppLogger.CurrentUser IsNot Nothing, AppLogger.CurrentUser.FullName, "Administrator")
            Dim userRole = If(AppLogger.CurrentUser IsNot Nothing, AppLogger.CurrentUser.Role.ToString(), "Admin")

            lblUserInfo = New Label With {
                .Text = userFullName,
                .Font = New Font("Segoe UI", 9.5F, FontStyle.Bold),
                .ForeColor = Color.FromArgb(15, 23, 42),
                .AutoSize = True,
                .Location = New Point(20, 8)
            }
            lblUserRole = New Label With {
                .Text = $"Role: {userRole}",
                .Font = New Font("Segoe UI", 8.5F),
                .ForeColor = Color.FromArgb(100, 116, 139),
                .AutoSize = True,
                .Location = New Point(20, 26)
            }

            btnLogout = New Button With {
                .Text = "Logout",
                .Size = New Size(75, 28),
                .Location = New Point(230, 10)
            }
            ThemeManager.StyleButton(btnLogout, False)
            AddHandler btnLogout.Click, AddressOf BtnLogout_Click

            pnlUserArea.Controls.Add(lblUserInfo)
            pnlUserArea.Controls.Add(lblUserRole)
            pnlUserArea.Controls.Add(btnLogout)

            pnlTopHeader.Controls.Add(lblAppTitle)
            pnlTopHeader.Controls.Add(pnlUserArea)

            pnlContentHost = New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = ThemeManager.BackgroundColor
            }

            Controls.Add(pnlContentHost)
            Controls.Add(pnlTopHeader)
            Controls.Add(pnlSidebar)
        End Sub

        Private Function CreateNavButton(text As String, topY As Integer) As Button
            Dim btn As New Button With {
                .Text = "   " & text,
                .TextAlign = ContentAlignment.MiddleLeft,
                .Location = New Point(0, topY),
                .Size = New Size(220, 44),
                .FlatStyle = FlatStyle.Flat,
                .Font = New Font("Segoe UI", 10.0F, FontStyle.Regular),
                .ForeColor = Color.FromArgb(203, 213, 225),
                .BackColor = Color.FromArgb(15, 23, 42),
                .Cursor = Cursors.Hand,
                .Tag = text
            }
            btn.FlatAppearance.BorderSize = 0
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(30, 41, 59)
            AddHandler btn.Click, Sub() NavigateTo(text)
            Return btn
        End Function

        Private Sub InitializeViews()
            _dashboardView = New DashboardView()
            _customersView = New CustomersView()
            _contactsView = New ContactsView()
            _salesView = New SalesView()
            _tasksView = New TasksView()
            _reportsView = New ReportsView()
            _settingsView = New SettingsView()

            AddHandler _settingsView.ThemeChanged, Sub() ApplyTheme()
        End Sub

        Public Sub NavigateTo(viewName As String)
            For Each btn In _navButtons
                If btn.Tag.ToString() = viewName Then
                    btn.BackColor = Color.FromArgb(37, 99, 235)
                    btn.ForeColor = Color.White
                    btn.Font = New Font("Segoe UI", 10.0F, FontStyle.Bold)
                Else
                    btn.BackColor = Color.FromArgb(15, 23, 42)
                    btn.ForeColor = Color.FromArgb(203, 213, 225)
                    btn.Font = New Font("Segoe UI", 10.0F, FontStyle.Regular)
                End If
            Next

            pnlContentHost.Controls.Clear()

            Select Case viewName
                Case "Dashboard"
                    _currentView = _dashboardView
                    _dashboardView.LoadDashboardData()
                Case "Customers"
                    _currentView = _customersView
                    _customersView.LoadCustomers()
                Case "Contacts"
                    _currentView = _contactsView
                    _contactsView.LoadData()
                Case "Sales"
                    _currentView = _salesView
                    _salesView.LoadSalesData()
                Case "Tasks"
                    _currentView = _tasksView
                    _tasksView.LoadTasksData()
                Case "Reports"
                    _currentView = _reportsView
                    _reportsView.GenerateReport()
                Case "Settings"
                    _currentView = _settingsView
            End Select

            If _currentView IsNot Nothing Then
                _currentView.Dock = DockStyle.Fill
                pnlContentHost.Controls.Add(_currentView)
            End If
        End Sub

        Public Sub ApplyTheme()
            BackColor = ThemeManager.BackgroundColor
            pnlContentHost.BackColor = ThemeManager.BackgroundColor
            pnlTopHeader.BackColor = ThemeManager.HeaderBackgroundColor
            lblAppTitle.ForeColor = ThemeManager.TextPrimaryColor
            lblUserInfo.ForeColor = ThemeManager.TextPrimaryColor
            lblUserRole.ForeColor = ThemeManager.TextSecondaryColor
        End Sub

        Private Sub BtnLogout_Click(sender As Object, e As EventArgs)
            Dim res = MessageBox.Show("Are you sure you want to sign out of the system?", "Sign Out", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
            If res = DialogResult.Yes Then
                AppLogger.Log("LOGOUT", "User", If(AppLogger.CurrentUser IsNot Nothing, AppLogger.CurrentUser.Id, 0), "User logged out.")
                AppLogger.CurrentUser = Nothing
                Hide()
                Using loginForm As New LoginForm()
                    If loginForm.ShowDialog() = DialogResult.OK Then
                        AppLogger.CurrentUser = loginForm.AuthenticatedUser
                        lblUserInfo.Text = AppLogger.CurrentUser.FullName
                        lblUserRole.Text = $"Role: {AppLogger.CurrentUser.Role}"
                        NavigateTo("Dashboard")
                        Show()
                    Else
                        Application.Exit()
                    End If
                End Using
            End If
        End Sub
    End Class
End Namespace
