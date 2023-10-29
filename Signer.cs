using System.Security.Cryptography;

namespace Lynx
{
    public static class Signer
    {
        public static byte[] Sign(byte[] key, byte[] bytes)
        {
            RSA RSA = RSA.Create();
            RSA.ImportRSAPrivateKey(key, out _);
            return RSA.SignData(bytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        }

        public static bool Verify(byte[] key, byte[] bytes, byte[] signedBytes)
        {
            RSA RSA = RSA.Create();
            RSA.ImportRSAPublicKey(key, out _);
            return RSA.VerifyData(bytes, signedBytes, HashAlgorithmName.SHA256, RSASignaturePadding.Pss);
        }
    }
}
