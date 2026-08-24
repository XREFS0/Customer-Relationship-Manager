Imports System
Imports System.Security.Cryptography
Imports System.Text

Namespace EnterpriseCRM.Services
    Public Class SecurityService
        Private Const SaltSize As Integer = 16
        Private Const HashSize As Integer = 32
        Private Const Iterations As Integer = 50000

        Public Shared Function GenerateSalt() As String
            Dim saltBytes(SaltSize - 1) As Byte
            Using rng = RandomNumberGenerator.Create()
                rng.GetBytes(saltBytes)
            End Using
            Return Convert.ToBase64String(saltBytes)
        End Function

        Public Shared Function HashPassword(password As String, saltBase64 As String) As String
            If String.IsNullOrEmpty(password) Then Throw New ArgumentNullException(NameOf(password))
            Dim saltBytes As Byte() = Convert.FromBase64String(saltBase64)
            Using pbkdf2 As New Rfc2898DeriveBytes(password, saltBytes, Iterations, HashAlgorithmName.SHA256)
                Dim hashBytes As Byte() = pbkdf2.GetBytes(HashSize)
                Return Convert.ToBase64String(hashBytes)
            End Using
        End Function

        Public Shared Function VerifyPassword(password As String, saltBase64 As String, storedHashBase64 As String) As Boolean
            If String.IsNullOrEmpty(password) OrElse String.IsNullOrEmpty(saltBase64) OrElse String.IsNullOrEmpty(storedHashBase64) Then
                Return False
            End If
            Dim computedHash As String = HashPassword(password, saltBase64)
            Return CryptographicOperations.FixedTimeEquals(Convert.FromBase64String(computedHash), Convert.FromBase64String(storedHashBase64))
        End Function
    End Class
End Namespace
