using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace BECOSOFT.Utilities.Models.Promotions {
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class PromotionArticleWrapper {
        private string _fashionColor;
        private string _fashionCollection;
        private string _fashionFabric;
        private string _type;
        public int ArticleID { get; set; }
        public int MatrixID { get; set; }
        /// <summary>
        /// Weight of the article (can be overruled by weight on <see cref="ArticleData"/>).
        /// </summary>
        public decimal Weight { get; set; }
        public int BrandID { get; set; }
        public int Group1ID { get; set; }
        public int Group2ID { get; set; }
        public int Group3ID { get; set; }
        public int Group4ID { get; set; }
        public int Group5ID { get; set; }
        public int DiscountGroup { get; set; }
        public HashSet<int> TagIDs { get; set; } = new HashSet<int>();
        public int SeasonID { get; set; }
        /// <summary>
        /// Setter automatically calls ToLower() on the <see langword="value"/>
        /// </summary>
        public string Type {
            get => _type;
            set => _type = value?.ToLower();
        }

        /// <summary>
        /// Setter automatically calls ToLower() on the <see langword="value"/>
        /// </summary>
        public string FashionColor {
            get => _fashionColor;
            set => _fashionColor = value?.ToLower();
        }
        
        /// <summary>
        /// Setter automatically calls ToLower() on the <see langword="value"/>
        /// </summary>
        public string FashionCollection {
            get => _fashionCollection;
            set => _fashionCollection = value?.ToLower();
        }
        
        /// <summary>
        /// Setter automatically calls ToLower() on the <see langword="value"/>
        /// </summary>
        public string FashionFabric {
            get => _fashionFabric;
            set => _fashionFabric = value?.ToLower();
        }

        public int GetGroupID(int index) {
            switch (index) {
                case 1:
                    return Group1ID;
                case 2:
                    return Group2ID;
                case 3:
                    return Group3ID;
                case 4:
                    return Group4ID;
                case 5:
                    return Group5ID;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private string DebuggerDisplay => $"{ArticleID} - Brand: {BrandID}";
    }
}