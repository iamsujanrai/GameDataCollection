namespace GameDataCollection.Models
{
    // Models/PrizeOption.cs
    public class PrizeOption
    {
        public int Index { get; set; }       // 0,1,2,...
        public string Label { get; set; }    // "$1", "$5", etc.
        public decimal Amount { get; set; }  // 1, 5, 10 ...
        public int Weight { get; set; }      // for probability (higher = more likely)
    }

    // Models/SpinResultDto.cs
    public class SpinResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string PrizeLabel { get; set; }
        public decimal PrizeAmount { get; set; }
        public int PrizeIndex { get; set; }      // which slice
        public double FinalRotationDeg { get; set; } // degrees to rotate the wheel
    }
}
