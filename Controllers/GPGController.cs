namespace GPGTest.Controllers;

using GPGTest.Services;
using Microsoft.AspNetCore.Mvc;
using System.IO;


[ApiController]
[Route("api/[controller]")]
public class GpgController : ControllerBase
{
    private readonly GpgService _gpgService;

    public GpgController()
    {
        // Pass the paths to the public and private keys here
        _gpgService = new GpgService("Keys/public.asc", "Keys/private.asc");
    }

    [HttpPost("encrypt")]
    public IActionResult Encrypt([FromBody] EncryptRequest data)
    {
        var encryptedText = _gpgService.Encrypt(data.PlainText);
        return Ok(encryptedText);
    }

    [HttpPost("decrypt")]
    public IActionResult Decrypt([FromBody] DecryptRequest request)
    {
        var decryptedText = _gpgService.Decrypt(request.EncryptedText, request.Passphrase);
        return Ok(decryptedText);
    }
}
public class EncryptRequest
{
    public string PlainText { get; set; } = "";
}

public class DecryptRequest
{
    public string EncryptedText { get; set; } = "";
    public string Passphrase { get; set; } = "";
}
