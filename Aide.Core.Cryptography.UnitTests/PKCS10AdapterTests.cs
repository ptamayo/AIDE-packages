using Aide.Core.Adapters;
using Aide.Core.Cryptography.Adapters;
using Aide.Core.Interfaces;
using Moq;
using NUnit.Framework;
using System;
using System.Text;

namespace Aide.Core.Cryptography.UnitTests
{
    [TestFixture]
    public class PKCS10AdapterTests
    {
        private Mock<IFileSystemAdapter> fsa;
        private PKCS10Adapter adapter;
        private const string certificateFilename = "certificate.cer";
        private const string certificate = "MIIFuzCCA6OgAwIBAgIUMzAwMDEwMDAwMDA0MDAwMDI0NDIwDQYJKoZIhvcNAQELBQAwggErMQ8wDQYDVQQDDAZBQyBVQVQxLjAsBgNVBAoMJVNFUlZJQ0lPIERFIEFETUlOSVNUUkFDSU9OIFRSSUJVVEFSSUExGjAYBgNVBAsMEVNBVC1JRVMgQXV0aG9yaXR5MSgwJgYJKoZIhvcNAQkBFhlvc2Nhci5tYXJ0aW5lekBzYXQuZ29iLm14MR0wGwYDVQQJDBQzcmEgY2VycmFkYSBkZSBjYWRpejEOMAwGA1UEEQwFMDYzNzAxCzAJBgNVBAYTAk1YMRkwFwYDVQQIDBBDSVVEQUQgREUgTUVYSUNPMREwDwYDVQQHDAhDT1lPQUNBTjERMA8GA1UELRMIMi41LjQuNDUxJTAjBgkqhkiG9w0BCQITFnJlc3BvbnNhYmxlOiBBQ0RNQS1TQVQwHhcNMTkwNjE3MjAxODA2WhcNMjMwNjE3MjAxODA2WjCB4jEnMCUGA1UEAxQeWkFQQVRFUklBIFVSVEFETyDRRVJJIFNBIERFIENWMScwJQYDVQQpFB5aQVBBVEVSSUEgVVJUQURPINFFUkkgU0EgREUgQ1YxJzAlBgNVBAoUHlpBUEFURVJJQSBVUlRBRE8g0UVSSSBTQSBERSBDVjElMCMGA1UELRQcWlXROTIwMjA4S0w0IC8gS0FITzY0MTEwMUIzOTEeMBwGA1UEBRMVIC8gS0FITzY0MTEwMUhOVExLUzA2MR4wHAYDVQQLFBVaYXBhdGVy7WEgVXJ0YWRvINFlcmkwggEiMA0GCSqGSIb3DQEBAQUAA4IBDwAwggEKAoIBAQCX4ICuLAA/GYfEdRUMBLolOHKOUTDyw//CQjcj6dmCUWP8Y2UIhbq1JJ5nClsmhg6WThXjYqzRiYSAYagzrtwWri8I7oURIoTDeuxU69UOg1shuCEo89prJfx8ZIGKwB+GedC6dFdKGZLKn4ksSuyfntXmW4rZLup2j24mQx+jpdmcQIGMe6A/2668ZhpYpCw/vwfH8edkS5X41yp/zj6Z9mIySRWaMHm5eT/X0D01l3gX2gJVXY5ebUsKIEmDCP5pmBmyFyJjqYUhYi3+nYARrVPdZ53RPigcoLspXyIBVf2CPansqOYiRIqxLjfsL6B1qGJmpFN1RHd+XF9GzkHzAgMBAAGjHTAbMAwGA1UdEwEB/wQCMAAwCwYDVR0PBAQDAgbAMA0GCSqGSIb3DQEBCwUAA4ICAQAw5nqLkVuaTWX/qnPqWpOlSfrfVF2plqAPu1sdgzU/vZGgZCLxq3cY3Dg02khdsM74A3fYFQGxFFo6zDt5Ru9VySBks2gmbebRuuYKAKHMoi0tNpvI2arOMaiJsq2yzGAox6e+MXhhQ1oV+28HaxjYIWuVSwWAzie+n6VloWXMDDxTg7t9URJ7d8E7ZrPZ2X0+7h6b8sfnOjTljUm+Mt6e6AIl1+lA7Ar4YeJgIOeU/RKSOcvrCGDNtz7kpn0XdsFC/m1xG1bIdhyv0zkRmKvtpVGROsSEHPLBwuTENIkm0Cw2/FQsPrG8S9Yer451Jv0H+heFfJXqVrHv8Azba5OkotK5IlPaHm9voxfhwI1aXHbUS+NwpTHHOeVsvzsC9LQDiLGO3MQLn7VFZebFa84CddwLWf/JTyPYIcg3I/BRDFqbrVNRRDJwVlxIgbU7ZhRIcjiRLFx6UbAd8B2fe+GHgxw/x/6Pop65ca32iGAJgojtXWdD5gZvh6PlhEhWMhIOCvAesE52L4wmIHofAIZjlPRXNaitZ74xDEIpQ9f5X0w/4ltB7OOBYYfu8z9lzCc4M2iSmtq1rvSYxnLI/nvt7D0uYrq82ZeG3kzungcS8qNjufGJeEHNeyDxMXF9rYU4KLxK8KLzA2Ldh7+3zywU/pwXhW/aTS42EViWPbffng==";
        private PKCS10Adapter.PublicCertificateInfo certificateInfo;

