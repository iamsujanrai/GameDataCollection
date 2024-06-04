using GameDataCollection.Models;

namespace GameDataCollection.Services
{
    public interface IEmailSetupService
    {
        Task Save(Email email);
        Email GetById(long id);
        Task<List<Email>> GetAll();
        void Delete(Email email);
        void ToggleStatus(Email email);
    }
}
