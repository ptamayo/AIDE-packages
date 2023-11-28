using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Dynamic.Core;
using Aide.Core.Interfaces;
using System.Threading.Tasks;
using System.Reflection;
using System.Text;

namespace Aide.Core.Data
{
	public class EfRepository<T> : IRepository<T> where T : class
	{
		private readonly DbContext _context;
		private DbSet<T> _entities;

		/// <summary>
		/// Entities
		/// </summary>
		protected virtual DbSet<T> Entities
		{
			get
			{
				if (_entities == null)
					_entities = _context.Set<T>();
				return _entities;
			}
		}

		/// <summary>
		/// Gets a table
		/// </summary>
		public virtual IQueryable<T> Table
		{
			get
			{
				return Entities;
			}
		}

		/// <summary>
		/// Gets a table with "no tracking" enabled (EF feature) Use it only when you load record(s) only for read-only operations
		/// </summary>
		public virtual IQueryable<T> TableNoTracking
		{
			get
			{
				return Entities.AsNoTracking();
			}
		}

		public EfRepository(DbContext context)
		{
			this._context = context;
		}

		public virtual async Task<T> GetByIdAsync(object id)
		{
			//see some suggested performance optimization (not tested)
			//http://stackoverflow.com/questions/11686225/dbset-find-method-ridiculously-slow-compared-to-singleordefault-on-id/11688189#comment34876113_11688189
			return await Entities.FindAsync(id).ConfigureAwait(false);
		}

		public static async Task<IPagedResult<T>> PaginateAsync(IPagingSettings pagingSettings, IQueryable<T> query)
		{
			if (pagingSettings == null)
				throw new ArgumentNullException("PageRequest cannot be null.");

			if (pagingSettings.PageNumber <= 0 || pagingSettings.PageSize <= 0)
				throw new ArgumentException("Invalid PageNumber or PageSize: Zero or negative numbers are not accepted.");

			if (pagingSettings.SortBy != null && pagingSettings.SortBy.Any())
            {
				query = ApplySort(query, pagingSettings.SortBy);
            }

			double tableCount = await query.CountAsync().ConfigureAwait(false);
			double pageCount = Math.Ceiling(tableCount / pagingSettings.PageSize);

			// At the moment the only valid scenario for the lines below is when you are in any page other than #1 in the grid
			// and then you perform a search by keyword resulting in few records so that everything is fit in 1 single page.
			if (pagingSettings.PageNumber > pageCount && pageCount > 0)
			{
				//throw new ArgumentOutOfRangeException($"PageNumber is out of range: The max page number allowed is {pageCount}");
				// Fix the page number instead of throwing up an exception
				pagingSettings.PageNumber = 1;
			}

			int skipRows = (pagingSettings.PageNumber - 1) * pagingSettings.PageSize;

			var results = await query.Skip(skipRows).Take(pagingSettings.PageSize).ToListAsync().ConfigureAwait(false);

			IPagedResult<T> pageResult = new PagedResult<T>
			{
				CurrentPage = pagingSettings.PageNumber,
				PageSize = results.Count(),
				PageCount = pageCount,
				RowCount = tableCount,
				Results = results,
			};

			return pageResult;
		}

		public static IPagedResult<T> Paginate(IPagingSettings pagingSettings, IEnumerable<T> collection)
		{
			if (pagingSettings == null)
				throw new ArgumentNullException("PageRequest cannot be null.");

			if (pagingSettings.PageNumber <= 0 || pagingSettings.PageSize <= 0)
				throw new ArgumentException("Invalid PageNumber or PageSize: Zero or negative numbers are not accepted.");

			double tableCount = 0;
			if (collection != null && collection.Any()) tableCount = collection.Count();
			double pageCount = Math.Ceiling(tableCount / pagingSettings.PageSize);

			// At the moment the only valid scenario for the lines below is when you are in any page other than #1 in the grid
			// and then you perform a search by keyword resulting in few records so that everything is fit in 1 single page.
			if (pagingSettings.PageNumber > pageCount && pageCount > 0)
			{
				//throw new ArgumentOutOfRangeException($"PageNumber is out of range: The max page number allowed is {pageCount}");
				// Fix the page number instead of throwing up an exception
				pagingSettings.PageNumber = 1;
			}

			var query = collection.AsQueryable();

			if (pagingSettings.SortBy != null && pagingSettings.SortBy.Any())
			{
				query = ApplySort(query, pagingSettings.SortBy);
			}

			int skipRows = (pagingSettings.PageNumber - 1) * pagingSettings.PageSize;
			var results = query.Skip(skipRows).Take(pagingSettings.PageSize).ToList();

			var pageResult = new PagedResult<T>
			{
				CurrentPage = pagingSettings.PageNumber,
				PageSize = results.Count(),
				PageCount = pageCount,
				RowCount = tableCount,
				Results = results,
			};

			return pageResult;
		}

