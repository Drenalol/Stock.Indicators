﻿using System;

namespace Skender.Stock.Indicators
{
    [Serializable]
    public class CorrResult : ResultBase
    {
        public decimal? VarianceA { get; set; }
        public decimal? VarianceB { get; set; }
        public decimal? Covariance { get; set; }
        public decimal? Correlation { get; set; }
        public decimal? RSquared { get; set; }

        // internal use only
        internal decimal PriceA { get; set; }
        internal decimal PriceB { get; set; }
        internal decimal PriceA2 { get; set; }
        internal decimal PriceB2 { get; set; }
        internal decimal PriceAB { get; set; }
    }
}
