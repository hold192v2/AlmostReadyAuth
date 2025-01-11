using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities
{
    public class RefreshSession : BaseEntity
    {
        public Guid UserId { get; set; }
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }

        public User User { get; set; }
    }
}
