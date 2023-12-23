using System.Security.Cryptography;

public static class PasswordGenerator
{

    private static readonly char[] chars = 
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789/&,%.<>[]{}()-_=+*^?#!@$".ToCharArray();
    public static string Generate(int length)
    {
        using var rng = RandomNumberGenerator.Create();
        
        var passwordChars = new char[length];
        byte[] randomBytes = new byte[length];

        rng.GetBytes(randomBytes);

        for (int i = 0; i < length; i++)
        {
            int charIndex = randomBytes[i] % chars.Length;
            passwordChars[i] = chars[charIndex];
        }
        return new string(passwordChars);
    }
}