/*
 * QUANTCONNECT.COM - Democratizing Finance, Empowering Individuals.
 * Lean Algorithmic Trading Engine v2.0. Copyright 2014 QuantConnect Corporation.
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

using QuantConnect.Data;
using System;
using System;
using System.Collections.Generic;
using QuantConnect.Data.Auxiliary;
using QuantConnect.Data.Market;
using QuantConnect.Indicators;
using QuantConnect.Interfaces;
using QuantConnect.Securities;

using System.Reflection;
using QuantConnect.Data;
using QuantConnect.Lean.Engine.TransactionHandlers;

namespace QuantConnect.Algorithm.CSharp
{
    /// <summary>
    /// Algorithm demonstrating FOREX asset types and requesting history on them in bulk. As FOREX uses
    /// QuoteBars you should request slices or
    /// </summary>
    /// <meta name="tag" content="using data" />
    /// <meta name="tag" content="history and warm up" />
    /// <meta name="tag" content="history" />
    /// <meta name="tag" content="forex" />
    public class BasicTemplateForexAlgorithm : QCAlgorithm
    {

        public BasicTemplateForexAlgorithm()
        {
             int stop = 1;
        }
        /// <summary>
        /// Initialise the data and resolution required, as well as the cash and start-end dates for your algorithm. All algorithms must initialized.
        /// </summary
        /// 
        QCAlgorithm algorithm;
        public override void Initialize()
        { 
            SetStartDate(2014, 5, 7);  //Set Start Date
            SetEndDate(2014, 5, 15);    //Set End Date
            SetCash(100000);             //Set Strategy Cash
            // Find more symbols here: http://quantconnect.com/data
            //AddForex("EURUSD", Resolution.Minute);
            //AddForex("EURUSD", Resolution.Minute);

            //var dailyHistory = History(5, Resolution.Daily);
            //var hourHistory = History(5, Resolution.Hour);
            //var minuteHistory = History(5, Resolution.Minute);
            //var secondHistory = History(5, Resolution.Second);
            algorithm = new QCAlgorithm();
            //algorithm.Initialize();


            // new forex - should be quotebar
            algorithm.SubscriptionManager = SubscriptionManager;
            //algorithm.Securities.SetSecurityService(Securities.SetSecurityService);
            var symbolPropertiesDatabase = SymbolPropertiesDatabase.FromDataFolder();
            //var mapFilePrimaryExchangeProvider = new MapFilePrimaryExchangeProvider(AlgorithmHandlers.MapFileProvider);
            var registeredTypesProvider = new RegisteredSecurityDataTypesProvider();

            Type typSecurityManager = typeof(SecurityManager);
            FieldInfo typeAccessSecurityManager = typSecurityManager.GetField("_securityService", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var sm = (SecurityService)typeAccessSecurityManager.GetValue(Securities);


            // Type typSecurityService = typeof(SecurityService);
            // FieldInfo typeAccessSecurityService = typSecurityService.GetField("_primaryExchangeProvider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            // var ss = typeAccessSecurityService.GetValue(sm);

            //algorithm.Securities.SetSecurityService(sm);
            algorithm.Securities = Securities;
            algorithm.Portfolio.Securities = Securities;


            /*
            var securityService = new SecurityService(algorithm.Portfolio.CashBook,
                marketHoursDatabase,
                symbolPropertiesDatabase,
                algorithm,
                registeredTypesProvider,
                new SecurityCacheProvider(algorithm.Portfolio),
                mapFilePrimaryExchangeProvider);*/

            AddForex("EURUSD", Resolution.Minute);
            var forexQuote = algorithm.AddForex("EURUSD", Resolution.Minute);
            // Assert.IsTrue(forexQuote.Subscriptions.Count() == 1);
            // Assert.IsTrue(GetMatchingSubscription(forexQuote, typeof(TradeBar)) != null);
            algorithm.Transactions.SetOrderProcessor(new BacktestingTransactionHandler());
            algorithm.Portfolio.Transactions = algorithm.Transactions;
            algorithm.PostInitialize();
        }

        /// <summary>
        /// OnData event is the primary entry point for your algorithm. Each new data point will be pumped in here.
        /// </summary>
        /// <param name="data">Slice object keyed by symbol containing the stock data</param>
        public override void OnData(Slice data)
        {
            if (!algorithm.Portfolio.Invested)
            {
                //if (data.QuoteBars.ContainsKey("EURUSD")) SetHoldings("EURUSD", .5);
                if (data.QuoteBars.ContainsKey("EURUSD")) 
                    algorithm.SetHoldings("EURUSD", .5);
                //Log(string.Join(", ", data.Values));
            }
        }
    }
}