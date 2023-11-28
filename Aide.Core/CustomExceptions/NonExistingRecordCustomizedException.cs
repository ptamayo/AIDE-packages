using System;

namespace Aide.Core.CustomExceptions
{
	[Serializable]
	public class NonExistingRecordCustomizedException : NullReferenceException
	{
		public NonExistingRecordCustomizedException() : base() { }
		public NonExistingRecordCustomizedException(string message) : base(message) { }
	}
}
