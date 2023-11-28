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
    public class PKCS8AdapterTests
    {
        private Mock<IFileSystemAdapter> fsa;
        private PKCS8Adapter adapter;
        private const string certificateFilename = "private.key";
        private const string certificate = "MIIFDjBABgkqhkiG9w0BBQ0wMzAbBgkqhkiG9w0BBQwwDgQIAgEAAoIBAQACAggAMBQGCCqGSIb3DQMHBAgwggS8AgEAMASCBMh4EHl7aNSCaMDA1VlRoXCZ5UUmqErAbucRFLOMmsAaFOpXTvw9AqcPbs75oxETQg3qB56TG1Cf2xjcp73gVNksxtSUUSaKuq92Ag16W6bbqKeJKSVMNQGhWGX8gUVJxw+vHXqTGRxOIFDbzgaGGPkZVPbifp2fHbRcCmJg2Ugb8eNe1MQByYHxN0UTw4OBR1Hs3D9k4c9STHzZ9OGWLnu6kc/z+b3GqWEGFwfs6vScvO87lw446GU6qRsAgPcazWX+bUf22mPx4YgdUyc4a0ARgQskZKdGoM9mNmyXCAW636uL/uyuudHjdM8EzDZE4dGd3aXgI3vuGb7Fqu21oOtdPkBCw7HsfOAjz9OZmLMNXGo/eWlJrVLO48yCwOXuyHjaWmRUqXVsggRXnCezML7IkLHX0Qqx0zbVu2RGRtKVv9ThFQxTkGqSWC3yMirEujU743ZLjZtgTmhv2cJ9iZ564R+jjYGe2IdY5akmyggjYdDpRkxrgw9jzmO1DVW5/8p2k19EcKnISIEDb65hq0fBucIxpnJJwMyJG+1N0pX4hb+A4GSdReFuxukZCTGfuMzKHQghalDdvEYIO/d6Fpo0eRXI5FPOlb4LNxq3P7vc8dqINrTtLQNQ7UEILZn3byvx5scaFO+FgP+a4H4aIHLR7DkYHEg5iHb2ZIBNvl7WgVmkvrJWF3x8ng0dVdfRcTLOyvFIq45bGW8sHDWO81gJp0PukxkeBKSrwC6BVDtUlAod7C1zcNQBKH9IGjWNEueGNnFEkMlHpmmM0HwJIPrlWFeLyZBDpD9DcWYHfVuKQs3jUOcVKbmqfobfhWlFfzkD3a0IMWtcxtr1IPF3DV6r1cyGkn660swMyh5SLKRUMtHnjismq7pNHEcKWI08NGFkf+ZXiB/ty2xMcbWU5yjKmly7ReSivh/8c7SkX90JXJCHfR2ryd1ToNy9pBGPvBlYw3jjRl8ZzL72I9WPxh18sc59ML5+NdiNW+7CcvuBL4XVTO+31HURXPOtPViQi4D/UnH2rW8dhH7X1VfGSNM4rs2w47hdCC7FjcWhBpdf9TnpFQx5WpbkS6P3MwsbJJrT7JkB8B60gAKexVI1KBBPj35n5UHAKUi2PaAenUUX6qbQaNgxwB8vFOgm44J7xef69n6AcIcjn7boWigm4uvRWS/TEsjta/bTX7Gx4qjF/1Ketgip0zliu+rbOyAihYxJSNIZv7PB1Nnj/7i2/AaqnBJex+Du7aRvjkoXu+QSh0/Fc1p05RQZX1AZno1PCcxzWL/49zU1rPONmPhwpnMW8A1RXq6wPqjC4+QMOaqNSndJSjxEzHSzbZcm8vMrgvkOnlgA0Np9nT0xj6tlLuRmMSFGdKFFa8FFGOcxXXyLHktoIqDABy3zF+jEa8ZRiBkR1A+NIhPMkgxCi2LLtbAhoWXdnHbTsqhbwIJc7zp25wvNi5iaJUvwIIzvHY52qCtmVNu/lFjkNHKZIYQ/rg8M6gVbeTtJYZkWCXBQ6QoafwZomjDIcBqf8Dz8BvcAPTOyrMLvOWUNtiE1PKNUVaf85IC/zZh/sWvCtG2/oDAwPm1qS704io0NfDtO7qc29Tu0CAQ6ncvGHDBZJ5WqWtjGLMNdMMQxHQc=";
        private const string secret = "12345678a";
        private const string data = "Primo Tamayo";
        private const string dataSigned = "CazC+rFwc1NAqjunNdrU9IMIs/JuRWani8WtzIoWQTBu/A6u0C3yMH3NeyAU4W66Qb/pa7F1YFycIMhe62rWDgqUkadvPf40/YeWhD9P1P5h0MNxtmjexvhc8Ym1d64m6DbhUjM76PJ18DO4K0C21pKyQFemPh/tVCqFXWm3ZZQUM8Hnh8/myxN36+dQln5V5hj65HWO9EpeawitcKG7fGvfiPxyiVjxqeFP4xPjdd1Dr2W/qHuJc3jGGyNxmvh2ufpTjimE+8W+WZPowDR+htbKbth7yGjeAluYR/qCWbcBFT8Fimac5tO2I9JWE0vhd0JOl2a+trp+T6TBTA16Bg==";

        [SetUp]
        public void Setup()
        {
            fsa = new Mock<IFileSystemAdapter>();
            adapter = new PKCS8Adapter(fsa.Object);
        }

        #region GenerateBase64Signature

        [Test]
        public void GenerateBase64Signature_WhenTheCertificateFileIsValid_ThenReturnTheDataSigned()
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

            var result = adapter.GenerateBase64Signature(certificateFilename, secret, data);

            #endregion

            #region Assert

            // Double check if the adapter verified the certificate file exists
            fsa.Verify(v => v.FileExists(It.IsAny<string>()), Times.Once);
            // Verify the adapter read all the bytes from the certificate file
            fsa.Verify(v => v.FileReadAllBytes(It.IsAny<string>()), Times.Once);
            // Verify the serial number of the certificate is correct
            Assert.AreEqual(dataSigned, result);

            #endregion
        }

        [Test]
        public void GenerateBase64Signature_WhenTheCertificateBytesIsValid_ThenReturnTheDataSigned()
        {
            #region Arrange

            var certificateBytes = Convert.FromBase64String(certificate);

            #endregion

            #region Act

            var result = adapter.GenerateBase64Signature(certificateBytes, secret, data);

            #endregion

            #region Assert

            // The adapter should NEVER verify the certificate file exists
            fsa.Verify(v => v.FileExists(It.IsAny<string>()), Times.Never);
            // The adapter should NEVER read all the bytes from the certificate file
            fsa.Verify(v => v.FileReadAllBytes(It.IsAny<string>()), Times.Never);
            // Verify the serial number of the certificate is correct
            Assert.AreEqual(dataSigned, result);

            #endregion
        }

        [Test]
        public void GenerateBase64Signature_WhenTheCertificateFileIsInvalid_ThenThrowsAnException()
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

            Assert.That(() => adapter.GenerateBase64Signature(certificateFilename, secret, data), Throws.Exception);

            #endregion
        }

        [Test]
        public void GenerateBase64Signature_WhenTheCertificateBytesIsInvalid_ThenThrowsAnException()
        {
            #region Arrange

            var invalidCertificateBytes = Encoding.UTF8.GetBytes("Invalid certificate");

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.GenerateBase64Signature(invalidCertificateBytes, secret, data), Throws.Exception);

            #endregion
        }

        [Test]
        public void GenerateBase64Signature_WhenTheSecretIsInvalidForCertificateFile_ThenThrowsAnException()
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

            #endregion

            #region Assert

            Assert.That(() => adapter.GenerateBase64Signature(certificateFilename, "Invalid secret", data), Throws.Exception);

            #endregion
        }

        [Test]
        public void GenerateBase64Signature_WhenTheSecretIsInvalidForCertificateBytes_ThenThrowsAnException()
        {
            #region Arrange

            var certificateBytes = Convert.FromBase64String(certificate);

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.GenerateBase64Signature(certificateBytes, "Invalid secret", data), Throws.Exception);

            #endregion
        }

        [Test]
        public void GenerateBase64Signature_WhenTheCertificateFileDoesNotExist_ThenThrowsFileNotFoundException()
        {
            #region Arrange

            fsa.Setup(x => x.FileExists(It.IsAny<string>())).Returns(false);

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.GenerateBase64Signature(certificateFilename, secret, data), Throws.Exception.TypeOf<System.IO.FileNotFoundException>());

            #endregion
        }

        [Test]
        public void GenerateBase64Signature_WhenTheCertificateFileIsNotProvided_ThenThrowsArgumentNullException()
        {
            #region Arrange

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.GenerateBase64Signature(filename: null, secret, data), Throws.Exception.TypeOf<ArgumentNullException>());

            #endregion
        }

        [Test]
        public void GenerateBase64Signature_WhenTheCertificateBytesIsNotProvided_ThenThrowsArgumentNullException()
        {
            #region Arrange

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.GenerateBase64Signature(certificateBytes: null, secret, data), Throws.Exception.TypeOf<ArgumentNullException>());

            #endregion
        }

        [Test]
        public void GenerateBase64Signature_WhenTheSecretIsNotProvidedForCertificateFile_ThenThrowsArgumentNullException()
        {
            #region Arrange

            fsa.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.GenerateBase64Signature(certificateFilename, null, data), Throws.Exception.TypeOf<ArgumentNullException>());

            #endregion
        }

        [Test]
        public void GenerateBase64Signature_WhenTheSecretIsNotProvidedForCertificateBytes_ThenThrowsArgumentNullException()
        {
            #region Arrange

            var certificateBytes = Convert.FromBase64String(certificate);

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.GenerateBase64Signature(certificateBytes, null, data), Throws.Exception.TypeOf<ArgumentNullException>());

            #endregion
        }

        [Test]
        public void GenerateBase64Signature_WhenTheDataToSignIsNotProvidedForCertificateFile_ThenThrowsArgumentNullException()
        {
            #region Arrange

            fsa.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.GenerateBase64Signature(certificateFilename, secret, null), Throws.Exception.TypeOf<ArgumentNullException>());

            #endregion
        }

        [Test]
        public void GenerateBase64Signature_WhenTheDataToSignIsNotProvidedForCertificateBytes_ThenThrowsArgumentNullException()
        {
            #region Arrange

            var certificateBytes = Convert.FromBase64String(certificate);

            #endregion

            #region Act

            #endregion

            #region Assert

            Assert.That(() => adapter.GenerateBase64Signature(certificateBytes, secret, null), Throws.Exception.TypeOf<ArgumentNullException>());

            #endregion
        }

        #endregion

        #region VerifyData

        [Test]
        public void VerifyData_WhenTheSignatureIsValidForCertificateFile_ThenReturnTrue()
        {
            #region Arrange

            var originalData = "Primo Tamayo";

            var readAllBytesResult = new FileSystemAdapter.Result
            {
                OperationCompletedSuccessfully = true,
                Value = Convert.FromBase64String(certificate)
            };
            fsa.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
            fsa.Setup(x => x.FileReadAllBytes(It.IsAny<string>())).Returns(readAllBytesResult);

            #endregion

            #region Act

            var isValidSignature = adapter.VerifyData(certificateFilename, secret, originalData, dataSigned);

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
            var certificateBytes = Convert.FromBase64String(certificate);

            #endregion

            #region Act

            var isValidSignature = adapter.VerifyData(certificateBytes, secret, originalData, dataSigned);

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

            var readAllBytesResult = new FileSystemAdapter.Result
            {
                OperationCompletedSuccessfully = true,
                Value = Convert.FromBase64String(certificate)
            };
            fsa.Setup(x => x.FileExists(It.IsAny<string>())).Returns(true);
            fsa.Setup(x => x.FileReadAllBytes(It.IsAny<string>())).Returns(readAllBytesResult);

            #endregion

            #region Act

            var isValidSignature = adapter.VerifyData(certificateFilename, secret, originalData, signatureBase64);

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

            var isValidSignature = adapter.VerifyData(certificateBytes, secret, originalData, signatureBase64);

            #endregion

            #region Assert

            // Verify the signature passes the validation with the public certificate
            Assert.IsFalse(isValidSignature);

            #endregion
        }

        [Test]
        public void VerifyData_WhenTheOriginalDataIsNotProvidedForCertificateFile_ThenThrowsArgumentNullException()
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

            #endregion

            #region Assert

            Assert.That(() => adapter.VerifyData(certificateFilename, secret, null, "signatureBase64"), Throws.Exception.TypeOf<ArgumentNullException>());

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

            Assert.That(() => adapter.VerifyData(certificateBytes, secret, null, "signatureBase64"), Throws.Exception.TypeOf<ArgumentNullException>());

            #endregion
        }

        [Test]
        public void VerifyData_WhenTheSignatureIsNotProvidedForCertificateFile_ThenThrowsArgumentNullException()
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

            #endregion

            #region Assert

            Assert.That(() => adapter.VerifyData(certificateFilename, secret, "original data", null), Throws.Exception.TypeOf<ArgumentNullException>());

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

            Assert.That(() => adapter.VerifyData(certificateBytes, secret, "original data", null), Throws.Exception.TypeOf<ArgumentNullException>());

            #endregion
        }

        #endregion
    }
}
