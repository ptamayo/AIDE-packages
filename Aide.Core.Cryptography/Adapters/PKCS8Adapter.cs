using Aide.Core.Interfaces;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Aide.Core.Cryptography.Adapters
{
    public interface IPKCS8Adapter
    {
        string GenerateBase64Signature(string filename, string secret, string data);
        string GenerateBase64Signature(byte[] certificateBytes, string secret, string data);
        bool VerifyData(string filename, string secret, string originalData, string signatureBase64);
        bool VerifyData(byte[] certificateBytes, string secret, string originalData, string signatureBase64);
    }

    public class PKCS8Adapter : IPKCS8Adapter, IDisposable
    {
        #region Dependencies

        private readonly IFileSystemAdapter _fsa;
        private bool _disposed = false;

        #endregion

        #region Default properties

        private readonly SHA256 _defaultHashAlgorithmInstance = SHA256.Create();
        private readonly HashAlgorithmName _defaultHashAlgorithm = HashAlgorithmName.SHA256;
        private readonly RSASignaturePadding _defaultSignaturePadding = RSASignaturePadding.Pkcs1;

        #endregion

        #region Constructor

        public PKCS8Adapter(IFileSystemAdapter fsa)
        {
            _fsa = fsa ?? throw new ArgumentNullException(nameof(fsa));
        }

        #endregion

        #region Methods

        public string GenerateBase64Signature(string filename, string secret, string data)
        {
            if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentNullException(nameof(filename));
            if (!_fsa.FileExists(filename)) throw new FileNotFoundException(nameof(filename));
            if (string.IsNullOrWhiteSpace(secret)) throw new ArgumentNullException(nameof(secret));
            if (string.IsNullOrWhiteSpace(data)) throw new ArgumentNullException(nameof(data));

            byte[] certificateBytes;
            var certificateBytesResult = _fsa.FileReadAllBytes(filename);
            if (certificateBytesResult.OperationCompletedSuccessfully)
            {
                certificateBytes = (byte[])certificateBytesResult.Value;
            }
            else
            {
                throw new Exception(certificateBytesResult.Message);
            }

            return GenerateBase64Signature(certificateBytes, secret, data);
        }

        public string GenerateBase64Signature(byte[] certificateBytes, string secret, string data)
        {
            if (certificateBytes == null) throw new ArgumentNullException(nameof(certificateBytes));
            if (string.IsNullOrWhiteSpace(secret)) throw new ArgumentNullException(nameof(secret));
            if (string.IsNullOrWhiteSpace(data)) throw new ArgumentNullException(nameof(data));

            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                var secretBytes = Encoding.UTF8.GetBytes(secret);
                RSA.ImportEncryptedPkcs8PrivateKey(secretBytes, certificateBytes, out _); // If the certificate is Base64-encoded or in the PEM text format, then you must Base64-decode the contents before calling this method.
                var dataBytes = Encoding.UTF8.GetBytes(data);
                var signedDataBytes = RSA.SignData(dataBytes, _defaultHashAlgorithm, _defaultSignaturePadding);
                var signedDataBase64 = Convert.ToBase64String(signedDataBytes);
                return signedDataBase64;
            }
        }

        public bool VerifyData(string filename, string secret, string originalData, string signatureBase64)
        {
            if (string.IsNullOrWhiteSpace(filename)) throw new ArgumentNullException(nameof(filename));
            if (!_fsa.FileExists(filename)) throw new FileNotFoundException(nameof(filename));
            if (string.IsNullOrWhiteSpace(secret)) throw new ArgumentNullException(nameof(secret));
            if (string.IsNullOrWhiteSpace(originalData)) throw new ArgumentNullException(nameof(originalData));
            if (string.IsNullOrWhiteSpace(signatureBase64)) throw new ArgumentNullException(nameof(signatureBase64));

            byte[] certificateBytes;
            var certificateBytesResult = _fsa.FileReadAllBytes(filename);
            if (certificateBytesResult.OperationCompletedSuccessfully)
            {
                certificateBytes = (byte[])certificateBytesResult.Value;
            }
            else
            {
                throw new Exception(certificateBytesResult.Message);
            }

            return VerifyData(certificateBytes, secret, originalData, signatureBase64);
        }

        public bool VerifyData(byte[] certificateBytes, string secret, string originalData, string signatureBase64)
        {
            if (string.IsNullOrWhiteSpace(originalData)) throw new ArgumentNullException(nameof(originalData));
            if (string.IsNullOrWhiteSpace(signatureBase64)) throw new ArgumentNullException(nameof(signatureBase64));

            using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                var secretBytes = Encoding.UTF8.GetBytes(secret);
                RSA.ImportEncryptedPkcs8PrivateKey(secretBytes, certificateBytes, out _); // If the certificate is Base64-encoded or in the PEM text format, then you must Base64-decode the contents before calling this method.

                var privateKeyParameters = RSA.ExportParameters(true);
                RSA.ImportParameters(privateKeyParameters);

                var originalDataBytes = Encoding.UTF8.GetBytes(originalData);
                var signatureBytes = Convert.FromBase64String(signatureBase64);
                return RSA.VerifyData(originalDataBytes, _defaultHashAlgorithmInstance, signatureBytes);
            }
        }

        #endregion

        #region Dispose

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
                    //if (_obj != null)
                    //{
                    //    _obj.Dispose();
                    //}
                }
                // Release unmanaged resources.
                // Set large fields to null.                
                _disposed = true;
            }
        }

        ~PKCS8Adapter()
        {
            Dispose(false);
        }

        #endregion
    }
}
