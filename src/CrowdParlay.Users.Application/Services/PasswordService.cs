using System.Security.Cryptography;
using CrowdParlay.Users.Application.Abstractions;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;

namespace CrowdParlay.Users.Application.Services;

public class PasswordService : IPasswordService
{
    public string HashPassword(string password)
    {
        // Source: https://github.com/dotnet/aspnetcore/blob/4963b764e3c03473022c75f13b7f82c531650001/src/Identity/Extensions.Core/src/PasswordHasher.cs#L141

        const int saltSize = 128 / 8;
        const int subkeyLength = 256 / 8;
        const int iterationCount = 100_000;
        const KeyDerivationPrf prf = KeyDerivationPrf.HMACSHA512;

        var salt = RandomNumberGenerator.GetBytes(saltSize);
        var subkey = KeyDerivation.Pbkdf2(password, salt, prf, iterationCount, subkeyLength);

        var outputBytes = new byte[13 + salt.Length + subkey.Length];
        
        // Byte [0] is format marker
        // In this case, 0x01 indicates the password is stored in the v3 format
        outputBytes[0] = 0x01;
        // Bytes [1-4] are PRF which is used for the key derivation
        // In this case, 2 indicates HMACSHA512
        WriteNetworkByteOrder(outputBytes, 1, (uint)prf);
        // Bytes [5-8] are number of iterations of the pseudo-random function to apply during the key derivation process
        // In this case, 100 000
        WriteNetworkByteOrder(outputBytes, 5, iterationCount);
        // Bytes [9-12] are salt length in bytes
        // In this case, 16
        WriteNetworkByteOrder(outputBytes, 9, saltSize);
        // Bytes [13-28] are salt
        Buffer.BlockCopy(salt, 0, outputBytes, 13, salt.Length);
        // Bytes [29-60] are subkey
        Buffer.BlockCopy(subkey, 0, outputBytes, 13 + saltSize, subkey.Length);

        return Convert.ToBase64String(outputBytes);
    }

    public bool VerifyPassword(string hashedPassword, string password)
    {
        // Source: https://github.com/dotnet/aspnetcore/blob/4963b764e3c03473022c75f13b7f82c531650001/src/Identity/Extensions.Core/src/PasswordHasher.cs#L250

        var hashedPasswordBytes = Convert.FromBase64String(hashedPassword);

        try
        {
            // Read header information
            var prf = (KeyDerivationPrf)ReadNetworkByteOrder(hashedPasswordBytes, 1);
            var iterationCount = (int)ReadNetworkByteOrder(hashedPasswordBytes, 5);
            var saltLength = (int)ReadNetworkByteOrder(hashedPasswordBytes, 9);

            // Read the salt: must be >= 128 bits
            if (saltLength < 128 / 8)
                return false;

            var salt = new byte[saltLength];
            Buffer.BlockCopy(hashedPasswordBytes, 13, salt, 0, salt.Length);

            // Read the subkey (the rest of the payload): must be >= 128 bits
            var subkeyLength = hashedPasswordBytes.Length - 13 - salt.Length;
            if (subkeyLength < 128 / 8)
                return false;

            var expectedSubkey = new byte[subkeyLength];
            Buffer.BlockCopy(hashedPasswordBytes, 13 + salt.Length, expectedSubkey, 0, expectedSubkey.Length);

            // Hash the incoming password and verify it
            var actualSubkey = KeyDerivation.Pbkdf2(password, salt, prf, iterationCount, subkeyLength);
            return CryptographicOperations.FixedTimeEquals(actualSubkey, expectedSubkey);
        }
        catch
        {
            // This should never occur except in the case of a malformed payload, where
            // we might go off the end of the array. Regardless, a malformed payload
            // implies verification failed.
            return false;
        }
    }

    private static uint ReadNetworkByteOrder(IReadOnlyList<byte> buffer, int offset) =>
        (uint)(buffer[offset + 0] << 24) |
        (uint)(buffer[offset + 1] << 16) |
        (uint)(buffer[offset + 2] << 8) |
        (uint)(buffer[offset + 3] << 0);

    private static void WriteNetworkByteOrder(IList<byte> buffer, int offset, uint value)
    {
        buffer[offset + 0] = (byte)(value >> 24);
        buffer[offset + 1] = (byte)(value >> 16);
        buffer[offset + 2] = (byte)(value >> 8);
        buffer[offset + 3] = (byte)(value >> 0);
    }
}