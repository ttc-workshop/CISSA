declare @procId uniqueidentifier;
declare @controlId uniqueidentifier;

set @procId = '{3B4FAC2C-A45B-4A4D-8A53-EA4998E41C71}'
set @controlId = '{3BE540F6-0FFD-4072-AEB5-90BB0815F57D}'

select *
from Editors
where Id = @controlId

update Editors 
set Process_Id = @procId
where Id = @controlId