using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using NodaTime;
using QLNet;
using QuantConnect.Algorithm.Framework.Alphas;
using QuantConnect.Algorithm.Framework.Portfolio;
using QuantConnect.Benchmarks;
using QuantConnect.Brokerages;
using QuantConnect.Data;
using QuantConnect.Data.Market;
using QuantConnect.Data.UniverseSelection;
using QuantConnect.Indicators;
using QuantConnect.Interfaces;
//using QuantConnect.Engine.TransactionHandlers;
using QuantConnect.Notifications;
using QuantConnect.Orders;
using QuantConnect.Scheduling;
using QuantConnect.Securities;
using QuantConnect.Securities.Equity;
using QuantConnect.Securities.Forex;
using QuantConnect.Securities.Future;
using QuantConnect.Storage;
using Option = QuantConnect.Securities.Option.Option;


namespace QuantConnect.Algorithm.CSharp.AAngel
{
    public abstract class SubAlgo : IAlgorithm
    {
        //public Dictionary<Symbol, Position> Positions => Portfolio.Positions;
        protected QCAlgorithm Algo { get; private set; }
        public string Name { get { return GetName(); } set { string ignore = value; } }
        SecurityPortfolioManager IAlgorithm.Portfolio { get { return null; } }
        public DummyPortfolio Portfolio { get; private set; }
        public SubAlgo(QCAlgorithm algo, decimal cash = 0)
        {
            Algo = algo;
            Portfolio = new DummyPortfolio(cash, algo.Securities);
        }

        protected abstract string GetName();
        public abstract void Initialize();

        protected Slice CurrentData;
        public void OnDataWrapper(Slice data)
        {
            CurrentData = data;
            Portfolio.OnData(data);
            OnData(data);
        }

        public abstract void OnData(Slice data);

        public void OnWarmupFinished()
        {
            //Algo.OnWarmupFinished();
        }

        public void SetHoldings(Symbol symbol, decimal weight)
        {
        	if (Securities[symbol].Exchange.DateTimeIsOpen(Time))
        	{
	            var qty = CalculateOrderQuantity(symbol, weight);
	            var tradePrice = (qty > 0) ? Securities[symbol].BidPrice : Securities[symbol].AskPrice;
	            Console.WriteLine($"{Time}, {symbol} {qty} @ {tradePrice}");
	            Portfolio.Trade(symbol, qty, tradePrice);	
        	}
        }

        public List<int> Liquidate(Symbol symbolToLiquidate = null, string tag = "Liquidated")
        {
            SetHoldings(symbolToLiquidate, 0);
            return new List<int>();
        }

        public decimal CalculateOrderQuantity(Symbol symbol, decimal target)
        {
            if (CurrentData == null) throw new KeyNotFoundException("Missing CurrentData");
            // TODO: deal with bid ask calculation 
            var targetQuantity = Portfolio.Percent(this, symbol, target, 0.5m * (Securities[symbol].BidPrice+ Securities[symbol].AskPrice));

            return targetQuantity;
        }

        public void SetCash(decimal startingCash)
        {
            Portfolio.SetCash(startingCash);
            //Algo.SetCash(startingCash);
        }
        /////////////////////////////////////////////////////////////////////////////////////////////////
        /// 
        /// 
        public Forex AddForex(string ticker, Resolution? resolution = null, string market = null, bool fillDataForward = true, decimal leverage = Security.NullLeverage)
        {
            return Algo.AddForex(ticker, resolution, market, fillDataForward, leverage);
        }
        public Equity AddEquity(string ticker, Resolution? resolution = null, string market = null, bool fillDataForward = true, decimal leverage = Security.NullLeverage, bool extendedMarketHours = false)
        {
            return Algo.AddEquity(ticker, resolution, market, fillDataForward, leverage, extendedMarketHours);
        }

	
        public void OnStrategyWeightChange(decimal oldWeight, decimal newWeight)
        {
            if (newWeight == 0)
            {
                OnDeactivatedStrategy();
            }
            else  if (newWeight > oldWeight)
            {
                OnWeightIncrease(oldWeight, newWeight);
            }
            else if (oldWeight > newWeight)
            {
                OnWeightDecrease(oldWeight, newWeight);
            }
        }
		
		
        protected virtual  void OnWeightDecrease(decimal oldWeight, decimal newWeight)
        {
            
        }

        protected virtual  void OnWeightIncrease(decimal oldWeight, decimal newWeight)
        {
            
        }

        protected virtual void OnDeactivatedStrategy()
        {
            
        }

        public virtual void OnData(Dividends data) // update this to Dividends dictionary
        {
        }

        public virtual void OnData(Splits data)
        {
        }

        /////////////////////////////////////////////////////////////////////////////////////////////////
        
        public TimeRules TimeRules => Algo.TimeRules;
        public DateRules DateRules => Algo.DateRules;
        
        public DateTime Time => Algo.Time;
        protected SecurityManager Securities => Algo.Securities;

        public ITimeKeeper TimeKeeper => Algo.TimeKeeper;

