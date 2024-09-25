namespace GPGTest.Controllers;

using GPGTest.Models;
using GPGTest.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class GpgController : ControllerBase
{
    private readonly GpgEncryptionHelper _gpgEncryptionHelper;

    public GpgController()
    {
        // Pass the paths to the public and private keys here
        _gpgEncryptionHelper = new GpgEncryptionHelper("Keys/public.asc", "Keys/private.asc");
    }

    [HttpPost("encrypt")]
    public IActionResult Encrypt([FromBody] EncryptRequest data)
    {
        var encryptedText = _gpgEncryptionHelper.Encrypt(data.PlainText);
        return Ok(encryptedText);
    }

    [HttpPost("decrypt")]
    public IActionResult Decrypt([FromBody] DecryptRequest request)
    {
        var decryptedText = _gpgEncryptionHelper.Decrypt(request.EncryptedText, request.Passphrase);
        return Ok(decryptedText);
    }
}
