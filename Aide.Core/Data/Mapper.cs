using System;

namespace Aide.Core.Data
{
	public abstract class Mapper
	{
		public static T StringToEnum<T>(string name)
		{
			//try
			//{
			return (T)Enum.Parse(typeof(T), name, ignoreCase: true);
			//}
			//catch { }
			//return (T)Enum.Parse(typeof(T), "0", ignoreCase: true);//Returns first value in the enum
		}
	}
}
