﻿use AgencyOnlineCheck;

declare @EmployeeId int = 2
declare 
	@Input varchar(max) = 
( 
	select
	@EmployeeId EmployeeId,
	'Fail' Status,
	'2018-12-02' DateCompleted
		for json path, include_null_values, without_array_wrapper
);

begin transaction
insert into AgencyOnlineCheck(EmployeeId, Status, DateCompleted)
	select
	@EmployeeId EmployeeId,
	'Fail' Status,
	'2018-12-02' DateCompleted

declare 
@Expected varchar(max) = 
(
	select * from AgencyOnlineCheck
	for json path, include_null_values
);

rollback transaction
exec _maintenance.ResetIdentities

exec _maintenance.SaveTestJson 'EDennis.Samples.Hr.InternalApi2', 'AgencyOnlineController', 'Post', 'PostAndGetMultiple',@EmployeeId,'Input', @Input
exec _maintenance.SaveTestJson 'EDennis.Samples.Hr.InternalApi2', 'AgencyOnlineController', 'Post', 'PostAndGetMultiple',@EmployeeId,'Expected', @Expected

exec _maintenance.GetTestJson 'EDennis.Samples.Hr.InternalApi2', 'AgencyOnlineController', 'Post', 'PostAndGetMultiple', @EmployeeId
		