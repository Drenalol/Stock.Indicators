﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Skender.Stock.Indicators
{
    public static partial class Indicator
    {
        // PARABOLIC SAR
        public static IEnumerable<ParabolicSarResult> GetParabolicSar(
            IEnumerable<Quote> history,
            decimal accelerationStep = (decimal)0.02,
            decimal maxAccelerationFactor = (decimal)0.2)
        {

            // clean quotes
            List<Quote> historyList = Cleaners.PrepareHistory(history).ToList();

            // check parameters
            ValidateParabolicSar(history, accelerationStep, maxAccelerationFactor);

            // initialize
            List<ParabolicSarResult> results = new List<ParabolicSarResult>();
            Quote first = historyList[0];

            decimal accelerationFactor = accelerationStep;
            decimal extremePoint = first.High;
            decimal priorSar = first.Low;
            bool isRising = true;  // initial guess

            // roll through history
            for (int i = 0; i < historyList.Count; i++)
            {
                Quote h = historyList[i];

                ParabolicSarResult result = new ParabolicSarResult
                {
                    Index = (int)h.Index,
                    Date = h.Date
                };

                // skip first one
                if (h.Index == 1)
                {
                    results.Add(result);
                    continue;
                }

                // was rising
                if (isRising)
                {
                    decimal currentSar = priorSar + accelerationFactor * (extremePoint - priorSar);

                    // turn down
                    if (h.Low < currentSar)
                    {
                        result.IsReversal = true;
                        result.Sar = extremePoint;

                        isRising = false;
                        accelerationFactor = accelerationStep;
                        extremePoint = h.Low;
                    }

                    // continue rising
                    else
                    {
                        result.IsReversal = false;
                        result.Sar = currentSar;

                        // SAR cannot be higher than last two lows
                        if (i >= 2)
                        {
                            decimal minLastTwo = Math.Min(historyList[i - 1].Low, historyList[i - 2].Low);

                            result.Sar = Math.Min((decimal)result.Sar, minLastTwo);
                        }
                        else
                        {
                            result.Sar = (decimal)result.Sar;
                        }

                        if (h.High > extremePoint)
                        {
                            extremePoint = h.High;
                            accelerationFactor = Math.Min(accelerationFactor += accelerationStep, maxAccelerationFactor);
                        }
                    }
                }

                // was falling
                else
                {
                    decimal currentSar = priorSar - accelerationFactor * (priorSar - extremePoint);

                    // turn up
                    if (h.High > currentSar)
                    {
                        result.IsReversal = true;
                        result.Sar = extremePoint;

                        isRising = true;
                        accelerationFactor = accelerationStep;
                        extremePoint = h.High;
                    }

                    // continue falling
                    else
                    {
                        result.IsReversal = false;
                        result.Sar = currentSar;

                        // SAR cannot be lower than last two highs
                        if (i >= 2)
                        {
                            decimal maxLastTwo = Math.Max(historyList[i - 1].High, historyList[i - 2].High);

                            result.Sar = Math.Max((decimal)result.Sar, maxLastTwo);
                        }
                        else
                        {
                            result.Sar = (decimal)result.Sar;
                        }

                        if (h.Low < extremePoint)
                        {
                            extremePoint = h.Low;
                            accelerationFactor = Math.Min(accelerationFactor += accelerationStep, maxAccelerationFactor);
                        }
                    }
                }

                priorSar = (decimal)result.Sar;

                results.Add(result);
            }

            // remove first trend to reversal, since it is an invalid guess
            ParabolicSarResult firstReversal = results
                .Where(x => x.IsReversal == true)
                .OrderBy(x => x.Index)
                .FirstOrDefault();

            if (firstReversal != null)
            {
                foreach (ParabolicSarResult r in results.Where(x => x.Index <= firstReversal.Index))
                {
                    r.Sar = null;
                    r.IsReversal = null;
                }
            }

            return results;
        }


        private static void ValidateParabolicSar(IEnumerable<Quote> history, decimal accelerationStep, decimal maxAccelerationFactor)
        {

            // check parameters
            if (accelerationStep <= 0)
            {
                throw new BadParameterException("Acceleration Step must be greater than 0 for Parabolic SAR.");
            }

            if (maxAccelerationFactor <= 0)
            {
                throw new BadParameterException("Max Acceleration Factor must be greater than 0 for Parabolic SAR.");
            }

            if (accelerationStep > maxAccelerationFactor)
            {
                throw new BadParameterException("Acceleration Step must be smaller than Max Accleration Factor for Parabolic SAR.");
            }

            // check history
            int qtyHistory = history.Count();
            int minHistory = 2;
            if (qtyHistory < minHistory)
            {
                throw new BadHistoryException("Insufficient history provided for Parabolic SAR.  " +
                        string.Format(englishCulture,
                        "You provided {0} periods of history when at least {1} is required.",
                        qtyHistory, minHistory));
            }

        }
    }

}
