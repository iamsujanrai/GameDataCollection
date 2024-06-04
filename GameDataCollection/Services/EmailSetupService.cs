using GameDataCollection.Models;
using GameDataCollection.Repositories;

namespace GameDataCollection.Services
{
    public class EmailSetupService : IEmailSetupService
    {
        private readonly IEmailSetupRepository _emailRepo;

        public EmailSetupService(IEmailSetupRepository emailRepo)
        {
            _emailRepo = emailRepo;
        }

        public void Delete(Email email)
        {
            try
            {
                _emailRepo.Delete(email);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Task<List<Email>> GetAll()
        {
            try
            {
                return _emailRepo.GetAll();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public Email GetById(long id)
        {
            try
            {
                return _emailRepo.GetById(id);
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task Save(Email email)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(email);

                await _emailRepo.InsertAsync(email);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void ToggleStatus(Email email)
        {
            try
            {
                if (email.IsActive)
                {
                    email.IsActive = false;
                } else
                {
                    email.IsActive = true;
                }

                _emailRepo.Update(email);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
