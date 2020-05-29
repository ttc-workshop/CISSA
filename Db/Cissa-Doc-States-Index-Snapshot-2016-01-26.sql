DROP INDEX [IUX_Document_States_State_Created_Doc] ON [dbo].[Document_States]
--  ([State_Type_Id], [Created], [Document_Id])
--ON [IndexFileGroup]
GO

DROP INDEX [IUX_Document_States_State_Expired_Doc] ON [dbo].[Document_States]
--  ([State_Type_Id], [Expired], [Document_Id])
--ON [IndexFileGroup]
GO

DROP INDEX [Ix_Document_States_Doc_Expired_Inc_Type_Created] ON [dbo].[Document_States]
--  ([Document_Id], [Expired])
--INCLUDE ([State_Type_Id], [Created])
--ON [IndexFileGroup]
GO

DROP INDEX [Ix_Document_States_Type_Expired_Doc] ON [dbo].[Document_States]
--  ([State_Type_Id], [Expired], [Document_Id])
--ON [IndexFileGroup]
GO

CREATE UNIQUE NONCLUSTERED INDEX [IUX_Document_States_Doc_Expired_State] ON [dbo].[Document_States] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[State_Type_Id] ASC
)
ON [IndexFileGroup]
GO

CREATE NONCLUSTERED INDEX [IX_Document_States_State_Expired_Doc] ON [dbo].[Document_States] 
(
	[State_Type_Id] ASC,
	[Expired] ASC,
	[Document_Id] ASC
)
ON [IndexFileGroup]
GO
