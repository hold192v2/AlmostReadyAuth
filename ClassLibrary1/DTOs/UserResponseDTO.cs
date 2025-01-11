using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Application.DTOs
{
    public class UserResponseDTO
    {
        public Guid Id { get; set; }
        public string Surname { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Patronymic { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string PassportData { get; set; } = string.Empty;
        public decimal Balance { get; set; } = 0;
        public bool Status { get; set; } = false;
        public List<RoleResponseDTO> Roles { get; set; } = new();
    }
}
