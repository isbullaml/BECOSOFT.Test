using BECOSOFT.Data.Migrator;
using BECOSOFT.ThirdParty.Migrations.Attributes;
using NLog;

namespace BECOSOFT.ThirdParty.Migrations.VatNumberDetails {
    [VatNumberDetailsMigration(version: 1, information: "Create VatNumberDetails table")]
    public class Migration_001_CreateVatNumberDetailsTable : BaseMigration<ThirdPartyMigrationType> {
        public Migration_001_CreateVatNumberDetailsTable() : base(LogManager.GetCurrentClassLogger()) { }

        protected override void Upgrade() {
            Execute(@"
                IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE object_id = OBJECT_ID('[thirdparty].[VatNumberDetails]') AND type = N'U')
                BEGIN
                    EXECUTE ('
                        CREATE TABLE [thirdparty].[VatNumberDetails] (
                            [Id]          INT           NOT NULL IDENTITY(1,1) CONSTRAINT PK_VatNumberDetails PRIMARY KEY,
                            [VatNumber]   NVARCHAR(30)  NOT NULL,
                            [CountryCode] NVARCHAR(5)   NULL,
                            [Name]        NVARCHAR(200) NULL,
                            [Address]     NVARCHAR(400) NULL,
                            [Street]      NVARCHAR(200) NULL,
                            [PostalCode]  NVARCHAR(20)  NULL,
                            [Place]       NVARCHAR(100) NULL,
                            [Number]      NVARCHAR(20)  NULL,
                            [Box]         NVARCHAR(20)  NULL,
                            [Province]    NVARCHAR(100) NULL,
                            [AddressLine] NVARCHAR(400) NULL,
                            [IsValid]     BIT           NOT NULL DEFAULT(0),
                            [LastUpdated] DATETIME2     NOT NULL DEFAULT(SYSDATETIME()),
                            CONSTRAINT UQ_VatNumberDetails_VatNumber UNIQUE ([VatNumber])
                        )
                    ');
                END
            ");
        }

        public override TableConsumingMigrationTableInfo GetTableInfo() => null;
    }
}
