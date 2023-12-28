using JomSibu.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JomSibu.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private JomSibuDatabaseContext _database;

        public LocationController(JomSibuDatabaseContext jomSibuDatabaseContext)
        {
            _database = jomSibuDatabaseContext;
        }

        [HttpGet]
        public Task<List<LocationsTable>> GetAllLocations()
        {
            return _database.LocationsTables.AsNoTracking()
                .ToListAsync();
        }

        [HttpPost]
        public Task CreateNewLocation(LocationsTable location)
        {
            _database.LocationsTables.Add(new LocationsTable
            {
                //Do I refer to LocationImageTables? and etc.?
                Name = location.Name,
                Address = location.Address,
                OperationDateTime = location.OperationDateTime,
                RecommendedDateTime = location.RecommendedDateTime,
                AverageReview = location.AverageReview,
            });

            return _database.SaveChangesAsync();
        }

        [HttpPost]
        [Route("UpdateLocation")]
        public Task UpdateLocation(int locationId, LocationsTable newLocation)
        {
            var oldLocation = _database.LocationsTables.FirstOrDefault(t=>t.Id == locationId);
            if (oldLocation != null)
            {
                oldLocation.Name = newLocation.Name;
                oldLocation.Address = newLocation.Address;
                oldLocation.OperationDateTime = newLocation.OperationDateTime;
                oldLocation.RecommendedDateTime = newLocation.RecommendedDateTime;
                oldLocation.AverageReview = newLocation.AverageReview;
            }

            return _database.SaveChangesAsync();
        }

        [HttpDelete]
        public Task DeleteLocation(int locationId)
        {
            var location = _database.LocationsTables.FirstOrDefault(x=>x.Id == locationId);
            if (location != null)
            {
                _database.LocationsTables.Remove(location);
            }

            return _database.SaveChangesAsync();
        }
    }
}