		public static IQueryable<T> ApplySort(IQueryable<T> query, string[] orderParams)
		{
			if (orderParams == null || !orderParams.Any()) return query;

			var propertyInfos = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
			var orderQueryBuilder = new StringBuilder();
			foreach (var param in orderParams)
			{
				if (string.IsNullOrWhiteSpace(param)) continue;

				var propertyFromQueryName = param.Split(" ")[0];
				var objectProperty = propertyInfos.FirstOrDefault(pi => pi.Name.Equals(propertyFromQueryName, StringComparison.InvariantCultureIgnoreCase));
				
				if (objectProperty == null) continue;

				var sortingOrder = param.EndsWith(" desc") ? "descending" : "ascending";
				orderQueryBuilder.Append($"{objectProperty.Name.ToString()} {sortingOrder}, ");
			}

			var orderQuery = orderQueryBuilder.ToString().TrimEnd(',', ' ');
			query = query.OrderBy(orderQuery);
			return query;
		}

		public virtual async Task<int> InsertAsync(T entity)
		{
			try
			{
				if (entity == null)
					throw new ArgumentNullException(nameof(entity));

				Entities.Add(entity);

				return await _context.SaveChangesAsync().ConfigureAwait(false);
			}
			catch (DbEntityValidationException dbEx)
			{
				//ensure that the detailed error text is saved in the Log
				throw new Exception(GetFullErrorTextAndRollbackEntityChanges(dbEx), dbEx);
			}
		}

		public virtual async Task<int> InsertAsync(IEnumerable<T> entities)
		{
			try
			{
				if (entities == null)
					throw new ArgumentNullException(nameof(entities));

				foreach (var entity in entities)
					Entities.Add(entity);

				return await _context.SaveChangesAsync().ConfigureAwait(false);
			}
			catch (DbEntityValidationException dbEx)
			{
				//ensure that the detailed error text is saved in the Log
				throw new Exception(GetFullErrorTextAndRollbackEntityChanges(dbEx), dbEx);
			}
		}

		public virtual async Task<int> UpdateAsync(T entity)
		{
			try
			{
				if (entity == null)
					throw new ArgumentNullException(nameof(entity));

				return await _context.SaveChangesAsync().ConfigureAwait(false);
			}
			catch (DbEntityValidationException dbEx)
			{
				//ensure that the detailed error text is saved in the Log
				throw new Exception(GetFullErrorTextAndRollbackEntityChanges(dbEx), dbEx);
			}
		}

		public virtual async Task<int> UpdateAsync(IEnumerable<T> entities)
		{
			try
			{
				if (entities == null)
					throw new ArgumentNullException(nameof(entities));

				return await _context.SaveChangesAsync().ConfigureAwait(false);
			}
			catch (DbEntityValidationException dbEx)
			{
				//ensure that the detailed error text is saved in the Log
				throw new Exception(GetFullErrorTextAndRollbackEntityChanges(dbEx), dbEx);
			}
		}

		public virtual async Task<int> DeleteAsync(T entity)
		{
			try
			{
				if (entity == null)
					throw new ArgumentNullException(nameof(entity));

				Entities.Remove(entity);

				return await _context.SaveChangesAsync().ConfigureAwait(false);
			}
			catch (DbEntityValidationException dbEx)
			{
				//ensure that the detailed error text is saved in the Log
				throw new Exception(GetFullErrorTextAndRollbackEntityChanges(dbEx), dbEx);
			}
		}

		public virtual async Task<int> DeleteAsync(IEnumerable<T> entities)
		{
			try
			{
				if (entities == null)
					throw new ArgumentNullException(nameof(entities));

				foreach (var entity in entities)
					Entities.Remove(entity);

				return await _context.SaveChangesAsync().ConfigureAwait(false);
			}
			catch (DbEntityValidationException dbEx)
			{
				//ensure that the detailed error text is saved in the Log
				throw new Exception(GetFullErrorTextAndRollbackEntityChanges(dbEx), dbEx);
			}
		}

		protected string GetFullErrorText(DbEntityValidationException exc)
		{
			var msg = string.Empty;
			//foreach (var validationErrors in exc.EntityValidationErrors)
			//	foreach (var error in validationErrors.ValidationErrors)
			//		msg += $"Property: {error.PropertyName} Error: {error.ErrorMessage}" + Environment.NewLine;
			return msg;
		}

		/// <summary>
		/// Rollback of entity changes and return full error message
		/// </summary>
		/// <param name="dbEx">Exception</param>
		/// <returns>Error</returns>
		protected string GetFullErrorTextAndRollbackEntityChanges(DbEntityValidationException dbEx)
		{
			var fullErrorText = GetFullErrorText(dbEx);

			//foreach (var entry in dbEx.EntityValidationErrors.Select(error => error.Entry))
			//{
			//	if (entry == null)
			//		continue;

			//	//rollback of entity changes
			//	entry.State = EntityState.Unchanged;
			//}

			//_context.SaveChanges();
			return fullErrorText;
		}
	}

	public class DbEntityValidationException : Exception
	{
		public DbEntityValidationException() : base() { }
		public DbEntityValidationException(string message) : base(message) { }
	}
}
