using System.Diagnostics;
using System.IO;
using System.Text;

namespace GPGTest.Services;

public class GpgEncryptionHelper
{
    private readonly string _publicKeyPath;
    private readonly string _privateKeyPath;
    private readonly string _tempFolderPath;

    public GpgEncryptionHelper(string publicKeyPath, string privateKeyPath)
    {
        _publicKeyPath = publicKeyPath;
        _privateKeyPath = publicKeyPath;

        // Define a folder for temporary files within the deployment directory
        _tempFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "TempFiles");

        // Create the folder if it doesn't exist
        if (!Directory.Exists(_tempFolderPath))
        {
            Directory.CreateDirectory(_tempFolderPath);
        }
    }

    public string Encrypt(string input)
    {
        string tempInputFile = Path.Combine(_tempFolderPath, Path.GetRandomFileName());
        string tempOutputFile = Path.Combine(_tempFolderPath, Path.GetRandomFileName());

        File.WriteAllText(tempInputFile, input);

        string arguments = $"--encrypt --recipient-file \"{_publicKeyPath}\" --output \"{tempOutputFile}\" \"{tempInputFile}\"";
        ExecuteGpgCommand(arguments);

        byte[] encryptedBytes = File.ReadAllBytes(tempOutputFile);
        string encryptedBase64 = Convert.ToBase64String(encryptedBytes);

        File.Delete(tempInputFile);
        File.Delete(tempOutputFile);

        return encryptedBase64;
    }

    public string Decrypt(string encryptedText, string passphrase)
    {
        string tempInputFile = Path.Combine(_tempFolderPath, Path.GetRandomFileName());
        string tempOutputFile = Path.Combine(_tempFolderPath, Path.GetRandomFileName());

        byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
        File.WriteAllBytes(tempInputFile, encryptedBytes);

        string arguments = $"--batch --yes --passphrase \"{passphrase}\" --pinentry-mode loopback --decrypt --output \"{tempOutputFile}\" \"{tempInputFile}\""; ExecuteGpgCommand(arguments);
        ExecuteGpgCommand(arguments);

        string decryptedText = File.ReadAllText(tempOutputFile);

        File.Delete(tempInputFile);
        File.Delete(tempOutputFile);

        return decryptedText;
    }

    private void ExecuteGpgCommand(string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = @"C:\Program Files (x86)\GnuPG\bin\gpg.exe",  // Absolute path to gpg.exe
                Arguments = $"--batch --yes {arguments}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                output.AppendLine(e.Data);
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
                error.AppendLine(e.Data);
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"GPG command failed: {error}");
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Failed to execute GPG command: {ex.Message}. Error output: {error}");
        }
    }
}
