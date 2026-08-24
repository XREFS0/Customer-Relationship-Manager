Imports System
Imports System.IO
Imports Microsoft.Data.Sqlite
Imports EnterpriseCRM.Models
Imports EnterpriseCRM.Services

Namespace EnterpriseCRM.Data
    Public Class DbInitializer
        Public Shared Sub Initialize()
            Dim path As String = DatabaseContext.DatabasePath
            Dim directoryName As String = System.IO.Path.GetDirectoryName(path)
            If Not Directory.Exists(directoryName) Then
                Directory.CreateDirectory(directoryName)
            End If

            Using conn = DatabaseContext.CreateConnection()
                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    CREATE TABLE IF NOT EXISTS Users (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Username TEXT NOT NULL UNIQUE,
                        PasswordHash TEXT NOT NULL,
                        PasswordSalt TEXT NOT NULL,
                        FullName TEXT NOT NULL,
                        Email TEXT,
                        Role INTEGER NOT NULL DEFAULT 2,
                        IsActive INTEGER NOT NULL DEFAULT 1,
                        CreatedAt TEXT NOT NULL,
                        LastLoginAt TEXT
                    );

                    CREATE TABLE IF NOT EXISTS Customers (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Name TEXT NOT NULL,
                        Company TEXT,
                        Email TEXT,
                        Phone TEXT,
                        Address TEXT,
                        City TEXT,
                        Country TEXT,
                        Status INTEGER NOT NULL DEFAULT 1,
                        Notes TEXT,
                        CreatedAt TEXT NOT NULL,
                        UpdatedAt TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS ContactHistory (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CustomerId INTEGER NOT NULL,
                        UserId INTEGER NOT NULL,
                        ContactType INTEGER NOT NULL DEFAULT 1,
                        Subject TEXT NOT NULL,
                        Notes TEXT,
                        ContactDate TEXT NOT NULL,
                        CreatedAt TEXT NOT NULL,
                        FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
                        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
                    );

                    CREATE TABLE IF NOT EXISTS SaleOpportunities (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        CustomerId INTEGER NOT NULL,
                        UserId INTEGER NOT NULL,
                        Title TEXT NOT NULL,
                        Amount REAL NOT NULL DEFAULT 0,
                        Stage INTEGER NOT NULL DEFAULT 1,
                        Probability INTEGER NOT NULL DEFAULT 10,
                        ExpectedCloseDate TEXT,
                        ClosedDate TEXT,
                        Notes TEXT,
                        CreatedAt TEXT NOT NULL,
                        UpdatedAt TEXT NOT NULL,
                        FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
                        FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
                    );

                    CREATE TABLE IF NOT EXISTS Tasks (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        Title TEXT NOT NULL,
                        Description TEXT,
                        CustomerId INTEGER,
                        AssignedToUserId INTEGER NOT NULL,
                        Priority INTEGER NOT NULL DEFAULT 2,
                        Status INTEGER NOT NULL DEFAULT 1,
                        DueDate TEXT,
                        CompletedDate TEXT,
                        CreatedAt TEXT NOT NULL,
                        FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE SET NULL,
                        FOREIGN KEY (AssignedToUserId) REFERENCES Users(Id) ON DELETE CASCADE
                    );

                    CREATE TABLE IF NOT EXISTS ActivityLogs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        UserId INTEGER NOT NULL,
                        UserName TEXT NOT NULL,
                        Action TEXT NOT NULL,
                        EntityName TEXT NOT NULL,
                        EntityId INTEGER,
                        Details TEXT,
                        Timestamp TEXT NOT NULL
                    );

                    CREATE TABLE IF NOT EXISTS AppSettings (
                        Key TEXT PRIMARY KEY,
                        Value TEXT NOT NULL,
                        Description TEXT
                    );

                    CREATE INDEX IF NOT EXISTS IX_Customers_Status ON Customers(Status);
                    CREATE INDEX IF NOT EXISTS IX_SaleOpportunities_Stage ON SaleOpportunities(Stage);
                    CREATE INDEX IF NOT EXISTS IX_Tasks_Status ON Tasks(Status);
                    CREATE INDEX IF NOT EXISTS IX_Tasks_AssignedTo ON Tasks(AssignedToUserId);
                    "
                    cmd.ExecuteNonQuery()
                End Using

                SeedDefaultData(conn)
            End Using
        End Sub

        Private Shared Sub SeedDefaultData(conn As SqliteConnection)
            Dim userCount As Long = 0
            Using cmd = conn.CreateCommand()
                cmd.CommandText = "SELECT COUNT(1) FROM Users;"
                userCount = Convert.ToInt64(cmd.ExecuteScalar())
            End Using

            If userCount = 0 Then
                Dim adminSalt = SecurityService.GenerateSalt()
                Dim adminHash = SecurityService.HashPassword("Admin@12345", adminSalt)
                
                Dim empSalt = SecurityService.GenerateSalt()
                Dim empHash = SecurityService.HashPassword("Employee@123", empSalt)

                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    INSERT INTO Users (Username, PasswordHash, PasswordSalt, FullName, Email, Role, IsActive, CreatedAt)
                    VALUES 
                    (@adminUser, @adminHash, @adminSalt, 'Moustafa El-Sayed', 'admin@enterprisecrm.com', 1, 1, @now),
                    (@empUser, @empHash, @empSalt, 'Sara Mahmoud', 's.mahmoud@enterprisecrm.com', 2, 1, @now);
                    "
                    cmd.Parameters.AddWithValue("@adminUser", "admin")
                    cmd.Parameters.AddWithValue("@adminHash", adminHash)
                    cmd.Parameters.AddWithValue("@adminSalt", adminSalt)
                    cmd.Parameters.AddWithValue("@empUser", "sara")
                    cmd.Parameters.AddWithValue("@empHash", empHash)
                    cmd.Parameters.AddWithValue("@empSalt", empSalt)
                    cmd.Parameters.AddWithValue("@now", "2026-01-15 09:00:00")
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    INSERT INTO Customers (Name, Company, Email, Phone, Address, City, Country, Status, Notes, CreatedAt, UpdatedAt)
                    VALUES
                    ('Ahmed Mansour', 'Al-Ahram Logistics Corp', 'a.mansour@alahramlogistics.com', '+20 100 234 5678', '45 El Tahrir St', 'Cairo', 'Egypt', 3, 'Key account for automated supply chain systems.', @d1, @d1),
                    ('Fatima El-Zahraa', 'Nile Cloud Solutions', 'fatima.z@nilecloud.com', '+20 112 345 6789', '12 Abbasia Blvd', 'Alexandria', 'Egypt', 3, 'Interested in multi-region enterprise tier.', @d2, @d2),
                    ('Tarek El-Shennawy', 'Al-Farouk Industrial Tech', 'tarek.s@alfarouk.com', '+20 122 890 1234', '88 El Merghany St, Heliopolis', 'Cairo', 'Egypt', 2, 'Scheduled ERP overhaul discussion for 2026.', @d3, @d3),
                    ('Nourhan Khalil', 'Al-Amal Retail Systems', 'nourhan.k@alamalretail.com', '+20 109 426 8550', '22 Makram Ebeid St, Nasr City', 'Cairo', 'Egypt', 1, 'Inbound inquiry from retail expo 2026.', @d4, @d4),
                    ('Khaled Abdel-Rahman', 'Al-Madina Dynamics Ltd', 'khaled.r@almadinadynamics.com', '+20 155 555 0142', '15 Corniche El Nil', 'Giza', 'Egypt', 3, 'Signed annual renewal contract 2026.', @d5, @d5);
                    "
                    cmd.Parameters.AddWithValue("@d1", "2026-01-10 10:00:00")
                    cmd.Parameters.AddWithValue("@d2", "2026-01-25 11:30:00")
                    cmd.Parameters.AddWithValue("@d3", "2026-02-05 14:15:00")
                    cmd.Parameters.AddWithValue("@d4", "2026-02-18 16:45:00")
                    cmd.Parameters.AddWithValue("@d5", "2026-01-05 08:30:00")
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    INSERT INTO ContactHistory (CustomerId, UserId, ContactType, Subject, Notes, ContactDate, CreatedAt)
                    VALUES
                    (1, 1, 2, 'Annual Architecture Review', 'Presented technical proposal for automated routing pipeline. Approved by CIO.', @cd1, @cd1),
                    (1, 1, 1, 'Contract Follow-up Call', 'Confirmed billing terms and SLA targets for 2026.', @cd2, @cd2),
                    (2, 2, 2, 'Discovery Meeting', 'Discussed data sovereignty and cloud security compliance for 2026.', @cd3, @cd3),
                    (3, 1, 1, 'Executive Intro Call', 'Introduced Al-Farouk leadership to our engineering directors.', @cd4, @cd4),
                    (5, 2, 3, 'Renewal Confirmation', 'Transmitted signed maintenance agreement and invoice for 2026.', @cd5, @cd5);
                    "
                    cmd.Parameters.AddWithValue("@cd1", "2026-02-10 11:00:00")
                    cmd.Parameters.AddWithValue("@cd2", "2026-02-20 14:00:00")
                    cmd.Parameters.AddWithValue("@cd3", "2026-02-15 10:30:00")
                    cmd.Parameters.AddWithValue("@cd4", "2026-02-22 15:00:00")
                    cmd.Parameters.AddWithValue("@cd5", "2026-01-28 09:30:00")
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    INSERT INTO SaleOpportunities (CustomerId, UserId, Title, Amount, Stage, Probability, ExpectedCloseDate, ClosedDate, Notes, CreatedAt, UpdatedAt)
                    VALUES
                    (1, 1, 'Global Cloud Deployment Expansion', 125000.00, 4, 100, @c1, @c1, 'Closed-Won deal signed by procurement in 2026.', @sd1, @sd1),
                    (2, 2, 'Regional Infrastructure Upgrade 2026', 64000.00, 3, 75, @c2, NULL, 'Legal department reviewing master services agreement.', @sd2, @sd2),
                    (3, 1, 'Factory Automation Telemetry Hub', 89500.00, 2, 50, @c3, NULL, 'Technical proof of concept is underway.', @sd3, @sd3),
                    (4, 2, 'Point-of-Sale Realtime Sync Module', 32000.00, 1, 20, @c4, NULL, 'Initial qualification calls scheduled.', @sd4, @sd4),
                    (5, 1, 'Enterprise Support Package Tier 3', 45000.00, 4, 100, @c5, @c5, '12-month tier 3 support plan agreement for 2026.', @sd5, @sd5);
                    "
                    cmd.Parameters.AddWithValue("@sd1", "2026-01-20 10:00:00")
                    cmd.Parameters.AddWithValue("@sd2", "2026-02-01 11:00:00")
                    cmd.Parameters.AddWithValue("@sd3", "2026-02-10 12:00:00")
                    cmd.Parameters.AddWithValue("@sd4", "2026-02-18 13:00:00")
                    cmd.Parameters.AddWithValue("@sd5", "2026-01-12 14:00:00")

                    cmd.Parameters.AddWithValue("@c1", "2026-02-15")
                    cmd.Parameters.AddWithValue("@c2", "2026-03-20")
                    cmd.Parameters.AddWithValue("@c3", "2026-04-15")
                    cmd.Parameters.AddWithValue("@c4", "2026-05-01")
                    cmd.Parameters.AddWithValue("@c5", "2026-02-01")
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    INSERT INTO Tasks (Title, Description, CustomerId, AssignedToUserId, Priority, Status, DueDate, CompletedDate, CreatedAt)
                    VALUES
                    ('Send Q1 Milestone Invoice', 'Generate final billing statements for Al-Ahram Logistics', 1, 1, 3, 3, @td1, @td1, @tcd1),
                    ('Schedule Security Audit Briefing', 'Coordinate with Nile Cloud compliance officer', 2, 2, 4, 2, @td2, NULL, @tcd2),
                    ('Prepare Hardware Specs Proposal', 'Draft custom edge device topology for Al-Farouk Industries', 3, 1, 2, 1, @td3, NULL, @tcd3),
                    ('Conduct Lead Discovery Call', 'Qualify Al-Amal retail multi-store requirements', 4, 2, 2, 1, @td4, NULL, @tcd4),
                    ('Send Executive Gift Package', 'Annual token of appreciation for Al-Madina renewal', 5, 1, 1, 3, @td5, @td5, @tcd5);
                    "
                    cmd.Parameters.AddWithValue("@td1", "2026-02-15 17:00:00")
                    cmd.Parameters.AddWithValue("@td2", "2026-03-05 10:00:00")
                    cmd.Parameters.AddWithValue("@td3", "2026-03-12 14:00:00")
                    cmd.Parameters.AddWithValue("@td4", "2026-03-08 11:30:00")
                    cmd.Parameters.AddWithValue("@td5", "2026-02-10 16:00:00")

                    cmd.Parameters.AddWithValue("@tcd1", "2026-02-01 09:00:00")
                    cmd.Parameters.AddWithValue("@tcd2", "2026-02-20 09:00:00")
                    cmd.Parameters.AddWithValue("@tcd3", "2026-02-22 09:00:00")
                    cmd.Parameters.AddWithValue("@tcd4", "2026-02-23 09:00:00")
                    cmd.Parameters.AddWithValue("@tcd5", "2026-01-20 09:00:00")
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    INSERT INTO ActivityLogs (UserId, UserName, Action, EntityName, EntityId, Details, Timestamp)
                    VALUES
                    (1, 'Moustafa El-Sayed', 'INITIALIZE', 'System', NULL, 'Enterprise CRM system and database initialized 2026.', @ad1),
                    (1, 'Moustafa El-Sayed', 'CREATE', 'SaleOpportunity', 1, 'Won Opportunity: Global Cloud Deployment Expansion ($125,000)', @ad2),
                    (2, 'Sara Mahmoud', 'UPDATE', 'Customer', 2, 'Updated account status for Nile Cloud Solutions to Active', @ad3);
                    "
                    cmd.Parameters.AddWithValue("@ad1", "2026-01-01 08:00:00")
                    cmd.Parameters.AddWithValue("@ad2", "2026-02-15 10:00:00")
                    cmd.Parameters.AddWithValue("@ad3", "2026-02-20 14:30:00")
                    cmd.ExecuteNonQuery()
                End Using

                Using cmd = conn.CreateCommand()
                    cmd.CommandText = "
                    INSERT INTO AppSettings (Key, Value, Description)
                    VALUES
                    ('CompanyName', 'Al-Rawasi Enterprise Systems', 'Organization Legal Name'),
                    ('Theme', 'Light', 'Default interface appearance (Light or Dark)'),
                    ('CurrencySymbol', '$', 'Standard reporting currency symbol'),
                    ('AutoBackupDays', '7', 'Frequency of automated backup checks');
                    "
                    cmd.ExecuteNonQuery()
                End Using
            End If
        End Sub
    End Class
End Namespace
