using System.Diagnostics;
using System.IO;
using System.Text;

namespace GPGTest.Services
{
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
            // Write input to a temporary file
            string tempInputFile = Path.GetTempFileName();
            string tempOutputFile = Path.GetTempFileName();
            File.WriteAllText(tempInputFile, input);

            // Call GPG to encrypt the file
            string arguments = $"--encrypt --recipient-file \"{_publicKeyPath}\" --output \"{tempOutputFile}\" \"{tempInputFile}\"";
            ExecuteGpgCommand(arguments);

            // Read and return the encrypted file as base64 string
            byte[] encryptedBytes = File.ReadAllBytes(tempOutputFile);
            string encryptedBase64 = Convert.ToBase64String(encryptedBytes);

            // Clean up temporary files
            File.Delete(tempInputFile);
            File.Delete(tempOutputFile);

            return encryptedBase64;
        }

        // Decrypts the encrypted text using the private key and passphrase
        public string Decrypt(string encryptedText, string passphrase)
        {
            // Write encrypted base64 string to a temporary file
            string tempInputFile = Path.GetTempFileName();
            string tempOutputFile = Path.GetTempFileName();
            byte[] encryptedBytes = Convert.FromBase64String(encryptedText);
            File.WriteAllBytes(tempInputFile, encryptedBytes);

            // Call GPG to decrypt the file
            string arguments = $"--batch --yes --passphrase \"{passphrase}\" --pinentry-mode loopback --decrypt --output \"{tempOutputFile}\" \"{tempInputFile}\""; ExecuteGpgCommand(arguments);

            // Read and return the decrypted file contents
            string decryptedText = File.ReadAllText(tempOutputFile);

            // Clean up temporary files
            File.Delete(tempInputFile);
            File.Delete(tempOutputFile);

            return decryptedText;
        }

        // Executes GPG command with provided arguments
        private void ExecuteGpgCommand(string arguments)
        {
            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "gpg",
                    Arguments = $"--batch --yes {arguments}", // Ensure non-interactive mode
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
                    throw new Exception($"GPG command failed: {error.ToString()}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to execute GPG command: {ex.Message}. Error output: {error.ToString()}");
            }
        }
    }
}
