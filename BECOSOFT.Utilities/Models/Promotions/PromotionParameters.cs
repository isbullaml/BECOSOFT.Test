using BECOSOFT.Utilities.Models.Prices;
using BECOSOFT.Utilities.Promotions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BECOSOFT.Utilities.Models.Promotions {
    /// <summary>
    /// This parameter object contains the various parameters, necessary to be able to process a Promotion request.
    /// </summary>
    public class PromotionParameters : BasePromotionParameters {

        /// <summary>
        /// A <see cref="List{T}"/> of <see cref="ArticleData"/> objects.
        /// </summary>
        public List<ArticleData> Data { get; set; } = new List<ArticleData>();


        public PromotionParameters(List<ArticleData> data) {
            Data = data;
        }

        public PromotionParameters() {
        }

        /// <summary>
        /// Create a clone of the current <see cref="PromotionParameters"/>, with new <paramref name="articleData"/>.
        /// </summary>
        /// <param name="articleData"></param>
        /// <returns></returns>
        public PromotionParameters Clone(List<ArticleData> articleData) {
            var pp = new PromotionParameters(articleData) {
                PromotionDate = PromotionDate,
                DocumentTypeID = DocumentTypeID,
                ContactID = ContactID,
                WarehouseID = WarehouseID,
                IsActiveOnWebshop = IsActiveOnWebshop,
                PromotionArticleContainer = PromotionArticleContainer,
                ActivatedPromotionIDs = ActivatedPromotionIDs,
                BarcodeActivatedPromotionIDs = BarcodeActivatedPromotionIDs,
                CountryID = CountryID,
                FilterArticles = FilterArticles,
                ChooseBestPromotion = ChooseBestPromotion,
            };
            pp.SetPriceContainer(ArticlePrices);
            return pp;
        }
    }
    /// <summary>
    /// This parameter object contains the various parameters, necessary to be able to process multiple Promotion requests.
    /// </summary>
    public class MultiPromotionParameters : BasePromotionParameters {

        /// <summary>
        /// A <see cref="List{T}"/> of <see cref="ArticleData"/> objects.
        /// </summary>
        public List<List<ArticleData>> Data { get; set; } = new List<List<ArticleData>>();


        public MultiPromotionParameters(List<List<ArticleData>> data) {
            Data = data;
        }

        public MultiPromotionParameters() {
        }
        
        public List<PromotionParameters> ToParameters() {
            return Data.Select(ConvertToParamers).ToList();

            PromotionParameters ConvertToParamers(List<ArticleData> data) {
                var pp = new PromotionParameters(data) {
                    PromotionDate = PromotionDate,
                    DocumentTypeID = DocumentTypeID,
                    ContactID = ContactID,
                    WarehouseID = WarehouseID,
                    IsActiveOnWebshop = IsActiveOnWebshop,
                    PromotionArticleContainer = PromotionArticleContainer,
                    ActivatedPromotionIDs = ActivatedPromotionIDs,
                    BarcodeActivatedPromotionIDs = BarcodeActivatedPromotionIDs,
                    CountryID = CountryID,
                    FilterArticles = FilterArticles,
                    ChooseBestPromotion = ChooseBestPromotion,
                    PriceLogic = PriceLogic,
                };
                pp.SetPriceContainer(ArticlePrices);
                return pp;
            }
        }
    }

    public abstract class BasePromotionParameters {
        /// <summary>
        /// Filter promotions that are active on <see cref="PromotionDate"/>.
        /// The <see cref="DayOfWeek"/> property on <see cref="PromotionDate"/> is used to determine whether is is contained in the <see cref="PromotionWrapper"/>.<see cref="PromotionWrapper.ActiveDays"/> list.
        /// </summary>
        public DateTime PromotionDate { get; set; } = DateTime.Now;

        /// <summary>
        /// Document type ID for which to process the active promotions
        /// </summary>
        public int DocumentTypeID { get; set; }

        /// <summary>
        /// Contact ID for which to filter the active promotions
        /// </summary>
        public int ContactID { get; set; }

        /// <summary>
        /// Warehouse ID where the promotions are active for
        /// </summary>
        public int WarehouseID { get; set; }

        /// <summary>
        /// Depending on the value of <see cref="IsActiveOnWebshop"/>, promotions active on webshop are also checked.
        /// </summary>
        public bool IsActiveOnWebshop { get; set; }

        /// <summary>
        /// A <see cref="List{T}"/> of promotions IDs that should be ignored
        /// </summary>
        public List<int> PromotionIDsToIgnore { get; set; } = new List<int>(0);

        /// <summary>
        /// A <see cref="List{T}"/> of promotions IDs that have been activated by a voucher
        /// </summary>
        public List<int> ActivatedPromotionIDs { get; set; } = new List<int>(0);

        /// <summary>
        /// A <see cref="List{T}"/> of promotions IDs that have been activated by a barcode
        /// </summary>
        public List<int> BarcodeActivatedPromotionIDs { get; set; } = new List<int>(0);

        /// <summary>
        /// Exclude promotions with a barcode (does not include promotions from <see cref="BarcodeActivatedPromotionIDs"/>)
        /// </summary>
        public bool ExcludeBarcodePromotions { get; set; }

        /// <summary>
        /// Defines the logic for converting the prices
        /// </summary>
        public PriceLogic PriceLogic { get; set; }

        /// <summary>
        /// Set <see cref="ArticlePrices"/> via <see cref="SetPriceContainer"/>
        /// </summary>
        public PriceContainer ArticlePrices { get; private set; }
        public PromotionArticleContainer PromotionArticleContainer { get; set; } = new PromotionArticleContainer();

        /// <summary>
        /// Is set <see cref="SetPriceContainer"/> (<see cref="PriceContainer"/> contains the <see cref="PriceTypeID"/>).
        /// /// </summary>
        public int PriceTypeID { get; private set; }
        public int CountryID { get; set; }

        /// <summary>
        /// If <see langword="true"/>, invalid articles are filtered from calculation.
        /// If <see langword="false"/>, no filtering is done.
        /// Default: <see langword="true"/>.
        /// </summary>
        public bool FilterArticles { get; set; } = true;

        /// <summary>
        /// Lets the <see cref="BasePromotionCalculator"/> decide if a <see cref="PromotionWrapper"/> is worth it (is a better promotion than the current prices).
        /// </summary>
        public bool ChooseBestPromotion { get; set; }

        /// <summary>
        /// Excludes promotions with a value voucher action from processing.
        /// </summary>
        public bool ExcludeVoucherPromotions { get; set; }

        /// <summary>
        /// Excludes promotions that have a <see cref="PromotionWrapper.ContactGroupID"/> or <see cref="PromotionWrapper.ContactSubGroupID"/> defined
        /// </summary>
        public bool ExcludeCustomerSpecific { get; set; }

        /// <summary>
        /// Excludes promotions with a value voucher action from processing.
        /// </summary>
        public bool ExcludeActionVoucher { get; set; }

        public bool EnableProcessLogging { get; set; }

        /// <summary>
        /// Enable PromotionGroupingIndex: Store "set" information (promotions where you have a <see cref="PromotionActionArticleType"/> action).
        /// </summary>
        public bool EnablePromotionGroupingIndex { get; set; }

        public void SetPriceContainer(PriceContainer container) {
            ArticlePrices = container;
            if (container == null) { return; }
            PriceTypeID = container.PriceTypeID;
        }
    }
}
