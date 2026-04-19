  IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'thirdparty')
      EXEC('CREATE SCHEMA [thirdparty]');

  CREATE TABLE [thirdparty].[VatNumberDetails] (
      [Id] INT NOT NULL IDENTITY(1,1) CONSTRAINT PK_VatNumberDetails PRIMARY KEY,
      [VatNumber] NVARCHAR(30) NOT NULL,
      [CountryCode] NVARCHAR(5) NULL, [Name] NVARCHAR(200) NULL,
      [Address] NVARCHAR(400) NULL, [Street] NVARCHAR(200) NULL,
      [PostalCode] NVARCHAR(20) NULL, [Place] NVARCHAR(100) NULL,
      [Number] NVARCHAR(20) NULL, [Box] NVARCHAR(20) NULL,
      [Province] NVARCHAR(100) NULL, [AddressLine] NVARCHAR(400) NULL,
      [IsValid] BIT NOT NULL DEFAULT(0),
      [LastUpdated] DATETIME2 NOT NULL DEFAULT(SYSDATETIME()),
      CONSTRAINT UQ_VatNumberDetails_VatNumber UNIQUE ([VatNumber])
  );
