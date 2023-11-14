using System.Security.Cryptography;
using System.Text;

namespace Lynx
{
    public static class Signer
    {
        readonly static HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA256;
        readonly static RSASignaturePadding padding = RSASignaturePadding.Pss;

        public static (byte[] publicKey, byte[] privateKey) MakeKeys()
        {
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider(2048);
            var privateParameters = RSA.ExportParameters(true);
            var publicParameters = RSA.ExportParameters(false);

            var privateKey = Packer.ToJson(privateParameters);
            var publicKey = Packer.ToJson(publicParameters);

            return (Encoding.UTF8.GetBytes(privateKey), Encoding.UTF8.GetBytes(publicKey));
        }

        public static byte[] Sign(byte[] privateKey, byte[] bytes)
        {
            var parameters = Packer.FromJson<RSAParameters>(Encoding.UTF8.GetString(privateKey));
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSA.ImportParameters(parameters);

            return RSA.SignData(bytes, hashAlgorithm, padding);
        }

        public static bool Verify(byte[] publicKey, byte[] bytes, byte[] signedBytes)
        {
            var parameters = Packer.FromJson<RSAParameters>(Encoding.UTF8.GetString(publicKey));
            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSA.ImportParameters(parameters);

            return RSA.VerifyData(bytes, signedBytes, hashAlgorithm, padding);
        }
    }
}
