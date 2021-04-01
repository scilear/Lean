using System;
using System.Collections.Generic;

namespace QuantConnect.Algorithm.CSharp.AAngel
{
    public class PositionTracker : Dictionary<int, PositionReplica>
    {
        private static int _positionId = 0;
        public static int PositionId { get { return _positionId++; } }

        static PositionTracker _tracker;
        public static PositionTracker Tracker { 
            get 
            {
                if (_tracker == null)
                    _tracker = new PositionTracker();
                return _tracker;
            }
        }
    }

    public class PositionReplica
    {
        int PositionId = -1;
        public PositionReplica()
        {
            PositionId = PositionTracker.PositionId;
            Changes = new List<PositionChanges>();
            PositionTracker.Tracker[PositionId] = this;
        }

        public void Add(decimal weight, decimal quantity)
        {
            if (quantity == 0)
                return;
            if (weight == 0)
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
                Quantity = 0;
            }
            else
            {
                if (Quantity == 0)
                {
                    PositionChanges change = new PositionChanges
                    {
                        PositionId = PositionId,
                        Type = PositionChanges.ChangeType.Open,
                        //Symbol = Symbol,
                        QuantityBefore = 0,
                        QuantityAfter = quantity,

                    };
                    Changes.Add(change);
                    Quantity += quantity;
                }
                else if (Quantity > 0 && quantity > 0 || Quantity > 0 && quantity > 0)
                {
                    PositionChanges change = new PositionChanges
                    {
                        PositionId = PositionId,
                        Type = PositionChanges.ChangeType.Increase,
                        //Symbol = Symbol,
                        QuantityBefore = Quantity,
                        QuantityAfter = Quantity+quantity,

                    };
                    Changes.Add(change);
                    Quantity += quantity;
                }
                else
                {
                    //dealing with a decrease which is a sign reversal => split in 2!
                    if (Math.Sign(Quantity / (Quantity + quantity)) == 1)//we dont change the direction of the position
                    {
                        PositionChanges change = new PositionChanges
                        {
                            PositionId = PositionId,
                            Type = PositionChanges.ChangeType.Decrease,
                            //Symbol = Symbol,
                            QuantityBefore = Quantity,
                            QuantityAfter = Quantity+quantity,

                        };
                        Changes.Add(change);   
                        Quantity += quantity;
                    }
                    else
                    {
                        var remainder = Quantity + quantity;
                        Add(0, -Quantity);
                        Add(weight, remainder);
                    }
                }
            }
            Weight = weight;
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

        public decimal  Quantity { get; set; }

        public decimal  Weight { get; set; }

        public decimal  ReplicationWeight { get; set; }
        public decimal  RealQuantity { get; set; }
        public bool     Replicated { get; set; }

        public List<PositionChanges> Changes { get;  set; }
    }
}