using System;
using System.Security.Cryptography;
using System.Text;
using System.IO;

namespace ECARTemplate.Services
{
    public class AesEncryptor
    {
        // Genera estas claves e IVs aleatoriamente y guárdalas de forma segura.

        // Clave de 32 bytes para AES-256 (256 bits)
        // Cada par de caracteres hexadecimales (00-FF) representa un byte.
        // Asegúrate de que haya exactamente 32 bytes aquí.
        private static readonly byte[] Key = new byte[32] {
            0x2B, 0x7E, 0x15, 0x16, 0x28, 0xAE, 0xD2, 0xA6,
            0xAB, 0xF7, 0x15, 0x88, 0x09, 0xCF, 0x4F, 0x3C,
            0xA5, 0x6E, 0x29, 0x74, 0x11, 0x7C, 0xA9, 0x93,
            0x2B, 0x5D, 0x2B, 0x56, 0x5F, 0x57, 0x00, 0x49
        };

        // IV (Initialization Vector) de 16 bytes (128 bits)
        // El IV debe ser único para cada operación de encriptación por seguridad.
        // Aquí se usa uno fijo solo para pruebas.
        private static readonly byte[] IV = new byte[16] {
            0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07,
            0x08, 0x09, 0x0A, 0x0B, 0x0C, 0x0D, 0x0E, 0x0F
        };

        // =========================================================

        public static string Encrypt(string plainText)
        {
            if (string.IsNullOrEmpty(plainText))
                return plainText;

            using (Aes aesAlg = Aes.Create())
            {
                // NO establecer KeySize. Se infiere de la longitud de Key.
                aesAlg.Key = Key; // Asignar el array de 32 bytes
                aesAlg.IV = IV;   // Asignar el array de 16 bytes

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                        {
                            swEncrypt.Write(plainText);
                        }
                        return Convert.ToBase64String(msEncrypt.ToArray());
                    }
                }
            }
        }

        public static string Decrypt(string cipherText)
        {
            if (string.IsNullOrEmpty(cipherText))
                return cipherText;

            byte[] cipherBytes = Convert.FromBase64String(cipherText);

            using (Aes aesAlg = Aes.Create())
            {
                // NO establecer KeySize. Se infiere de la longitud de Key.
                aesAlg.Key = Key; // Asignar el array de 32 bytes
                aesAlg.IV = IV;   // Asignar el array de 16 bytes

                ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msDecrypt = new MemoryStream(cipherBytes))
                {
                    using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                    {
                        using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                        {
                            return srDecrypt.ReadToEnd();
                        }
                    }
                }
            }
        }
    }
}