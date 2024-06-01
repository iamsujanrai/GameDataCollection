using Microsoft.AspNetCore.Mvc.Rendering;

namespace GameDataCollection.ViewModels
{
    public class GameRecordViewModel
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string? RefferedBy { get; set; }
        public string Email { get; set; }
        public string? FacebookName { get; set; }
        public string? GameUserId { get; set; }
        public int StateId { get; set; }
        public int GameId { get; set; }

        public IEnumerable<SelectListItem>? States { get; set; }
        public IEnumerable<SelectListItem>? Games { get; set; }
    }
}
