use [bags_test];

-- create table to hold all of the old ids
create table [dbo].[old]
(
	[id] int primary key identity(1,1) not null,
	[old_id] int not null
);

-- create table to hold all of the new ids, this table will have same number of rows autoincremented as `old`
create table [dbo].[new]
(
	[id] int primary key identity(1,1) not null,
	[new_id] int not null,
);

-- create a new table to hold our newly randomized products
create table [dbo].[ProductsRandomized]
(
	[Id] int identity(1,1) not null,
	[Price] bigint not null,
	[Name] nvarchar(max) not null,
	[Asin] nvarchar(450) not null,
	[ImagesJson] nvarchar(max) not null,
	constraint [PK_ProductsRandomized] primary key clustered ([Id])
);

-- populate intermediate tables by getting all product IDs in current order and then in random order
insert into [dbo].[old] (old_id) select [Id] from [dbo].[Products] order by [Id];
insert into [dbo].[new] (new_id) select [Id] from [dbo].[Products] order by rand();

-- populate randomized Products table by getting the original Products and getting the ID from the randomized table (via a couple joins)
set IDENTITY_INSERT [dbo].[ProductsRandomized] ON;

insert into
	[dbo].[ProductsRandomized]
	(
		[Id],
		[Price],
		[Name],
		[Asin],
		[ImagesJson]
	)
select
	[new].[new_id],
	[Products].[Price],
	[Products].[Name],
	[Products].[Asin],
	[Products].[ImagesJson]
from
	[dbo].[Products]
	join [dbo].[old] on [Products].[Id] = [old].[old_id]
	join [dbo].[new] on [old].[id] = [new].[id];

set IDENTITY_INSERT [dbo].[ProductsRandomized] OFF;

-- remove foreign keys
alter table [dbo].[ProductTags] drop constraint [FK_ProductTags_Products_ProductId];
alter table [dbo].[ProductTags] drop constraint [FK_ProductTags_Tags_TagId];

-- update tag join table to point at new ids
update
	[dbo].[ProductTags]
set
	[ProductTags].[ProductId] = [new].[new_id]
from
	[dbo].[ProductTags]
	join [dbo].[old] on [ProductTags].[ProductId] = [old].[old_id]
	join [dbo].[new] on [old].[id] = [new].[id];

-- drop old table
drop table [dbo].[Products];

-- rename table
EXECUTE sp_rename N'dbo.ProductsRandomized', N'Products', 'OBJECT'

-- rename primary key
alter table [dbo].[Products] drop constraint [PK_ProductsRandomized]
alter table [dbo].[Products] add constraint [PK_Products] primary key clustered([Id])

-- add back foreign keys
alter table [dbo].[ProductTags] with check add constraint [FK_ProductTags_Tags_TagId] foreign key([TagId]) references [dbo].[Tags] ([Id]) on delete cascade
alter table [dbo].[ProductTags] check constraint [FK_ProductTags_Tags_TagId]
alter table [dbo].[ProductTags] with check add constraint [FK_ProductTags_Products_ProductId] foreign key([ProductId]) references [dbo].[Products] ([Id]) on delete cascade;
alter table [dbo].[ProductTags] check constraint [FK_ProductTags_Products_ProductId];

-- drop temporary tables
drop table [dbo].[new];
drop table [dbo].[old];
