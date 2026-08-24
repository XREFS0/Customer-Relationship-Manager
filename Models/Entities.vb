Imports System

Namespace EnterpriseCRM.Models
    Public Class User
        Public Property Id As Integer
        Public Property Username As String = String.Empty
        Public Property PasswordHash As String = String.Empty
        Public Property PasswordSalt As String = String.Empty
        Public Property FullName As String = String.Empty
        Public Property Email As String = String.Empty
        Public Property Role As UserRole = UserRole.Employee
        Public Property IsActive As Boolean = True
        Public Property CreatedAt As DateTime = DateTime.Now
        Public Property LastLoginAt As Nullable(Of DateTime)
    End Class

    Public Class Customer
        Public Property Id As Integer
        Public Property Name As String = String.Empty
        Public Property Company As String = String.Empty
        Public Property Email As String = String.Empty
        Public Property Phone As String = String.Empty
        Public Property Address As String = String.Empty
        Public Property City As String = String.Empty
        Public Property Country As String = String.Empty
        Public Property Status As CustomerStatus = CustomerStatus.Lead
        Public Property Notes As String = String.Empty
        Public Property CreatedAt As DateTime = DateTime.Now
        Public Property UpdatedAt As DateTime = DateTime.Now

        Public ReadOnly Property StatusName As String
            Get
                Return Status.ToString()
            End Get
        End Property
    End Class

    Public Class ContactHistory
        Public Property Id As Integer
        Public Property CustomerId As Integer
        Public Property CustomerName As String = String.Empty
        Public Property UserId As Integer
        Public Property UserName As String = String.Empty
        Public Property ContactType As ContactType = ContactType.Call
        Public Property Subject As String = String.Empty
        Public Property Notes As String = String.Empty
        Public Property ContactDate As DateTime = DateTime.Now
        Public Property CreatedAt As DateTime = DateTime.Now

        Public ReadOnly Property TypeName As String
            Get
                Return ContactType.ToString()
            End Get
        End Property
    End Class

    Public Class SaleOpportunity
        Public Property Id As Integer
        Public Property CustomerId As Integer
        Public Property CustomerName As String = String.Empty
        Public Property UserId As Integer
        Public Property UserName As String = String.Empty
        Public Property Title As String = String.Empty
        Public Property Amount As Decimal
        Public Property Stage As PipelineStage = PipelineStage.NewLead
        Public Property Probability As Integer = 10
        Public Property ExpectedCloseDate As Nullable(Of DateTime)
        Public Property ClosedDate As Nullable(Of DateTime)
        Public Property Notes As String = String.Empty
        Public Property CreatedAt As DateTime = DateTime.Now
        Public Property UpdatedAt As DateTime = DateTime.Now

        Public ReadOnly Property StageName As String
            Get
                Select Case Stage
                    Case PipelineStage.NewLead : Return "New Lead"
                    Case PipelineStage.Contacted : Return "Contacted"
                    Case PipelineStage.Negotiation : Return "Negotiation"
                    Case PipelineStage.Won : Return "Won"
                    Case PipelineStage.Lost : Return "Lost"
                    Case Else : Return Stage.ToString()
                End Select
            End Get
        End Property
    End Class

    Public Class CrmTask
        Public Property Id As Integer
        Public Property Title As String = String.Empty
        Public Property Description As String = String.Empty
        Public Property CustomerId As Nullable(Of Integer)
        Public Property CustomerName As String = String.Empty
        Public Property AssignedToUserId As Integer
        Public Property AssignedToUserName As String = String.Empty
        Public Property Priority As TaskPriority = TaskPriority.Medium
        Public Property Status As TaskStatus = TaskStatus.Pending
        Public Property DueDate As Nullable(Of DateTime)
        Public Property CompletedDate As Nullable(Of DateTime)
        Public Property CreatedAt As DateTime = DateTime.Now

        Public ReadOnly Property PriorityName As String
            Get
                Return Priority.ToString()
            End Get
        End Property

        Public ReadOnly Property StatusName As String
            Get
                Return Status.ToString()
            End Get
        End Property
    End Class

    Public Class ActivityLog
        Public Property Id As Integer
        Public Property UserId As Integer
        Public Property UserName As String = String.Empty
        Public Property Action As String = String.Empty
        Public Property EntityName As String = String.Empty
        Public Property EntityId As Nullable(Of Integer)
        Public Property Details As String = String.Empty
        Public Property Timestamp As DateTime = DateTime.Now
    End Class

    Public Class AppSetting
        Public Property Key As String = String.Empty
        Public Property Value As String = String.Empty
        Public Property Description As String = String.Empty
    End Class

    Public Class DashboardMetrics
        Public Property TotalCustomers As Integer
        Public Property ActiveLeads As Integer
        Public Property CompletedTasks As Integer
        Public Property TotalSalesWonAmount As Decimal
        Public Property PipelineTotalAmount As Decimal
        Public Property WinRatePercentage As Double
        Public Property PendingTasksCount As Integer
    End Class

    Public Class StageSummary
        Public Property Stage As PipelineStage
        Public Property StageName As String = String.Empty
        Public Property Count As Integer
        Public Property TotalAmount As Decimal
    End Class

    Public Class MonthlySalesSummary
        Public Property MonthYear As String = String.Empty
        Public Property MonthDate As DateTime
        Public Property TotalAmount As Decimal
        Public Property DealsWon As Integer
    End Class
End Namespace
