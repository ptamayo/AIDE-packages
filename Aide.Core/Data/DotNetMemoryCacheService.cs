using Microsoft.Extensions.Caching.Memory;
using System;
using Aide.Core.Interfaces;

namespace Aide.Core.Data
{
	public class DotNetMemoryCacheService : ICacheService, IDisposable
	{
		private readonly MemoryCache _cache;
		private readonly EnumTimeExpression _timeExpression;
		private readonly bool _isEnabled;
		private bool _disposed = false;

		public DotNetMemoryCacheService(EnumTimeExpression timeExpression, bool isEnabled)
        {
            var cacheOptions = new MemoryCacheOptions();
            _cache = new MemoryCache(cacheOptions);
            _timeExpression = timeExpression;
            _isEnabled = isEnabled;
        }

        public object Get(string key)
		{
			return _cache.Get(key);
		}

		public T Get<T>(string key)
		{
			return _cache.Get<T>(key);
		}

		public void Set(string key, object value)
		{
			if (!_isEnabled) return;
			var cacheTimeInMinutes = GetMinutesLeftToMidNight();
			Set(key, value, cacheTimeInMinutes);
		}

		public void Set(string key, object value, double cacheTimeInMinutes)
		{
			if (!_isEnabled) return;
			if (_timeExpression == EnumTimeExpression.LocalTime)
			{
				_cache.Set(key, value, DateTimeOffset.Now.AddMinutes(cacheTimeInMinutes));
			}
			else
			{
				_cache.Set(key, value, DateTimeOffset.UtcNow.AddMinutes(cacheTimeInMinutes));
			}
		}

		public bool Exist(string key)
		{
			if (!_isEnabled) return false;
			object result;
			return _cache.TryGetValue(key, out result);
		}

		public double GetMinutesLeftToMidNight()
		{
			TimeSpan timestamp;
			switch (_timeExpression)
			{
				case EnumTimeExpression.LocalTime:
					var localMidNight = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 59, 0);
					timestamp = localMidNight.Subtract(DateTime.Now);
					break;
				default: // Applies to EnumTimeExpression.CoordinatedUniversalTime and others (if any)
					var utcMidNight = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, DateTime.UtcNow.Day, 23, 59, 0);
					timestamp = utcMidNight.Subtract(DateTime.UtcNow);
					break;
			}

			return timestamp.TotalMinutes;
		}

		public void Remove(string key)
		{
			if (!_isEnabled) return;
			if (Exist(key))
			{
				_cache.Remove(key);
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
        {
			if (!_disposed)
			{
				if (disposing)
				{
					if (_cache != null)
					{
						_cache.Dispose();
					}
				}
				// Release unmanaged resources.
				// Set large fields to null.                
				_disposed = true;
			}
		}

        ~DotNetMemoryCacheService()
        {
			Dispose(false);
        }
	}

	public enum EnumTimeExpression
	{
		LocalTime,
		CoordinatedUniversalTime
	}
}
