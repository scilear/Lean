using System;

namespace QuantConnect.Algorithm.CSharp.AAngel
{
    public class PositionChanges
    {
        public int PositionId { get; set; }
        public ChangeType Type { get; set; }
        //public Symbol Symbol { get; set; }
        public decimal QuantityBefore { get; set; }
        public decimal QuantityAfter{ get; set; }
        public decimal GetAddedQuantity()
        {
            return Math.Round(QuantityAfter - QuantityBefore, 4);
        }

        public enum ChangeType
        {
            Open,
            Close,
            Increase,
            Decrease
        }
    }
}