        [SetUp]
        public void Setup()
        {
            certificateInfo = new PKCS10Adapter.PublicCertificateInfo
            {
                SerialNumber = "30001000000400002442",
                UTCDateIssued = DateTime.SpecifyKind(Convert.ToDateTime("6/17/2019 20:18:06 PM"), DateTimeKind.Utc),
                UTCDateExpiration = DateTime.SpecifyKind(Convert.ToDateTime("6/17/2023 20:18:06 PM"), DateTimeKind.Utc),
                CertificateBase64String = certificate
            };
            fsa = new Mock<IFileSystemAdapter>();
            adapter = new PKCS10Adapter(fsa.Object);
        }

        #region GetPublicCertificateInfo

        [Test]
        public void GetPublicCertificateInfo_WhenTheCertificateFileIsValid_ThenReturnTheCertificateInfo()
        {
            #region Arrange

            var readAllBytesResult = new FileSystemAdapter.Result
            {
                OperationCompletedSuccessfully = true,
                Value = Convert.FromBase64String(certificate)
            };
            fsa.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
            fsa.Setup(x => x.FileReadAllBytes(It.IsAny<string>())).Returns(readAllBytesResult);

            #endregion

            #region Act

            var result = adapter.GetPublicCertificateInfo(certificateFilename);

            #endregion

            #region Assert

            // Double check if the adapter verified the certificate file exists
            fsa.Verify(v => v.FileExists(It.IsAny<string>()), Times.Once);
            // Verify the adapter read all the bytes from the certificate file
            fsa.Verify(v => v.FileReadAllBytes(It.IsAny<string>()), Times.Once);
            // Verify the serial number of the certificate is correct
            Assert.AreEqual(certificateInfo.SerialNumber, result.SerialNumber);
            // Verify the issue date of the certificate is correct
            Assert.AreEqual(certificateInfo.UTCDateIssued, result.UTCDateIssued);
            // Verify the expiration date of the certificate is correct
            Assert.AreEqual(certificateInfo.UTCDateExpiration, result.UTCDateExpiration);
            // Verify the base64 string of the certificate is correct
            Assert.AreEqual(certificateInfo.CertificateBase64String, result.CertificateBase64String);

            #endregion
        }

        [Test]
        public void GetPublicCertificateInfo_WhenTheCertificateBytesIsValid_ThenReturnTheCertificateInfo()
        {
            #region Arrange

            var certificateBytes = Convert.FromBase64String(certificate);

            #endregion

            #region Act

            var result = adapter.GetPublicCertificateInfo(certificateBytes);

            #endregion

            #region Assert

            // The adapter NEVER verifies if the certificate file exists
            fsa.Verify(v => v.FileExists(It.IsAny<string>()), Times.Never);
            // The adapter NEVER read the bytes from a certificate file
            fsa.Verify(v => v.FileReadAllBytes(It.IsAny<string>()), Times.Never);
            // Verify the serial number of the certificate is correct
            Assert.AreEqual(certificateInfo.SerialNumber, result.SerialNumber);
            // Verify the issue date of the certificate is correct
            Assert.AreEqual(certificateInfo.UTCDateIssued, result.UTCDateIssued);
            // Verify the expiration date of the certificate is correct
            Assert.AreEqual(certificateInfo.UTCDateExpiration, result.UTCDateExpiration);
            // Verify the base64 string of the certificate is correct
            Assert.AreEqual(certificateInfo.CertificateBase64String, result.CertificateBase64String);

            #endregion
        }

