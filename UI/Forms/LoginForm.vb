Imports System
Imports System.Drawing
Imports System.Windows.Forms
Imports EnterpriseCRM.Models
Imports EnterpriseCRM.Services
Imports EnterpriseCRM.UI.Theme

Namespace EnterpriseCRM.UI.Forms
    Public Class LoginForm
        Inherits Form

        Private txtUsername As TextBox
        Private txtPassword As TextBox
        Private btnLogin As Button
        Private lblError As Label
        Private ReadOnly _authService As New AuthService()

        Public Property AuthenticatedUser As User

        Public Sub New()
            InitializeComponent()
        End Sub

        Private Sub InitializeComponent()
            Text = "Enterprise CRM - Authentication"
            Size = New Size(420, 480)
            FormBorderStyle = FormBorderStyle.FixedDialog
            MaximizeBox = False
            MinimizeBox = False
            StartPosition = FormStartPosition.CenterScreen
            Font = New Font("Segoe UI", 9.5F)
            BackColor = Color.FromArgb(248, 250, 252)

            Dim pnlBrand As New Panel With {
                .Dock = DockStyle.Top,
                .Height = 130,
                .BackColor = Color.FromArgb(15, 23, 42),
                .Padding = New Padding(24, 25, 24, 0)
            }
            Dim lblBrandTitle As New Label With {
                .Text = "ENTERPRISE CRM",
                .Font = New Font("Segoe UI", 16.0F, FontStyle.Bold),
                .ForeColor = Color.White,
                .AutoSize = True,
                .Location = New Point(24, 30)
            }
            Dim lblBrandSubtitle As New Label With {
                .Text = "Customer Relationship Management Suite",
                .Font = New Font("Segoe UI", 9.0F),
                .ForeColor = Color.FromArgb(148, 163, 184),
                .AutoSize = True,
                .Location = New Point(26, 62)
            }
            pnlBrand.Controls.Add(lblBrandTitle)
            pnlBrand.Controls.Add(lblBrandSubtitle)

            Dim pnlBody As New Panel With {
                .Dock = DockStyle.Fill,
                .BackColor = Color.White,
                .Padding = New Padding(30, 25, 30, 20)
            }

            Dim lblUser As New Label With {.Text = "Username", .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold), .ForeColor = Color.FromArgb(71, 85, 105), .Location = New Point(30, 20), .AutoSize = True}
            txtUsername = New TextBox With {.Location = New Point(30, 44), .Size = New Size(345, 30), .Font = New Font("Segoe UI", 10.5F), .Text = "admin"}

            Dim lblPass As New Label With {.Text = "Password", .Font = New Font("Segoe UI", 9.0F, FontStyle.Bold), .ForeColor = Color.FromArgb(71, 85, 105), .Location = New Point(30, 88), .AutoSize = True}
            txtPassword = New TextBox With {.Location = New Point(30, 112), .Size = New Size(345, 30), .Font = New Font("Segoe UI", 10.5F), .UseSystemPasswordChar = True, .Text = "Admin@12345"}

            lblError = New Label With {
                .ForeColor = Color.FromArgb(220, 38, 38),
                .Font = New Font("Segoe UI", 8.5F),
                .Location = New Point(30, 150),
                .Size = New Size(345, 30),
                .Visible = False
            }

            btnLogin = New Button With {
                .Text = "Sign In",
                .Location = New Point(30, 185),
                .Size = New Size(345, 42),
                .Font = New Font("Segoe UI", 10.0F, FontStyle.Bold),
                .BackColor = Color.FromArgb(37, 99, 235),
                .ForeColor = Color.White,
                .FlatStyle = FlatStyle.Flat,
                .Cursor = Cursors.Hand
            }
            btnLogin.FlatAppearance.BorderSize = 0
            AddHandler btnLogin.Click, AddressOf BtnLogin_Click

            Dim lblHint As New Label With {
                .Text = "Default Credentials: admin / Admin@12345",
                .Font = New Font("Segoe UI", 8.0F, FontStyle.Italic),
                .ForeColor = Color.FromArgb(148, 163, 184),
                .TextAlign = ContentAlignment.MiddleCenter,
                .Location = New Point(30, 240),
                .Size = New Size(345, 20)
            }

            pnlBody.Controls.Add(lblUser)
            pnlBody.Controls.Add(txtUsername)
            pnlBody.Controls.Add(lblPass)
            pnlBody.Controls.Add(txtPassword)
            pnlBody.Controls.Add(lblError)
            pnlBody.Controls.Add(btnLogin)
            pnlBody.Controls.Add(lblHint)

            Controls.Add(pnlBody)
            Controls.Add(pnlBrand)
            AcceptButton = btnLogin
        End Sub

        Private Sub BtnLogin_Click(sender As Object, e As EventArgs)
            lblError.Visible = False
            Try
                Dim user = _authService.Login(txtUsername.Text.Trim(), txtPassword.Text)
                If user IsNot Nothing Then
                    AuthenticatedUser = user
                    DialogResult = DialogResult.OK
                    Close()
                Else
                    lblError.Text = "Invalid username or password."
                    lblError.Visible = True
                End If
            Catch ex As Exception
                lblError.Text = ex.Message
                lblError.Visible = True
            End Try
        End Sub
    End Class
End Namespace
