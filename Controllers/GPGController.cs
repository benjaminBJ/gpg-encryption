namespace GPGTest.Controllers;

using GPGTest.Models;
using GPGTest.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Authentication;

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
        try {
            var encryptedText = _gpgEncryptionHelper.Encrypt(data.PlainText);
            return Ok(encryptedText);
        }
        catch (Exception ex)
        {
            return MapExceptionsToHttp(ex);
        }
    }

    [HttpPost("decrypt")]
    public IActionResult Decrypt([FromBody] DecryptRequest request)
    {
        try
        {
            var decryptedText = _gpgEncryptionHelper.Decrypt(request.EncryptedText, request.Passphrase);
            return Ok(decryptedText);
        }
        catch (Exception ex)
        {
            return MapExceptionsToHttp(ex);
        }
    }

    private IActionResult MapExceptionsToHttp(Exception ex)
    {
        if (ex.Message != null)
        {
            return StatusCode(500, new { message = ex.Message });

        }
        return StatusCode(500, new { message = "An error occurred" });

    }
   }
