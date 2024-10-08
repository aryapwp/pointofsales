/*
Run this script on:

        .\SQL2016.OL_DEV    -  This database will be modified

to synchronize it with:

        .\SQL2016.so

You are recommended to back up your database before running this script

*/
SET NUMERIC_ROUNDABORT OFF
GO
SET ANSI_PADDING, ANSI_WARNINGS, CONCAT_NULL_YIELDS_NULL, ARITHABORT, QUOTED_IDENTIFIER, ANSI_NULLS ON
GO
SET XACT_ABORT ON
GO
SET TRANSACTION ISOLATION LEVEL Serializable
GO
BEGIN TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[COM_CUSTOMER]'
GO
IF OBJECT_ID(N'[dbo].[COM_CUSTOMER]', 'U') IS NULL
CREATE TABLE [dbo].[COM_CUSTOMER]
(
[COM_CUSTOMER_ID] [int] NOT NULL IDENTITY(1, 1),
[CUSTOMER_NAME] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NULL
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_COM_CUSTOMER] on [dbo].[COM_CUSTOMER]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PK_COM_CUSTOMER]', 'PK') AND parent_object_id = OBJECT_ID(N'[dbo].[COM_CUSTOMER]', 'U'))
ALTER TABLE [dbo].[COM_CUSTOMER] ADD CONSTRAINT [PK_COM_CUSTOMER] PRIMARY KEY CLUSTERED  ([COM_CUSTOMER_ID])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[SO_ITEM]'
GO
IF OBJECT_ID(N'[dbo].[SO_ITEM]', 'U') IS NULL
CREATE TABLE [dbo].[SO_ITEM]
(
[SO_ITEM_ID] [bigint] NOT NULL IDENTITY(1, 1),
[SO_ORDER_ID] [bigint] NOT NULL CONSTRAINT [DF_Table_1_SALES_SO_ID] DEFAULT ((-99)),
[ITEM_NAME] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL CONSTRAINT [DF_SO_ITEM_ITEM_NAME] DEFAULT (''),
[QUANTITY] [int] NOT NULL CONSTRAINT [DF_SO_ITEM_QUANTITY] DEFAULT ((-99)),
[PRICE] [float] NOT NULL CONSTRAINT [DF_SO_ITEM_PRICE] DEFAULT ((0.0))
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_SO_ITEM] on [dbo].[SO_ITEM]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PK_SO_ITEM]', 'PK') AND parent_object_id = OBJECT_ID(N'[dbo].[SO_ITEM]', 'U'))
ALTER TABLE [dbo].[SO_ITEM] ADD CONSTRAINT [PK_SO_ITEM] PRIMARY KEY CLUSTERED  ([SO_ITEM_ID])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating [dbo].[SO_ORDER]'
GO
IF OBJECT_ID(N'[dbo].[SO_ORDER]', 'U') IS NULL
CREATE TABLE [dbo].[SO_ORDER]
(
[SO_ORDER_ID] [bigint] NOT NULL IDENTITY(1, 1),
[ORDER_NO] [varchar] (20) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL CONSTRAINT [DF_SO_ORDER_ORDER_NO] DEFAULT (''),
[ORDER_DATE] [datetime] NOT NULL CONSTRAINT [DF_SO_ORDER_ORDER_DATE] DEFAULT ('1900-01-01'),
[COM_CUSTOMER_ID] [int] NOT NULL CONSTRAINT [DF_SO_ORDER_COM_CUSTOMER_ID] DEFAULT ('-99'),
[ADDRESS] [varchar] (100) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL CONSTRAINT [DF_SO_ORDER_ADDRESS] DEFAULT ('')
)
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
PRINT N'Creating primary key [PK_SO_ORDER] on [dbo].[SO_ORDER]'
GO
IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[PK_SO_ORDER]', 'PK') AND parent_object_id = OBJECT_ID(N'[dbo].[SO_ORDER]', 'U'))
ALTER TABLE [dbo].[SO_ORDER] ADD CONSTRAINT [PK_SO_ORDER] PRIMARY KEY CLUSTERED  ([SO_ORDER_ID])
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
COMMIT TRANSACTION
GO
IF @@ERROR <> 0 SET NOEXEC ON
GO
-- This statement writes to the SQL Server Log so SQL Monitor can show this deployment.
IF HAS_PERMS_BY_NAME(N'sys.xp_logevent', N'OBJECT', N'EXECUTE') = 1
BEGIN
    DECLARE @databaseName AS nvarchar(2048), @eventMessage AS nvarchar(2048)
    SET @databaseName = REPLACE(REPLACE(DB_NAME(), N'\', N'\\'), N'"', N'\"')
    SET @eventMessage = N'Redgate SQL Compare: { "deployment": { "description": "Redgate SQL Compare deployed to ' + @databaseName + N'", "database": "' + @databaseName + N'" }}'
    EXECUTE sys.xp_logevent 55000, @eventMessage
END
GO
DECLARE @Success AS BIT
SET @Success = 1
SET NOEXEC OFF
IF (@Success = 1) PRINT 'The database update succeeded'
ELSE BEGIN
	IF @@TRANCOUNT > 0 ROLLBACK TRANSACTION
	PRINT 'The database update failed'
END
GO
