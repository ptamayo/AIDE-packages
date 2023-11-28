namespace Aide.Core.Interfaces
{
	public interface ICacheService
	{
		object Get(string key);
		T Get<T>(string key);
		void Set(string key, object value, double cacheTimeInMinutes);
		void Set(string key, object value);
		bool Exist(string key);
		double GetMinutesLeftToMidNight();
		void Remove(string key);
	}
}
