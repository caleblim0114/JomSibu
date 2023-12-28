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

        [HttpPost]
        public Task CreateNewVoucher(VouchersTable voucher)
        {
            _database.VouchersTables.Add(new VouchersTable
            {
                //IsUsed? Do I need to refer to UserVouchersTable?
                Title = voucher.Title,
                Description = voucher.Description,
                ExpiryDate = voucher.ExpiryDate,
                ImagePath = voucher.ImagePath,
            });

            return _database.SaveChangesAsync();
        }

        [HttpPost]
        [Route("UserVoucher")]
        public Task CreateUserVoucher(int userId, int voucherId)
        {
            _database.UserVouchersTables.Add(new UserVouchersTable
            {
                UserId = userId,
                VoucherId = voucherId,
            });

            return _database.SaveChangesAsync();
        }

        [HttpPost]
        [Route("UpdateVoucher")]
        public Task UpdateVoucher(int voucherId, VouchersTable newVoucher)
        {
            var oldVoucher = _database.VouchersTables.FirstOrDefault(t => t.Id == voucherId);
            if (oldVoucher != null)
            {
                oldVoucher.Title = newVoucher.Title;
                oldVoucher.Description = newVoucher.Description;
                oldVoucher.ExpiryDate = newVoucher.ExpiryDate;
                oldVoucher.ImagePath = newVoucher.ImagePath;
            }

            return _database.SaveChangesAsync();
        }

        [HttpDelete]
        public Task DeleteVoucher(int voucherId)
        {
            var voucher = _database.VouchersTables.FirstOrDefault(x=>x.Id==voucherId);
            if(voucher != null)
            {
                _database.VouchersTables.Remove(voucher);
            }

            return _database.SaveChangesAsync();
        }
    }
}
