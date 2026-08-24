Imports System
Imports System.Collections.Generic
Imports Microsoft.Data.Sqlite
Imports EnterpriseCRM.Models

Namespace EnterpriseCRM.Data.Repositories
    Public Class ContactRepository
        Inherits RepositoryBase
        Implements IRepository(Of ContactHistory)

        Public Function GetAll() As List(Of ContactHistory) Implements IRepository(Of ContactHistory).GetAll
            Dim list As New List(Of ContactHistory)()
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    SELECT c.Id, c.CustomerId, IFNULL(cust.Name, '') as CustomerName, c.UserId, IFNULL(u.FullName, '') as UserName,
                           c.ContactType, c.Subject, c.Notes, c.ContactDate, c.CreatedAt
                    FROM ContactHistory c
                    LEFT JOIN Customers cust ON c.CustomerId = cust.Id
                    LEFT JOIN Users u ON c.UserId = u.Id
                    ORDER BY c.ContactDate DESC;"
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            list.Add(MapContact(reader))
                        End While
                    End Using
                End Using
            End Using
            Return list
        End Function

        Public Function GetById(id As Integer) As ContactHistory Implements IRepository(Of ContactHistory).GetById
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    SELECT c.Id, c.CustomerId, IFNULL(cust.Name, '') as CustomerName, c.UserId, IFNULL(u.FullName, '') as UserName,
                           c.ContactType, c.Subject, c.Notes, c.ContactDate, c.CreatedAt
                    FROM ContactHistory c
                    LEFT JOIN Customers cust ON c.CustomerId = cust.Id
                    LEFT JOIN Users u ON c.UserId = u.Id
                    WHERE c.Id = @Id;"
                    cmd.Parameters.AddWithValue("@Id", id)
                    Using reader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Return MapContact(reader)
                        End If
                    End Using
                End Using
            End Using
            Return Nothing
        End Function

        Public Function GetByCustomerId(customerId As Integer) As List(Of ContactHistory)
            Dim list As New List(Of ContactHistory)()
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    SELECT c.Id, c.CustomerId, IFNULL(cust.Name, '') as CustomerName, c.UserId, IFNULL(u.FullName, '') as UserName,
                           c.ContactType, c.Subject, c.Notes, c.ContactDate, c.CreatedAt
                    FROM ContactHistory c
                    LEFT JOIN Customers cust ON c.CustomerId = cust.Id
                    LEFT JOIN Users u ON c.UserId = u.Id
                    WHERE c.CustomerId = @CustomerId
                    ORDER BY c.ContactDate DESC;"
                    cmd.Parameters.AddWithValue("@CustomerId", customerId)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            list.Add(MapContact(reader))
                        End While
                    End Using
                End Using
            End Using
            Return list
        End Function

        Public Function Add(entity As ContactHistory) As Integer Implements IRepository(Of ContactHistory).Add
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    INSERT INTO ContactHistory (CustomerId, UserId, ContactType, Subject, Notes, ContactDate, CreatedAt)
                    VALUES (@CustomerId, @UserId, @ContactType, @Subject, @Notes, @ContactDate, @CreatedAt);
                    SELECT last_insert_rowid();"
                    
                    cmd.Parameters.AddWithValue("@CustomerId", entity.CustomerId)
                    cmd.Parameters.AddWithValue("@UserId", entity.UserId)
                    cmd.Parameters.AddWithValue("@ContactType", CInt(entity.ContactType))
                    cmd.Parameters.AddWithValue("@Subject", entity.Subject)
                    cmd.Parameters.AddWithValue("@Notes", If(entity.Notes, String.Empty))
                    cmd.Parameters.AddWithValue("@ContactDate", entity.ContactDate.ToString("yyyy-MM-dd HH:mm:ss"))
                    cmd.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))

                    Dim newId = Convert.ToInt32(cmd.ExecuteScalar())
                    entity.Id = newId
                    Return newId
                End Using
            End Using
        End Function

        Public Function Update(entity As ContactHistory) As Boolean Implements IRepository(Of ContactHistory).Update
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    UPDATE ContactHistory SET
                        CustomerId = @CustomerId,
                        UserId = @UserId,
                        ContactType = @ContactType,
                        Subject = @Subject,
                        Notes = @Notes,
                        ContactDate = @ContactDate
                    WHERE Id = @Id;"

                    cmd.Parameters.AddWithValue("@Id", entity.Id)
                    cmd.Parameters.AddWithValue("@CustomerId", entity.CustomerId)
                    cmd.Parameters.AddWithValue("@UserId", entity.UserId)
                    cmd.Parameters.AddWithValue("@ContactType", CInt(entity.ContactType))
                    cmd.Parameters.AddWithValue("@Subject", entity.Subject)
                    cmd.Parameters.AddWithValue("@Notes", If(entity.Notes, String.Empty))
                    cmd.Parameters.AddWithValue("@ContactDate", entity.ContactDate.ToString("yyyy-MM-dd HH:mm:ss"))

                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function Delete(id As Integer) As Boolean Implements IRepository(Of ContactHistory).Delete
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "DELETE FROM ContactHistory WHERE Id = @Id;"
                    cmd.Parameters.AddWithValue("@Id", id)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Private Function MapContact(reader As SqliteDataReader) As ContactHistory
            Return New ContactHistory With {
                .Id = reader.GetInt32(0),
                .CustomerId = reader.GetInt32(1),
                .CustomerName = If(reader.IsDBNull(2), String.Empty, reader.GetString(2)),
                .UserId = If(reader.IsDBNull(3), 0, reader.GetInt32(3)),
                .UserName = If(reader.IsDBNull(4), String.Empty, reader.GetString(4)),
                .ContactType = CType(reader.GetInt32(5), ContactType),
                .Subject = reader.GetString(6),
                .Notes = If(reader.IsDBNull(7), String.Empty, reader.GetString(7)),
                .ContactDate = DateTime.Parse(reader.GetString(8)),
                .CreatedAt = DateTime.Parse(reader.GetString(9))
            }
        End Function
    End Class
End Namespace
