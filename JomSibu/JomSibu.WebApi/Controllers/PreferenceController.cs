using JomSibu.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JomSibu.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PreferenceController : ControllerBase
    {
        private JomSibuDatabaseContext _database;

        public PreferenceController(JomSibuDatabaseContext jomSibuDatabaseContext)
        {
            _database = jomSibuDatabaseContext;
        }

        [HttpGet]
        public Task<List<PreferencesTable>> GetAllPreferences()
        {
            return _database.PreferencesTables.AsNoTracking()
                .ToListAsync();
        }

        [HttpPost]
        public Task CreateNewPreference(PreferencesTable preference)
        {
            _database.PreferencesTables.Add(new PreferencesTable
            {
                Name = preference.Name,
            });

            return _database.SaveChangesAsync();
        }

        [HttpPost]
        [Route("UserPreference")]
        public Task CreateUserPreference(int userId, int preferenceId)
        {
            _database.UserPreferencesTables.Add(new UserPreferencesTable
            {
                UserId = userId,
                PreferenceId = preferenceId,
            });

            return _database.SaveChangesAsync();
        }

        [HttpPost]
        [Route("LocationPreference")]
        public Task CreateLocationPreference(int locationId, int preferenceId)
        {
            _database.LocationPreferencesTables.Add(new LocationPreferencesTable
            {
                LocationId = locationId,
                PreferenceId = preferenceId,
            });

            return _database.SaveChangesAsync();
        }

        [HttpPost]
        [Route("UpdatePreference")]
        public Task UpdatePreference(int preferenceId, PreferencesTable newPreference)
        {
            var oldPreference = _database.PreferencesTables.FirstOrDefault(x => x.Id == preferenceId);
            if (oldPreference != null)
            {
                oldPreference.Name = newPreference.Name;
            }

            return _database.SaveChangesAsync();
        }

        [HttpDelete]
        public Task DeletePreference(int preferenceId)
        {
            var preference = _database.PreferencesTables.FirstOrDefault(x=>x.Id == preferenceId);
            if(preference != null)
            {
                _database.PreferencesTables.Remove(preference);
            }

            return _database.SaveChangesAsync();
        }
    }
}
