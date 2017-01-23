using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace DHCPSharp.DAL
{
    public class SQLiteRepo<T> : IRepo<T> where T: class,new()
    {
        private SQLiteAsyncConnection db;

        public SQLiteRepo(SQLiteAsyncConnection db)
        {
            this.db = db;
        }

        public AsyncTableQuery<T> AsQueryable() =>
            db.Table<T>();

        public async Task<List<T>> Get() =>
            await db.Table<T>().ToListAsync().ConfigureAwait(false);

        public async Task<List<T>> Get<TValue>(Expression<Func<T, bool>> predicate = null, Expression<Func<T, TValue>> orderBy = null)
        {
            var query = db.Table<T>();

            if (predicate != null)
                query = query.Where(predicate);

            if (orderBy != null)
                query = query.OrderBy<TValue>(orderBy);

            return await query.ToListAsync().ConfigureAwait(false);
        }

        public async Task<T> Get(int id) =>
             await db.FindAsync<T>(id).ConfigureAwait(false);

        public async Task<T> Get(Expression<Func<T, bool>> predicate) =>
            await db.FindAsync<T>(predicate).ConfigureAwait(false);

        public async Task<int> Insert(T entity) =>
             await db.InsertAsync(entity).ConfigureAwait(false);

        public async Task<int> Update(T entity) =>
             await db.UpdateAsync(entity).ConfigureAwait(false);

        public async Task<int> Delete(T entity) =>
             await db.DeleteAsync(entity).ConfigureAwait(false);
    }
}
