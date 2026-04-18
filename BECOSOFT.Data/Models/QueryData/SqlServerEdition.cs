using BECOSOFT.Utilities.Attributes;

namespace BECOSOFT.Data.Models.QueryData {
    public enum SqlServerEdition {
        Unknown,

        /// <summary>
        /// Enterprise Edition
        /// </summary>
        [Code("Enterprise Edition")]
        Enterprise = 1804890536,

        /// <summary>
        /// Enterprise Edition: Core-based Licensing
        /// </summary>
        [Code("Enterprise Edition: Core-based Licensing")]
        EnterpriseWithCorebasedLicensing = 1872460670,

        /// <summary>
        /// Enterprise Evaluation Edition
        /// </summary>
        [Code("Enterprise Evaluation Edition")]
        EnterpriseEvaluation = 610778273,

        /// <summary>
        /// Business Intelligence Edition
        /// </summary>
        [Code("Business Intelligence Edition")]
        BusinessIntelligence = 284895786,

        /// <summary>
        /// Developer Edition
        /// </summary>
        [Code("Developer Edition")]
        Developer = -2117995310,

        /// <summary>
        /// 'Express Edition
        /// </summary>
        [Code("Express Edition")]
        Express = -1592396055,

        /// <summary>
        /// Express Edition with Advanced Services
        /// </summary>
        [Code("Express Edition with Advanced Services")]
        ExpressWithAdvancedServices = -133711905,

        /// <summary>
        /// Standard Edition
        /// </summary>
        [Code("Standard Edition")]
        Standard = -1534726760,

        /// <summary>
        /// Web Edition
        /// </summary>
        [Code("Web Edition")]
        Web = 1293598313,

        /// <summary>
        /// SQL Azure (indicates SQL Database or Azure Synapse Analytics)
        /// </summary>
        [Code("SQL Azure")]
        SQLAzure = 1674378470,

        /// <summary>
        /// Azure SQL Edge Developer (indicates the development only edition for Azure SQL Edge)
        /// </summary>
        [Code("Azure SQL Edge Developer")]
        AzureSQLEdgeDeveloper = -1461570097,

        /// <summary>
        /// Azure SQL Edge (indicates the paid edition for Azure SQL Edge)
        /// </summary>
        [Code("Azure SQL Edge")]
        AzureSQLEdge = 1994083197,
    }
}