alter table Editors add Process_Id uniqueidentifier
go

ALTER TABLE [dbo].[Editors]  WITH CHECK ADD  CONSTRAINT [FK_Editors_Workflow_Processes] FOREIGN KEY([Process_Id])
REFERENCES [dbo].[Workflow_Processes] ([Id])
GO

ALTER TABLE [dbo].[Editors] CHECK CONSTRAINT [FK_Editors_Workflow_Processes]
GO

