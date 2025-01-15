using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbonents.Dtos
{
    public class TransferForAuthDto
    {
        public string AbonentId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
