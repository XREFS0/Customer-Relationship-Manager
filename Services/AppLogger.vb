Imports System
Imports System.IO
Imports EnterpriseCRM.Data.Repositories
Imports EnterpriseCRM.Models

Namespace EnterpriseCRM.Services
    Public Class AppLogger
        Private Shared _logRepo As New ActivityLogRepository()
        Private Shared _logFilePath As String = String.Empty

        Public Shared Property CurrentUser As User = Nothing

        Public Shared ReadOnly Property LogFilePath As String
            Get
                If String.IsNullOrEmpty(_logFilePath) Then
                    Dim logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs")
                    If Not Directory.Exists(logDir) Then
                        Directory.CreateDirectory(logDir)
                    End If
                    _logFilePath = Path.Combine(logDir, $"crm_log_{DateTime.Today:yyyyMMdd}.txt")
                End If
                Return _logFilePath
            End Get
        End Property

        Public Shared Sub Log(action As String, entityName As String, Optional entityId As Nullable(Of Integer) = Nothing, Optional details As String = "")
            Try
                Dim userId As Integer = If(CurrentUser IsNot Nothing, CurrentUser.Id, 1)
                Dim userName As String = If(CurrentUser IsNot Nothing, CurrentUser.FullName, "System")

                Dim logEntry As New ActivityLog With {
                    .UserId = userId,
                    .UserName = userName,
                    .Action = action,
                    .EntityName = entityName,
                    .EntityId = entityId,
                    .Details = details,
                    .Timestamp = DateTime.Now
                }

                _logRepo.Add(logEntry)

                Dim line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{userName}] [{action}] {entityName} (ID: {If(entityId.HasValue, entityId.Value.ToString(), "-")}): {details}"
                File.AppendAllText(LogFilePath, line & Environment.NewLine)
            Catch
            End Try
        End Sub

        Public Shared Sub LogException(ex As Exception, Optional context As String = "")
            Try
                Dim line = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [ERROR] [{context}] {ex.Message}{Environment.NewLine}{ex.StackTrace}"
                File.AppendAllText(LogFilePath, line & Environment.NewLine)
            Catch
            End Try
        End Sub
    End Class
End Namespace
