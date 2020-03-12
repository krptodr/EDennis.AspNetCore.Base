﻿use Hr123;
declare @ProjectName varchar(255) = 'Hr.RepoApi'
declare @ClassName varchar(255) = 'PersonController'
declare @MethodName varchar(255) = 'GetWithDynamicLinq'
declare @TestScenario varchar(255) = 'With Select'
declare @TestCase varchar(255) = 'B'

declare @ControllerPath varchar(255) = 'api/Person'
declare @Where varchar(255) = 'FirstName.Contains("h")'
declare @Select varchar(255) = 'new(FirstName,LastName)'
declare @OrderBy varchar(255) = 'LastName desc, FirstName desc'
declare @Skip int = 0
declare @Take int = 10

declare @currentPage int;
declare @pageCount int;
declare @pageSize int;
declare @rowCount int;

select @rowCount = count(*) from Person where FirstName like '%h%'
set @currentPage = 1 + ceiling(convert(decimal(10,2),@Skip)/@Take)
set @pageCount = ceiling(convert(decimal(10,2),@rowCount)/@Take)
set @pageSize = @Take

declare @ExpectedStatusCode int = 200 --Success

declare 
	@Expected varchar(max) = 
(
	select
		@currentPage [CurrentPage],
		@pageCount [PageCount],
		@pageSize [PageSize],
		@rowCount [RowCount],
		(
			select FirstName, LastName 
				from Person
				where FirstName like '%h%'
				order by LastName desc, FirstName desc
				offset @Skip rows fetch next @Take rows only
				for json path, include_null_values
		) Data
		
		for json path, without_array_wrapper
);


exec _.SaveTestJson @ProjectName, @ClassName, @MethodName, @TestScenario, @TestCase, 'ControllerPath', @ControllerPath
exec _.SaveTestJson @ProjectName, @ClassName, @MethodName, @TestScenario, @TestCase, 'Select', @Select
exec _.SaveTestJson @ProjectName, @ClassName, @MethodName, @TestScenario, @TestCase, 'Where', @Where
exec _.SaveTestJson @ProjectName, @ClassName, @MethodName, @TestScenario, @TestCase, 'OrderBy', @OrderBy
exec _.SaveTestJson @ProjectName, @ClassName, @MethodName, @TestScenario, @TestCase, 'Skip', @Skip
exec _.SaveTestJson @ProjectName, @ClassName, @MethodName, @TestScenario, @TestCase, 'Take', @Take
exec _.SaveTestJson @ProjectName, @ClassName, @MethodName, @TestScenario, @TestCase, 'Expected', @Expected
exec _.SaveTestJson @ProjectName, @ClassName, @MethodName, @TestScenario, @TestCase, 'ExpectedStatusCode', @ExpectedStatusCode
exec  _.GetTestJson @ProjectName, @ClassName, @MethodName, @TestScenario, @TestCase