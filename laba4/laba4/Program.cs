using System.Text;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Parameters;

class Program
{
    static void Main()
    {
       
        string inputFile = "input.txt";
        string encryptedFile = "encrypted.bin";
        string decryptedFile = "decrypted.txt";
        
        string keyFile = "key.bin";
        byte[] newKey = GenerateKey();
        File.WriteAllBytes(keyFile, newKey);
        
        byte[] key = File.ReadAllBytes(keyFile);

        Console.Write("Используемый ключ: ");
        foreach (var b in key)
        {
            Console.Write(b);   
        }
        Console.WriteLine();
        
        string originalText = File.ReadAllText(inputFile, Encoding.UTF8);
        
        Console.Write("Оригинальный текст: ");
        Console.WriteLine(originalText);

        
        EncryptFile(inputFile, encryptedFile, keyFile);
        byte[] encryptedData = File.ReadAllBytes(encryptedFile);
        
        Console.Write("Зашифрованный текст: ");
        foreach (var b in encryptedData)
        {
            Console.Write(b);   
        }
        Console.WriteLine();
        
        DecryptFile(encryptedFile, decryptedFile, keyFile);
        string decryptedText = File.ReadAllText(decryptedFile, Encoding.UTF8);
        Console.Write("Расшифрованный текст: ");
        Console.Write(decryptedText);
    }

    static byte[] GenerateKey()
    {
        byte[] key = new byte[16];
        new Random().NextBytes(key);
        return key;
    }

    static void EncryptFile(string inputFile, string outputFile, string keyFile)
    {
        try
        {
            byte[] key = File.ReadAllBytes(keyFile);

            var engine = new IdeaEngine();
            var keyParam = new KeyParameter(key);
            var cipher = new BufferedBlockCipher(engine);
            cipher.Init(true, keyParam);

            byte[] originalData = File.ReadAllBytes(inputFile);
            
            byte[] paddedData = PadData(originalData);
            Console.Write("Оригинальный текст в байтах: ");
            foreach (var b in paddedData)
            {
                Console.Write(b);   
            }
            Console.WriteLine();
            byte[] encryptedData = cipher.ProcessBytes(paddedData);
            

            File.WriteAllBytes(outputFile, encryptedData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при шифровании: {ex.Message}");
        }
    }

    static void DecryptFile(string inputFile, string outputFile, string keyFile)
    {
        try
        {
            byte[] key = File.ReadAllBytes(keyFile);
            if (key.Length != 16)
                throw new ArgumentException("Длина ключа должна составлять 16 байт.");

            var engine = new IdeaEngine();
            var keyParam = new KeyParameter(key);
            var cipher = new BufferedBlockCipher(engine);
            cipher.Init(false, keyParam);

            byte[] encryptedData = File.ReadAllBytes(inputFile);
            byte[] decryptedData = cipher.ProcessBytes(encryptedData);
            byte[] unpaddedData = UnpadData(decryptedData);

            File.WriteAllBytes(outputFile, unpaddedData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при дешифровании: {ex.Message}");
        }
    }

    static byte[] PadData(byte[] data)
    {
        int padLength = 8 - (data.Length % 8);
        byte[] paddedData = new byte[data.Length + padLength];
        Array.Copy(data, paddedData, data.Length);

        for (int i = data.Length; i < paddedData.Length; i++)
        {
            paddedData[i] = (byte)padLength;
        }

        return paddedData;
    }

    static byte[] UnpadData(byte[] data)
    {
        int padLength = data[data.Length - 1];
        byte[] unpaddedData = new byte[data.Length - padLength];
        Array.Copy(data, unpaddedData, unpaddedData.Length);

        return unpaddedData;
    }
}