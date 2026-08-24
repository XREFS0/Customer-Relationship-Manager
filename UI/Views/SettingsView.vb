Imports System
Imports System.Diagnostics
Imports System.Drawing
Imports System.IO
Imports System.Windows.Forms
Imports EnterpriseCRM.Data.Repositories
Imports EnterpriseCRM.Services
Imports EnterpriseCRM.UI.Theme

Namespace EnterpriseCRM.UI.Views
    Public Class SettingsView
        Inherits UserControl

        Private ReadOnly _settingsRepo As New AppSettingRepository()
        Private ReadOnly _backupService As New BackupRestoreService()
        Private ReadOnly _authService As New AuthService()

        Private txtCompanyName As TextBox
        Private txtCurrency As TextBox
        Private cmbTheme As ComboBox
        Private btnSaveSettings As Button

        Private btnBackup As Button
        Private btnRestore As Button
        Private btnOpenLog As Button

        Private txtOldPass As TextBox
        Private txtNewPass As TextBox
        Private btnChangePass As Button

        Public Event ThemeChanged As EventHandler

        Public Sub New()
            InitializeComponent()
            LoadSettings()
        End Sub

        Private Sub InitializeComponent()
            Dock = DockStyle.Fill
            BackColor = ThemeManager.BackgroundColor
            Font = New Font("Segoe UI", 9.5F)

            Dim pnlScroll As New Panel With {.Dock = DockStyle.Fill, .AutoScroll = True, .Padding = New Padding(24, 20, 24, 20)}

            Dim pnlHeader As New Panel With {.Dock = DockStyle.Top, .Height = 65}
            Dim lblTitle As New Label With {
                .Text = "System Configuration & Maintenance",
                .Font = New Font("Segoe UI", 16.0F, FontStyle.Bold),
                .ForeColor = ThemeManager.TextPrimaryColor,
                .AutoSize = True
            }
            Dim lblSub As New Label With {
                .Text = "Manage organization profile, security credentials, database backup & audit logs.",
                .Font = New Font("Segoe UI", 8.5F),
                .ForeColor = ThemeManager.TextSecondaryColor,
                .Location = New Point(0, 32),
                .AutoSize = True
            }
            pnlHeader.Controls.Add(lblTitle)
            pnlHeader.Controls.Add(lblSub)

            Dim grpGeneral = CreateCard("Organization & Display Settings", 190)
            Dim lblComp = New Label With {.Text = "Company Name:", .Location = New Point(20, 40), .AutoSize = True, .ForeColor = ThemeManager.TextSecondaryColor}
            txtCompanyName = New TextBox With {.Location = New Point(20, 62), .Size = New Size(280, 26)}

            Dim lblCurr = New Label With {.Text = "Currency Symbol:", .Location = New Point(320, 40), .AutoSize = True, .ForeColor = ThemeManager.TextSecondaryColor}
            txtCurrency = New TextBox With {.Location = New Point(320, 62), .Size = New Size(100, 26)}

            Dim lblThm = New Label With {.Text = "Interface Theme:", .Location = New Point(440, 40), .AutoSize = True, .ForeColor = ThemeManager.TextSecondaryColor}
            cmbTheme = New ComboBox With {.DropDownStyle = ComboBoxStyle.DropDownList, .Location = New Point(440, 62), .Size = New Size(160, 26)}
            cmbTheme.Items.AddRange(New Object() {"Enterprise Light", "Slate Dark"})
            cmbTheme.SelectedIndex = 0

            btnSaveSettings = New Button With {.Text = "Save Preferences", .Location = New Point(20, 115), .Size = New Size(160, 34)}
            ThemeManager.StyleButton(btnSaveSettings, True)
            AddHandler btnSaveSettings.Click, AddressOf BtnSaveSettings_Click

            grpGeneral.Controls.Add(lblComp)
            grpGeneral.Controls.Add(txtCompanyName)
            grpGeneral.Controls.Add(lblCurr)
            grpGeneral.Controls.Add(txtCurrency)
            grpGeneral.Controls.Add(lblThm)
            grpGeneral.Controls.Add(cmbTheme)
            grpGeneral.Controls.Add(btnSaveSettings)

            Dim grpDb = CreateCard("Database Backup & Maintenance", 160)
            grpDb.Top = 270
            Dim lblDbDesc = New Label With {
                .Text = "Create timestamped SQLite database backups or restore system data from an archive.",
                .Location = New Point(20, 40),
                .AutoSize = True,
                .ForeColor = ThemeManager.TextSecondaryColor
            }
            btnBackup = New Button With {.Text = "Create Database Backup (.bak)", .Location = New Point(20, 75), .Size = New Size(220, 36)}
            btnRestore = New Button With {.Text = "Restore Database Backup", .Location = New Point(260, 75), .Size = New Size(200, 36)}
            btnOpenLog = New Button With {.Text = "View System Log File", .Location = New Point(480, 75), .Size = New Size(180, 36)}

            ThemeManager.StyleButton(btnBackup, True)
            ThemeManager.StyleButton(btnRestore, False)
            ThemeManager.StyleButton(btnOpenLog, False)

            AddHandler btnBackup.Click, AddressOf BtnBackup_Click
            AddHandler btnRestore.Click, AddressOf BtnRestore_Click
            AddHandler btnOpenLog.Click, AddressOf BtnOpenLog_Click

            grpDb.Controls.Add(lblDbDesc)
            grpDb.Controls.Add(btnBackup)
            grpDb.Controls.Add(btnRestore)
            grpDb.Controls.Add(btnOpenLog)

            Dim grpSecurity = CreateCard("Account Security & Password", 180)
            grpSecurity.Top = 450

            Dim lblOld = New Label With {.Text = "Current Password:", .Location = New Point(20, 40), .AutoSize = True, .ForeColor = ThemeManager.TextSecondaryColor}
            txtOldPass = New TextBox With {.Location = New Point(20, 62), .Size = New Size(220, 26), .UseSystemPasswordChar = True}

            Dim lblNew = New Label With {.Text = "New Password (min 6 chars):", .Location = New Point(260, 40), .AutoSize = True, .ForeColor = ThemeManager.TextSecondaryColor}
            txtNewPass = New TextBox With {.Location = New Point(260, 62), .Size = New Size(220, 26), .UseSystemPasswordChar = True}

            btnChangePass = New Button With {.Text = "Update Password", .Location = New Point(20, 110), .Size = New Size(160, 34)}
            ThemeManager.StyleButton(btnChangePass, True)
            AddHandler btnChangePass.Click, AddressOf BtnChangePass_Click

            grpSecurity.Controls.Add(lblOld)
            grpSecurity.Controls.Add(txtOldPass)
            grpSecurity.Controls.Add(lblNew)
            grpSecurity.Controls.Add(txtNewPass)
            grpSecurity.Controls.Add(btnChangePass)

            pnlScroll.Controls.Add(grpSecurity)
            pnlScroll.Controls.Add(grpDb)
            pnlScroll.Controls.Add(grpGeneral)
            pnlScroll.Controls.Add(pnlHeader)

            Controls.Add(pnlScroll)
        End Sub

        Private Function CreateCard(title As String, height As Integer) As Panel
            Dim pnl As New Panel With {
                .Dock = DockStyle.Top,
                .Height = height,
                .BackColor = ThemeManager.CardBackgroundColor,
                .Padding = New Padding(20),
                .Margin = New Padding(0, 0, 0, 16)
            }
            Dim lbl As New Label With {
                .Text = title,
                .Font = New Font("Segoe UI", 11.0F, FontStyle.Bold),
                .ForeColor = ThemeManager.TextPrimaryColor,
                .Location = New Point(20, 12),
                .AutoSize = True
            }
            pnl.Controls.Add(lbl)
            Return pnl
        End Function

        Private Sub LoadSettings()
            txtCompanyName.Text = _settingsRepo.GetValue("CompanyName", "Al-Rawasi Enterprise Systems")
            txtCurrency.Text = _settingsRepo.GetValue("CurrencySymbol", "$")
            Dim curTheme = _settingsRepo.GetValue("Theme", "Light")
            cmbTheme.SelectedIndex = If(curTheme = "Dark", 1, 0)
        End Sub

        Private Sub BtnSaveSettings_Click(sender As Object, e As EventArgs)
            _settingsRepo.SetValue("CompanyName", txtCompanyName.Text.Trim(), "Organization Legal Name")
            _settingsRepo.SetValue("CurrencySymbol", txtCurrency.Text.Trim(), "Currency Symbol")
            
            Dim themeChoice = If(cmbTheme.SelectedIndex = 1, "Dark", "Light")
            _settingsRepo.SetValue("Theme", themeChoice, "Active UI Theme")

            ThemeManager.CurrentTheme = If(themeChoice = "Dark", AppThemeMode.Dark, AppThemeMode.Light)
            AppLogger.Log("SETTINGS_UPDATE", "System", Nothing, "Updated system settings and preferences.")

            RaiseEvent ThemeChanged(Me, EventArgs.Empty)
            MessageBox.Show("Settings saved successfully.", "Settings Updated", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Sub

        Private Sub BtnBackup_Click(sender As Object, e As EventArgs)
            Using sfd As New SaveFileDialog With {
                .Filter = "Database Backup (*.bak)|*.bak|SQLite DB (*.db)|*.db",
                .FileName = $"crm_backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak"
            }
                If sfd.ShowDialog() = DialogResult.OK Then
                    Try
                        _backupService.CreateBackup(sfd.FileName)
                        MessageBox.Show("Database backup created successfully!", "Backup Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    Catch ex As Exception
                        MessageBox.Show($"Backup failed: {ex.Message}", "Backup Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                    End Try
                End If
            End Using
        End Sub

        Private Sub BtnRestore_Click(sender As Object, e As EventArgs)
            Dim warn = MessageBox.Show("Restoring a database will overwrite current CRM data. Are you sure you want to proceed?", "Restore Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
            If warn = DialogResult.Yes Then
                Using ofd As New OpenFileDialog With {
                    .Filter = "Database Backup (*.bak;*.db)|*.bak;*.db|All Files (*.*)|*.*"
                }
                    If ofd.ShowDialog() = DialogResult.OK Then
                        Try
                            _backupService.RestoreBackup(ofd.FileName)
                            MessageBox.Show("Database restored successfully! The application should be restarted.", "Restore Complete", MessageBoxButtons.OK, MessageBoxIcon.Information)
                        Catch ex As Exception
                            MessageBox.Show($"Restore failed: {ex.Message}", "Restore Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                        End Try
                    End If
                End Using
            End If
        End Sub

        Private Sub BtnOpenLog_Click(sender As Object, e As EventArgs)
            Dim logPath = AppLogger.LogFilePath
            If File.Exists(logPath) Then
                Process.Start(New ProcessStartInfo(logPath) With {.UseShellExecute = True})
            Else
                MessageBox.Show("Log file has not been created yet.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        End Sub

        Private Sub BtnChangePass_Click(sender As Object, e As EventArgs)
            If AppLogger.CurrentUser Is Nothing Then
                MessageBox.Show("No active user logged in.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                Return
            End If

            If String.IsNullOrWhiteSpace(txtOldPass.Text) OrElse String.IsNullOrWhiteSpace(txtNewPass.Text) Then
                MessageBox.Show("Please enter both current and new password.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning)
                Return
            End If

            Try
                Dim ok = _authService.ChangePassword(AppLogger.CurrentUser.Id, txtOldPass.Text, txtNewPass.Text)
                If ok Then
                    MessageBox.Show("Password changed successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
                    txtOldPass.Clear()
                    txtNewPass.Clear()
                Else
                    MessageBox.Show("Failed to change password. Please check your credentials.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
                End If
            Catch ex As Exception
                MessageBox.Show(ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub
    End Class
End Namespace
