namespace GPGTest.Services;

using Org.BouncyCastle.Bcpg;
using Org.BouncyCastle.Bcpg.OpenPgp;
using Org.BouncyCastle.Security;
using System.IO;
using System.Text;
public class GpgService
{
    private readonly string _publicKeyPath;
    private readonly string _privateKeyPath;

    public GpgService(string publicKeyPath, string privateKeyPath)
    {
        _publicKeyPath = publicKeyPath;
        _privateKeyPath = privateKeyPath;
    }

    // Encrypts the input string using the public key
    public string Encrypt(string input)
    {
        using (var publicKeyStream = File.OpenRead(_publicKeyPath))
        using (var outputStream = new MemoryStream())
        {
            var clearData = Encoding.UTF8.GetBytes(input); // Convert input to UTF-8 bytes
            PgpPublicKey pubKey = ReadPublicKey(publicKeyStream);

            var encryptedDataGenerator = new PgpEncryptedDataGenerator(SymmetricKeyAlgorithmTag.Aes256, true, new SecureRandom());
            encryptedDataGenerator.AddMethod(pubKey);

            using (var armoredOutputStream = new ArmoredOutputStream(outputStream))
            using (var encryptionStream = encryptedDataGenerator.Open(armoredOutputStream, clearData.Length))
            {
                encryptionStream.Write(clearData, 0, clearData.Length);
            }

            return Convert.ToBase64String(outputStream.ToArray()); // Return base64 encoded string
        }
    }

    // Decrypts the encrypted text using the private key and passphrase
    public string Decrypt(string encryptedText, string passphrase)
    {
        using (var privateKeyStream = File.OpenRead(_privateKeyPath))
        using (var inputStream = new MemoryStream(Convert.FromBase64String(encryptedText)))
        {
            PgpObjectFactory pgpF = new PgpObjectFactory(PgpUtilities.GetDecoderStream(inputStream));
            PgpEncryptedDataList enc = (PgpEncryptedDataList)(pgpF.NextPgpObject() ?? pgpF.NextPgpObject());

            PgpSecretKeyRingBundle pgpSec = new PgpSecretKeyRingBundle(PgpUtilities.GetDecoderStream(privateKeyStream));

            foreach (PgpPublicKeyEncryptedData pked in enc.GetEncryptedDataObjects())
            {
                PgpPrivateKey privateKey = FindSecretKey(pgpSec, pked.KeyId, passphrase.ToCharArray());
                if (privateKey != null)
                {
                    using (var clearStream = pked.GetDataStream(privateKey))
                    using (var reader = new StreamReader(clearStream))
                    {
                        return reader.ReadToEnd(); // Return decrypted text
                    }
                }
            }
        }

        throw new ArgumentException("Secret key for decryption not found.");
    }

    // Reads the public key from the stream
    private PgpPublicKey ReadPublicKey(Stream input)
    {
        PgpPublicKeyRingBundle pgpPub = new PgpPublicKeyRingBundle(PgpUtilities.GetDecoderStream(input));
        foreach (PgpPublicKeyRing keyRing in pgpPub.GetKeyRings())
        {
            foreach (PgpPublicKey key in keyRing.GetPublicKeys())
            {
                if (key.IsEncryptionKey)
                {
                    return key;
                }
            }
        }
        throw new ArgumentException("Encryption key not found in the public key ring.");
    }

    // Finds the secret (private) key from the key ring bundle using the key ID and passphrase
    private PgpPrivateKey FindSecretKey(PgpSecretKeyRingBundle pgpSec, long keyId, char[] passphrase)
    {
        PgpSecretKey secretKey = pgpSec.GetSecretKey(keyId);
        return secretKey?.ExtractPrivateKey(passphrase);
    }
}