        [Test]
        public void GetPublicCertificateInfo_WhenTheCertificateFileIsInvalid_ThenThrowsAnException()
        {
            #region Arrange

            var readAllBytesResult = new FileSystemAdapter.Result
            {
                OperationCompletedSuccessfully = true,
                Value = Encoding.UTF8.GetBytes("Invalid certificate")
        };
            fsa.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
            fsa.Setup(x => x.FileReadAllBytes(It.IsAny<string>())).Returns(readAllBytesResult);

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.GetPublicCertificateInfo(certificateFilename), Throws.Exception);

            #endregion
        }

        [Test]
        public void GetPublicCertificateInfo_WhenTheCertificateBytesIsInvalid_ThenThrowsAnException()
        {
            #region Arrange

            var certificateBytes = Encoding.UTF8.GetBytes("Invalid certificate");

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.GetPublicCertificateInfo(certificateBytes), Throws.Exception);

            #endregion
        }

        [Test]
        public void GetPublicCertificateInfo_WhenTheCertificateFileDoesNotExist_ThenThrowsFileNotFoundException()
        {
            #region Arrange

            fsa.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.GetPublicCertificateInfo(certificateFilename), Throws.Exception.TypeOf<System.IO.FileNotFoundException>());

            #endregion
        }

        [Test]
        public void GetPublicCertificateInfo_WhenTheCertificateFileIsNotProvided_ThenThrowsArgumentNullException()
        {
            #region Arrange

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.GetPublicCertificateInfo(filename: null), Throws.Exception.TypeOf<ArgumentNullException>());

            #endregion
        }

        [Test]
        public void GetPublicCertificateInfo_WhenTheCertificateBytesIsNotProvided_ThenThrowsArgumentNullException()
        {
            #region Arrange

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.GetPublicCertificateInfo(certificateBytes: null), Throws.Exception.TypeOf<ArgumentNullException>());

            #endregion
        }

        #endregion

        #region VerifyData

        [Test]
        public void VerifyData_WhenTheSignatureIsValidForCertificateFile_ThenReturnTrue()
        {
            #region Arrange

            var originalData = "Primo Tamayo";
            var signatureBase64 = "CazC+rFwc1NAqjunNdrU9IMIs/JuRWani8WtzIoWQTBu/A6u0C3yMH3NeyAU4W66Qb/pa7F1YFycIMhe62rWDgqUkadvPf40/YeWhD9P1P5h0MNxtmjexvhc8Ym1d64m6DbhUjM76PJ18DO4K0C21pKyQFemPh/tVCqFXWm3ZZQUM8Hnh8/myxN36+dQln5V5hj65HWO9EpeawitcKG7fGvfiPxyiVjxqeFP4xPjdd1Dr2W/qHuJc3jGGyNxmvh2ufpTjimE+8W+WZPowDR+htbKbth7yGjeAluYR/qCWbcBFT8Fimac5tO2I9JWE0vhd0JOl2a+trp+T6TBTA16Bg==";

            #endregion

            #region Act

            var isValidSignature = adapter.VerifyData(certificateInfo, originalData, signatureBase64);

            #endregion

            #region Assert

            // Verify the signature passes the validation with the public certificate
            Assert.IsTrue(isValidSignature);

            #endregion
        }

        [Test]
        public void VerifyData_WhenTheSignatureIsValidForCertificateBytes_ThenReturnTrue()
        {
            #region Arrange

            var originalData = "Primo Tamayo";
            var signatureBase64 = "CazC+rFwc1NAqjunNdrU9IMIs/JuRWani8WtzIoWQTBu/A6u0C3yMH3NeyAU4W66Qb/pa7F1YFycIMhe62rWDgqUkadvPf40/YeWhD9P1P5h0MNxtmjexvhc8Ym1d64m6DbhUjM76PJ18DO4K0C21pKyQFemPh/tVCqFXWm3ZZQUM8Hnh8/myxN36+dQln5V5hj65HWO9EpeawitcKG7fGvfiPxyiVjxqeFP4xPjdd1Dr2W/qHuJc3jGGyNxmvh2ufpTjimE+8W+WZPowDR+htbKbth7yGjeAluYR/qCWbcBFT8Fimac5tO2I9JWE0vhd0JOl2a+trp+T6TBTA16Bg==";
            var certificateBytes = Convert.FromBase64String(certificate);

            #endregion

            #region Act

            var isValidSignature = adapter.VerifyData(certificateBytes, originalData, signatureBase64);

            #endregion

            #region Assert

            // Verify the signature passes the validation with the public certificate
            Assert.IsTrue(isValidSignature);

            #endregion
        }

        [Test]
        public void VerifyData_WhenTheSignatureIsInvalidForCertificateFile_ThenReturnFalse()
        {
            #region Arrange

            var originalData = "Primo Tamayo";
            var signatureBase64 = "Invalid signature";

            #endregion

            #region Act

            var isValidSignature = adapter.VerifyData(certificateInfo, originalData, signatureBase64);

            #endregion

            #region Assert

            // Verify the signature passes the validation with the public certificate
            Assert.IsFalse(isValidSignature);

            #endregion
        }

        [Test]
        public void VerifyData_WhenTheSignatureIsInvalidForCertificateBytes_ThenReturnFalse()
        {
            #region Arrange

            var originalData = "Primo Tamayo";
            var signatureBase64 = "Invalid signature";
            var certificateBytes = Convert.FromBase64String(certificate);

            #endregion

            #region Act

            var isValidSignature = adapter.VerifyData(certificateBytes, originalData, signatureBase64);

            #endregion

            #region Assert

            // Verify the signature passes the validation with the public certificate
            Assert.IsFalse(isValidSignature);

            #endregion
        }

        [Test]
        public void VerifyData_WhenTheCertificateInfoIsNotProvided_ThenThrowsArgumentNullException()
        {
            #region Arrange

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.VerifyData((PKCS10Adapter.PublicCertificateInfo)null, "original data", "signatureBase64"), Throws.Exception.TypeOf<ArgumentNullException>());

            #endregion
        }

        [Test]
        public void VerifyData_WhenTheCertificateBytesIsNotProvided_ThenThrowsArgumentNullException()
        {
            #region Arrange

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.VerifyData((byte[])null, "original data", "signatureBase64"), Throws.Exception.TypeOf<ArgumentNullException>());

            #endregion
        }

        [Test]
        public void VerifyData_WhenTheOriginalDataIsNotProvidedForCertificateFile_ThenThrowsArgumentNullException()
        {
            #region Arrange

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.VerifyData(certificateInfo, null, "signatureBase64"), Throws.Exception.TypeOf<ArgumentNullException>());

            #endregion
        }

        [Test]
        public void VerifyData_WhenTheOriginalDataIsNotProvidedForCertificateBytes_ThenThrowsArgumentNullException()
        {
            #region Arrange

            var certificateBytes = Convert.FromBase64String(certificate);

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.VerifyData(certificateBytes, null, "signatureBase64"), Throws.Exception.TypeOf<ArgumentNullException>());

            #endregion
        }

        [Test]
        public void VerifyData_WhenTheSignatureIsNotProvidedForCertificateFile_ThenThrowsArgumentNullException()
        {
            #region Arrange

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.VerifyData(certificateInfo, "original data", null), Throws.Exception.TypeOf<ArgumentNullException>());

            #endregion
        }

        [Test]
        public void VerifyData_WhenTheSignatureIsNotProvidedForCertificateBytes_ThenThrowsArgumentNullException()
        {
            #region Arrange

            var certificateBytes = Convert.FromBase64String(certificate);

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.VerifyData(certificateBytes, "original data", null), Throws.Exception.TypeOf<ArgumentNullException>());

            #endregion
        }

        #endregion
    }
}
