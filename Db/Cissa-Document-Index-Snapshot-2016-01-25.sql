CREATE TABLE [dbo].[Documents] (
  [Id] uniqueidentifier NOT NULL,
  [Created] datetime NULL,
  [Deleted] bit NULL,
  [Def_Id] uniqueidentifier NULL,
  [UserId] uniqueidentifier NOT NULL,
  [Organization_Id] uniqueidentifier NULL,
  [Org_Position_Id] uniqueidentifier NULL,
  [Last_Modified] datetime NULL,
  CONSTRAINT [XPKDocuments] PRIMARY KEY CLUSTERED ([Id])
)
ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [_dta_index_Documents_8_245575913__K1_K4_K8_11] ON [dbo].[Documents]
  ([Id], [Def_Id], [Organization_Id])
INCLUDE ([Last_Modified])
WITH (
  PAD_INDEX = OFF,
  DROP_EXISTING = OFF,
  STATISTICS_NORECOMPUTE = OFF,
  SORT_IN_TEMPDB = OFF,
  ONLINE = OFF,
  ALLOW_ROW_LOCKS = ON,
  ALLOW_PAGE_LOCKS = ON)
ON [PRIMARY]
GO

ALTER INDEX [_dta_index_Documents_8_245575913__K1_K4_K8_11]
  ON [dbo].[Documents]
  DISABLE
GO

CREATE NONCLUSTERED INDEX [_dta_index_Documents_8_245575913__K8_K1_K4] ON [dbo].[Documents]
  ([Organization_Id], [Id], [Def_Id])
WITH (
  PAD_INDEX = OFF,
  DROP_EXISTING = OFF,
  STATISTICS_NORECOMPUTE = OFF,
  SORT_IN_TEMPDB = OFF,
  ONLINE = OFF,
  ALLOW_ROW_LOCKS = ON,
  ALLOW_PAGE_LOCKS = ON)
ON [PRIMARY]
GO

ALTER INDEX [_dta_index_Documents_8_245575913__K8_K1_K4]
  ON [dbo].[Documents]
  DISABLE
GO

CREATE NONCLUSTERED INDEX [Ix_Document_Def_Id_Deleted_Id] ON [dbo].[Documents]
  ([Def_Id], [Deleted], [Id])
WITH (
  PAD_INDEX = OFF,
  DROP_EXISTING = OFF,
  STATISTICS_NORECOMPUTE = OFF,
  SORT_IN_TEMPDB = OFF,
  ONLINE = OFF,
  ALLOW_ROW_LOCKS = ON,
  ALLOW_PAGE_LOCKS = ON)
ON [IndexFileGroup]
GO

CREATE NONCLUSTERED INDEX [Ix_Document_Def_Id_Id] ON [dbo].[Documents]
  ([Def_Id], [Id])
WITH (
  PAD_INDEX = OFF,
  DROP_EXISTING = OFF,
  STATISTICS_NORECOMPUTE = OFF,
  SORT_IN_TEMPDB = OFF,
  ONLINE = OFF,
  ALLOW_ROW_LOCKS = ON,
  ALLOW_PAGE_LOCKS = ON)
ON [IndexFileGroup]
GO

CREATE NONCLUSTERED INDEX [Ix_Document_Def_Org_Deleted_Id] ON [dbo].[Documents]
  ([Def_Id], [Organization_Id], [Deleted], [Id])
WITH (
  PAD_INDEX = OFF,
  DROP_EXISTING = OFF,
  STATISTICS_NORECOMPUTE = OFF,
  SORT_IN_TEMPDB = OFF,
  ONLINE = OFF,
  ALLOW_ROW_LOCKS = ON,
  ALLOW_PAGE_LOCKS = ON)
ON [IndexFileGroup]
GO

CREATE NONCLUSTERED INDEX [Ix_Document_Def_Org_Id] ON [dbo].[Documents]
  ([Def_Id], [Organization_Id], [Id])
WITH (
  PAD_INDEX = OFF,
  DROP_EXISTING = OFF,
  STATISTICS_NORECOMPUTE = OFF,
  SORT_IN_TEMPDB = OFF,
  ONLINE = OFF,
  ALLOW_ROW_LOCKS = ON,
  ALLOW_PAGE_LOCKS = ON)
ON [IndexFileGroup]
GO

CREATE NONCLUSTERED INDEX [Ix_Document_Def_Org_Modified_Id] ON [dbo].[Documents]
  ([Def_Id], [Organization_Id], [Last_Modified] DESC, [Id])
WITH (
  PAD_INDEX = OFF,
  DROP_EXISTING = OFF,
  STATISTICS_NORECOMPUTE = OFF,
  SORT_IN_TEMPDB = OFF,
  ONLINE = OFF,
  ALLOW_ROW_LOCKS = ON,
  ALLOW_PAGE_LOCKS = ON)
ON [IndexFileGroup]
GO
