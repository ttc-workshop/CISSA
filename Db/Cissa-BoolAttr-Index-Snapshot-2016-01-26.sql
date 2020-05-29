DROP INDEX [Ix_Boolean_Attribute_Def_Expired_Doc] ON [dbo].[Boolean_Attributes]
GO

DROP INDEX [Ix_Boolean_Attribute_Def_Value_Expired] ON [dbo].[Boolean_Attributes]
GO

DROP INDEX [Ix_Boolean_Attribute_Doc_Expired_Inc_Def_Value_Created] ON [dbo].[Boolean_Attributes]
GO

CREATE UNIQUE NONCLUSTERED INDEX [Ix_Boolean_Attribute_Doc_Expired_Def_Value] ON [dbo].[Boolean_Attributes] 
(
	[Document_Id] ASC,
	[Expired] ASC,
	[Def_Id] ASC,
	[Value] ASC
)
ON [IndexFileGroup]
GO

CREATE NONCLUSTERED INDEX [Ix_Boolean_Attribute_Def_Expired_Doc_Value] ON [dbo].[Boolean_Attributes] 
(
	[Def_Id] ASC,
	[Expired] ASC,
	[Document_Id] ASC,
	[Value] ASC
)
ON [IndexFileGroup]
GO


