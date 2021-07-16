/****** Object: SqlProcedure [dbo].[spTable_ins] Script Date: 9 Jul 2021 4:14:15 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[spTable_ins](
	@AppID int,
	@TableName nvarchar(50),
	@Caption nvarchar(500),
	@DefaultView nvarchar(50) = 'Grid',
	@Bound bit = 0,
	@Src nvarchar(50) = null,
	@TableID int = -1 output
)
/********1*********2*********3*********4*********5*********6*********7*********8*********9
** Insert Table record
*****************************************************************************************/
AS

------------------------------------------------------------------------------------------
--  BEGIN ROUTINE
------------------------------------------------------------------------------------------
Declare @TranCount 				int	= @@TranCount
Declare @ErrPos					int -- Place holder for location of SQL Error
Declare @Err 					int	-- Place holder for last SQL Error
Declare @RowCount 				int	-- Place holder for Last affected row count

if @TranCount = 0 BEGIN TRAN

------------------------------------------------------------------------------------------
--  Declare local variables
------------------------------------------------------------------------------------------

Declare @ShowResults			bit = iif(@TableID < 0, 1, 0)
Declare @SQLDatabaseName		nvarchar(50)
Declare @SQLTableName			nvarchar(50)
Declare @FullSQLTableName		nvarchar(50)
Declare @DBChangeSQL			nvarchar(500)
Declare @SQL					nvarchar(1000)
Declare @PropertyValue			nvarchar(100)
Declare @FieldName				nvarchar(50)
Declare @Type					nvarchar(50)
Declare @Width					int
Declare @InRowKey				bit
Declare @Hidden					bit
Declare @FieldID				int
Declare @MacroID                int

------------------------------------------------------------------------------------------
--  1) Get SQL name of Database
------------------------------------------------------------------------------------------
select	@SQLDatabaseName = (case when UserDefined = 1 then 'U_' else '' end) + AppName
from	Application
where	AppID = @AppID

select @ErrPos=1, @Err=@@error, @RowCount=@@RowCount
if @Err != 0 goto End_Routine

------------------------------------------------------------------------------------------
--  2) Initialise CREATE TABLE statement
------------------------------------------------------------------------------------------
if @Bound = 1
  BEGIN
	set @SQLTableName = @TableName
	set @FullSQLTableName = @SQLDatabaseName + '.dbo.' + @SQLTableName

	if @FullSQLTableName is not null and OBJECT_ID(@FullSQLTableName) is null
	  BEGIN
		set @DBChangeSQL = 'create table ' + @FullSQLTableName
			+ '(ParentID int, ID int identity)'
	  END
  END

select @ErrPos=2, @Err=@@error, @RowCount=@@RowCount
if @Err != 0 goto End_Routine

------------------------------------------------------------------------------------------
--  3) Get next available TableID
------------------------------------------------------------------------------------------
select @TableID = isnull(max(TableID), 0) + 1 from CiTable where AppID = @AppID

select @ErrPos=3, @Err=@@error, @RowCount=@@RowCount
if @Err != 0 goto End_Routine

------------------------------------------------------------------------------------------
--  4) Insert Table record
------------------------------------------------------------------------------------------
insert
into  CiTable
(	AppID,
	TableID,
	TableName,
	Caption,
	DefaultView,
	Bound,
	SQLTableName,
	Src
)
values
(	@AppID,
	@TableID,
	@TableName,
	@Caption,
	@DefaultView,
	@Bound,
	@SQLTableName,
	@Src
)

select @ErrPos=4, @Err=@@error, @RowCount=@@RowCount
if @Err != 0 goto End_Routine

------------------------------------------------------------------------------------------
--  5) Create SELECT macro
------------------------------------------------------------------------------------------
if @Bound = 1
  BEGIN
	exec spMacro_ins @AppID, @TableID, 'Select', '-', 'Select', @MacroID output 

	select @ErrPos=5, @Err=@@error, @RowCount=@@RowCount
	if @Err != 0 goto End_Routine
  END

