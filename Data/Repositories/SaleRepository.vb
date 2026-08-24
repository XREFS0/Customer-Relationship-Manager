Imports System
Imports System.Collections.Generic
Imports Microsoft.Data.Sqlite
Imports EnterpriseCRM.Models

Namespace EnterpriseCRM.Data.Repositories
    Public Class SaleRepository
        Inherits RepositoryBase
        Implements IRepository(Of SaleOpportunity)

        Public Function GetAll() As List(Of SaleOpportunity) Implements IRepository(Of SaleOpportunity).GetAll
            Dim list As New List(Of SaleOpportunity)()
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    SELECT s.Id, s.CustomerId, IFNULL(c.Name, '') as CustomerName, s.UserId, IFNULL(u.FullName, '') as UserName,
                           s.Title, s.Amount, s.Stage, s.Probability, s.ExpectedCloseDate, s.ClosedDate, s.Notes, s.CreatedAt, s.UpdatedAt
                    FROM SaleOpportunities s
                    LEFT JOIN Customers c ON s.CustomerId = c.Id
                    LEFT JOIN Users u ON s.UserId = u.Id
                    ORDER BY s.Id DESC;"
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            list.Add(MapSale(reader))
                        End While
                    End Using
                End Using
            End Using
            Return list
        End Function

        Public Function GetById(id As Integer) As SaleOpportunity Implements IRepository(Of SaleOpportunity).GetById
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    SELECT s.Id, s.CustomerId, IFNULL(c.Name, '') as CustomerName, s.UserId, IFNULL(u.FullName, '') as UserName,
                           s.Title, s.Amount, s.Stage, s.Probability, s.ExpectedCloseDate, s.ClosedDate, s.Notes, s.CreatedAt, s.UpdatedAt
                    FROM SaleOpportunities s
                    LEFT JOIN Customers c ON s.CustomerId = c.Id
                    LEFT JOIN Users u ON s.UserId = u.Id
                    WHERE s.Id = @Id;"
                    cmd.Parameters.AddWithValue("@Id", id)
                    Using reader = cmd.ExecuteReader()
                        If reader.Read() Then
                            Return MapSale(reader)
                        End If
                    End Using
                End Using
            End Using
            Return Nothing
        End Function

        Public Function GetByCustomerId(customerId As Integer) As List(Of SaleOpportunity)
            Dim list As New List(Of SaleOpportunity)()
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    SELECT s.Id, s.CustomerId, IFNULL(c.Name, '') as CustomerName, s.UserId, IFNULL(u.FullName, '') as UserName,
                           s.Title, s.Amount, s.Stage, s.Probability, s.ExpectedCloseDate, s.ClosedDate, s.Notes, s.CreatedAt, s.UpdatedAt
                    FROM SaleOpportunities s
                    LEFT JOIN Customers c ON s.CustomerId = c.Id
                    LEFT JOIN Users u ON s.UserId = u.Id
                    WHERE s.CustomerId = @CustomerId
                    ORDER BY s.Id DESC;"
                    cmd.Parameters.AddWithValue("@CustomerId", customerId)
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            list.Add(MapSale(reader))
                        End While
                    End Using
                End Using
            End Using
            Return list
        End Function

        Public Function Add(entity As SaleOpportunity) As Integer Implements IRepository(Of SaleOpportunity).Add
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    INSERT INTO SaleOpportunities (CustomerId, UserId, Title, Amount, Stage, Probability, ExpectedCloseDate, ClosedDate, Notes, CreatedAt, UpdatedAt)
                    VALUES (@CustomerId, @UserId, @Title, @Amount, @Stage, @Probability, @ExpectedCloseDate, @ClosedDate, @Notes, @CreatedAt, @UpdatedAt);
                    SELECT last_insert_rowid();"
                    
                    cmd.Parameters.AddWithValue("@CustomerId", entity.CustomerId)
                    cmd.Parameters.AddWithValue("@UserId", entity.UserId)
                    cmd.Parameters.AddWithValue("@Title", entity.Title)
                    cmd.Parameters.AddWithValue("@Amount", entity.Amount)
                    cmd.Parameters.AddWithValue("@Stage", CInt(entity.Stage))
                    cmd.Parameters.AddWithValue("@Probability", entity.Probability)
                    cmd.Parameters.AddWithValue("@ExpectedCloseDate", If(entity.ExpectedCloseDate.HasValue, entity.ExpectedCloseDate.Value.ToString("yyyy-MM-dd"), DBNull.Value))
                    cmd.Parameters.AddWithValue("@ClosedDate", If(entity.ClosedDate.HasValue, entity.ClosedDate.Value.ToString("yyyy-MM-dd"), DBNull.Value))
                    cmd.Parameters.AddWithValue("@Notes", If(entity.Notes, String.Empty))
                    cmd.Parameters.AddWithValue("@CreatedAt", entity.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"))
                    cmd.Parameters.AddWithValue("@UpdatedAt", entity.UpdatedAt.ToString("yyyy-MM-dd HH:mm:ss"))

                    Dim newId = Convert.ToInt32(cmd.ExecuteScalar())
                    entity.Id = newId
                    Return newId
                End Using
            End Using
        End Function

        Public Function Update(entity As SaleOpportunity) As Boolean Implements IRepository(Of SaleOpportunity).Update
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    UPDATE SaleOpportunities SET
                        CustomerId = @CustomerId,
                        UserId = @UserId,
                        Title = @Title,
                        Amount = @Amount,
                        Stage = @Stage,
                        Probability = @Probability,
                        ExpectedCloseDate = @ExpectedCloseDate,
                        ClosedDate = @ClosedDate,
                        Notes = @Notes,
                        UpdatedAt = @UpdatedAt
                    WHERE Id = @Id;"

                    cmd.Parameters.AddWithValue("@Id", entity.Id)
                    cmd.Parameters.AddWithValue("@CustomerId", entity.CustomerId)
                    cmd.Parameters.AddWithValue("@UserId", entity.UserId)
                    cmd.Parameters.AddWithValue("@Title", entity.Title)
                    cmd.Parameters.AddWithValue("@Amount", entity.Amount)
                    cmd.Parameters.AddWithValue("@Stage", CInt(entity.Stage))
                    cmd.Parameters.AddWithValue("@Probability", entity.Probability)
                    cmd.Parameters.AddWithValue("@ExpectedCloseDate", If(entity.ExpectedCloseDate.HasValue, entity.ExpectedCloseDate.Value.ToString("yyyy-MM-dd"), DBNull.Value))
                    cmd.Parameters.AddWithValue("@ClosedDate", If(entity.ClosedDate.HasValue, entity.ClosedDate.Value.ToString("yyyy-MM-dd"), DBNull.Value))
                    cmd.Parameters.AddWithValue("@Notes", If(entity.Notes, String.Empty))
                    cmd.Parameters.AddWithValue("@UpdatedAt", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"))

                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function Delete(id As Integer) As Boolean Implements IRepository(Of SaleOpportunity).Delete
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "DELETE FROM SaleOpportunities WHERE Id = @Id;"
                    cmd.Parameters.AddWithValue("@Id", id)
                    Return cmd.ExecuteNonQuery() > 0
                End Using
            End Using
        End Function

        Public Function GetStageBreakdown() As List(Of StageSummary)
            Dim list As New List(Of StageSummary)()
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    SELECT Stage, COUNT(1) as StageCount, IFNULL(SUM(Amount), 0) as TotalAmount
                    FROM SaleOpportunities
                    GROUP BY Stage
                    ORDER BY Stage ASC;"
                    Using reader = cmd.ExecuteReader()
                        While reader.Read()
                            Dim stg = CType(reader.GetInt32(0), PipelineStage)
                            list.Add(New StageSummary With {
                                .Stage = stg,
                                .StageName = stg.ToString(),
                                .Count = reader.GetInt32(1),
                                .TotalAmount = Convert.ToDecimal(reader.GetDouble(2))
                            })
                        End While
                    End Using
                End Using
            End Using
            Return list
        End Function

        Public Function GetTotalWonAmount() As Decimal
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "SELECT IFNULL(SUM(Amount), 0) FROM SaleOpportunities WHERE Stage = @WonStage;"
                    cmd.Parameters.AddWithValue("@WonStage", CInt(PipelineStage.Won))
                    Return Convert.ToDecimal(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

        Public Function GetTotalPipelineAmount() As Decimal
            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "SELECT IFNULL(SUM(Amount), 0) FROM SaleOpportunities WHERE Stage NOT IN (@WonStage, @LostStage);"
                    cmd.Parameters.AddWithValue("@WonStage", CInt(PipelineStage.Won))
                    cmd.Parameters.AddWithValue("@LostStage", CInt(PipelineStage.Lost))
                    Return Convert.ToDecimal(cmd.ExecuteScalar())
                End Using
            End Using
        End Function

        Private Function MapSale(reader As SqliteDataReader) As SaleOpportunity
            Return New SaleOpportunity With {
                .Id = reader.GetInt32(0),
                .CustomerId = reader.GetInt32(1),
                .CustomerName = If(reader.IsDBNull(2), String.Empty, reader.GetString(2)),
                .UserId = If(reader.IsDBNull(3), 0, reader.GetInt32(3)),
                .UserName = If(reader.IsDBNull(4), String.Empty, reader.GetString(4)),
                .Title = reader.GetString(5),
                .Amount = Convert.ToDecimal(reader.GetDouble(6)),
                .Stage = CType(reader.GetInt32(7), PipelineStage),
                .Probability = reader.GetInt32(8),
                .ExpectedCloseDate = If(reader.IsDBNull(9), CType(Nothing, Nullable(Of DateTime)), DateTime.Parse(reader.GetString(9))),
                .ClosedDate = If(reader.IsDBNull(10), CType(Nothing, Nullable(Of DateTime)), DateTime.Parse(reader.GetString(10))),
                .Notes = If(reader.IsDBNull(11), String.Empty, reader.GetString(11)),
                .CreatedAt = DateTime.Parse(reader.GetString(12)),
                .UpdatedAt = DateTime.Parse(reader.GetString(13))
            }
        End Function
    End Class
End Namespace
