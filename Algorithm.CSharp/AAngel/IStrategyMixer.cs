namespace QuantConnect.Algorithm.CSharp.AAngel
{
    internal interface IStrategyMixer
    {
        decimal GetWeight(string strategy);

        void Update(QCAlgorithm algo);
    }
}
