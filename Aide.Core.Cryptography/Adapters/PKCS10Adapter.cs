using Aide.Core.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace Aide.Core.Cryptography.Adapters
{
    public interface IPKCS10Adapter
    {
        PKCS10Adapter.PublicCertificateInfo GetPublicCertificateInfo(string filename);
        PKCS10Adapter.PublicCertificateInfo GetPublicCertificateInfo(byte[] certificateBytes);
        bool VerifyData(string filename, string originalData, string signatureBase64);
        bool VerifyData(byte[] certificateBytes, string originalData, string signatureBase64);
        bool VerifyData(PKCS10Adapter.PublicCertificateInfo publicCertificateInfo, string originalData, string signatureBase64);
    }

    public class PKCS10Adapter : IPKCS10Adapter
    {
        #region Dependencies

        private readonly IFileSystemAdapter _fsa;

        #endregion

        #region Default properties

        private readonly HashAlgorithmName _defaultHashAlgorithm = HashAlgorithmName.SHA256;
        private readonly RSASignaturePadding _defaultSignaturePadding = RSASignaturePadding.Pkcs1;

        #endregion

        #region Constructor

        public PKCS10Adapter(IFileSystemAdapter fsa)
        {
            _fsa = fsa ?? throw new ArgumentNullException(nameof(fsa));
        }

        #endregion

        #region Methods

        public PublicCertificateInfo GetPublicCertificateInfo(string filename)
        {
            if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentNullException(nameof(filename));
            if (!_fsa.FileExists(filename)) throw new FileNotFoundException(nameof(filename));

            var certificateBytesResult = _fsa.FileReadAllBytes(filename);
            if (certificateBytesResult.OperationCompletedSuccessfully)
            {
                var certificateBytes = (byte[])certificateBytesResult.Value;
                return GetPublicCertificateInfo(certificateBytes);
            }
            else
            {
                throw new Exception(certificateBytesResult.Message);
            }
        }

        public PublicCertificateInfo GetPublicCertificateInfo(byte[] certificateBytes)
        {
            if (certificateBytes == null) throw new ArgumentNullException(nameof(certificateBytes));

            var certificate = new X509Certificate2(certificateBytes);

            var serialNumberByteArray = certificate.GetSerialNumber();
            Array.Reverse(serialNumberByteArray, 0, serialNumberByteArray.Length);
            var serialNumber = Encoding.UTF8.GetString(serialNumberByteArray);

            var certificateByteArray = certificate.GetRawCertData();
            var certificateBase64String = Convert.ToBase64String(certificateByteArray);

            // Setting the DateTimeKind to UTC
            var dateIssued = DateTime.SpecifyKind(certificate.NotBefore.ToUniversalTime(), DateTimeKind.Utc);
            var dateExpiration = DateTime.SpecifyKind(certificate.NotAfter.ToUniversalTime(), DateTimeKind.Utc);

            return new PublicCertificateInfo
            {
                SerialNumber = serialNumber,
                CertificateBase64String = certificateBase64String,
                UTCDateIssued = dateIssued,
                UTCDateExpiration = dateExpiration
            };
        }

        public bool VerifyData(string filename, string originalData, string signatureBase64)
        {
            if (string.IsNullOrWhiteSpace(originalData)) throw new ArgumentNullException(nameof(originalData));
            if (string.IsNullOrWhiteSpace(signatureBase64)) throw new ArgumentNullException(nameof(signatureBase64));

            var publicCertificateInfo = GetPublicCertificateInfo(filename);
            return VerifyData(publicCertificateInfo, originalData, signatureBase64);
        }

        public bool VerifyData(byte[] certificateBytes, string originalData, string signatureBase64)
        {
            if (string.IsNullOrWhiteSpace(originalData)) throw new ArgumentNullException(nameof(originalData));
            if (string.IsNullOrWhiteSpace(signatureBase64)) throw new ArgumentNullException(nameof(signatureBase64));

            var publicCertificateInfo = GetPublicCertificateInfo(certificateBytes);
            return VerifyData(publicCertificateInfo, originalData, signatureBase64);
        }

        public bool VerifyData(PublicCertificateInfo publicCertificateInfo, string originalData, string signatureBase64)
        {
            if (publicCertificateInfo == null) throw new ArgumentNullException(nameof(publicCertificateInfo));
            if (string.IsNullOrWhiteSpace(originalData)) throw new ArgumentNullException(nameof(originalData));
            if (string.IsNullOrWhiteSpace(signatureBase64)) throw new ArgumentNullException(nameof(signatureBase64));

            var certificateBytes = Convert.FromBase64String(publicCertificateInfo.CertificateBase64String);
            var certificate = new X509Certificate2(certificateBytes);
            using (var sha256 = SHA256.Create())
            {
                using (var rsa = certificate.GetRSAPublicKey())
                {
                    var originalDataBytes = Encoding.UTF8.GetBytes(originalData);
                    var signatureBytes = Convert.FromBase64String(signatureBase64);
                    return rsa.VerifyData(originalDataBytes, signatureBytes, _defaultHashAlgorithm, _defaultSignaturePadding);
                }
            }
        }

        #endregion

        #region Local classes

        public class PublicCertificateInfo
        {
            public string SerialNumber { get; set; }
            public string CertificateBase64String { get; set; }
            public DateTime UTCDateIssued { get; set; }
            public DateTime UTCDateExpiration { get; set; }
        }

        #endregion
    }
}
