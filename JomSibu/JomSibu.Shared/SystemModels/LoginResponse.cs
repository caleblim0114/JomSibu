using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SMCRecycle.Shared.SystemModels
{
    public class LoginResponse
    {
        public string Token { get; set; }
        public DateTime Expiration { get; set; }
        public int? Role { get; set; }
        public int UserId { get; set; }
    }
}
