Imports System
Imports System.IO
Imports Microsoft.Data.Sqlite

Namespace EnterpriseCRM.Data
    Public Class DatabaseContext
        Private Shared _connectionString As String = String.Empty
        Private Shared _dbPath As String = String.Empty

        Public Shared ReadOnly Property DatabasePath As String
            Get
                If String.IsNullOrEmpty(_dbPath) Then
                    Dim appDir As String = AppDomain.CurrentDomain.BaseDirectory
                    Dim dataDir As String = Path.Combine(appDir, "Data")
                    If Not Directory.Exists(dataDir) Then
                        Directory.CreateDirectory(dataDir)
                    End If
                    _dbPath = Path.Combine(dataDir, "crm_enterprise.db")
                    _connectionString = $"Data Source={_dbPath};"
                End If
                Return _dbPath
            End Get
        End Property

        Public Shared ReadOnly Property ConnectionString As String
            Get
                If String.IsNullOrEmpty(_connectionString) Then
                    Dim path = DatabasePath
                End If
                Return _connectionString
            End Get
        End Property

        Public Shared Function CreateConnection() As SqliteConnection
            Dim conn As New SqliteConnection(ConnectionString)
            conn.Open()
            Using cmd = conn.CreateCommand()
                cmd.CommandText = "PRAGMA foreign_keys = ON; PRAGMA journal_mode = WAL;"
                cmd.ExecuteNonQuery()
            End Using
            Return conn
        End Function
    End Class
End Namespace
