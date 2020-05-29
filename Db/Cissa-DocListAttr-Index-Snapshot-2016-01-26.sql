DROP INDEX [Ix_DocumentList_Attribute_Def_Expired_Doc] ON [dbo].[DocumentList_Attributes]
--  ([Def_Id], [Expired], [Document_Id])
--INCLUDE ([Value])
--ON [IndexFileGroup]
GO

DROP INDEX [Ix_DocumentList_Attribute_Def_Expired_Value_Doc] ON [dbo].[DocumentList_Attributes]
--  ([Def_Id], [Expired], [Value], [Document_Id])
--ON [IndexFileGroup]
GO

DROP INDEX [Ix_DocumentList_Attribute_Doc_Expired_Inc_Def_Value_Created] ON [dbo].[DocumentList_Attributes]
--  ([Document_Id], [Expired])
--INCLUDE ([Def_Id], [Value], [Created])
--ON [IndexFileGroup]
GO

CREATE UNIQUE NONCLUSTERED INDEX [Ix_DocumentList_Attribute_Doc_Expired_Def_Value] ON [dbo].[DocumentList_Attributes] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[Def_Id] ASC,
	[Value] ASC
)
ON [IndexFileGroup]
GO

CREATE NONCLUSTERED INDEX [Ix_DocumentList_Attribute_Def_Expired_Doc_Value] ON [dbo].[DocumentList_Attributes] 
(
	[Def_Id] ASC,
	[Expired] ASC,
	[Document_Id] ASC,
	[Value] ASC
)
ON [IndexFileGroup]
GO
