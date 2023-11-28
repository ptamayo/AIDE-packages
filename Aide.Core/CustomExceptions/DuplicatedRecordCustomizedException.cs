using System;

namespace Aide.Core.CustomExceptions
{
	[Serializable]
	public class DuplicatedRecordCustomizedException : ArgumentException
	{
		public DuplicatedRecordCustomizedException() : base() { }
		public DuplicatedRecordCustomizedException(string message) : base(message) { }
	}
}
