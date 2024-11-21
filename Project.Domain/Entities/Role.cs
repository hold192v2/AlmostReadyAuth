using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Entities
{
    public class Role : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public List<User> Users { get; set; } = new();

        public Role(string name) 
        {
            Name = name;
        }
    }
}
