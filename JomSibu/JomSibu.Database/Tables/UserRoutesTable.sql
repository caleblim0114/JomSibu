CREATE TABLE [dbo].[UserRoutesTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UserId] INT NULL, 
    [LocationId] INT NULL, 
    [DateTime] DATETIME NULL, 
    CONSTRAINT [FK_UserRoutesTable_ToUserDetailsTable] FOREIGN KEY ([UserId]) REFERENCES [UserDetailsTable]([Id]), 
    CONSTRAINT [FK_UserRoutesTable_ToLocationsTable] FOREIGN KEY ([LocationId]) REFERENCES [LocationsTable]([Id])
)