        public SubscriptionManager SubscriptionManager => Algo.SubscriptionManager;

        SecurityManager IAlgorithm.Securities => Algo.Securities;

        public UniverseManager UniverseManager => Algo.UniverseManager;

        //SecurityPortfolioManager IAlgorithm.Portfolio => Algo.Portfolio;

        public SecurityTransactionManager Transactions => Algo.Transactions;

        public IBrokerageModel BrokerageModel => Algo.BrokerageModel;

        public IBrokerageMessageHandler BrokerageMessageHandler
        {
            get { return Algo.BrokerageMessageHandler; }
            set { Algo.BrokerageMessageHandler = value; }
        }


        public NotificationManager Notify => Algo.Notify;

        public ScheduleManager Schedule => Algo.Schedule;

        public IHistoryProvider HistoryProvider
        {
            get { return Algo.HistoryProvider; }
            set { Algo.HistoryProvider = value; }
        }
        public AlgorithmStatus Status
        {
            get { return Algo.Status; }
            set { Algo.Status = value; }
        }

        public bool IsWarmingUp => Algo.IsWarmingUp;



        DateTime IAlgorithm.Time => Algo.Time;

        public DateTimeZone TimeZone => Algo.TimeZone;

        public DateTime UtcTime => Algo.UtcTime;

        public DateTime StartDate => Algo.StartDate;

        public DateTime EndDate => Algo.EndDate;

        public string AlgorithmId => Algo.AlgorithmId;

        public bool LiveMode => Algo.LiveMode;

        public UniverseSettings UniverseSettings => Algo.UniverseSettings;

        public ConcurrentQueue<string> DebugMessages => Algo.DebugMessages;

        public ConcurrentQueue<string> ErrorMessages => Algo.ErrorMessages;

        public ConcurrentQueue<string> LogMessages => Algo.LogMessages;

        public Exception RunTimeError
        {
            get { return Algo.RunTimeError; }
            set { Algo.RunTimeError = value; }
        }

        public ConcurrentDictionary<string, string> RuntimeStatistics => Algo.RuntimeStatistics;

        public IBenchmark Benchmark => Algo.Benchmark;

        public ITradeBuilder TradeBuilder => Algo.TradeBuilder;

        public IAlgorithmSettings Settings => Algo.Settings;

        public IOptionChainProvider OptionChainProvider => Algo.OptionChainProvider;

        public IFutureChainProvider FutureChainProvider => Algo.FutureChainProvider;

        public ObjectStore ObjectStore => Algo.ObjectStore;

        public Slice CurrentSlice => Algo.CurrentSlice;

        public ISecurityInitializer SecurityInitializer => Algo.SecurityInitializer;

        public string AccountCurrency => Algo.AccountCurrency;


        /////////////////////////////////////////////////////////////////////////////
        /// No reason to touch the below

        public event AlgorithmEvent<GeneratedInsightsCollection> InsightsGenerated
        {
            add
            {
                Algo.InsightsGenerated += value;
            }

            remove
            {
                Algo.InsightsGenerated -= value;
            }
        }



        public void PostInitialize()
        {
            Algo.PostInitialize();
        }


        public string GetParameter(string name)
        {
            return Algo.GetParameter(name);
        }

        public void SetParameters(Dictionary<string, string> parameters)
        {
            Algo.SetParameters(parameters);
        }

        public bool Shortable(Symbol symbol, decimal quantity)
        {
            return Algo.Shortable(symbol, quantity);
        }

        public void SetBrokerageModel(IBrokerageModel brokerageModel)
        {
            Algo.SetBrokerageModel(brokerageModel);
        }



        public void OnFrameworkData(Slice slice)
        {
            Algo.OnFrameworkData(slice);
        }

        public void OnSecuritiesChanged(SecurityChanges changes)
        {
            Algo.OnSecuritiesChanged(changes);
        }

        public void OnFrameworkSecuritiesChanged(SecurityChanges changes)
        {
            Algo.OnFrameworkSecuritiesChanged(changes);
        }

        public void OnEndOfTimeStep()
        {
            Algo.OnEndOfTimeStep();
        }

        public void Debug(string message)
        {
            Algo.Debug(message);
        }

        public void Log(string message)
        {
            Algo.Log(message);
        }

        public void Error(string message)
        {
            Algo.Error(message);
        }

        public virtual void OnMarginCall(List<SubmitOrderRequest> requests)
        {
            Algo.OnMarginCall(requests);
        }

        public virtual void OnMarginCallWarning()
        {
            Algo.OnMarginCallWarning();
        }

        public virtual void OnEndOfDay()
        {
        	//var t = Test1();
            //throw new Exception("not derived");
        }
        // public virtual int Test1()
        // {
        // 	return 1;	
        // }

        public virtual void OnEndOfDay(Symbol symbol)
        {
            //Algo.OnEndOfDay(symbol);
        }

        public virtual void OnEndOfAlgorithm()
        {
            //Algo.OnEndOfAlgorithm();
        }

        public virtual void OnOrderEvent(OrderEvent newEvent)
        {
            //Algo.OnOrderEvent(newEvent);
        }

