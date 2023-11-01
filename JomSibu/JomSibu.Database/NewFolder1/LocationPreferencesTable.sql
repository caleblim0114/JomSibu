CREATE TABLE [dbo].[LocationPreferencesTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [LocationId] INT NULL, 
    [PreferenceId] INT NULL, 
    CONSTRAINT [FK_LocationPreferencesTable_ToLocationsTable] FOREIGN KEY ([LocationId]) REFERENCES [LocationsTable]([Id]), 
    CONSTRAINT [FK_LocationPreferencesTable_ToPreferencesTable] FOREIGN KEY ([PreferenceId]) REFERENCES [PreferencesTable]([Id])
)
