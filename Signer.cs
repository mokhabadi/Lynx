using System.Security.Cryptography;

namespace Lynx
{
    public static class Signer
    {
        readonly static HashAlgorithmName hashAlgorithm = HashAlgorithmName.SHA256;
        readonly static RSASignaturePadding padding = RSASignaturePadding.Pss;

        public static byte[] Sign(byte[] privateKey, byte[] bytes)
        {
            RSA RSA = RSA.Create();
            RSA.ImportRSAPrivateKey(privateKey, out _);
            return RSA.SignData(bytes, hashAlgorithm, padding);
        }

        public static bool Verify(byte[] publicKey, byte[] bytes, byte[] signedBytes)
        {
            RSA RSA = RSA.Create();
            RSA.ImportRSAPublicKey(publicKey, out _);
            return RSA.VerifyData(bytes, signedBytes, hashAlgorithm, padding);
        }
    }
}
