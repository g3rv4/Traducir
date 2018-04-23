using System;
using SimpleMigrations;

namespace Traducir.Migrations.Migrations
{
    [Migration(1, "Create migration helper functions")]
    public class CreateHelperFunctions : Migration
    {
        protected override void Up()
        {
            // shamelessly stolen from
            // https://github.com/StackExchange/StackID/blob/master/OpenIdProvider/Migrations/000%20-%20Create%20Initial%20Tables.sql
            Execute(@"
    -- Create some migration helper functions

	-- drop functions if they exist
	IF OBJECT_ID('fnColumnExists') IS NOT NULL
	BEGIN
		 DROP FUNCTION fnColumnExists
	END

    IF OBJECT_ID('fnIndexExists') IS NOT NULL
	BEGIN
		 DROP FUNCTION fnIndexExists
	END

	IF OBJECT_ID('fnTableExists') IS NOT NULL
	BEGIN
		DROP FUNCTION fnTableExists
	END

	IF OBJECT_ID('fnConstraintExists') IS NOT NULL
	BEGIN
		DROP FUNCTION fnConstraintExists
	END");

    Execute(@"
	-- create fnColumnExists(table, column)
	CREATE FUNCTION fnColumnExists(
		@table_name nvarchar(max),
		@column_name nvarchar(max)
	)
	RETURNS bit
	BEGIN
		DECLARE @found bit
		SET @found = 0
		IF	EXISTS (
				SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS
				WHERE TABLE_NAME = @table_name AND COLUMN_NAME = @column_name )
		BEGIN
			SET @found = 1
		END


		RETURN @found
	END");

    Execute(@"
	-- create fnIndexExists(table, index)
	CREATE FUNCTION fnIndexExists(
		@table_name nvarchar(max),
		@index_name nvarchar(max)
	)
	RETURNS bit
	BEGIN
		DECLARE @found bit
		SET @found = 0
		IF	EXISTS (
				SELECT 1 FROM sys.indexes
				WHERE object_id = OBJECT_ID(@table_name) AND name = @index_name )
		BEGIN
			SET @found = 1
		END


		RETURN @found
	END");

    Execute(@"
	-- create fnTableExists(table)
	-- see: http://stackoverflow.com/questions/167576/sql-server-check-if-table-exists/167680#167680
	CREATE FUNCTION fnTableExists(
		@table_name nvarchar(max)
	)
	RETURNS bit
	BEGIN
		DECLARE @found bit
		SET @found = 0
		IF EXISTS (
			SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE
				TABLE_TYPE = 'BASE TABLE' AND
				TABLE_NAME = @table_name)
		BEGIN
			SET @found = 1
		END

		RETURN @found
	END");

    Execute(@"
	--create fnConstraintExists(table, constraint)
	CREATE FUNCTION fnConstraintExists(
		@table_name nvarchar(max),
		@constraint_name nvarchar(max)
	)
	RETURNS bit
	BEGIN
		DECLARE @found  bit
		SET @found = 0
		IF EXISTS (
			SELECT 1 FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS WHERE
				TABLE_NAME = @table_name AND
				CONSTRAINT_NAME = @constraint_name)
		BEGIN
			SET @found = 1
		END

		RETURN @found
	END;");
        }

        protected override void Down()
        {
            throw new NotImplementedException();
        }
    }
}