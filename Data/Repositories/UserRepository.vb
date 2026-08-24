Imports System
Imports System.Collections.Generic
Imports Microsoft.Data.Sqlite
Imports EnterpriseCRM.Models

Namespace EnterpriseCRM.Data.Repositories
    Public Interface IRepository(Of T)
        Function GetAll() As List(Of T)
        Function GetById(id As Integer) As T
        Function Add(entity As T) As Integer
        Function Update(entity As T) As Boolean
        Function Delete(id As Integer) As Boolean
    End Interface

    Public Class UserRepository
        Inherits RepositoryBase
        Implements IRepository(Of User)

        Public Function GetAll() As List(Of User) Implements IRepository(Of User).GetAll
            Dim list As New List(Of User)()
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "SELECT Id, Username, PasswordHash, PasswordSalt, FullName, Email, Role, IsActive, CreatedAt, LastLoginAt FROM Users ORDER BY FullName;"
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            list.Add(MapUser(reader))
                        End While
                    End Using
                End Using
            End Using
            Return list
        End Function

        Public Function GetById(id As Integer) As User Implements IRepository(Of User).GetById
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "SELECT Id, Username, PasswordHash, PasswordSalt, FullName, Email, Role, IsActive, CreatedAt, LastLoginAt FROM Users WHERE Id = @Id;"
                    cmd.Parameters.AddWithValue("@Id", id)
                    Using reader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Return MapUser(reader)
                        End If
                    End Using
                End Using
            End Using
            Return Nothing
        End Function

        Public Function GetByUsername(username As String) As User
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "SELECT Id, Username, PasswordHash, PasswordSalt, FullName, Email, Role, IsActive, CreatedAt, LastLoginAt FROM Users WHERE LOWER(Username) = LOWER(@Username);"
                    cmd.Parameters.AddWithValue("@Username", username)
                    Using reader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Return MapUser(reader)
                        End If
                    End Using
                End Using
            End Using
            Return Nothing
        End Function

        Public Function Add(entity As User) As Integer Implements IRepository(Of User).Add
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    INSERT INTO Users (Username, PasswordHash, PasswordSalt, FullName, Email, Role, IsActive, CreatedAt)
                    VALUES (@Username, @PasswordHash, @PasswordSalt, @FullName, @Email, @Role, @IsActive, @CreatedAt);
                    SELECT last_insert_rowid();"
                    cmd.Parameters.AddWithValue("@Username", entity.Username)
                    cmd.Parameters.AddWithValue("@PasswordHash", entity.PasswordHash)
                    cmd.Parameters.AddWithValue("@PasswordSalt", entity.PasswordSalt)
                    cmd.Parameters.AddWithValue("@FullName", entity.FullName)
                    cmd.Parameters.AddWithValue("@Email", If(entity.Email, DBNull.Value))
                    cmd.Parameters.AddWithValue("@Role", CInt(entity.Role))
                    cmd.Parameters.AddWithValue("@IsActive", If(entity.IsActive, 1, 0))
                    cmd.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
                    Dim newId = Convert.ToInt32(cmd.ExecuteScalar())
                    entity.Id = newId
                    Return newId
                End Using
            End Using
        End Function

        Public Function Update(entity As User) As Boolean Implements IRepository(Of User).Update
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    UPDATE Users SET
                        FullName = @FullName,
                        Email = @Email,
                        Role = @Role,
                        IsActive = @IsActive
                    WHERE Id = @Id;"
                    cmd.Parameters.AddWithValue("@Id", entity.Id)
                    cmd.Parameters.AddWithValue("@FullName", entity.FullName)
                    cmd.Parameters.AddWithValue("@Email", If(entity.Email, DBNull.Value))
                    cmd.Parameters.AddWithValue("@Role", CInt(entity.Role))
                    cmd.Parameters.AddWithValue("@IsActive", If(entity.IsActive, 1, 0))
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function UpdatePassword(userId As Integer, passwordHash As String, salt As String) As Boolean
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "UPDATE Users SET PasswordHash = @Hash, PasswordSalt = @Salt WHERE Id = @Id;"
                    cmd.Parameters.AddWithValue("@Id", userId)
                    cmd.Parameters.AddWithValue("@Hash", passwordHash)
                    cmd.Parameters.AddWithValue("@Salt", salt)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function UpdateLastLogin(userId As Integer) As Boolean
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "UPDATE Users SET LastLoginAt = @Now WHERE Id = @Id;"
                    cmd.Parameters.AddWithValue("@Id", userId)
                    cmd.Parameters.AddWithValue("@Now", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function Delete(id As Integer) As Boolean Implements IRepository(Of User).Delete
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "DELETE FROM Users WHERE Id = @Id;"
                    cmd.Parameters.AddWithValue("@Id", id)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Private Function MapUser(reader As SqliteDataReader) As User
            Return New User With {
                .Id = reader.GetInt32(0),
                .Username = reader.GetString(1),
                .PasswordHash = reader.GetString(2),
                .PasswordSalt = reader.GetString(3),
                .FullName = reader.GetString(4),
                .Email = If(reader.IsDBNull(5), String.Empty, reader.GetString(5)),
                .Role = CType(reader.GetInt32(6), UserRole),
                .IsActive = (reader.GetInt32(7) = 1),
                .CreatedAt = DateTime.Parse(reader.GetString(8)),
                .LastLoginAt = If(reader.IsDBNull(9), CType(Nothing, Nullable(Of DateTime)), DateTime.Parse(reader.GetString(9)))
            }
        End Function
    End Class

    Public MustInherit Class RepositoryBase
    End Class
End Namespace
