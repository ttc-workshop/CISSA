CREATE TABLE [Workflow_Gates] (
	Id uniqueidentifier not null,
	Process_Id uniqueidentifier,
	CONSTRAINT [PK_Workflow_Gates] PRIMARY KEY CLUSTERED ([Id] ASC)
	WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Workflow_Gate_Object_Defs]') AND parent_object_id = OBJECT_ID(N'[dbo].[Workflow_Gate_Refs]'))
ALTER TABLE [dbo].[Workflow_Gates] DROP CONSTRAINT [FK_Workflow_Gate_Object_Defs]
GO

ALTER TABLE [dbo].[Workflow_Gates] WITH NOCHECK ADD  CONSTRAINT [FK_Workflow_Gate_Workflow_Defs] FOREIGN KEY([Id])
REFERENCES [dbo].[Workflow_Defs] ([Id])
GO

ALTER TABLE [dbo].[Workflow_Gates] CHECK CONSTRAINT [FK_Workflow_Gate_Workflow_Defs]
GO

CREATE TABLE [Workflow_Gate_Refs] (
	Id uniqueidentifier not null,
	Service_Url varchar(1000),
	[User_Name] varchar(500),
	[User_Password] varchar(500),
	Store_Location smallint,
	Store_Name smallint,
	Find_Type smallint,
	Subject_Name varchar(500),
	CONSTRAINT [PK_Workflow_Gate_Refs] PRIMARY KEY CLUSTERED ([Id] ASC)
	WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

IF  EXISTS (SELECT * FROM sys.foreign_keys WHERE object_id = OBJECT_ID(N'[dbo].[FK_Workflow_Gate_Ref_Object_Defs]') AND parent_object_id = OBJECT_ID(N'[dbo].[Workflow_Gate_Refs]'))
ALTER TABLE [dbo].[Workflow_Gate_Refs] DROP CONSTRAINT [FK_Workflow_Gate_Ref_Object_Defs]
GO

ALTER TABLE [dbo].[Workflow_Gate_Refs] WITH NOCHECK ADD  CONSTRAINT [FK_Workflow_Gate_Ref_Workflow_Defs] FOREIGN KEY([Id])
REFERENCES [dbo].[Workflow_Defs] ([Id])
GO

ALTER TABLE [dbo].[Workflow_Gate_Refs] CHECK CONSTRAINT [FK_Workflow_Gate_Ref_Workflow_Defs]
GO

CREATE TABLE [Gate_Call_Activities] (
	Id uniqueidentifier not null,
	Gate_Id uniqueidentifier,
	Process_Name varchar(500),
	[User_Name] varchar(500),
	[User_Password] varchar(500),
	CONSTRAINT [PK_Gate_Call_Activities] PRIMARY KEY CLUSTERED ([Id] ASC)
	WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY]
GO

ALTER TABLE [dbo].[Gate_Call_Activities] WITH NOCHECK ADD  CONSTRAINT [FK_Gate_Call_Activity_Workflow_Activities] FOREIGN KEY([Id])
REFERENCES [dbo].[Workflow_Activities] ([Id])
GO

ALTER TABLE [dbo].[Gate_Call_Activities] CHECK CONSTRAINT [FK_Gate_Call_Activity_Workflow_Activities]
GO

ALTER TABLE [dbo].[Gate_Call_Activities] WITH NOCHECK ADD  CONSTRAINT [FK_Gate_Call_Activity_Workflow_Gate_Refs] FOREIGN KEY([Gate_Id])
REFERENCES [dbo].[Workflow_Gate_Refs] ([Id])
GO

ALTER TABLE [dbo].[Gate_Call_Activities] CHECK CONSTRAINT [FK_Gate_Call_Activity_Workflow_Gate_Refs]
GO
