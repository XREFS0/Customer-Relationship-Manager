Imports System
Imports System.Windows.Forms
Imports EnterpriseCRM.Data
Imports EnterpriseCRM.Services
Imports EnterpriseCRM.UI.Forms

Namespace EnterpriseCRM
    Public Module Program
        <STAThread>
        Public Sub Main()
            Application.EnableVisualStyles()
            Application.SetCompatibleTextRenderingDefault(False)

            AddHandler Application.ThreadException, Sub(s, e)
                AppLogger.LogException(e.Exception, "Application.ThreadException")
                MessageBox.Show($"An unexpected application error occurred: {e.Exception.Message}", "System Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Sub

            AddHandler AppDomain.CurrentDomain.UnhandledException, Sub(s, e)
                Dim ex = TryCast(e.ExceptionObject, Exception)
                If ex IsNot Nothing Then
                    AppLogger.LogException(ex, "AppDomain.UnhandledException")
                End If
            End Sub

            Try
                DbInitializer.Initialize()

                Using loginForm As New LoginForm()
                    If loginForm.ShowDialog() = DialogResult.OK Then
                        AppLogger.CurrentUser = loginForm.AuthenticatedUser
                        Application.Run(New MainForm())
                    Else
                        Application.Exit()
                    End If
                End Using
            Catch ex As Exception
                AppLogger.LogException(ex, "Application.Startup")
                MessageBox.Show($"Startup Error: {ex.Message}", "Fatal Startup Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        End Sub
    End Module
End Namespace
