﻿use hr;

declare @id int = 2

begin transaction
delete from EmployeePosition where EmployeeId = @id;
delete from Employee where Id = @id;

declare 
	@expected varchar(max) = 
(
	select * from Employee
	for json path, include_null_values
);

rollback transaction
exec _maintenance.SaveTestJson 'EDennis.Samples.Hr.InternalApi1', 'EmployeeRepo', 'Delete','DeleteAndGetMultiple',@Id,'Id', @Id
exec _maintenance.SaveTestJson 'EDennis.Samples.Hr.InternalApi1', 'EmployeeRepo', 'Delete','DeleteAndGetMultiple',@Id,'Expected', @expected

exec  _maintenance.GetTestJson 'EDennis.Samples.Hr.InternalApi1', 'EmployeeRepo', 'Delete','DeleteAndGetMultiple',@Id