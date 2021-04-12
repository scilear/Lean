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
                var replicationIsOn = (RealQuantity != 0);
                if (sameDirection)
                {
                    PositionChanges change = new PositionChanges
                    {
                        PositionId = PositionId,
                        Type = (replicationIsOn) ? PositionChanges.ChangeType.Increase : PositionChanges.ChangeType.Open,
                        //Symbol = Symbol,
                        QuantityBefore = (replicationIsOn) ? Quantity : 0,
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
                                Type = (replicationIsOn) ? PositionChanges.ChangeType.Decrease : PositionChanges.ChangeType.Open,
                                
                                //Symbol = Symbol,
                                QuantityBefore = (replicationIsOn) ? Quantity : 0,
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

			if (Quantity > 0 && RealQuantity < 0 || Quantity < 0 && RealQuantity > 0)
			{
				throw new Exception($"Replication mismatch on {symbol} Qty {Quantity} and Real Qty {RealQuantity}");
			}
			// FIX: if the Replication takes the quantity back to 0, 
			// then we mark the position as non replicated so that it can resume properly
			// and not send decrease signal to be misinterpreted in PortfolioMixer and taking the position negative
            Replicated = (RealQuantity != 0);
        }
        public void DoneReplicating()
        {
            Changes.Clear();
        }
        
        public void RestartReplication()
        {
        	
            if (RealQuantity != 0)
            {
            	//something was not right!
            	throw new Exception($"Restarting replication with RealQuantity={RealQuantity} should be 0.");
            	
            }
            Replicated = true;
        }

        public void GenerateReplicaLiquidationChanges()
        {
        	//first remove Changes that may not have been processed as we do not want them on
        	Changes.Clear();
        	
        	//then clear out any remaining position, 
        	//if there was an opening in the changes it does not matter, we close on realQuantity
        	//if there was a close, it will be done anyway!
        	
        	if (Replicated)
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
	            //RealQuantity = 0;
	            ReplicationWeight = 0;
	            //RealQuantity = 0;
        	}
        	Replicated = false;
        }
    }
}
