using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Aide.Core.Interfaces
{
	public interface IRepository<T> where T : class
	{
		/// <summary>
		/// Get entity by identifier
		/// </summary>
		/// <param name="id">Identifier</param>
		/// <returns>Entity</returns>
		//T GetById(object id);
		Task<T> GetByIdAsync(object id);

		/// <summary>
		/// Insert entity
		/// </summary>
		/// <param name="entity">Entity</param>
		//void Insert(T entity);
		Task<int> InsertAsync(T entity);

		/// <summary>
		/// Insert entities
		/// </summary>
		/// <param name="entities">Entities</param>
		//void Insert(IEnumerable<T> entities);
		Task<int> InsertAsync(IEnumerable<T> entities);

		/// <summary>
		/// Update entity
		/// </summary>
		/// <param name="entity">Entity</param>
		//void Update(T entity);
		Task<int> UpdateAsync(T entity);

		/// <summary>
		/// Update entities
		/// </summary>
		/// <param name="entities">Entities</param>
		//void Update(IEnumerable<T> entities);
		Task<int> UpdateAsync(IEnumerable<T> entities);

		/// <summary>
		/// Delete entity
		/// </summary>
		/// <param name="entity">Entity</param>
		//void Delete(T entity);
		Task<int> DeleteAsync(T entity);

		/// <summary>
		/// Delete entities
		/// </summary>
		/// <param name="entities">Entities</param>
		//void Delete(IEnumerable<T> entities);
		Task<int> DeleteAsync(IEnumerable<T> entities);

		/// <summary>
		/// Gets a table
		/// </summary>
		IQueryable<T> Table { get; }

		/// <summary>
		/// Gets a table with "no tracking" enabled (EF feature) Use it only when you load record(s) only for read-only operations
		/// </summary>
		IQueryable<T> TableNoTracking { get; }
	}
}
