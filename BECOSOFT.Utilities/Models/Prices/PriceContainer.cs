using System;
using BECOSOFT.Utilities.Extensions.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Transactions;

namespace BECOSOFT.Utilities.Models.Prices {
    public class PriceContainer {
        private readonly Dictionary<int, PriceData> _articlePrices;
        public List<int> ArticleIDsWithPrices => _articlePrices.Keys.ToSafeList();
        public int PriceTypeID { get; }
        public PriceLogic PriceLogic { get; }

        public PriceContainer(int priceTypeID, PriceLogic priceLogic, List<PriceData> articlePrices) : this(priceTypeID, priceLogic) {
            _articlePrices = articlePrices.ToDictionary(ap => ap.ArticleID);
        }

        public PriceContainer(int priceTypeID, PriceLogic priceLogic) {
            PriceTypeID = priceTypeID;
            PriceLogic = priceLogic;
            _articlePrices = new Dictionary<int, PriceData>();
        }

        public PriceCalculation GetPriceCalculation(int articleID, decimal quantity) {
            if (articleID == 0) { return PriceCalculation.Empty; }
            var articlePrice = _articlePrices.TryGetValueWithDefault(articleID);
            if (articlePrice == null) {
                return PriceCalculation.Empty;
            }
            var priceCalculation = articlePrice.GetPriceCalculation(quantity);
            if (priceCalculation.IsEmpty && quantity < 1m) {
                priceCalculation = articlePrice.GetPriceCalculation(1m);
            }
            return priceCalculation;
        }

        public bool Contains(int articleID) {
            return _articlePrices.ContainsKey(articleID);
        }

        public void UpdateArticlePrices(Dictionary<int, PriceData> articlePrices) {
            _articlePrices.AddRange(articlePrices);
        }
    }

    public readonly struct PriceLogic {
        public bool DocumentTypeIsInclusive { get; }
        public bool PriceTypeIsInclusive { get;  }
        public bool IsVatLiable { get; }

        public bool ConvertToExclusive => !DocumentTypeIsInclusive && PriceTypeIsInclusive;
        public bool ConvertToInclusive => DocumentTypeIsInclusive && !PriceTypeIsInclusive;
        public bool ConvertToNotVatLiable => DocumentTypeIsInclusive && PriceTypeIsInclusive && !IsVatLiable;
        public bool KeepInclusive => DocumentTypeIsInclusive && PriceTypeIsInclusive && IsVatLiable;
        public bool KeepExclusive => !DocumentTypeIsInclusive && !PriceTypeIsInclusive;

        public PriceLogic(bool documentTypeIsInclusive, bool priceTypeIsInclusive, bool isVatLiable) {
            DocumentTypeIsInclusive = documentTypeIsInclusive;
            PriceTypeIsInclusive = priceTypeIsInclusive;
            IsVatLiable = isVatLiable;
        }

        public PriceLogic(bool documentTypeIsInclusive, bool priceTypeIsInclusive) 
            : this(documentTypeIsInclusive, priceTypeIsInclusive, true) {
        }

        public static PriceLogic Purchase(bool priceTypeIsInclusive) {
            return new PriceLogic(false, priceTypeIsInclusive, false);
        }
    }
}
