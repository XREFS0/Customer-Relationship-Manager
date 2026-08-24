Imports System
Imports System.Collections.Generic
Imports Microsoft.Data.Sqlite
Imports EnterpriseCRM.Models

Namespace EnterpriseCRM.Data.Repositories
    Public Class CustomerRepository
        Inherits RepositoryBase
        Implements IRepository(Of Customer)

        Public Function GetAll() As List(Of Customer) Implements IRepository(Of Customer).GetAll
            Dim list As New List(Of Customer)()
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "SELECT Id, Name, Company, Email, Phone, Address, City, Country, Status, Notes, CreatedAt, UpdatedAt FROM Customers ORDER BY Id DESC;"
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            list.Add(MapCustomer(reader))
                        End While
                    End Using
                End Using
            End Using
            Return list
        End Function

        Public Function GetById(id As Integer) As Customer Implements IRepository(Of Customer).GetById
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "SELECT Id, Name, Company, Email, Phone, Address, City, Country, Status, Notes, CreatedAt, UpdatedAt FROM Customers WHERE Id = @Id;"
                    cmd.Parameters.AddWithValue("@Id", id)
                    Using reader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Return MapCustomer(reader)
                        End If
                    End Using
                End Using
            End Using
            Return Nothing
        End Function

        Public Function Search(searchTerm As String, Optional statusFilter As Nullable(Of CustomerStatus) = Nothing) As List(Of Customer)
            Dim list As New List(Of Customer)()
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    Dim sql As String = "SELECT Id, Name, Company, Email, Phone, Address, City, Country, Status, Notes, CreatedAt, UpdatedAt FROM Customers WHERE 1=1 "
                    
                    If Not String.IsNullOrWhiteSpace(searchTerm) Then
                        sql &= " AND (Name LIKE @Search OR Company LIKE @Search OR Email LIKE @Search OR Phone LIKE @Search OR City LIKE @Search) "
                        cmd.Parameters.AddWithValue("@Search", $"%{searchTerm.Trim()}%")
                    End If

                    If statusFilter.HasValue Then
                        sql &= " AND Status = @Status "
                        cmd.Parameters.AddWithValue("@Status", CInt(statusFilter.Value))
                    End If

                    sql &= " ORDER BY Id DESC;"
                    cmd.CommandText = sql

                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            list.Add(MapCustomer(reader))
                        End While
                    End Using
                End Using
            End Using
            Return list
        End Function

        Public Function Add(entity As Customer) As Integer Implements IRepository(Of Customer).Add
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    INSERT INTO Customers (Name, Company, Email, Phone, Address, City, Country, Status, Notes, CreatedAt, UpdatedAt)
                    VALUES (@Name, @Company, @Email, @Phone, @Address, @City, @Country, @Status, @Notes, @CreatedAt, @UpdatedAt);
                    SELECT last_insert_rowid();"
                    
                    cmd.Parameters.AddWithValue("@Name", entity.Name)
                    cmd.Parameters.AddWithValue("@Company", If(entity.Company, String.Empty))
                    cmd.Parameters.AddWithValue("@Email", If(entity.Email, String.Empty))
                    cmd.Parameters.AddWithValue("@Phone", If(entity.Phone, String.Empty))
                    cmd.Parameters.AddWithValue("@Address", If(entity.Address, String.Empty))
                    cmd.Parameters.AddWithValue("@City", If(entity.City, String.Empty))
                    cmd.Parameters.AddWithValue("@Country", If(entity.Country, String.Empty))
                    cmd.Parameters.AddWithValue("@Status", CInt(entity.Status))
                    cmd.Parameters.AddWithValue("@Notes", If(entity.Notes, String.Empty))
                    cmd.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
                    cmd.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"))

                    Dim newId = Convert.ToInt32(cmd.ExecuteScalar())
                    entity.Id = newId
                    Return newId
                End Using
            End Using
        End Function

        Public Function Update(entity As Customer) As Boolean Implements IRepository(Of Customer).Update
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    UPDATE Customers SET
                        Name = @Name,
                        Company = @Company,
                        Email = @Email,
                        Phone = @Phone,
                        Address = @Address,
                        City = @City,
                        Country = @Country,
                        Status = @Status,
                        Notes = @Notes,
                        UpdatedAt = @UpdatedAt
                    WHERE Id = @Id;"

                    cmd.Parameters.AddWithValue("@Id", entity.Id)
                    cmd.Parameters.AddWithValue("@Name", entity.Name)
                    cmd.Parameters.AddWithValue("@Company", If(entity.Company, String.Empty))
                    cmd.Parameters.AddWithValue("@Email", If(entity.Email, String.Empty))
                    cmd.Parameters.AddWithValue("@Phone", If(entity.Phone, String.Empty))
                    cmd.Parameters.AddWithValue("@Address", If(entity.Address, String.Empty))
                    cmd.Parameters.AddWithValue("@City", If(entity.City, String.Empty))
                    cmd.Parameters.AddWithValue("@Country", If(entity.Country, String.Empty))
                    cmd.Parameters.AddWithValue("@Status", CInt(entity.Status))
                    cmd.Parameters.AddWithValue("@Notes", If(entity.Notes, String.Empty))
                    cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))

                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function Delete(id As Integer) As Boolean Implements IRepository(Of Customer).Delete
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "DELETE FROM Customers WHERE Id = @Id;"
                    cmd.Parameters.AddWithValue("@Id", id)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function GetTotalCount() As Integer
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "SELECT COUNT(1) FROM Customers;"
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

        Public Function GetLeadsCount() As Integer
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "SELECT COUNT(1) FROM Customers WHERE Status = @Status;"
                    cmd.Parameters.AddWithValue("@Status", CInt(CustomerStatus.Lead))
                    Return Convert.ToInt32(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

        Private Function MapCustomer(reader As SqliteDataReader) As Customer
            Return New Customer With {
                .Id = reader.GetInt32(0),
                .Name = reader.GetString(1),
                .Company = If(reader.IsDBNull(2), String.Empty, reader.GetString(2)),
                .Email = If(reader.IsDBNull(3), String.Empty, reader.GetString(3)),
                .Phone = If(reader.IsDBNull(4), String.Empty, reader.GetString(4)),
                .Address = If(reader.IsDBNull(5), String.Empty, reader.GetString(5)),
                .City = If(reader.IsDBNull(6), String.Empty, reader.GetString(6)),
                .Country = If(reader.IsDBNull(7), String.Empty, reader.GetString(7)),
                .Status = CType(reader.GetInt32(8), CustomerStatus),
                .Notes = If(reader.IsDBNull(9), String.Empty, reader.GetString(9)),
                .CreatedAt = DateTime.Parse(reader.GetString(10)),
                .UpdatedAt = DateTime.Parse(reader.GetString(11))
            }
        End Function
    End Class
End Namespace
