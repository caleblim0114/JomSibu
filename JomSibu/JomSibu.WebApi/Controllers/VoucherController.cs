using JomSibu.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JomSibu.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VoucherController : ControllerBase
    {
        private JomSibuDatabaseContext _database;

        public VoucherController(JomSibuDatabaseContext jomSibuDatabaseContext)
        {
            _database = jomSibuDatabaseContext;
        }

        [HttpGet]
        public Task<List<VouchersTable>> GetAllVouchers()
        {
            return _database.VouchersTables.AsNoTracking()
                .ToListAsync();
        }
    }
}
