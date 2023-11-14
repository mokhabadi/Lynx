using System.Security.Cryptography;

namespace Lynx
{
    public static class Signer
    {
        readonly static int keySize = 2048;
        readonly static HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA256;
        readonly static RSASignaturePadding padding = RSASignaturePadding.Pkcs1;

        public static (byte[] publicKey, byte[] privateKey) MakeKeys()
        {
            RSACryptoServiceProvider RSA = new(keySize);
            byte[] publicKey = RSA.ExportCspBlob(false);
            byte[] privateKey = RSA.ExportCspBlob(true);
            return (publicKey, privateKey);
        }

        public static byte[] Sign(byte[] privateKey, byte[] bytes)
        {
            RSACryptoServiceProvider RSA = new();
            RSA.ImportCspBlob(privateKey);
            return RSA.SignData(bytes, hashAlgorithm, padding);
        }

        public static bool Verify(byte[] publicKey, byte[] bytes, byte[] signedBytes)
        {
            RSACryptoServiceProvider RSA = new();
            RSA.ImportCspBlob(publicKey);
            return RSA.VerifyData(bytes, signedBytes, hashAlgorithm, padding);
        }
    }
}