------------------------------------------------------------------------------------------
--  6) Create INSERT macro
------------------------------------------------------------------------------------------
if @Bound = 1
  BEGIN
	exec spMacro_ins @AppID, @TableID, 'Insert', '-', 'Insert', @MacroID output 

	select @ErrPos=6, @Err=@@error, @RowCount=@@RowCount
	if @Err != 0 goto End_Routine
  END

------------------------------------------------------------------------------------------
--  7) Create UPDATE macro
------------------------------------------------------------------------------------------
if @Bound = 1
  BEGIN
	exec spMacro_ins @AppID, @TableID, 'Update', '-', 'Update', @MacroID output 

	select @ErrPos=7, @Err=@@error, @RowCount=@@RowCount
	if @Err != 0 goto End_Routine
  END

------------------------------------------------------------------------------------------
--  8) Create DELETE macro
------------------------------------------------------------------------------------------
if @Bound = 1
  BEGIN
	exec spMacro_ins @AppID, @TableID, 'Delete', '-', 'Delete', @MacroID output 

	select @ErrPos=8, @Err=@@error, @RowCount=@@RowCount
	if @Err != 0 goto End_Routine
  END

------------------------------------------------------------------------------------------
--  9) Create DEFAULT macro
------------------------------------------------------------------------------------------
if @Bound = 1
  BEGIN
	exec spMacro_ins @AppID, @TableID, 'Default', '-', 'Default', @MacroID output 

	select @ErrPos=9, @Err=@@error, @RowCount=@@RowCount
	if @Err != 0 goto End_Routine
  END

------------------------------------------------------------------------------------------
--  10) Create ParentID field in nested Table (hidden)
------------------------------------------------------------------------------------------
exec spField_ins @AppID, @TableID, 'ParentID', 'Parent ID', 'FKey', 10, false, true,
	@FieldID output

select @ErrPos=10, @Err=@@error, @RowCount=@@RowCount
if @Err != 0 goto End_Routine

------------------------------------------------------------------------------------------
--  11) Create ID field in Table (visible)
------------------------------------------------------------------------------------------
exec spField_ins @AppID, @TableID, 'ID', 'ID', 'Identity', 10, true, false,
	@FieldID output

select @ErrPos=11, @Err=@@error, @RowCount=@@RowCount
if @Err != 0 goto End_Routine

------------------------------------------------------------------------------------------
--  12) Setup inherited fields and macros
------------------------------------------------------------------------------------------
exec spTable_syncSource @AppID, @TableID, @Src, null

select @ErrPos=12, @Err=@@error, @RowCount=@@RowCount
if @Err != 0 goto End_Routine

------------------------------------------------------------------------------------------
--  END ROUTINE
------------------------------------------------------------------------------------------
End_Routine:
if @Err = 0
  BEGIN
	if @TranCount = 0 COMMIT TRAN
	if @ShowResults = 1 select @TableID as TableID, @DBChangeSQL as DBChangeSQL
  END
else
  BEGIN
	Declare @ErrMsg as nvarchar(50) = 'Application error ' + cast(@ErrPos as nvarchar(30))
	RAISERROR (@ErrMsg,
		15,
		1)
	If @TranCount = 0 ROLLBACK TRAN
  END

return @Err

/****** Object: SqlProcedure [dbo].[spTable_updShort] Script Date: 9 Jul 2021 4:14:30 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[spTable_updShort](
	@AppID int,
	@TableID int,
	@TableName nvarchar(50),
	@Caption nvarchar(500),
	@DefaultView nvarchar(50) = 'Grid',
	@Bound bit = false,
	@Src nvarchar(50) = null,
	@ParentTableID int = null
)
/********1*********2*********3*********4*********5*********6*********7*********8*********9
** Update Table record
*****************************************************************************************/
AS

------------------------------------------------------------------------------------------
--  BEGIN ROUTINE
------------------------------------------------------------------------------------------
Declare @TranCount 				int	= @@TranCount
Declare @ErrPos					int -- Place holder for location of SQL Error
Declare @Err 					int	-- Place holder for last SQL Error
Declare @RowCount 				int	-- Place holder for Last affected row count
Declare @FieldID				int

