using JomSibu.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JomSibu.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdvertisementController : ControllerBase
    {
        private JomSibuDatabaseContext _database;

        public AdvertisementController(JomSibuDatabaseContext jomSibuDatabaseContext)
        {
            _database = jomSibuDatabaseContext;
        }

        [HttpGet]
        public Task<List<AdvertisementsTable>> GetAdvertisements()
        {
            return _database.AdvertisementsTables.AsNoTracking()
                .ToListAsync();
        }

        [HttpPost]
        public Task CreateNewAdvertisement(AdvertisementsTable advertisement)
        {
            _database.AdvertisementsTables.Add(new AdvertisementsTable
            {
                Title = advertisement.Title,
                ImagePath = advertisement.ImagePath,
                Description = advertisement.Description,
            });

            return _database.SaveChangesAsync();
        }

        [HttpPost]
        [Route("UpdateAdvertisement")]
        public Task UpdateAdvertisement(int advertisemendId, AdvertisementsTable newAdvertisement)
        {
            var oldAdvertisement = _database.AdvertisementsTables.FirstOrDefault(t => t.Id == advertisemendId);
            if (oldAdvertisement != null)
            {
                oldAdvertisement.Title = newAdvertisement.Title;
                oldAdvertisement.ImagePath = newAdvertisement.ImagePath;
                oldAdvertisement.Description = newAdvertisement.Description;
            }

            return _database.SaveChangesAsync();
        }

        [HttpDelete]
        public Task DeleteAdvertisement(int advertisementId)
        {
            var advertisement = _database.AdvertisementsTables.FirstOrDefault(x => x.Id == advertisementId);
            if(advertisement != null)
            {
                _database.AdvertisementsTables.Remove(advertisement);
            }

            return _database.SaveChangesAsync();
        }
    }
}
