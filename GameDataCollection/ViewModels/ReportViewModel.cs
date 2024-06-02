using GameDataCollection.Models;

namespace GameDataCollection.ViewModels
{
    public class ReportViewModel
    {
        public ReportViewModel()
        {
            GameRecords = [];
        }
        public List<GameRecord>? GameRecords { get; set; }
    }
}
