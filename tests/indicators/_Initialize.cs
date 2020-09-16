﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using Skender.Stock.Indicators;
using System.Collections.Generic;
using System.Globalization;

namespace Internal.Tests
{
    [TestClass]
    public abstract class TestBase
    {
        internal static readonly CultureInfo englishCulture = new CultureInfo("en-US", false);

        internal static IEnumerable<Quote> history;
        internal static IEnumerable<Quote> historyOther;

        [AssemblyInitialize]
        public static void Initialize(TestContext testContext)
        {
            history = History.GetHistory();
            historyOther = History.GetHistoryOther();
        }
    }
}
