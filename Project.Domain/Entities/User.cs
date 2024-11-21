using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Surname { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Patronymic { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string PassportData { get; set; } = string.Empty;
        public decimal Balance { get; set; } = 0;
        public bool Status { get; set; } = false;
        public List<Role> Roles { get; set; } = new();
        public Guid RefreshToken { get; set; }

        public User(string phoneNumber, string password)
        {
            PhoneNumber = phoneNumber;
            Password = password;
        }
        public void UpdateUser(User newUser)
        {
            Password = newUser.Password;
            Roles = newUser.Roles;
        }

        public void GenerateRefreshToken()
        {
            RefreshToken = Guid.NewGuid();
        }
        public void AddRole(Role role)
        {
            Roles.Add(role);
        }
    }
}
