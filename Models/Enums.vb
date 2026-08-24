Namespace EnterpriseCRM.Models
    Public Enum UserRole
        Admin = 1
        Employee = 2
    End Enum

    Public Enum CustomerStatus
        Lead = 1
        Prospect = 2
        Active = 3
        Inactive = 4
    End Enum

    Public Enum PipelineStage
        NewLead = 1
        Contacted = 2
        Negotiation = 3
        Won = 4
        Lost = 5
    End Enum

    Public Enum ContactType
        [Call] = 1
        Meeting = 2
        Email = 3
        Note = 4
        Message = 5
    End Enum

    Public Enum TaskPriority
        Low = 1
        Medium = 2
        High = 3
        Urgent = 4
    End Enum

    Public Enum TaskStatus
        Pending = 1
        InProgress = 2
        Completed = 3
        Cancelled = 4
    End Enum
End Namespace
