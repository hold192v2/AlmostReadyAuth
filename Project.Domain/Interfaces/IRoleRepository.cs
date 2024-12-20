﻿using Project.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Domain.Interfaces
{
    public interface IRoleRepository : IBaseOperationRepository<Role>
    {
        Task<List<Role>> GetRoles(List<Guid> ids);
    }
}
