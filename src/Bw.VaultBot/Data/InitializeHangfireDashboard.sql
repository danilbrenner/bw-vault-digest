IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[HangfireDashboardUsers]') AND type in (N'U'))
BEGIN
    CREATE TABLE [dbo].[HangfireDashboardUsers] (
        [Username] NVARCHAR(50) NOT NULL PRIMARY KEY,
        [PasswordHash] NVARCHAR(MAX) NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL,
        [LastLoginAt] DATETIME2 NULL
    );
END
