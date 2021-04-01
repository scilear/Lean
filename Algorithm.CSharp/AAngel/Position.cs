using System;
using System.Collections.Generic;

namespace QuantConnect.Algorithm.CSharp.AAngel
{
    //TODO: merge Position and PositionReplica
    public class Position
    {
        private Symbol symbol;
        public decimal Quantity { get; private set; }
        private decimal averageCost;
        private decimal lastPrice = 0;
        
        int PositionId = -1;

        

        public decimal  ReplicationWeight { get; set; }
        public decimal  RealQuantity { get; set; }
        public bool     Replicated { get; set; }

        public List<PositionChanges> Changes { get;  set; }
        
        //private List<Tuple<decimal, decimal>> trades
        public Position(Symbol symbol)
        {
            this.symbol = symbol;
            //PositionId = PositionTracker.PositionId;
            Changes = new List<PositionChanges>();
            //PositionTracker.Tracker[PositionId] = this;
        }

        internal decimal AddTrade(decimal qty, decimal tradePrice)
        {
            if (qty == 0) return 0;
            if (Quantity == 0)
            {
                PositionChanges change = new PositionChanges
                {
                    PositionId = PositionId,
                    Type = PositionChanges.ChangeType.Open,
                    //Symbol = Symbol,
                    QuantityBefore = 0,
                    QuantityAfter = qty,

                };
                Changes.Add(change);
                Quantity = qty;
                averageCost = tradePrice;
            }
            else
            {
                var sameDirection= Math.Sign(Quantity / qty) == 1;
                if (sameDirection)
                {
                    PositionChanges change = new PositionChanges
                    {
                        PositionId = PositionId,
                        Type = PositionChanges.ChangeType.Increase,
                        //Symbol = Symbol,
                        QuantityBefore = Quantity,
                        QuantityAfter = Quantity+qty,

                    };
                    Changes.Add(change);
                    
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
                        //averagecost does not change
                        // TODO track cash proceeds per position?
                        if (Quantity + qty == 0)
                        {
                            PositionChanges change = new PositionChanges
                            {
                                PositionId = PositionId,
                                Type = PositionChanges.ChangeType.Close,
                                //Symbol = Symbol,
                                QuantityBefore = Quantity,
                                QuantityAfter = 0,

                            };
                            Changes.Add(change);
                        }
                        else
                        {
                            PositionChanges change = new PositionChanges
                            {
                                PositionId = PositionId,
                                Type = PositionChanges.ChangeType.Decrease,
                                //Symbol = Symbol,
                                QuantityBefore = Quantity,
                                QuantityAfter = Quantity+qty,

                            };
                            Changes.Add(change);   
                        }
                        
                        Quantity += qty;
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
        
        public void Replicate(decimal weight, decimal quantity)
        {
            ReplicationWeight = weight;
            RealQuantity += quantity;

            Replicated = true;
        }
        public void DoneReplicating()
        {
            Changes.Clear();
        }

        public void GenerateReplicaLiquidationChanges()
        {
            PositionChanges change = new PositionChanges
            {
                PositionId = PositionId,
                Type = PositionChanges.ChangeType.Close,
                //Symbol = Symbol,
                QuantityBefore = Quantity,
                QuantityAfter = 0,

            };
            Changes.Add(change);
        }
    }
}