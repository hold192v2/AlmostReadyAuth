using Project.Domain.Entities;
using Project.Domain.Interfaces;
using Project.Infrastructure.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infrastructure.Repositories
{
    public class BaseRepository<T> : IBaseOperationRepository<T>
        where T : BaseEntity
    {

        private readonly AppDbContext appDbContext;

        public BaseRepository(AppDbContext appDbContext)
        {
            this.appDbContext = appDbContext;
        }
        public void Create(T entity)
        {
            entity.DateCreated = DateTimeOffset.UtcNow;
            appDbContext.Add(entity);

        }

        public void Delete(T entity)
        {
            entity.DateDeleted = DateTimeOffset.UtcNow;
            appDbContext.Remove(entity);
        }

        public List<T> GetAll()
        {
            return appDbContext.Set<T>().ToList();
        }

        public T GetById(Guid id)
        {
            return appDbContext.Set<T>().FirstOrDefault(x => x.Id == id);
        }

        public void Update(T entity)
        {
            entity.DateUpdated = DateTimeOffset.UtcNow;
            appDbContext.Update(entity);
        }
    }
}
