using System;

namespace BECOSOFT.Data.Migrator {
    public readonly struct IndexOptimizeParameters : IEquatable<IndexOptimizeParameters> {
        private readonly int? _fillFactor;
        private readonly int? _minimumReorganize;
        private readonly int? _minimumRebuild;
        private readonly int? _minimumPageCount;
        private readonly bool? _updateStatistics;

        public IndexOptimizeParameters(int? fillFactor = null, int? minimumReorganize = null, int? minimumRebuild = null, 
                                       int? minimumPageCount = null, bool? updateStatistics = null) {
            _fillFactor = fillFactor;
            _minimumReorganize = minimumReorganize;
            _minimumRebuild = minimumRebuild;
            _minimumPageCount = minimumPageCount;
            _updateStatistics = updateStatistics;
        }

        public int FillFactor => _fillFactor ?? 95;
        public int MinimumReorganize => _minimumReorganize ?? 5;
        public int MinimumRebuild => _minimumRebuild ?? 30;
        public int MinimumPageCount => _minimumPageCount ?? 1000;
        public bool UpdateStatistics => _updateStatistics ?? true;

        public bool Equals(IndexOptimizeParameters other) {
            return _fillFactor == other._fillFactor
                   && _minimumReorganize == other._minimumReorganize
                   && _minimumRebuild == other._minimumRebuild 
                   && _minimumPageCount == other._minimumPageCount
                   && _updateStatistics == other._updateStatistics;
        }

        public override bool Equals(object obj) {
            return obj is IndexOptimizeParameters other && Equals(other);
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = _fillFactor.GetHashCode();
                hashCode = (hashCode * 397) ^ _minimumReorganize.GetHashCode();
                hashCode = (hashCode * 397) ^ _minimumRebuild.GetHashCode();
                hashCode = (hashCode * 397) ^ _minimumPageCount.GetHashCode();
                hashCode = (hashCode * 397) ^ _updateStatistics.GetHashCode();
                return hashCode;
            }
        }
    }
}