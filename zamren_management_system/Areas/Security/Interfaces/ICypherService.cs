namespace zamren_management_system.Areas.Security.Interfaces;

public interface ICypherService
{
    /// <summary>
    /// Encrypts a plaintext string.
    /// </summary>
    /// <param name="val">The plaintext string to encrypt.</param>
    /// <returns>The encrypted string.</returns>
    public string Encrypt(object? val);

    /// <summary>
    /// Decrypts an encrypted string.
    /// </summary>
    /// <param name="val">The encrypted string to decrypt.</param>
    /// <returns>The decrypted string.</returns>
    public string Decrypt(object? val);
}