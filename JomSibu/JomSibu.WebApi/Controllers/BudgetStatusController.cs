using JomSibu.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JomSibu.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BudgetStatusController : ControllerBase
    {
        private JomSibuDatabaseContext _database;

        public BudgetStatusController(JomSibuDatabaseContext jomSibuDatabaseContext)
        {
            _database = jomSibuDatabaseContext;
        }

        [HttpGet]
        public Task<List<BudgetStatusesTable>> GetAllBudgetStatuses()
        {
            return _database.BudgetStatusesTables.AsNoTracking()
                .ToListAsync();
        }

        [HttpPost]
        public Task CreateNewBudgetStatus(BudgetStatusesTable budgetStatus)
        {
            _database.BudgetStatusesTables.Add(new BudgetStatusesTable
            {
                StatusName = budgetStatus.StatusName
            });

            return _database.SaveChangesAsync();
        }

        [HttpPost]
        [Route("UpdateBudgetStatus")]
        public Task UpdateBudgetStatus(int budgetStatusId, BudgetStatusesTable newBudgetStatus)
        {
            var oldBudgetStatus = _database.BudgetStatusesTables.FirstOrDefault(t=>t.Id== budgetStatusId);
            if (oldBudgetStatus != null)
            {
                //Do I refer to other table?
                oldBudgetStatus.StatusName = newBudgetStatus.StatusName;
            }

            return _database.SaveChangesAsync();
        }

        [HttpDelete]
        public Task DeleteBudgetStatus(int budgetStatusId)
        {
            var budgetStatus = _database.BudgetStatusesTables.FirstOrDefault(x=>x.Id==budgetStatusId);
            if(budgetStatus != null)
            {
                _database.BudgetStatusesTables.Remove(budgetStatus);
            }

            return _database.SaveChangesAsync();
        }
    }
}