if @TranCount = 0 BEGIN TRAN

------------------------------------------------------------------------------------------
--  Declare local variables
------------------------------------------------------------------------------------------

Declare @SQLDatabaseName		nvarchar(50)
Declare @OldSQLTableName		nvarchar(50)
Declare @NewSQLTableName		nvarchar(50)
Declare @OldFullSQLTableName	nvarchar(50)
Declare @NewFullSQLTableName	nvarchar(50)
Declare @DBChangeSQL			nvarchar(500)
Declare @SQL					nvarchar(1000)
Declare @PropertyValue			nvarchar(100)
Declare @OldSrc					nvarchar(50)

------------------------------------------------------------------------------------------
--  1) Get SQL names of Database and Table
------------------------------------------------------------------------------------------
select	@SQLDatabaseName = (case when UserDefined = 1 then 'U_' else '' end) + A.AppName,
		@OldSQLTableName = T.SQLTableName,
		@NewSQLTableName = @TableName,
		@OldSrc = T.Src
from	Application A
left outer join CiTable T
on		T.AppID = A.AppID
where	A.AppID = @AppID
and		T.TableID = @TableID

select @ErrPos=1, @Err=@@error, @RowCount=@@RowCount
if @Err !=0 goto End_Routine

------------------------------------------------------------------------------------------
--  2) Initialise CREATE TABLE or RENAME TABLE statements
------------------------------------------------------------------------------------------
if @Bound = 1
  BEGIN
	set @OldFullSQLTableName = @SQLDatabaseName + '.dbo.' + @OldSQLTableName
	set @NewFullSQLTableName = @SQLDatabaseName + '.dbo.' + @NewSQLTableName

	if	@OldSQLTableName != @NewSQLTableName
	and	OBJECT_ID(@OldFullSQLTableName) is not null
	and	OBJECT_ID(@NewFullSQLTableName) is null
	  BEGIN
		set @DBChangeSQL = 'exec ' + @SQLDatabaseName + '.dbo.sp_rename ' +
			@OldSQLTableName + ', ' + @NewSQLTableName
	  END
	else if	OBJECT_ID(@OldFullSQLTableName) is null
	and	OBJECT_ID(@NewFullSQLTableName) is null
	  BEGIN
		set @DBChangeSQL = 'create table ' + @NewFullSQLTableName + '(ID int identity)'
	  END
  END

select @ErrPos=2, @Err=@@error, @RowCount=@@RowCount
if @Err !=0 goto End_Routine

------------------------------------------------------------------------------------------
--  3) Update Table record
------------------------------------------------------------------------------------------
update	CiTable
set		TableName = @TableName,
		Caption = @Caption,
		DefaultView = @DefaultView,
		Bound = @Bound,
		SQLTableName = @NewSQLTableName,
		Src = @Src,
		ParentTableID = @ParentTableID
where	AppID = @AppID
and		TableID = @TableID

select @ErrPos=3, @Err=@@error, @RowCount=@@RowCount
if @Err !=0 goto End_Routine

------------------------------------------------------------------------------------------
--  4) Regenerate Table CRUD SQL statements
------------------------------------------------------------------------------------------
exec spTableSQL_build @AppID, @TableID

select @ErrPos=4, @Err=@@error, @RowCount=@@RowCount
if @Err !=0 goto End_Routine

------------------------------------------------------------------------------------------
--  5) Synchronise inherited tables
------------------------------------------------------------------------------------------
exec spTable_syncSource @AppID, @TableID, @Src, @OldSrc

select @ErrPos=5, @Err=@@error, @RowCount=@@RowCount
if @Err != 0 goto End_Routine

