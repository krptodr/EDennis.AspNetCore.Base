﻿use Color2Db;
declare @ProjectName varchar(255) = 'Colors2Api'
declare @ClassName varchar(255) = 'RgbController'
declare @MethodName varchar(255) = 'Delete'
declare @TestScenario varchar(255) = 'No Content'
declare @TestCase varchar(255) = 'B'
declare @LinqWhere varchar(255) = 'Id ge -999148 and Id le -999143'

declare @ControllerPath varchar(255) = 'api/Rgb'
declare @TargetId int = -999146
declare @ExpectedStatusCode int = 204 --No Content

begin transaction

declare @Expected varchar(max) = 
(
	select * from Rgb 
		where Id between -999148 and -999143
			and Id <> @TargetId
	for json path
);

rollback transaction
--exec _.ResetSequences --only needed if no explicit Ids are provided

exec _.SaveTestJson @ProjectName, @ClassName, @MethodName,@TestScenario,@TestCase,'ControllerPath', @ControllerPath
exec _.SaveTestJson @ProjectName, @ClassName, @MethodName, @TestScenario, @TestCase, 'Id', @TargetId
exec _.SaveTestJson @ProjectName, @ClassName, @MethodName, @TestScenario, @TestCase, 'Expected', @Expected
exec _.SaveTestJson @ProjectName, @ClassName, @MethodName, @TestScenario, @TestCase, 'ExpectedStatusCode', @ExpectedStatusCode
exec _.SaveTestJson @ProjectName, @ClassName, @MethodName, @TestScenario, @TestCase, 'LinqWhere', @LinqWhere

exec _.GetTestJson @ProjectName, @ClassName, @MethodName, @TestScenario, @TestCase