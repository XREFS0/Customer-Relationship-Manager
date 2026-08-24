Imports System
Imports System.Collections.Generic
Imports Microsoft.Data.Sqlite
Imports EnterpriseCRM.Models

Namespace EnterpriseCRM.Data.Repositories
    Public Class TaskRepository
        Inherits RepositoryBase
        Implements IRepository(Of CrmTask)

        Public Function GetAll() As List(Of CrmTask) Implements IRepository(Of CrmTask).GetAll
            Dim list As New List(Of CrmTask)()
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    SELECT t.Id, t.Title, t.Description, t.CustomerId, IFNULL(c.Name, '') as CustomerName,
                           t.AssignedToUserId, IFNULL(u.FullName, '') as AssignedToUserName,
                           t.Priority, t.Status, t.DueDate, t.CompletedDate, t.CreatedAt
                    FROM Tasks t
                    LEFT JOIN Customers c ON t.CustomerId = c.Id
                    LEFT JOIN Users u ON t.AssignedToUserId = u.Id
                    ORDER BY t.Id DESC;"
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            list.Add(MapTask(reader))
                        End While
                    End Using
                End Using
            End Using
            Return list
        End Function

        Public Function GetById(id As Integer) As CrmTask Implements IRepository(Of CrmTask).GetById
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    SELECT t.Id, t.Title, t.Description, t.CustomerId, IFNULL(c.Name, '') as CustomerName,
                           t.AssignedToUserId, IFNULL(u.FullName, '') as AssignedToUserName,
                           t.Priority, t.Status, t.DueDate, t.CompletedDate, t.CreatedAt
                    FROM Tasks t
                    LEFT JOIN Customers c ON t.CustomerId = c.Id
                    LEFT JOIN Users u ON t.AssignedToUserId = u.Id
                    WHERE t.Id = @Id;"
                    cmd.Parameters.AddWithValue("@Id", id)
                    Using reader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Return MapTask(reader)
                        End If
                    End Using
                End Using
            End Using
            Return Nothing
        End Function

        Public Function GetByCustomerId(customerId As Integer) As List(Of CrmTask)
            Dim list As New List(Of CrmTask)()
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    SELECT t.Id, t.Title, t.Description, t.CustomerId, IFNULL(c.Name, '') as CustomerName,
                           t.AssignedToUserId, IFNULL(u.FullName, '') as AssignedToUserName,
                           t.Priority, t.Status, t.DueDate, t.CompletedDate, t.CreatedAt
                    FROM Tasks t
                    LEFT JOIN Customers c ON t.CustomerId = c.Id
                    LEFT JOIN Users u ON t.AssignedToUserId = u.Id
                    WHERE t.CustomerId = @CustomerId
                    ORDER BY t.DueDate ASC;"
                    cmd.Parameters.AddWithValue("@CustomerId", customerId)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            list.Add(MapTask(reader))
                        End While
                    End Using
                End Using
            End Using
            Return list
        End Function

        Public Function Add(entity As CrmTask) As Integer Implements IRepository(Of CrmTask).Add
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    INSERT INTO Tasks (Title, Description, CustomerId, AssignedToUserId, Priority, Status, DueDate, CompletedDate, CreatedAt)
                    VALUES (@Title, @Description, @CustomerId, @AssignedToUserId, @Priority, @Status, @DueDate, @CompletedDate, @CreatedAt);
                    SELECT last_insert_rowid();"
                    
                    cmd.Parameters.AddWithValue("@Title", entity.Title)
                    cmd.Parameters.AddWithValue("@Description", If(entity.Description, String.Empty))
                    cmd.Parameters.AddWithValue("@CustomerId", If(entity.CustomerId.HasValue, entity.CustomerId.Value, DBNull.Value))
                    cmd.Parameters.AddWithValue("@AssignedToUserId", entity.AssignedToUserId)
                    cmd.Parameters.AddWithValue("@Priority", CInt(entity.Priority))
                    cmd.Parameters.AddWithValue("@Status", CInt(entity.Status))
                    cmd.Parameters.AddWithValue("@DueDate", If(entity.DueDate.HasValue, entity.DueDate.Value.ToString("yyyy-MM-dd HH:mm:ss"), DBNull.Value))
                    cmd.Parameters.AddWithValue("@CompletedDate", If(entity.CompletedDate.HasValue, entity.CompletedDate.Value.ToString("yyyy-MM-dd HH:mm:ss"), DBNull.Value))
                    cmd.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))

                    Dim newId = Convert.ToInt32(cmd.ExecuteScalar())
                    entity.Id = newId
                    Return newId
                End Using
            End Using
        End Function

        Public Function Update(entity As CrmTask) As Boolean Implements IRepository(Of CrmTask).Update
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    UPDATE Tasks SET
                        Title = @Title,
                        Description = @Description,
                        CustomerId = @CustomerId,
                        AssignedToUserId = @AssignedToUserId,
                        Priority = @Priority,
                        Status = @Status,
                        DueDate = @DueDate,
                        CompletedDate = @CompletedDate
                    WHERE Id = @Id;"

                    cmd.Parameters.AddWithValue("@Id", entity.Id)
                    cmd.Parameters.AddWithValue("@Title", entity.Title)
                    cmd.Parameters.AddWithValue("@Description", If(entity.Description, String.Empty))
                    cmd.Parameters.AddWithValue("@CustomerId", If(entity.CustomerId.HasValue, entity.CustomerId.Value, DBNull.Value))
                    cmd.Parameters.AddWithValue("@AssignedToUserId", entity.AssignedToUserId)
                    cmd.Parameters.AddWithValue("@Priority", CInt(entity.Priority))
                    cmd.Parameters.AddWithValue("@Status", CInt(entity.Status))
                    cmd.Parameters.AddWithValue("@DueDate", If(entity.DueDate.HasValue, entity.DueDate.Value.ToString("yyyy-MM-dd HH:mm:ss"), DBNull.Value))
                    cmd.Parameters.AddWithValue("@CompletedDate", If(entity.CompletedDate.HasValue, entity.CompletedDate.Value.ToString("yyyy-MM-dd HH:mm:ss"), DBNull.Value))

                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function Delete(id As Integer) As Boolean Implements IRepository(Of CrmTask).Delete
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "DELETE FROM Tasks WHERE Id = @Id;"
                    cmd.Parameters.AddWithValue("@Id", id)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function GetCompletedCount() As Integer
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "SELECT COUNT(1) FROM Tasks WHERE Status = @Status;"
                    cmd.Parameters.AddWithValue("@Status", CInt(TaskStatus.Completed))
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

        Public Function GetPendingCount() As Integer
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "SELECT COUNT(1) FROM Tasks WHERE Status IN (@Pending, @InProgress);"
                    cmd.Parameters.AddWithValue("@Pending", CInt(TaskStatus.Pending))
                    cmd.Parameters.AddWithValue("@InProgress", CInt(TaskStatus.InProgress))
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

        Private Function MapTask(reader As SqliteDataReader) As CrmTask
            Return New CrmTask With {
                .Id = reader.GetInt32(0),
                .Title = reader.GetString(1),
                .Description = If(reader.IsDBNull(2), String.Empty, reader.GetString(2)),
                .CustomerId = If(reader.IsDBNull(3), CType(Nothing, Nullable(Of Integer)), reader.GetInt32(3)),
                .CustomerName = If(reader.IsDBNull(4), String.Empty, reader.GetString(4)),
                .AssignedToUserId = reader.GetInt32(5),
                .AssignedToUserName = If(reader.IsDBNull(6), String.Empty, reader.GetString(6)),
                .Priority = CType(reader.GetInt32(7), TaskPriority),
                .Status = CType(reader.GetInt32(8), TaskStatus),
                .DueDate = If(reader.IsDBNull(9), CType(Nothing, Nullable(Of DateTime)), DateTime.Parse(reader.GetString(9))),
                .CompletedDate = If(reader.IsDBNull(10), CType(Nothing, Nullable(Of DateTime)), DateTime.Parse(reader.GetString(10))),
                .CreatedAt = DateTime.Parse(reader.GetString(11))
            }
        End Function
    End Class
End Namespace
