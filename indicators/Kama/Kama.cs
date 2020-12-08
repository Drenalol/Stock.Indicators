﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Skender.Stock.Indicators
{
    public static partial class Indicator
    {
        // KAUFMAN's ADAPTIVE MOVING AVERAGE
        public static IEnumerable<KamaResult> GetKama<TQuote>(
            IEnumerable<TQuote> history,
            int erPeriod = 10,
            int fastPeriod = 2,
            int slowPeriod = 30)
            where TQuote : IQuote
        {

            // clean quotes
            List<TQuote> historyList = history.Sort();

            // check parameters
            ValidateKama(history, erPeriod, fastPeriod, slowPeriod);

            // initialize
            List<KamaResult> results = new List<KamaResult>(historyList.Count);
            decimal scFast = 2m / (fastPeriod + 1);
            decimal scSlow = 2m / (slowPeriod + 1);
            bool overflow = false;

            // roll through history
            for (int i = 0; i < historyList.Count; i++)
            {
                TQuote h = historyList[i];
                int index = i + 1;

                KamaResult r = new KamaResult
                {
                    Date = h.Date
                };

                if (index > erPeriod)
                {
                    // ER period change
                    decimal change = Math.Abs(h.Close - historyList[i - erPeriod].Close);

                    // volatility
                    decimal sumPV = 0m;
                    for (int p = i - erPeriod + 1; p <= i; p++)
                    {
                        sumPV += Math.Abs(historyList[p].Close - historyList[p - 1].Close);
                    }

                    if (sumPV != 0)
                    {
                        try
                        {
                            // efficiency ratio and smoothing constant
                            decimal er = change / sumPV;
                            decimal sc = er * (scFast - scSlow) + scSlow;  // squared later

                            // kama calculation
                            decimal? pk = results[i - 1].Kama;  // prior KAMA
                            r.Kama = pk + sc * sc * (h.Close - pk);
                        }

                        // handle overflow, which can happen in extreme variation cases
                        catch (OverflowException)
                        {
                            r.Kama = null;
                            overflow = true;
                        }
                    }

                    // handle flatline case
                    else
                    {
                        r.Kama = h.Close;
                    }
                }

                // initial value
                else if (index == erPeriod)
                {
                    r.Kama = h.Close;
                }

                results.Add(r);
            }

            // soft-report overflow
            if (overflow)
            {
                Console.WriteLine(
                     "WARNING: Extreme price variation caused an Overflow condition in KAMA.  " +
                     "Impacted KAMA values were set to NULL.");
            }

            return results;
        }


        private static void ValidateKama<TQuote>(
            IEnumerable<TQuote> history, int erPeriod, int fastPeriod, int slowPeriod)
            where TQuote : IQuote
        {

            // check parameters
            if (erPeriod <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(erPeriod), erPeriod,
                    "Efficiency Ratio period must be greater than 0 for KAMA.");
            }

            if (fastPeriod <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(fastPeriod), fastPeriod,
                    "Fast EMA period must be greater than 0 for KAMA.");
            }

            if (slowPeriod <= fastPeriod)
            {
                throw new ArgumentOutOfRangeException(nameof(slowPeriod), slowPeriod,
                    "Slow EMA period must be greater than Fast EMA period for KAMA.");
            }

            // check history
            int qtyHistory = history.Count();
            int minHistory = Math.Max(2 * erPeriod, erPeriod + 100);
            if (qtyHistory < minHistory)
            {
                string message = "Insufficient history provided for KAMA.  " +
                    string.Format(englishCulture,
                    "You provided {0} periods of history when at least {1} is required.  "
                    + "Since this uses a smoothing technique, for an ER period of {2}, "
                    + "we recommend you use at least {3} data points prior to the intended "
                    + "usage date for maximum precision.",
                    qtyHistory, minHistory, erPeriod, erPeriod + 250);

                throw new BadHistoryException(nameof(history), message);
            }


        }
    }

}