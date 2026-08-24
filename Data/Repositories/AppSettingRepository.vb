Imports System
Imports System.Collections.Generic
Imports Microsoft.Data.Sqlite
Imports EnterpriseCRM.Models

Namespace EnterpriseCRM.Data.Repositories
    Public Class AppSettingRepository
        Inherits RepositoryBase

        Public Function GetValue(key As String, Optional defaultValue As String = "") As String
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "SELECT Value FROM AppSettings WHERE Key = @Key;"
                    cmd.Parameters.AddWithValue("@Key", key)
                    Dim result = cmd.ExecuteScalar()
                    If result IsNot Nothing AndAlso Not Convert.IsDBNull(result) Then
                        Return Convert.ToString(result)
                    End If
                End Using
            End Using
            Return defaultValue
        End Function

        Public Function SetValue(key As String, value As String, Optional description As String = "") As Boolean
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    INSERT INTO AppSettings (Key, Value, Description)
                    VALUES (@Key, @Value, @Description)
                    ON CONFLICT(Key) DO UPDATE SET Value = excluded.Value;"
                    cmd.Parameters.AddWithValue("@Key", key)
                    cmd.Parameters.AddWithValue("@Value", value)
                    cmd.Parameters.AddWithValue("@Description", description)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function GetAll() As Dictionary(Of String, String)
            Dim dict As New Dictionary(Of String, String)()
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "SELECT Key, Value FROM AppSettings;"
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            dict(reader.GetString(0)) = reader.GetString(1)
                        End While
                    End Using
                End Using
            End Using
            Return dict
        End Function
    End Class
End Namespace
