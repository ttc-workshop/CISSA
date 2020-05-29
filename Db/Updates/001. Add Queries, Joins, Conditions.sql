create table Queries (
	Id uniqueidentifier not null,
	Document_Id uniqueidentifier,
	Alias varchar(100),
	CONSTRAINT [XPK_Queries] PRIMARY KEY CLUSTERED ([Id] ASC)
);

create table Query_Sources (
	Id uniqueidentifier not null,
	Document_Id uniqueidentifier,
	Query_Id uniqueidentifier,
	Alias varchar(100),
	Join_Type smallint,
	CONSTRAINT [XPK_Query_Sources] PRIMARY KEY CLUSTERED ([Id] ASC)
);
	
create table Conditions (
	Id uniqueidentifier not null,
	Expression smallint, -- Condition_Expressions
	Left_Source varchar(100),
	Left_Attribute_Id uniqueidentifier, -- Attribute_Defs
	Left_Attribute_Name varchar(100),
	Left_Value varchar(1000),
    Left_Param_Name varchar(100),
	Operation smallint, -- Compare_Operations
	Right_Source varchar(100),
	Right_Attribute_Id uniqueidentifier, -- Attribute_Defs
	Right_Attribute_Name varchar(100),
	Right_Value varchar(1000),
    Right_Param_Name varchar(100),
	CONSTRAINT [XPK_Conditions] PRIMARY KEY CLUSTERED ([Id] ASC)
);

alter table Queries WITH NOCHECK ADD CONSTRAINT FK_Queries_Object_Defs FOREIGN KEY([Id])
REFERENCES Object_Defs (Id)
GO

alter table Query_Sources WITH NOCHECK ADD CONSTRAINT FK_Query_Sources_Object_Defs FOREIGN KEY([Id])
REFERENCES Object_Defs (Id)
GO

alter table Conditions WITH NOCHECK ADD CONSTRAINT FK_Conditions_Object_Defs FOREIGN KEY([Id])
REFERENCES Object_Defs (Id)
GO

create table Condition_Expressions (
	Id smallint not null,
	Name varchar(20)
);

create table Join_Types (
	Id smallint not null,
	Name varchar(100)
);

insert into Condition_Expressions (Id, Name) values (1, 'AND');
insert into Condition_Expressions (Id, Name) values (2, 'OR');
insert into Condition_Expressions (Id, Name) values (3, 'AND NOT');
insert into Condition_Expressions (Id, Name) values (4, 'OR NOT');

-- Equal, NotEqual, GreatThen, GreatEqual, LessThen, LessEqual, Contains, NotContains, Like, NotLike, Between, NotBetween, In, NotIn, IsNull, 
-- IsNotNull, Include, Is, NotIs, Exp, Levenstein

insert into Join_Types (Id, Name) values (1, 'INNER JOIN');
insert into Join_Types (Id, Name) values (2, 'LEFT OUTER JOIN');
insert into Join_Types (Id, Name) values (3, 'RIGHT OUTER JOIN');
insert into Join_Types (Id, Name) values (4, 'FULL OUTER JOIN');