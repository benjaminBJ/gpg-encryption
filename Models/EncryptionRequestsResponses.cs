namespace GPGTest.Models;

public class EncryptRequest
{
    public string PlainText { get; set; } = "";
}

public class DecryptRequest
{
    public string EncryptedText { get; set; } = "";
    public string Passphrase { get; set; } = "";
}
