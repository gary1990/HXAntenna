create procedure [dbo].[p_testrecordexcel_Rp]
	@testtimestart datetime2,
	@testtimestop datetime2,
	@serialnumber nvarchar(100)
as
begin
	SET NOCOUNT ON;

	DECLARE @testrecord TABLE
	(
		TestResultId int,
		TestTime datetime2(0),
		SerialNumber nvarchar(100),
		Result int,
		TestResultItemId int,
		JobNumber nvarchar(20),
		IsEsc int,
		IsLatestTest int,
		ResultItem int,
		TestItemName nvarchar(50),
		TestTimeItem datetime2(0),
		TestresultItemDegreeId int,
		Degree decimal(6,2),
		ResultItemDegree int,
		TestResultItemDegreeValId int,
		Port int,
		ResultItemDegreeVal int,
		TestData decimal(8,2),
		Symbol int,
		StandardValue decimal(8,2)
	)

	insert into @testrecord
	select
		a.Id as TestResultId,
		a.TestTime,
		b.Name as SerialNumber,
		a.Result,
		c.Id as TestResultItemId,
		e.JobNumber,
		c.IsEsc,
		c.IsLatestTest,
		c.ResultItem,
		d.Name as TestItemName,
		c.TestTimeItem,
		f.Id as TestresultItemDegreeId,
		f.Degree,
		f.ResultItemDegree,
		g.Id as TestResultItemDegreeValId,
		g.Port,
		g.ResultItemDegreeVal,
		g.TestData,
		h.Symbol,
		h.StandardValue
	from TestResult a
	join SerialNumber b
	on b.Id = a.SerialNumberId
	join TestResultItem c
	on c.TestResultId = a.Id
	join TestItem d
	on c.TestItemId = d.Id
	join AntennaUser e
	on c.AntennaUserId = e.Id
	join TestResultItemDegree f
	on f.TestResultItemId = c.Id
	join TestResultItemDegreeVal g
	on g.TestResultItemDegreeId = f.Id
	join TestStandard h
	on g.TestStandardId = h.Id
	where
	c.IsLatestTest = 1--各测试项最新上传的记录
	and a.TestTime >= @testtimestart
	and a.TestTime <= @testtimestop
	and b.Name like '%' + IsNull(@serialnumber,b.Name) + '%'


	DECLARE @testrecord_maxport TABLE
	(
		TestResultId int,
		TestTime datetime2(0),
		SerialNumber nvarchar(100),
		Result int,
		TestResultItemId int,
		JobNumber nvarchar(20),
		IsEsc int,
		IsLatestTest int,
		ResultItem int,
		TestItemName nvarchar(50),
		TestTimeItem datetime2(0),
		TestresultItemDegreeId int,
		Degree decimal(6,2),
		ResultItemDegree int,
		TestResultItemDegreeValId int,
		Port int,
		ResultItemDegreeVal int,
		TestData decimal(8,2),
		Symbol int,
		StandardValue decimal(8,2),
		maxport int,
		maxdegree int
	)

	insert into @testrecord_maxport
		select aa.*,bb.maxport,cc.maxdegree from @testrecord aa
		join
		(
			select distinct a.TestItemName,max(a.Port) as maxport
			from @testrecord a
			group by a.TestItemName
		) bb
		on aa.TestItemName = bb.TestItemName
		join
		(
			select aa.SerialNumber,COUNT(aa.Degree) as maxdegree
			from 
			(
			select distinct a.SerialNumber, a.Degree
			from @testrecord a
			) aa
			group by aa.SerialNumber
		) cc
		on aa.SerialNumber = cc.SerialNumber

	select * from @testrecord_maxport a order by a.TestTime desc

end