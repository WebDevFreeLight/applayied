USE [AGHKHO_HISDemo]
GO
/****** Object:  StoredProcedure [dbo].[Sproc_prg_searchforpatient_AllLocations]    Script Date: 20/10/2026 12:06:12 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Sproc_prg_searchforpatient_AllLocations @National_ID_No ='2260467655'
declare                                                 @Registration_no              CHAR(10) = '',
                                                        @Full_Name                    VARCHAR(100) = '',
                                                        @First_Name                   VARCHAR(20) = '',
                                                        @Family_Name                  VARCHAR(20) = '',
                                                        @Father_Name                  VARCHAR(20) = '',
                                                        @National_ID_No               VARCHAR(20) = '',
                                                        @Company_ID_No                VARCHAR(15) = '',
                                                        @Insurance_ID_No              VARCHAR(100) = '',
                                                        @Old_National_ID_No           VARCHAR(20) = '',
                                                        @Company_Name                 VARCHAR(100) = '',
                                                        @Mobile                       VARCHAR(15) = '',
                                                        @DateOfBirth                  VARCHAR(100) = NULL,
                                                        @Passport_No                  VARCHAR(15) = '',
                                                        @Status                       VARCHAR(10)=NULL,
                                                        @Status_Not_Equal             VARCHAR(10)=NULL,
                                                        @Insurance_Administrator_code VARCHAR(100)=NULL,
                                                        @Insurance_code               VARCHAR(100)=NULL,
                                                        @Company_code                 VARCHAR(100)=NULL,
                                                        @Insurance_Name               VARCHAR(100)=NULL,
                                                        @Insurance_Administrator_Name VARCHAR(100)=NULL,
                                                        @GOSI_No                      VARCHAR(100)=NULL,
                                                        @Aramco_MRNo                  VARCHAR(100)=NULL,
                                                        @Residence_PH                 VARCHAR(32)=NULL,
                                                        @Card_Code                    VARCHAR(32)=NULL,
                                                        @Encoded_From                 VARCHAR(10)='20-10-2026', -- dd-mm-yyyy
                                                        @Encoded_To                   VARCHAR(10)='20-10-2026'  -- dd-mm-yyyy

    SET DATEFORMAT DMY

    DECLARE @Cmd NVARCHAR(MAX)
    DECLARE @Where VARCHAR(MAX)
    DECLARE @InnerWhere VARCHAR(MAX) -- filters on PR.* columns; applied inside the derived table where alias PR is visible

    SET @Where=''
    SET @InnerWhere=''

    IF Isnull(@Encoded_From, '') <> ''
      SET @InnerWhere = @InnerWhere + ' and ( convert(date,PR.Encoded_Date) >= convert(date, '''
                        + @Encoded_From + ''',103))'

    IF Isnull(@Encoded_To, '') <> ''
      SET @InnerWhere = @InnerWhere + ' and ( convert(date,PR.Encoded_Date) <= convert(date, '''
                        + @Encoded_To + ''',103))'


    IF ( @Registration_no <> '' )
      SET @where = @where + ' and (Registration_no LIKE '''
                   + Rtrim(@Registration_no) + '%'')'

    IF ( @First_Name <> '' )
      SET @where = @where + ' AND (First_Name LIKE '''
                   + Rtrim(@First_Name) + '%'')'

    IF ( @Father_Name <> '' )
      SET @where = @where + ' AND (Father_Name LIKE '''
                   + Rtrim(@Father_Name) + '%'')'

    IF ( @Full_Name <> '' )
      SET @where = @where + ' AND ( FullName LIKE '''
                   + Rtrim(@Full_Name) + '%'')'

    IF ( @Company_Name <> '' )
      SET @where = @where + ' AND (  Company_Name LIKE '''
                   + Rtrim(@Company_Name) + '%'')'

    IF ( @Family_Name <> '' )
      SET @where = @where + ' AND(Family_Name LIKE '''
                   + Rtrim(@Family_Name) + '%'')'

    IF Isnull(@Insurance_Administrator_Name, '') <> ''
       AND @Insurance_Administrator_Name <> ''
      BEGIN
          SET @where = @where + ' AND English_Name LIKE ''%'
                       + @Insurance_Administrator_Name + '%'''
      END

    IF Isnull(@Insurance_Name, '') <> ''
       AND @Insurance_Name <> ''
      BEGIN
          SET @where = @where + ' AND Insurance_Name LIKE '''
                       + @Insurance_Name + '%'''
      END

    IF Isnull(@Insurance_Code, '') <> ''
       AND @Insurance_Code <> ''
      BEGIN
          SET @where = @where + ' AND Insurance_Code= '''
                       + @Insurance_Code + ''''
      END

    IF Isnull(@Company_code, '') <> ''
       AND @Company_code <> ''
      BEGIN
          SET @where = @where + ' AND Company_code= '''
                       + @Company_code + ''''
      END

    IF Isnull(@Aramco_MRNo, '') <> ''
       AND @Aramco_MRNo <> ''
      BEGIN
          SET @where = @where
                       + ' AND  ltrim(rtrim(Aramco_MRNO))= ltrim(rtrim('''
                       + @Aramco_MRNo + '''))'
      END

    IF Isnull(@Card_Code, '') <> ''
       AND @Card_Code <> ''
      BEGIN
          SET @where = @where + ' AND  Card_Code= ''' + @Card_Code
                       + ''''
      END

    ----
    ------	
    IF ( @Old_National_ID_No <> '' )
      SET @where = @where
                   + ' AND ( Old_National_ID_No LIKE ''%'
                   + Rtrim(@Old_National_ID_No) + '%'')'

    IF Isnull(@GOSI_No, '') <> ''
       AND @GOSI_No <> ''
      BEGIN
          SET @where = @where + ' AND  GOSI_No= ''' + @GOSI_No + ''''
      END

    IF ( @Insurance_Administrator_Code IS NOT NULL
         AND @Insurance_Administrator_Code <> '' )
      SET @where = @where
                   + ' AND   Insurance_Administrator_Code = '''
                   + Rtrim(@Insurance_Administrator_Code) + ''''

    IF ( @National_ID_No <> '' )
      SET @where = @where + ' AND (National_ID_No LIKE ''%'
                   + Rtrim(@National_ID_No) + '%'')'

    IF ( @Company_ID_No <> '' )
      SET @where = @where + ' AND(Company_ID_No = '''
                   + Rtrim(@Company_ID_No) + ' '')'

    IF ( @Insurance_ID_No <> '' )
      SET @where = @where + ' AND( Insurance_ID_No = '''
                   + Rtrim(@Insurance_ID_No) + ' '')'

    IF ( @Mobile <> '' )
      SET @where = @where + 'AND (Mobile LIKE ''%'
                   + Rtrim(@Mobile) + '%'')'

    IF ( @Residence_PH <> '' )
      SET @where = @where + 'AND (Residence_PH = '''
                   + Rtrim(@Residence_PH) + ''')'

    IF ( @Passport_No <> '' )
      SET @where = @where + 'AND (Passport_No LIKE ''%'
                   + Rtrim(@Passport_No) + '%'')'

    IF ( @Status IS NOT NULL )
      SET @where = @where + 'AND   Status=''' + @Status + ''''

    IF ( @Status_Not_Equal IS NOT NULL )
      SET @where = @where + 'AND   Status<>'''
                   + @Status_Not_Equal + ''''

    IF ( @DateOfBirth IS NOT NULL
         AND @DateOfBirth <> '' )
      SET @where = @where
                   + 'AND (Convert(varchar(10),Date_Of_Birth,103)='''
                   + Rtrim(CONVERT(VARCHAR(10), @DateOfBirth, 103))
                   + ''')'

    IF Object_id('tempdb..#Locations') IS NOT NULL
      DROP TABLE #Locations

    CREATE TABLE #Locations
      (
         rowID                INT IDENTITY(1, 1),
         QVW_PRG_DatabaseName VARCHAR(100),
         Location_Code        VARCHAR(3),
         Short_Location_Name  VARCHAR(50)
      )

    IF Object_id('tempdb..#Files') IS NOT NULL
      DROP TABLE #Files

    CREATE TABLE #Files
      (
         Location_Code                VARCHAR(3),
         Short_Location_Name          VARCHAR(50),
         Old_registration_no          VARCHAR(10),
         registration_no              VARCHAR(10),
         centRegistration_no          VARCHAR(10),
		 MappedTo          VARCHAR(10),
         first_name                   VARCHAR(50),
         Father_name                  VARCHAR(50),
         family_name                  VARCHAR(50),
         Grand_Father_Name            VARCHAR(50),
         FullName                     VARCHAR(250),
         date_of_birth                VARCHAR(10),
         Company_Name                 NVARCHAR(200),
         Kin_Description              NVARCHAR(200),
         Nationality_Name             NVARCHAR(200),
         National_ID_No               NVARCHAR(20),
         Old_National_ID_No           NVARCHAR(20),
         Company_ID_No                NVARCHAR(20),
         Insurance_ID_No              NVARCHAR(50),
         Card_Code                    NVARCHAR(50),
         Mobile                       NVARCHAR(25),
         Passport_No                  NVARCHAR(25),
         Status                       NVARCHAR(25),
         Sex                          NVARCHAR(25),
         Aramco_MRNO                  NVARCHAR(25),
         BloodGroup                   NVARCHAR(25),
         Gosi_no                      NVARCHAR(25),
         Residence_PH                 NVARCHAR(25),
         StatusText                   NVARCHAR(25),
         Insurance_Administrator_Code NVARCHAR(50),
         Insurance_Administrator_Name NVARCHAR(250),
         Insurance_Name               NVARCHAR(250),
         Insurance_Coden              INT,
         Company_Code                 INT,
         ARFullName                   NVARCHAR(250),
         EmailId                      NVARCHAR(250),
         Address                      NVARCHAR(250),
         Age                          NVARCHAR(250),
		 IndexUpdated				   bit,
         Nationality_ID_Limit         INT,
         English_Name                 NVARCHAR(250),
		 isIndexed						int,
		 DataVerified  bit,
		 Encoded_Date				   DATETIME,
		 Seq						   INT IDENTITY(1, 1) -- insert order: first branch gets the lowest Seq
      )

    INSERT INTO #Locations
    SELECT QVW_PRG_DatabaseName,
           Location_Code,
           Short_Location_Name
    FROM   tbl_MST_Hospital_Locations WITH(NOLOCK)
    WHERE  Isnull(QVW_PRG_DatabaseName, '') <> ''
		AND Location_Code <> (SELECT Location_Code FROM tbl_MST_Hospital_Locations WHERE Status = 1 )

    DECLARE @DetailCnt INT = @@ROWCOUNT -- Get the Count for the loop  
    DECLARE @DetailCntr INT = 1
    DECLARE @SQL NVARCHAR(MAX)
    DECLARE @base NVARCHAR(100)
    DECLARE @Short_Location_Name VARCHAR(50)
	DECLARE @Location_Code        VARCHAR(3) 


   SET @SQL =' INSERT INTO #Files  '
    SET @SQL =@SQL + ' Select * from (SELECT pr.Location_Code  as Location_Code,loc.Short_Location_Name   as Location_Name, PR.Old_registration_no   , PR.registration_no , 
                                        i.Registration_no as centRegistration_no,    
										 dbo.fn_GetMappedFile(PR.registration_no   ) as mappedto,
										PR.first_name ,
										PR.Father_name, 
										PR.family_name, 
										PR.Grand_Father_Name,
										PR.first_name +'' ''+PR.Father_name+'' '' + PR.Grand_Father_Name +'' ''+ PR.family_name as FullName,
										convert(varchar,PR.date_of_birth,103) as date_of_birth , 
										--(SELECT English_Name FROM tbl_CMT_Company  where company_Code = PR.Company_Code)  as Company_Name,
										COM.English_Name as Company_Name,
										--(SELECT lookuptext from tbl_mst_lookupmaster where Lookupvalue=PR.relation and LookupTypeid=''2'' )  as Kin_Description,
										LM.lookuptext as Kin_Description,
										--dbo.fx_getNationality(PR.Nationality_ID) as Nationality_Name,
										CM.Nationality_Name,
										PR.National_ID_No, PR.Old_National_ID_No,
										PR.Company_ID_No,
										PR.Insurance_ID_No,
										pr.Card_Code,
										PR.Mobile,
										PR.Passport_No ,										
										PR.Status,
										CASE WHEN pr.Sex = ''M'' THEN ''Male'' ELSE ''Female'' END AS Sex,
										 pr.Aramco_MRNO,
										 pr.BloodGroup,
										 pr.Gosi_no,
										 pr.Residence_PH,
										CASE pr.Status 
											WHEN ''A'' THEN ''ACTIVE''
											WHEN ''C'' THEN ''IN-ACTIVE''
											WHEN ''X'' THEN ''EXPIRED''
											WHEN ''I'' THEN ''IN PATIENT''
										END AS StatusText,
										CA.Insurance_Administrator_Code,
										CA.English_Name AS Insurance_Administrator_Name,
										INS.English_name as Insurance_Name,
										COM.Insurance_Code,
										COM.Company_Code,
										ltrim(rtrim(isnull(PR.Local_First_Name,'''') +'' ''+ isnull(PR.Local_Father_Name,'''')+'' ''
										+ isnull(PR.Local_Grand_Father_Name,'''') +'' ''+isnull(PR.Local_Family_Name,''''))) as ARFullName 
										,pr.Email_Address as [EmailId]
										,	pr.Local_Address1 as [Address]	
										,dbo.fn_PRG_ComputeAge_WithWords_VOLD(pr.date_of_birth) as Age 
										,isnull(pr.IndexUpdated,''0'') as IndexUpdated ,isnull( CM. Nationality_ID_Limit,10) as Nationality_ID_Limit
										,COM.English_Name,dbo.fn_PRG_IsFileIndexed(PR.registration_no ) as isIndexed
										, isnull( PR.DataVerified,0) as DataVerified
										, PR.Encoded_Date
				FROM    dbo.tbl_PRG_Patient_Registration AS PR
				INNER JOIN dbo.tbl_CMT_Company COM WITH(NOLOCK) ON COM.Company_Code = PR.Company_Code
				INNER JOIN tbl_MST_Hospital_Locations loc WITH(NOLOCK)  on PR.Location_Code = loc.Location_Code
				Left JOIN dbo.tbl_CMT_INSURANCE INS WITH(NOLOCK)  ON INS.Insurance_Code = COM.Insurance_Code
				LEFT JOIN tbl_CMT_Insurance_Administrator CA WITH(NOLOCK)  ON CA.Insurance_Administrator_code = COM.Insurance_Administrator_code
				left join 	tbl_mst_lookupmaster  LM WITH(NOLOCK)  on  LM.Lookupvalue=PR.relation and LM.LookupTypeid=''2''
				left join tbl_MST_Country_Master CM WITH(NOLOCK)  on CM.Country_Code=PR.Nationality_Id	
				left join tbl_PRG_Patient_Index i on i.Registration_no = pr.Registration_no
				where 1=1 ' + @InnerWhere + '
				) as tbl  where 1=1 ' + @where
	PRINT @SQL
    EXEC Sp_executesql    @SQL



    WHILE @DetailCntr <= @DetailCnt
      BEGIN
          SET @base =NULL

          SELECT @base = QVW_PRG_DatabaseName,
                 @Short_Location_Name = Short_Location_Name,
				 @Location_Code=Location_Code
          FROM   #Locations
          WHERE  rowID = @DetailCntr
  print @base
    SET @SQL =' INSERT INTO #Files  '
    SET @SQL =@SQL + ' Select * from (SELECT ''' +  @Location_Code + ''' as Location_Code,''' + @Short_Location_Name  + ''' as Location_Name, PR.Old_registration_no   , PR.registration_no , 
                                        i.Registration_no as centRegistration_no,  ' + @base
              + '.dbo.fn_GetMappedFile(PR.registration_no   ) as mappedto,

										PR.first_name ,
										PR.Father_name, 
										PR.family_name, 
										PR.Grand_Father_Name,
										PR.first_name +'' ''+ PR.family_name as FullName,
										convert(varchar,PR.date_of_birth,103) as date_of_birth , 
										--(SELECT English_Name FROM tbl_CMT_Company  where company_Code = PR.Company_Code)  as Company_Name,
										COM.English_Name as Company_Name,
										--(SELECT lookuptext from tbl_mst_lookupmaster where Lookupvalue=PR.relation and LookupTypeid=''2'' )  as Kin_Description,
										LM.lookuptext as Kin_Description,
										--dbo.fx_getNationality(PR.Nationality_ID) as Nationality_Name,
										CM.Nationality_Name,
										PR.National_ID_No, PR.Old_National_ID_No,
										PR.Company_ID_No,
										PR.Insurance_ID_No,
										pr.Card_Code,
										PR.Mobile,
										PR.Passport_No ,										
										PR.Status,
										CASE WHEN pr.Sex = ''M'' THEN ''Male'' ELSE ''Female'' END AS Sex,
										 pr.Aramco_MRNO,
										 pr.BloodGroup,
										 pr.Gosi_no,
										 pr.Residence_PH,
										CASE pr.Status 
											WHEN ''A'' THEN ''ACTIVE''
											WHEN ''C'' THEN ''IN-ACTIVE''
											WHEN ''X'' THEN ''EXPIRED''
											WHEN ''I'' THEN ''IN PATIENT''
										END AS StatusText,
										CA.Insurance_Administrator_Code,
										CA.English_Name AS Insurance_Administrator_Name,
										INS.English_name as Insurance_Name,
										COM.Insurance_Code,
										COM.Company_Code,
										ltrim(rtrim(isnull(PR.Local_First_Name,'''') +'' ''+ isnull(PR.Local_Father_Name,'''')+'' ''
										+ isnull(PR.Local_Grand_Father_Name,'''') +'' ''+isnull(PR.Local_Family_Name,''''))) as ARFullName 
										,pr.Email_Address as [EmailId]
										,	pr.Local_Address1 as [Address]	
										,dbo.fn_PRG_ComputeAge_WithWords_VOLD(pr.date_of_birth) as Age 
										,isnull(pr.IndexUpdated,''0'') as IndexUpdated ,isnull( CM. Nationality_ID_Limit,10) as Nationality_ID_Limit
										,COM.English_Name,' + @base  + '.dbo.fn_PRG_IsFileIndexed(PR.registration_no ) as isIndexed
										, isnull( PR.DataVerified,0) as DataVerified
										, PR.Encoded_Date
				FROM ' + @base + '.dbo.tbl_PRG_Patient_Registration AS PR
				LEFT OUTER JOIN dbo.tbl_CMT_Company COM WITH(NOLOCK)  ON COM.Company_Code = PR.Company_Code
				Left JOIN dbo.tbl_CMT_INSURANCE INS WITH(NOLOCK)  ON INS.Insurance_Code = COM.Insurance_Code
				LEFT JOIN tbl_CMT_Insurance_Administrator CA WITH(NOLOCK)  ON CA.Insurance_Administrator_code = COM.Insurance_Administrator_code
				left join 	tbl_mst_lookupmaster  LM WITH(NOLOCK)  on  LM.Lookupvalue=PR.relation and LM.LookupTypeid=''2''
				left join tbl_MST_Country_Master CM WITH(NOLOCK)  on CM.Country_Code=PR.Nationality_Id	
				left join dbo.tbl_PRG_Patient_Index i on i.Registration_no = pr.Registration_no
				where 1=1 ' + @InnerWhere + '
				) as tbl  where 1=1 ' + @where

    PRINT @SQL

    EXEC Sp_executesql    @SQL
          SET @DetailCntr = @DetailCntr + 1
      END


    -- where else do the found files exist? checked on ALL dates (no @InnerWhere / @where here)
    IF Object_id('tempdb..#FileLocations') IS NOT NULL
      DROP TABLE #FileLocations

    CREATE TABLE #FileLocations
      (
         registration_no VARCHAR(10) COLLATE DATABASE_DEFAULT,
         Location_Code   VARCHAR(3) COLLATE DATABASE_DEFAULT
      )

    INSERT INTO #FileLocations
    SELECT DISTINCT PR.registration_no,
                    PR.Location_Code
    FROM   dbo.tbl_PRG_Patient_Registration AS PR WITH(NOLOCK)
    WHERE  EXISTS (SELECT 1
                   FROM   #Files F
                   WHERE  F.registration_no COLLATE DATABASE_DEFAULT = PR.registration_no)

    SET @DetailCntr = 1

    WHILE @DetailCntr <= @DetailCnt
      BEGIN
          SELECT @base = QVW_PRG_DatabaseName,
                 @Location_Code = Location_Code
          FROM   #Locations
          WHERE  rowID = @DetailCntr

          SET @SQL = ' INSERT INTO #FileLocations
                       SELECT DISTINCT PR.registration_no, ''' + @Location_Code + '''
                       FROM   ' + @base + '.dbo.tbl_PRG_Patient_Registration AS PR WITH(NOLOCK)
                       WHERE  EXISTS (SELECT 1
                                      FROM   #Files F
                                      WHERE  F.registration_no COLLATE DATABASE_DEFAULT = PR.registration_no)'

          EXEC Sp_executesql @SQL

          SET @DetailCntr = @DetailCntr + 1
      END

    -- drop every file number that exists in more than one location (any Encoded_Date)
    DELETE FROM #Files
    WHERE  registration_no IN (SELECT registration_no
                               FROM   #FileLocations
                               GROUP  BY registration_no
                               HAVING COUNT(DISTINCT Location_Code) > 1)

    -- a single location can still return the same file twice (e.g. join fan-out): keep one row
    ;WITH Dups AS
      (
         SELECT ROW_NUMBER() OVER (PARTITION BY registration_no ORDER BY Encoded_Date, Seq) AS rn
         FROM   #Files
      )
    DELETE FROM Dups
    WHERE  rn > 1

    ALTER TABLE #Files DROP COLUMN Seq

    -- one result set per location
    DECLARE @OutLocations TABLE
      (
         rowID         INT IDENTITY(1, 1),
         Location_Code VARCHAR(3)
      )

    INSERT INTO @OutLocations (Location_Code)
    SELECT DISTINCT Location_Code
    FROM   #Files
    ORDER  BY Location_Code

    DECLARE @OutCnt INT = @@ROWCOUNT
    DECLARE @OutCntr INT = 1

    WHILE @OutCntr <= @OutCnt
      BEGIN
          SELECT @Location_Code = Location_Code
          FROM   @OutLocations
          WHERE  rowID = @OutCntr

          SELECT *
          FROM   #Files
          WHERE  Location_Code = @Location_Code
          ORDER  BY Encoded_Date

          SET @OutCntr = @OutCntr + 1
      END

    -- how many files per location
    SELECT Location_Code,
           Short_Location_Name,
           COUNT(*) AS Files_Count
    FROM   #Files
    GROUP  BY Location_Code,
              Short_Location_Name
    ORDER  BY Files_Count DESC


    DROP TABLE #Locations

    DROP TABLE #Files

    DROP TABLE #FileLocations

