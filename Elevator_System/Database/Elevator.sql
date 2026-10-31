-- ElevatorDb : tables for the two-lift system.
-- The program runs this script by itself the first time it starts (it is found with a relative path:
-- <program folder>\Database\Elevator.sql). You can also run it yourself in SSMS or SQL Server Object Explorer.
-- It is safe to run more than once.

IF OBJECT_ID(N'dbo.Floors', N'U') IS NULL
CREATE TABLE dbo.Floors
(
    FloorId   INT          NOT NULL PRIMARY KEY,
    FloorName NVARCHAR(30) NOT NULL
);
GO

IF OBJECT_ID(N'dbo.Elevators', N'U') IS NULL
CREATE TABLE dbo.Elevators
(
    ElevatorId   INT          NOT NULL PRIMARY KEY,
    ElevatorName NVARCHAR(30) NOT NULL
);
GO

IF OBJECT_ID(N'dbo.ElevatorLog', N'U') IS NULL
CREATE TABLE dbo.ElevatorLog
(
    LogId      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
    ElevatorId INT           NULL CONSTRAINT FK_ElevatorLog_Elevators REFERENCES dbo.Elevators (ElevatorId),
    FloorId    INT           NULL CONSTRAINT FK_ElevatorLog_Floors    REFERENCES dbo.Floors (FloorId),
    EventType  NVARCHAR(40)  NOT NULL,
    Message    NVARCHAR(200) NOT NULL,
    LoggedAt   DATETIME2(3)  NOT NULL CONSTRAINT DF_ElevatorLog_LoggedAt DEFAULT (SYSDATETIME())
);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Floors)
INSERT INTO dbo.Floors (FloorId, FloorName) VALUES (1, N'Ground Floor'), (2, N'First Floor');
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Elevators)
INSERT INTO dbo.Elevators (ElevatorId, ElevatorName) VALUES (1, N'Elevator A'), (2, N'Elevator B');
GO
