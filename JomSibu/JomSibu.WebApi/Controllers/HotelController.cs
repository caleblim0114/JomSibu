using JomSibu.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;

namespace JomSibu.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        private JomSibuDatabaseContext _database;

        public HotelController(JomSibuDatabaseContext jomSibuDatabaseContext)
        {
            _database = jomSibuDatabaseContext;
        }

        [HttpGet]
        public Task<List<HotelsTable>> GetAllHotels()
        {
            return _database.HotelsTables.AsNoTracking()
                .ToListAsync();
        }

        [HttpPost]
        public Task CreateNewHotel(HotelsTable hotel)
        {
            _database.HotelsTables.Add(new HotelsTable
            {
                Address = hotel.Address,
                Name = hotel.Name,
                BudgetStatus = hotel.BudgetStatus,
            });

            return _database.SaveChangesAsync();
        }

        [HttpPost]
        [Route("UpdateHotel")]
        public Task UpdateHotel(int hotelId, HotelsTable newHotel)
        {
            var oldHotel = _database.HotelsTables.FirstOrDefault(t => t.Id == hotelId);
            if (oldHotel != null)
            {
                oldHotel.Name = newHotel.Name;
                oldHotel.Address = newHotel.Address;
                oldHotel.BudgetStatusId = newHotel.BudgetStatusId;
            }

            return _database.SaveChangesAsync();
        }

        [HttpDelete]
        public Task DeleteHotel(int hotelId)
        {
            var hotel = _database.HotelsTables.FirstOrDefault(x=>x.Id==hotelId);

            if(hotel != null)
            {
                _database.HotelsTables.Remove(hotel);
            }

            return _database.SaveChangesAsync();
        }
    }
}
