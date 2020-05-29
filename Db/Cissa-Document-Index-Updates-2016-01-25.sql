DROP INDEX [_dta_index_Documents_8_245575913__K1_K4_K8_11]
  ON [dbo].[Documents]
GO

DROP INDEX [_dta_index_Documents_8_245575913__K8_K1_K4]
  ON [dbo].[Documents]
GO

DROP INDEX [Ix_Document_Def_Id_Deleted_Id] ON [dbo].[Documents]
GO

DROP INDEX [Ix_Document_Def_Id_Id] ON [dbo].[Documents]
GO

DROP INDEX [Ix_Document_Def_Org_Deleted_Id] ON [dbo].[Documents]
GO

DROP INDEX [Ix_Document_Def_Org_Id] ON [dbo].[Documents]
GO

DROP INDEX [Ix_Document_Def_Org_Modified_Id] ON [dbo].[Documents]
GO

CREATE NONCLUSTERED INDEX [Ix_Document_Id_Def_Org_Deleted_Modified] ON [dbo].[Documents] 
(
	[Id] ASC,
	[Def_Id] ASC,
	[Organization_Id] ASC,
	[Deleted] ASC,
	[Last_Modified] DESC
)
ON [IndexFileGroup]
GO

CREATE NONCLUSTERED INDEX [Ix_Document_Def_Org_Deleted_Id_Modified] ON [dbo].[Documents] 
(
	[Def_Id] ASC,
	[Organization_Id] ASC,
	[Deleted] ASC,
	[Id] ASC,
	[Last_Modified] DESC
)
ON [IndexFileGroup]
GO

CREATE NONCLUSTERED INDEX [Ix_Document_Def_Org_Deleted_Modified] ON [dbo].[Documents] 
(
	[Def_Id] ASC,
	[Organization_Id] ASC,
	[Deleted] ASC,
	[Last_Modified] DESC
)
ON [IndexFileGroup]
GO
