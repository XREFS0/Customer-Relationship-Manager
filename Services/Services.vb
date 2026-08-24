Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.Text
Imports EnterpriseCRM.Data
Imports EnterpriseCRM.Data.Repositories
Imports EnterpriseCRM.Models

Namespace EnterpriseCRM.Services
    Public Class AuthService
        Private ReadOnly _userRepo As New UserRepository()

        Public Function Login(username As String, password As String) As User
            If String.IsNullOrWhiteSpace(username) OrElse String.IsNullOrWhiteSpace(password) Then
                Throw New ArgumentException("Username and password are required.")
            End If

            Dim user = _userRepo.GetByUsername(username)
            If user Is Nothing OrElse Not user.IsActive Then
                Return Nothing
            End If

            Dim isValid = SecurityService.VerifyPassword(password, user.PasswordSalt, user.PasswordHash)
            If isValid Then
                _userRepo.UpdateLastLogin(user.Id)
                AppLogger.CurrentUser = user
                AppLogger.Log("LOGIN", "User", user.Id, $"User {user.Username} logged in successfully.")
                Return user
            End If

            Return Nothing
        End Function

        Public Function ChangePassword(userId As Integer, oldPassword As String, newPassword As String) As Boolean
            Dim user = _userRepo.GetById(userId)
            If user Is Nothing Then Return False

            If Not SecurityService.VerifyPassword(oldPassword, user.PasswordSalt, user.PasswordHash) Then
                Throw New InvalidOperationException("Current password is incorrect.")
            End If

            If String.IsNullOrWhiteSpace(newPassword) OrElse newPassword.Length < 6 Then
                Throw New ArgumentException("New password must be at least 6 characters long.")
            End If

            Dim newSalt = SecurityService.GenerateSalt()
            Dim newHash = SecurityService.HashPassword(newPassword, newSalt)
            Dim success = _userRepo.UpdatePassword(userId, newHash, newSalt)
            If success Then
                AppLogger.Log("CHANGE_PASSWORD", "User", userId, "Password changed successfully.")
            End If
            Return success
        End Function

        Public Function ResetPassword(userId As Integer, newPassword As String) As Boolean
            If String.IsNullOrWhiteSpace(newPassword) OrElse newPassword.Length < 6 Then
                Throw New ArgumentException("Password must be at least 6 characters long.")
            End If
            Dim newSalt = SecurityService.GenerateSalt()
            Dim newHash = SecurityService.HashPassword(newPassword, newSalt)
            Return _userRepo.UpdatePassword(userId, newHash, newSalt)
        End Function
    End Class

    Public Class CustomerService
        Private ReadOnly _customerRepo As New CustomerRepository()
        Private ReadOnly _contactRepo As New ContactRepository()
        Private ReadOnly _saleRepo As New SaleRepository()
        Private ReadOnly _taskRepo As New TaskRepository()

        Public Function GetAllCustomers() As List(Of Customer)
            Return _customerRepo.GetAll()
        End Function

        Public Function GetCustomerById(id As Integer) As Customer
            Return _customerRepo.GetById(id)
        End Function

        Public Function SearchCustomers(query As String, Optional status As Nullable(Of CustomerStatus) = Nothing) As List(Of Customer)
            Return _customerRepo.Search(query, status)
        End Function

        Public Function SaveCustomer(customer As Customer) As Boolean
            ValidateCustomer(customer)
            If customer.Id = 0 Then
                Dim newId = _customerRepo.Add(customer)
                AppLogger.Log("CREATE", "Customer", newId, $"Created customer: {customer.Name} ({customer.Company})")
                Return newId > 0
            Else
                Dim success = _customerRepo.Update(customer)
                If success Then
                    AppLogger.Log("UPDATE", "Customer", customer.Id, $"Updated customer: {customer.Name}")
                End If
                Return success
            End If
        End Function

        Public Function DeleteCustomer(id As Integer) As Boolean
            Dim cust = _customerRepo.GetById(id)
            Dim success = _customerRepo.Delete(id)
            If success AndAlso cust IsNot Nothing Then
                AppLogger.Log("DELETE", "Customer", id, $"Deleted customer: {cust.Name}")
            End If
            Return success
        End Function

        Private Sub ValidateCustomer(customer As Customer)
            If customer Is Nothing Then Throw New ArgumentNullException(NameOf(customer))
            If String.IsNullOrWhiteSpace(customer.Name) Then
                Throw New ArgumentException("Customer name is required.")
            End If
        End Sub
    End Class

    Public Class SalesService
        Private ReadOnly _saleRepo As New SaleRepository()

        Public Function GetAllSales() As List(Of SaleOpportunity)
            Return _saleRepo.GetAll()
        End Function

        Public Function GetSaleById(id As Integer) As SaleOpportunity
            Return _saleRepo.GetById(id)
        End Function

        Public Function GetSalesByCustomer(customerId As Integer) As List(Of SaleOpportunity)
            Return _saleRepo.GetByCustomerId(customerId)
        End Function

        Public Function SaveSale(sale As SaleOpportunity) As Boolean
            If String.IsNullOrWhiteSpace(sale.Title) Then
                Throw New ArgumentException("Opportunity title is required.")
            End If
            If sale.CustomerId <= 0 Then
                Throw New ArgumentException("Please select a valid customer.")
            End If
            If sale.Amount < 0 Then
                Throw New ArgumentException("Amount cannot be negative.")
            End If

            If sale.Id = 0 Then
                Dim newId = _saleRepo.Add(sale)
                AppLogger.Log("CREATE", "SaleOpportunity", newId, $"Created opportunity: {sale.Title} (${sale.Amount:N2})")
                Return newId > 0
            Else
                Dim success = _saleRepo.Update(sale)
                If success Then
                    AppLogger.Log("UPDATE", "SaleOpportunity", sale.Id, $"Updated opportunity: {sale.Title} (Stage: {sale.StageName})")
                End If
                Return success
            End If
        End Function

        Public Function DeleteSale(id As Integer) As Boolean
            Dim sale = _saleRepo.GetById(id)
            Dim success = _saleRepo.Delete(id)
            If success AndAlso sale IsNot Nothing Then
                AppLogger.Log("DELETE", "SaleOpportunity", id, $"Deleted opportunity: {sale.Title}")
            End If
            Return success
        End Function

        Public Function UpdateStage(saleId As Integer, newStage As PipelineStage) As Boolean
            Dim sale = _saleRepo.GetById(saleId)
            If sale Is Nothing Then Return False
            sale.Stage = newStage
            If newStage = PipelineStage.Won OrElse newStage = PipelineStage.Lost Then
                sale.ClosedDate = DateTime.Now
            End If
            Return _saleRepo.Update(sale)
        End Function
    End Class

    Public Class TaskService
        Private ReadOnly _taskRepo As New TaskRepository()

        Public Function GetAllTasks() As List(Of CrmTask)
            Return _taskRepo.GetAll()
        End Function

        Public Function GetTasksByCustomer(customerId As Integer) As List(Of CrmTask)
            Return _taskRepo.GetByCustomerId(customerId)
        End Function

        Public Function SaveTask(task As CrmTask) As Boolean
            If String.IsNullOrWhiteSpace(task.Title) Then
                Throw New ArgumentException("Task title is required.")
            End If
            If task.AssignedToUserId <= 0 Then
                Throw New ArgumentException("Task must be assigned to a user.")
            End If

            If task.Status = TaskStatus.Completed AndAlso Not task.CompletedDate.HasValue Then
                task.CompletedDate = DateTime.Now
            End If

            If task.Id = 0 Then
                Dim newId = _taskRepo.Add(task)
                AppLogger.Log("CREATE", "Task", newId, $"Created task: {task.Title}")
                Return newId > 0
            Else
                Dim success = _taskRepo.Update(task)
                If success Then
                    AppLogger.Log("UPDATE", "Task", task.Id, $"Updated task: {task.Title} (Status: {task.StatusName})")
                End If
                Return success
            End If
        End Function

        Public Function DeleteTask(id As Integer) As Boolean
            Dim task = _taskRepo.GetById(id)
            Dim success = _taskRepo.Delete(id)
            If success AndAlso task IsNot Nothing Then
                AppLogger.Log("DELETE", "Task", id, $"Deleted task: {task.Title}")
            End If
            Return success
        End Function
    End Class

    Public Class ContactService
        Private ReadOnly _contactRepo As New ContactRepository()

        Public Function GetAllContacts() As List(Of ContactHistory)
            Return _contactRepo.GetAll()
        End Function

        Public Function GetContactsByCustomer(customerId As Integer) As List(Of ContactHistory)
            Return _contactRepo.GetByCustomerId(customerId)
        End Function

        Public Function SaveContact(contact As ContactHistory) As Boolean
            If String.IsNullOrWhiteSpace(contact.Subject) Then
                Throw New ArgumentException("Interaction subject is required.")
            End If
            If contact.CustomerId <= 0 Then
                Throw New ArgumentException("Valid customer must be selected.")
            End If

            If contact.Id = 0 Then
                Dim newId = _contactRepo.Add(contact)
                AppLogger.Log("CREATE", "ContactHistory", newId, $"Logged {contact.TypeName} with Customer #{contact.CustomerId}")
                Return newId > 0
            Else
                Return _contactRepo.Update(contact)
            End If
        End Function

        Public Function DeleteContact(id As Integer) As Boolean
            Return _contactRepo.Delete(id)
        End Function
    End Class

    Public Class DashboardService
        Private ReadOnly _customerRepo As New CustomerRepository()
        Private ReadOnly _saleRepo As New SaleRepository()
        Private ReadOnly _taskRepo As New TaskRepository()
        Private ReadOnly _activityRepo As New ActivityLogRepository()

        Public Function GetDashboardMetrics() As DashboardMetrics
            Dim metrics As New DashboardMetrics()
            metrics.TotalCustomers = _customerRepo.GetTotalCount()
            metrics.ActiveLeads = _customerRepo.GetLeadsCount()
            metrics.CompletedTasks = _taskRepo.GetCompletedCount()
            metrics.PendingTasksCount = _taskRepo.GetPendingCount()
            metrics.TotalSalesWonAmount = _saleRepo.GetTotalWonAmount()
            metrics.PipelineTotalAmount = _saleRepo.GetTotalPipelineAmount()

            Dim totalClosed = 0
            Dim wonCount = 0
            For Each stage In _saleRepo.GetStageBreakdown()
                If stage.Stage = PipelineStage.Won Then wonCount = stage.Count
                If stage.Stage = PipelineStage.Won OrElse stage.Stage = PipelineStage.Lost Then
                    totalClosed += stage.Count
                End If
            Next
            metrics.WinRatePercentage = If(totalClosed > 0, Math.Round((wonCount / CDbl(totalClosed)) * 100.0, 1), 0)

            Return metrics
        End Function

        Public Function GetPipelineStagesSummary() As List(Of StageSummary)
            Return _saleRepo.GetStageBreakdown()
        End Function

        Public Function GetRecentActivities(Optional count As Integer = 10) As List(Of ActivityLog)
            Return _activityRepo.GetAll(count)
        End Function
    End Class

    Public Class BackupRestoreService
        Public Function CreateBackup(destinationPath As String) As Boolean
            Try
                Dim sourceDb = DatabaseContext.DatabasePath
                If Not File.Exists(sourceDb) Then
                    Throw New FileNotFoundException("Database file not found.")
                End If

                Using conn = DatabaseContext.CreateConnection()
                    Using cmd = conn.CreateCommand()
                        cmd.CommandText = "PRAGMA wal_checkpoint(FULL);"
                        cmd.ExecuteNonQuery()
                    End Using
                End Using

                File.Copy(sourceDb, destinationPath, True)
                AppLogger.Log("BACKUP", "Database", Nothing, $"Database backed up to: {destinationPath}")
                Return True
            Catch ex As Exception
                AppLogger.LogException(ex, "CreateBackup")
                Throw
            End Try
        End Function

        Public Function RestoreBackup(backupFilePath As String) As Boolean
            Try
                If Not File.Exists(backupFilePath) Then
                    Throw New FileNotFoundException("Backup file does not exist.")
                End If

                Dim targetDb = DatabaseContext.DatabasePath
                Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools()

                File.Copy(backupFilePath, targetDb, True)
                AppLogger.Log("RESTORE", "Database", Nothing, $"Database restored from: {backupFilePath}")
                Return True
            Catch ex As Exception
                AppLogger.LogException(ex, "RestoreBackup")
                Throw
            End Try
        End Function
    End Class

    Public Class ReportExportService
        Public Sub ExportCustomersToCsv(customers As List(Of Customer), filePath As String)
            Dim sb As New StringBuilder()
            sb.AppendLine("ID,Name,Company,Email,Phone,City,Country,Status,Created Date")
            For Each c In customers
                sb.AppendLine($"{c.Id},""{EscapeCsv(c.Name)}"",""{EscapeCsv(c.Company)}"",""{EscapeCsv(c.Email)}"",""{EscapeCsv(c.Phone)}"",""{EscapeCsv(c.City)}"",""{EscapeCsv(c.Country)}"",""{c.StatusName}"",""{c.CreatedAt:yyyy-MM-dd}""")
            Next
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8)
        End Sub

        Public Sub ExportSalesToCsv(sales As List(Of SaleOpportunity), filePath As String)
            Dim sb As New StringBuilder()
            sb.AppendLine("ID,Opportunity Title,Customer,Assigned Rep,Amount,Stage,Probability,Expected Close Date,Created Date")
            For Each s In sales
                Dim expDate = If(s.ExpectedCloseDate.HasValue, s.ExpectedCloseDate.Value.ToString("yyyy-MM-dd"), "")
                sb.AppendLine($"{s.Id},""{EscapeCsv(s.Title)}"",""{EscapeCsv(s.CustomerName)}"",""{EscapeCsv(s.UserName)}"",{s.Amount:F2},""{s.StageName}"",{s.Probability}%,""{expDate}"",""{s.CreatedAt:yyyy-MM-dd}""")
            Next
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8)
        End Sub

        Public Sub ExportHtmlReport(title As String, headers As List(Of String), rows As List(Of List(Of String)), filePath As String)
            Dim sb As New StringBuilder()
            sb.AppendLine("<!DOCTYPE html>")
            sb.AppendLine("<html><head><meta charset='utf-8'><title>" & title & "</title>")
            sb.AppendLine("<style>")
            sb.AppendLine("body { font-family: 'Segoe UI', Arial, sans-serif; margin: 30px; color: #1e293b; background: #fff; }")
            sb.AppendLine("h1 { color: #0f172a; border-bottom: 2px solid #2563eb; padding-bottom: 10px; margin-bottom: 5px; }")
            sb.AppendLine(".meta { color: #64748b; font-size: 13px; margin-bottom: 25px; }")
            sb.AppendLine("table { width: 100%; border-collapse: collapse; margin-top: 15px; }")
            sb.AppendLine("th { background-color: #f1f5f9; color: #334155; text-align: left; padding: 10px 12px; font-size: 13px; font-weight: 600; border-bottom: 2px solid #cbd5e1; }")
            sb.AppendLine("td { padding: 10px 12px; border-bottom: 1px solid #e2e8f0; font-size: 13px; }")
            sb.AppendLine("tr:nth-child(even) { background-color: #f8fafc; }")
            sb.AppendLine(".footer { margin-top: 40px; font-size: 12px; color: #94a3b8; text-align: center; }")
            sb.AppendLine("</style></head><body>")
            sb.AppendLine($"<h1>{title}</h1>")
            sb.AppendLine($"<div class='meta'>Generated on {DateTime.Now:MMMM dd, yyyy HH:mm:ss} | Enterprise CRM Engine</div>")
            sb.AppendLine("<table><thead><tr>")
            For Each h In headers
                sb.AppendLine($"<th>{h}</th>")
            Next
            sb.AppendLine("</tr></thead><tbody>")
            For Each row In rows
                sb.AppendLine("<tr>")
                For Each cell In row
                    sb.AppendLine($"<td>{System.Net.WebUtility.HtmlEncode(cell)}</td>")
                Next
                sb.AppendLine("</tr>")
            Next
            sb.AppendLine("</tbody></table>")
            sb.AppendLine("<div class='footer'>Confidential - Internal CRM Business Analytics Report</div>")
            sb.AppendLine("</body></html>")
            File.WriteAllText(filePath, sb.ToString(), Encoding.UTF8)
        End Sub

        Private Function EscapeCsv(val As String) As String
            If String.IsNullOrEmpty(val) Then Return ""
            Return val.Replace("""", """""")
        End Function
    End Class
End Namespace
