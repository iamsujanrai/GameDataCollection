namespace GameDataCollection.Models
{
    public class SpinSetting
    {
        public int Id { get; set; }
        public int CooldownHours { get; set; } = 24;
        public int MaxSpinsPerPeriod { get; set; } = 5;
    }
}
