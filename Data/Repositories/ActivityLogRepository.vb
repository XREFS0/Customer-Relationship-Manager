Imports System
Imports System.Collections.Generic
Imports Microsoft.Data.Sqlite
Imports EnterpriseCRM.Models

Namespace EnterpriseCRM.Data.Repositories
    Public Class ActivityLogRepository
        Inherits RepositoryBase

        Public Function GetAll(Optional limit As Integer = 50) As List(Of ActivityLog)
            Dim list As New List(Of ActivityLog)()
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "SELECT Id, UserId, UserName, Action, EntityName, EntityId, Details, Timestamp FROM ActivityLogs ORDER BY Id DESC LIMIT @Limit;"
                    cmd.Parameters.AddWithValue("@Limit", limit)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            list.Add(MapLog(reader))
                        End While
                    End Using
                End Using
            End Using
            Return list
        End Function

        Public Function Add(entity As ActivityLog) As Integer
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    INSERT INTO ActivityLogs (UserId, UserName, Action, EntityName, EntityId, Details, Timestamp)
                    VALUES (@UserId, @UserName, @Action, @EntityName, @EntityId, @Details, @Timestamp);
                    SELECT last_insert_rowid();"
                    cmd.Parameters.AddWithValue("@UserId", entity.UserId)
                    cmd.Parameters.AddWithValue("@UserName", entity.UserName)
                    cmd.Parameters.AddWithValue("@Action", entity.Action)
                    cmd.Parameters.AddWithValue("@EntityName", entity.EntityName)
                    cmd.Parameters.AddWithValue("@EntityId", If(entity.EntityId.HasValue, entity.EntityId.Value, DBNull.Value))
                    cmd.Parameters.AddWithValue("@Details", entity.Details)
                    cmd.Parameters.AddWithValue("@Timestamp", entity.Timestamp.ToString("yyyy-MM-dd HH:mm:ss"))
                    Dim newId = Convert.ToInt32(cmd.ExecuteScalar())
                    entity.Id = newId
                    Return newId
                End Using
            End Using
        End Function

        Private Function MapLog(reader As SqliteDataReader) As ActivityLog
            Return New ActivityLog With {
                .Id = reader.GetInt32(0),
                .UserId = reader.GetInt32(1),
                .UserName = reader.GetString(2),
                .Action = reader.GetString(3),
                .EntityName = reader.GetString(4),
                .EntityId = If(reader.IsDBNull(5), CType(Nothing, Nullable(Of Integer)), reader.GetInt32(5)),
                .Details = If(reader.IsDBNull(6), String.Empty, reader.GetString(6)),
                .Timestamp = DateTime.Parse(reader.GetString(7))
            }
        End Function
    End Class
End Namespace
