﻿SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetLastModified] 
AS
BEGIN
	SET NOCOUNT ON;
	select ProjectName, max(SysStart) LastModified
		from ProjectSetting ps 
		group by ProjectName
END