------------------------------------------------------------------------------------------
--  6) Update field caption of ParentID field
------------------------------------------------------------------------------------------
if @ParentTableID is not null
  BEGIN
	update	FC
	set		FC.Caption = FP.Caption,
			FC.Hidden = 0
	from	CiField FC
	join	CiField FP
	on		FP.AppID = FC.AppID
	and		FP.TableID = @ParentTableID
	and		FP.FieldName = 'ID'
	where	FC.AppID = @AppID
	and		FC.TableID = @TableID
	and		FC.FieldName = 'ParentID'
  END
else
  BEGIN
	update	CiField
	set		Caption = 'Parent ID',
			Hidden = 1
	where	AppID = @AppID
	and		TableID = @TableID
	and		FieldName = 'ParentID'
  END

select @ErrPos=6, @Err=@@error, @RowCount=@@RowCount
if @Err != 0 goto End_Routine

------------------------------------------------------------------------------------------
--  END ROUTINE
------------------------------------------------------------------------------------------
End_Routine:
if @Err = 0
  BEGIN
	if @TranCount = 0 COMMIT TRAN
	select @DBChangeSQL as DBChangeSQL
  END
else
  BEGIN
	Declare @ErrorMessage as nvarchar(50) = 'Application error ' + cast(@ErrPos as nvarchar(30))
	RAISERROR (@ErrorMessage,
		15,
		1)
	If @TranCount = 0 ROLLBACK TRAN
  END

return @Err

/****** Object: SqlProcedure [dbo].[spTable_del] Script Date: 9 Jul 2021 4:14:48 PM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

ALTER PROCEDURE [dbo].[spTable_del](
	@AppID int,
	@TableID int
)
/********1*********2*********3*********4*********5*********6*********7*********8*********9
** Delete Table records
*****************************************************************************************/
AS

------------------------------------------------------------------------------------------
--  BEGIN ROUTINE
------------------------------------------------------------------------------------------
Declare @TranCount 				int	= @@TranCount
Declare @ErrPos					int -- Place holder for location of SQL Error
Declare @Err 					int	-- Place holder for last SQL Error
Declare @RowCount 				int	-- Place holder for Last affected row count

if @TranCount = 0 BEGIN TRAN

------------------------------------------------------------------------------------------
--  Declare local variables
------------------------------------------------------------------------------------------

Declare @SQLDatabaseName		nvarchar(50)
Declare @DBChangeSQL			nvarchar(500)

------------------------------------------------------------------------------------------
--  1) Get SQL name of Database
------------------------------------------------------------------------------------------
select	@SQLDatabaseName = (case when UserDefined = 1 then 'U_' else '' end) + AppName
from	Application
where	AppID = @AppID

select @ErrPos=1, @Err=@@error, @RowCount=@@RowCount
if @Err !=0 goto End_Routine

------------------------------------------------------------------------------------------
--  2) Initialise DROP TABLE statement
------------------------------------------------------------------------------------------
select	@DBChangeSQL = 'drop Table ' + @SQLDatabaseName + '.dbo.' + SQLTableName
from	CiTable
where	AppID = @AppID
and		TableID = @TableID
and		Bound = 1
and		OBJECT_ID(@SQLDatabaseName + '.dbo.' + SQLTableName) is not null

select @ErrPos=2, @Err=@@error, @RowCount=@@RowCount
if @Err !=0 goto End_Routine

------------------------------------------------------------------------------------------
--  3) Delete Table record
------------------------------------------------------------------------------------------
delete from	CiTable
where	AppID = @AppID
and		TableID = @TableID

select @ErrPos=3, @Err=@@error, @RowCount=@@RowCount
if @Err !=0 goto End_Routine

------------------------------------------------------------------------------------------
--  END ROUTINE
------------------------------------------------------------------------------------------
End_Routine:
if @Err = 0
  BEGIN
	if @TranCount = 0 COMMIT TRAN
	select @DBChangeSQL as DBChangeSQL
  END
else
  BEGIN
	Declare @ErrorMessage as nvarchar(50) = 'Application error ' + cast(@ErrPos as nvarchar(30))
	RAISERROR (@ErrorMessage,
		15,
		1)
	If @TranCount = 0 ROLLBACK TRAN
  END

return @Err