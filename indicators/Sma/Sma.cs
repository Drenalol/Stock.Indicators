﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Skender.Stock.Indicators
{
    public static partial class Indicator
    {
        // SIMPLE MOVING AVERAGE
        public static IEnumerable<SmaResult> GetSma(
            IEnumerable<Quote> history, int lookbackPeriod, bool extended = false)
        {

            // clean quotes
            List<Quote> historyList = Cleaners.PrepareHistory(history).ToList();

            // check parameters
            ValidateSma(history, lookbackPeriod);

            // initialize
            List<SmaResult> results = new List<SmaResult>();

            // roll through history
            for (int i = 0; i < historyList.Count; i++)
            {
                Quote h = historyList[i];

                SmaResult result = new SmaResult
                {
                    Index = (int)h.Index,
                    Date = h.Date
                };

                if (h.Index >= lookbackPeriod)
                {
                    decimal sumSma = 0m;
                    for (int p = (int)h.Index - lookbackPeriod; p < h.Index; p++)
                    {
                        Quote d = historyList[p];
                        sumSma += d.Close;
                    }

                    result.Sma = sumSma / lookbackPeriod;

                    // add optional extended values
                    if (extended)
                    {
                        decimal sumMad = 0m;
                        decimal sumMse = 0m;
                        decimal sumMape = 0m;

                        for (int p = (int)h.Index - lookbackPeriod; p < h.Index; p++)
                        {
                            Quote d = historyList[p];
                            sumMad += Math.Abs(d.Close - (decimal)result.Sma);
                            sumMse += (d.Close - (decimal)result.Sma) * (d.Close - (decimal)result.Sma);
                            sumMape += Math.Abs(d.Close - (decimal)result.Sma) / d.Close;
                        }

                        // mean absolute deviation
                        result.Mad = sumMad / lookbackPeriod;

                        // mean squared error
                        result.Mse = sumMse / lookbackPeriod;

                        // mean absolute percent error
                        result.Mape = sumMape / lookbackPeriod;
                    }
                }

                results.Add(result);
            }

            return results;
        }


        private static void ValidateSma(IEnumerable<Quote> history, int lookbackPeriod)
        {

            // check parameters
            if (lookbackPeriod <= 0)
            {
                throw new BadParameterException("Lookback period must be greater than 0 for SMA.");
            }

            // check history
            int qtyHistory = history.Count();
            int minHistory = lookbackPeriod;
            if (qtyHistory < minHistory)
            {
                throw new BadHistoryException("Insufficient history provided for SMA.  " +
                        string.Format(englishCulture,
                        "You provided {0} periods of history when at least {1} is required.",
                        qtyHistory, minHistory));
            }

        }
    }

}