        public virtual void OnAssignmentOrderEvent(OrderEvent assignmentEvent)
        {
            //Algo.OnAssignmentOrderEvent(assignmentEvent);
        }

        public virtual void OnBrokerageMessage(BrokerageMessageEvent messageEvent)
        {
            //Algo.OnBrokerageMessage(messageEvent);
        }

        public void OnBrokerageDisconnect()
        {
            //Algo.OnBrokerageDisconnect();
        }

        public void OnBrokerageReconnect()
        {
            //Algo.OnBrokerageReconnect();
        }

        public void SetDateTime(DateTime time)
        {
            Algo.SetDateTime(time);
        }

        public void SetStartDate(DateTime start)
        {
            Algo.SetStartDate(start);
        }

        public void SetEndDate(DateTime end)
        {
            Algo.SetEndDate(end);
        }

        public void SetAlgorithmId(string algorithmId)
        {
            Algo.SetAlgorithmId(algorithmId);
        }

        public void SetLocked()
        {
            Algo.SetLocked();
        }

        public bool GetLocked()
        {
            return Algo.GetLocked();
        }

        public void AddChart(Chart chart)
        {
            Algo.AddChart(chart);
        }

        public List<Chart> GetChartUpdates(bool clearChartData = false)
        {
            return Algo.GetChartUpdates(clearChartData);
        }

        public Security AddSecurity(SecurityType securityType, string symbol, Resolution? resolution, string market, bool fillDataForward, decimal leverage, bool extendedMarketHours)
        {
            return Algo.AddSecurity(securityType, symbol, resolution, market, fillDataForward, leverage, extendedMarketHours);
        }

        public Future AddFutureContract(Symbol symbol, Resolution? resolution = null, bool fillDataForward = true, decimal leverage = 0)
        {
            return Algo.AddFutureContract(symbol, resolution, fillDataForward, leverage);
        }

        public Option AddOptionContract(Symbol symbol, Resolution? resolution = null, bool fillDataForward = true, decimal leverage = 0)
        {
            return Algo.AddOptionContract(symbol, resolution, fillDataForward, leverage);
        }

        public bool RemoveSecurity(Symbol symbol)
        {
            return Algo.RemoveSecurity(symbol);
        }

        public void SetAccountCurrency(string accountCurrency)
        {
            Algo.SetAccountCurrency(accountCurrency);
        }


        public void SetCash(string symbol, decimal startingCash, decimal conversionRate = 0)
        {
            Algo.SetCash(symbol, startingCash, conversionRate);
        }


        public void SetLiveMode(bool live)
        {
            Algo.SetLiveMode(live);
        }

        public void SetFinishedWarmingUp()
        {
            Algo.SetFinishedWarmingUp();
        }

        public IEnumerable<HistoryRequest> GetWarmupHistoryRequests()
        {
            return Algo.GetWarmupHistoryRequests();
        }

        public void SetMaximumOrders(int max)
        {
            Algo.SetMaximumOrders(max);
        }

        public void SetBrokerageMessageHandler(IBrokerageMessageHandler handler)
        {
            Algo.SetBrokerageMessageHandler(handler);
        }

        public void SetHistoryProvider(IHistoryProvider historyProvider)
        {
            Algo.SetHistoryProvider(historyProvider);
        }
        
        public IEnumerable<TradeBar> History(Symbol symbol, int lookback, Resolution resolution)
        {
            return ((QCAlgorithm)Algo).History(symbol, lookback, resolution);
        }
        public void SetRunTimeError(Exception exception)
        {
            Algo.SetRunTimeError(exception);
        }

        public void SetStatus(AlgorithmStatus status)
        {
            Algo.SetStatus(status);
        }

        public void SetAvailableDataTypes(Dictionary<SecurityType, List<TickType>> availableDataTypes)
        {
            Algo.SetAvailableDataTypes(availableDataTypes);
        }

        public void SetOptionChainProvider(IOptionChainProvider optionChainProvider)
        {
            Algo.SetOptionChainProvider(optionChainProvider);
        }

        public void SetFutureChainProvider(IFutureChainProvider futureChainProvider)
        {
            Algo.SetFutureChainProvider(futureChainProvider);
        }

        public void SetCurrentSlice(Slice slice)
        {
            Algo.SetCurrentSlice(slice);
        }

        public void SetApi(IApi api)
        {
            Algo.SetApi(api);
        }

        public void SetObjectStore(IObjectStore objectStore)
        {
            Algo.SetObjectStore(objectStore);
        }


        //////////////////////////////////////////////////////////////////
        public void Plot(string plot, string graph, double value)
        {
            Algo.Plot(plot, graph, value);
        }
        public void Plot(string plot, string graph, decimal value)
        {
            Algo.Plot(plot, graph, value);
        }        
        public void Plot(string chart, Indicator first, Indicator second = null, Indicator third = null, Indicator fourth = null)
        {
            Algo.Plot(chart, new[] { first, second, third, fourth }.Where(x => x != null).ToArray());
            
        }
    }
}
