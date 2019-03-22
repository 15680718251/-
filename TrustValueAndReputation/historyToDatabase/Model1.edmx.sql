
-- --------------------------------------------------
-- Entity Designer DDL Script for SQL Server 2005, 2008, and Azure
-- --------------------------------------------------
-- Date Created: 04/14/2014 15:29:42
-- Generated from EDMX file: D:\张红辉\Program2014NEW\Program20131226NEW\src\GIS.GeoNames\Model1.edmx
-- --------------------------------------------------

SET QUOTED_IDENTIFIER OFF;
GO
USE [indiatest];
GO
IF SCHEMA_ID(N'dbo') IS NULL EXECUTE(N'CREATE SCHEMA [dbo]');
GO

-- --------------------------------------------------
-- Dropping existing FOREIGN KEY constraints
-- --------------------------------------------------


-- --------------------------------------------------
-- Dropping existing tables
-- --------------------------------------------------

IF OBJECT_ID(N'[dbo].[NODES]', 'U') IS NOT NULL
    DROP TABLE [dbo].[NODES];
GO
IF OBJECT_ID(N'[dbo].[RELATION_ALL_RELS]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RELATION_ALL_RELS];
GO
IF OBJECT_ID(N'[dbo].[RELATIONS]', 'U') IS NOT NULL
    DROP TABLE [dbo].[RELATIONS];
GO
IF OBJECT_ID(N'[dbo].[WAY_NODE_RELS]', 'U') IS NOT NULL
    DROP TABLE [dbo].[WAY_NODE_RELS];
GO
IF OBJECT_ID(N'[dbo].[WAYS]', 'U') IS NOT NULL
    DROP TABLE [dbo].[WAYS];
GO

-- --------------------------------------------------
-- Creating all tables
-- --------------------------------------------------

-- Creating table 'RELATION_ALL_REL'
CREATE TABLE [dbo].[RELATION_ALL_REL] (
    [id] int IDENTITY(1,1) NOT NULL,
    [relationid] nvarchar(20)  NOT NULL,
    [memberid] nvarchar(20)  NOT NULL,
    [type] nvarchar(10)  NOT NULL,
    [number] int  NOT NULL,
    [role] nvarchar(20)  NULL
);
GO

-- Creating table 'WAY_NODE_REL'
CREATE TABLE [dbo].[WAY_NODE_REL] (
    [id] int IDENTITY(1,1) NOT NULL,
    [wayid] nvarchar(20)  NOT NULL,
    [nodeid] nvarchar(20)  NOT NULL,
    [number] int  NOT NULL
);
GO

-- Creating table 'NODES'
CREATE TABLE [dbo].[NODES] (
    [osmid] nvarchar(20)  NOT NULL,
    [user] nvarchar(50)  NULL,
    [uid] nvarchar(50)  NULL,
    [lat] float  NOT NULL,
    [lon] float  NOT NULL,
    [visible] bit  NULL,
    [version] smallint  NULL,
    [changeset] nvarchar(20)  NULL,
    [timestamp] datetime  NOT NULL,
    [issimple] bit  NOT NULL,
    [fc] nvarchar(20)  NULL,
    [dsg] nvarchar(20)  NULL,
    [code] nchar(2)  NULL,
    [gbcode] nvarchar(20)  NULL,
    [gbdes] nvarchar(20)  NULL,
    [tags] nvarchar(max)  NULL,
    [bz] nvarchar(50)  NULL,
    [name] nvarchar(200)  NULL,
    [name_en] nvarchar(200)  NULL,
    [name_zh] nvarchar(200)  NULL
);
GO

-- Creating table 'RELATIONS'
CREATE TABLE [dbo].[RELATIONS] (
    [osmid] nvarchar(20)  NOT NULL,
    [user] nvarchar(50)  NULL,
    [uid] nvarchar(50)  NULL,
    [visible] bit  NULL,
    [version] smallint  NULL,
    [changeset] nvarchar(20)  NULL,
    [timestamp] datetime  NOT NULL,
    [issimple] bit  NOT NULL,
    [fc] nvarchar(20)  NULL,
    [dsg] nvarchar(20)  NULL,
    [code] nchar(2)  NULL,
    [gbcode] nvarchar(20)  NULL,
    [gbdes] nvarchar(20)  NULL,
    [tags] nvarchar(max)  NULL,
    [bz] nvarchar(50)  NULL,
    [name] nvarchar(200)  NULL,
    [name_en] nvarchar(200)  NULL,
    [name_zh] nvarchar(200)  NULL
);
GO

-- Creating table 'WAYS'
CREATE TABLE [dbo].[WAYS] (
    [osmid] nvarchar(20)  NOT NULL,
    [user] nvarchar(50)  NULL,
    [uid] nvarchar(50)  NULL,
    [visible] bit  NULL,
    [version] smallint  NULL,
    [changeset] nvarchar(20)  NULL,
    [timestamp] datetime  NOT NULL,
    [issimple] bit  NOT NULL,
    [fc] nvarchar(20)  NULL,
    [dsg] nvarchar(20)  NULL,
    [code] nchar(2)  NULL,
    [gbcode] nvarchar(20)  NULL,
    [gddes] nvarchar(20)  NULL,
    [tags] nvarchar(max)  NULL,
    [bz] nvarchar(50)  NULL,
    [name] nvarchar(200)  NULL,
    [name_en] nvarchar(200)  NULL,
    [name_zh] nvarchar(200)  NULL
);
GO

-- --------------------------------------------------
-- Creating all PRIMARY KEY constraints
-- --------------------------------------------------

-- Creating primary key on [id] in table 'RELATION_ALL_REL'
ALTER TABLE [dbo].[RELATION_ALL_REL]
ADD CONSTRAINT [PK_RELATION_ALL_REL]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [id] in table 'WAY_NODE_REL'
ALTER TABLE [dbo].[WAY_NODE_REL]
ADD CONSTRAINT [PK_WAY_NODE_REL]
    PRIMARY KEY CLUSTERED ([id] ASC);
GO

-- Creating primary key on [osmid] in table 'NODES'
ALTER TABLE [dbo].[NODES]
ADD CONSTRAINT [PK_NODES]
    PRIMARY KEY CLUSTERED ([osmid] ASC);
GO

-- Creating primary key on [osmid] in table 'RELATIONS'
ALTER TABLE [dbo].[RELATIONS]
ADD CONSTRAINT [PK_RELATIONS]
    PRIMARY KEY CLUSTERED ([osmid] ASC);
GO

-- Creating primary key on [osmid] in table 'WAYS'
ALTER TABLE [dbo].[WAYS]
ADD CONSTRAINT [PK_WAYS]
    PRIMARY KEY CLUSTERED ([osmid] ASC);
GO

-- --------------------------------------------------
-- Creating all FOREIGN KEY constraints
-- --------------------------------------------------

-- --------------------------------------------------
-- Script has ended
-- --------------------------------------------------