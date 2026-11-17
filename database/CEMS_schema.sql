/* =====================================================================
   CEMS - Campus Event Management System
   Microsoft SQL Server schema (reference script)

   NOTE: You do NOT need to run this script. The application creates the
   same database automatically (Entity Framework Core EnsureCreated) the
   first time it starts, and loads demo data. This script is provided for
   marking / documentation and for creating the database manually.
   ===================================================================== */
IF DB_ID(N'CEMS') IS NULL CREATE DATABASE CEMS;
GO
USE CEMS;
GO

CREATE TABLE Users (
    UserId        INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Users PRIMARY KEY,
    FullName      NVARCHAR(100) NOT NULL,
    Email         NVARCHAR(150) NOT NULL,
    PasswordHash  NVARCHAR(256) NOT NULL,           -- salted PBKDF2 hash, never the plain password
    ContactNumber NVARCHAR(20)  NULL,
    Department    NVARCHAR(100) NULL,
    Role          NVARCHAR(20)  NOT NULL,           -- Student | Staff | Admin
    CreatedAt     DATETIME2     NOT NULL
);
CREATE UNIQUE INDEX IX_Users_Email ON Users (Email);

CREATE TABLE Events (
    EventId     INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Events PRIMARY KEY,
    Title       NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) NOT NULL,
    EventDate   DATETIME2     NOT NULL,
    Venue       NVARCHAR(200) NOT NULL,
    Category    NVARCHAR(50)  NOT NULL,             -- Seminar | Workshop | Sports | Club Activity
    MaxSeats    INT           NOT NULL,
    OrganiserId INT           NOT NULL,
    Status      NVARCHAR(20)  NOT NULL,             -- Active | Cancelled | Completed
    CreatedAt   DATETIME2     NOT NULL,
    CONSTRAINT FK_Events_Users_OrganiserId FOREIGN KEY (OrganiserId) REFERENCES Users (UserId)
);
CREATE INDEX IX_Events_OrganiserId ON Events (OrganiserId);

CREATE TABLE Registrations (
    RegistrationId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Registrations PRIMARY KEY,
    UserId         INT           NOT NULL,
    EventId        INT           NOT NULL,
    RegisteredAt   DATETIME2     NOT NULL,
    Status         NVARCHAR(20)  NOT NULL,          -- Registered | Attended | Cancelled
    CancelledAt    DATETIME2     NULL,
    CancelReason   NVARCHAR(500) NULL,
    Remarks        NVARCHAR(200) NULL,
    CONSTRAINT FK_Registrations_Users_UserId   FOREIGN KEY (UserId)  REFERENCES Users (UserId),
    CONSTRAINT FK_Registrations_Events_EventId FOREIGN KEY (EventId) REFERENCES Events (EventId)
);
-- one ACTIVE registration per user per event (cancelled rows stay as history)
CREATE UNIQUE INDEX IX_Registrations_UserId_EventId ON Registrations (UserId, EventId) WHERE [Status] = 'Registered';
CREATE INDEX IX_Registrations_EventId ON Registrations (EventId);

CREATE TABLE RegistrationLogs (
    LogId          INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_RegistrationLogs PRIMARY KEY,
    RegistrationId INT           NOT NULL,
    ActorUserId    INT           NOT NULL,          -- who made the change
    Action         NVARCHAR(30)  NOT NULL,
    Details        NVARCHAR(500) NULL,
    LoggedAt       DATETIME2     NOT NULL,
    CONSTRAINT FK_RegistrationLogs_Registrations FOREIGN KEY (RegistrationId) REFERENCES Registrations (RegistrationId),
    CONSTRAINT FK_RegistrationLogs_Users_ActorUserId FOREIGN KEY (ActorUserId) REFERENCES Users (UserId)
);
CREATE INDEX IX_RegistrationLogs_RegistrationId ON RegistrationLogs (RegistrationId);
CREATE INDEX IX_RegistrationLogs_ActorUserId ON RegistrationLogs (ActorUserId);

CREATE TABLE Feedbacks (
    FeedbackId  INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Feedbacks PRIMARY KEY,
    EventId     INT           NOT NULL,
    UserId      INT           NOT NULL,
    Rating      INT           NOT NULL,
    Comments    NVARCHAR(1000) NULL,
    SubmittedAt DATETIME2     NOT NULL,
    CONSTRAINT CK_Feedbacks_Rating CHECK ([Rating] BETWEEN 1 AND 5),
    CONSTRAINT FK_Feedbacks_Events_EventId FOREIGN KEY (EventId) REFERENCES Events (EventId),
    CONSTRAINT FK_Feedbacks_Users_UserId   FOREIGN KEY (UserId)  REFERENCES Users (UserId)
);
CREATE UNIQUE INDEX IX_Feedbacks_EventId_UserId ON Feedbacks (EventId, UserId);   -- one feedback per user per event
CREATE INDEX IX_Feedbacks_UserId ON Feedbacks (UserId);

CREATE TABLE Notifications (
    NotificationId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Notifications PRIMARY KEY,
    UserId         INT           NOT NULL,
    Message        NVARCHAR(500) NOT NULL,
    IsRead         BIT           NOT NULL,
    CreatedAt      DATETIME2     NOT NULL,
    CONSTRAINT FK_Notifications_Users_UserId FOREIGN KEY (UserId) REFERENCES Users (UserId)
);
CREATE INDEX IX_Notifications_UserId ON Notifications (UserId);

CREATE TABLE Enquiries (
    EnquiryId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_Enquiries PRIMARY KEY,
    EventId   INT            NOT NULL,
    UserId    INT            NOT NULL,
    Subject   NVARCHAR(200)  NOT NULL,
    Message   NVARCHAR(1000) NOT NULL,
    Reply     NVARCHAR(1000) NULL,
    SentAt    DATETIME2      NOT NULL,
    RepliedAt DATETIME2      NULL,
    CONSTRAINT FK_Enquiries_Events_EventId FOREIGN KEY (EventId) REFERENCES Events (EventId),
    CONSTRAINT FK_Enquiries_Users_UserId   FOREIGN KEY (UserId)  REFERENCES Users (UserId)
);
CREATE INDEX IX_Enquiries_EventId ON Enquiries (EventId);
CREATE INDEX IX_Enquiries_UserId ON Enquiries (UserId);

CREATE TABLE EventResources (
    ResourceId INT IDENTITY(1,1) NOT NULL CONSTRAINT PK_EventResources PRIMARY KEY,
    EventId    INT           NOT NULL,
    FileName   NVARCHAR(200) NOT NULL,
    FilePath   NVARCHAR(500) NOT NULL,
    UploadedAt DATETIME2     NOT NULL,
    CONSTRAINT FK_EventResources_Events_EventId FOREIGN KEY (EventId) REFERENCES Events (EventId)
);
CREATE INDEX IX_EventResources_EventId ON EventResources (EventId);
GO
