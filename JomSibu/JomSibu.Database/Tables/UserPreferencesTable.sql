CREATE TABLE [dbo].[UserPreferencesTable]
(
	[Id] INT NOT NULL PRIMARY KEY IDENTITY, 
    [UserId] INT NULL, 
    [PreferenceId] INT NULL, 
    CONSTRAINT [FK_UserPreferencesTable_ToUserDetailsTable] FOREIGN KEY ([UserId]) REFERENCES [UserDetailsTable]([Id]), 
    CONSTRAINT [FK_UserPreferencesTable_ToPreferencesTable] FOREIGN KEY ([PreferenceId]) REFERENCES [PreferencesTable]([Id])
)
