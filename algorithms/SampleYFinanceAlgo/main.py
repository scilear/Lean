# region imports
from AlgorithmImports import *
# endregion

class SampleYFinanceAlgo(QCAlgorithm):

    def initialize(self):
        self.set_start_date(2024, 1, 2)
        self.set_end_date(2024, 1, 12)
        self.set_cash(100000)
        self.spy = self.add_equity("SPY", Resolution.DAILY).symbol

    def on_data(self, data: Slice):
        """Buy SPY once daily yfinance bars are available locally."""
        if not data.contains_key(self.spy):
            return

        if not self.portfolio.invested:
            self.set_holdings(self.spy, 1)
            self.debug(f"Purchased {self.spy} from local daily data")
