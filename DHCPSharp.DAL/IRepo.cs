using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace DHCPSharp.DAL
{
    public interface IRepository<T> where T : class
    {
        string TableName { get; }
        List<T> GetAll();
        T Get(int id);
        int Insert(T entity);
        bool Update(T entity);
        void Delete(T entity);
    }

    public interface IRepo<T> where T : class, new()
    {
        Task<List<T>> Get();
        Task<T> Get(int id);
        Task<List<T>> Get<TValue>(Expression<Func<T, bool>> predicate = null, Expression<Func<T, TValue>> orderBy = null);
        Task<T> Get(Expression<Func<T, bool>> predicate);
        AsyncTableQuery<T> AsQueryable();
        Task<int> Insert(T entity);
        Task<int> Update(T entity);
        Task<int> Delete(T entity);
    }
}
