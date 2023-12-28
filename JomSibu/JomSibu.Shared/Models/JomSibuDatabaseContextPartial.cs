using System;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace JomSibu.Shared.Models
{
	public partial class JomSibuDatabaseContext : IdentityDbContext<IdentityUser>
	{
        public async Task<UserDetailsTable> GetUserByAspNetUserIdAsync(string aspNetUserId)
        {
            return await UserDetailsTables.AsNoTracking()
                .FirstAsync(x => x.AspNetUserId == aspNetUserId);
        }

        private void QueryFilter(ref ModelBuilder builder)
        {
            builder.Entity<UserDetailsTable>().HasQueryFilter(b => b.IsDeleted != true);
        }
    }
}

