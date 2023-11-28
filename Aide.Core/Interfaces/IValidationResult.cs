namespace Aide.Core.Interfaces
{
	public interface ILicensingValidationResult
	{
		bool AnyFailures();
		string GetValidationDetails();
	}
}
