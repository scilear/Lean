using System;

namespace QuantConnect.Algorithm.CSharp.AAngel
{
    //TODO: merge Position and PositionReplica
    public class Position
    {
        private Symbol symbol;
        public decimal Quantity { get; private set; }
        private decimal averageCost;
        private decimal lastPrice = 0;
        //private List<Tuple<decimal, decimal>> trades
        public Position(Symbol symbol)
        {
            this.symbol = symbol;
        }

        internal decimal AddTrade(decimal qty, decimal tradePrice)
        {
            if (qty == 0) return 0;
            if (Quantity == 0)
            {
                Quantity = qty;
                averageCost = tradePrice;
            }
            else
            {
                var sameDirection= Math.Sign(Quantity / qty) == 1;
                if (sameDirection)
                {
                    Quantity += qty;
                    averageCost = (averageCost * Quantity + qty * tradePrice) / (Quantity + qty);
                }
                else
                {
                    if (Math.Abs(qty) > Math.Abs(Quantity)) // changing side
                    {
                        var remainder = qty + Quantity;
                        AddTrade(-Quantity, tradePrice);
                        AddTrade(remainder, tradePrice);
                    }
                    else // closing or partially closing
                    {
                        Quantity += qty;
                        //averagecost does not change
                        // TODO track cash proceeds per position?
                    }
                }
            }

            lastPrice = tradePrice;

            // TODO: create CashImpact method
            return qty * tradePrice;
        }

        internal decimal GetMarketValue()
        {
            return lastPrice * Quantity;
        }

        internal decimal UpdatePrice(decimal close)
        {
            lastPrice = close;
            return Quantity * lastPrice;
        }
    }
}