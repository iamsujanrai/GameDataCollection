namespace GameDataCollection.Models
{
    public class Email
    {
        public Email()
        {
            IsActive = true;
        }

        public long Id { get; set; }
        public required string MemberEmail { get; set; }
        public bool IsActive { get; set; }
    }
}
