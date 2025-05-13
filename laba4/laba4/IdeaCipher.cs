using System.Security.Cryptography;
using System.Text;

class IdeaCipher
{
    private const int TWO_SIXTEEN = 65536;
    private const uint TWO_SIXTEEN_PLUS_1 = 65537;
    private const int BLOCK_SIZE_BITS = 64;
    private const int BLOCK_SIZE_BYTES = 8;
    private const string KEY_FILE = "key.bin";

    static void Мain()
    {
        string inputFile = "input.txt";
        string encryptedFile = "encrypted.bin";
        string decryptedFile = "decrypted.txt";
        
        string keyFile = "key.bin";
        byte[] key = GenerateRandomKey();
        File.WriteAllBytes(KEY_FILE, key);
        
        EncryptFile(key);
        DecryptFile(key);
    }

    private static byte[] GenerateRandomKey()
    {
        using var rng = RandomNumberGenerator.Create();
        byte[] key = new byte[16];
        rng.GetBytes(key);
        return key;
    }

    private static void EncryptFile(byte[] key)
    {
        string inputFile = "input.txt";
        string outputFile = "output.txt";

        byte[] plaintext = File.ReadAllBytes(inputFile);
        byte[] paddedData = AddPadding(plaintext);
        byte[] encryptedData = ProcessIdea(paddedData, key, false);
        File.WriteAllBytes(outputFile, encryptedData);
        Console.WriteLine("Данные зашифрованы и сохранены в " + outputFile);
    }

    private static void DecryptFile(byte[] key)
    {
        string inputFile = "output.txt";
        string outputFile = "decrypted.txt";

        byte[] encryptedData = File.ReadAllBytes(inputFile);
        byte[] decryptedData = ProcessIdea(encryptedData, key, true);
        byte[] unpaddedData = RemovePadding(decryptedData);
        File.WriteAllBytes(outputFile, unpaddedData);
    }

    private static byte[] AddPadding(byte[] data)
    {
        int padLength = BLOCK_SIZE_BYTES - (data.Length % BLOCK_SIZE_BYTES);
        if (padLength == 0) padLength = BLOCK_SIZE_BYTES;
        
        byte[] padded = new byte[data.Length + padLength];
        Array.Copy(data, padded, data.Length);
        for (int i = data.Length; i < padded.Length; i++)
        {
            padded[i] = (byte)padLength;
        }
        return padded;
    }

    private static byte[] RemovePadding(byte[] data)
    {
        if (data.Length == 0) return data;
        
        int padLength = data[data.Length - 1];
            
        byte[] unpadded = new byte[data.Length - padLength];
        Array.Copy(data, unpadded, unpadded.Length);
        return unpadded;
    }

    private static byte[] ProcessIdea(byte[] data, byte[] key, bool decrypt)
    {
        List<byte> result = new List<byte>();
        string keyBits = BytesToBinaryString(key);

        for (int i = 0; i < data.Length; i += BLOCK_SIZE_BYTES)
        {
            byte[] block = new byte[BLOCK_SIZE_BYTES];
            int bytesToCopy = Math.Min(BLOCK_SIZE_BYTES, data.Length - i);
            Array.Copy(data, i, block, 0, bytesToCopy);

            if (bytesToCopy < BLOCK_SIZE_BYTES)
            {
                Array.Clear(block, bytesToCopy, BLOCK_SIZE_BYTES - bytesToCopy);
            }

            string blockBits = BytesToBinaryString(block);
            string processedBlock = Idea(blockBits, keyBits, decrypt);
            result.AddRange(BinaryStringToBytes(processedBlock));
        }

        return result.ToArray();
    }

    private static string Idea(string block, string key, bool decrypt)
    {
        List<string> X = SplitIntoParts(block, 4, 16).ToList();
        List<string> Z = GenerateSubkeys(key);

        if (decrypt)
            Z = GenerateDecryptKeys(Z);

        for (int round = 0; round < 8; round++)
        {
            int multiplier = round * 6;
            string[] K = Z.Skip(multiplier).Take(6).ToArray();

            string one = MMul(X[0], K[0]);
            string two = MSum(X[1], K[1]);
            string three = MSum(X[2], K[2]);
            string four = MMul(X[3], K[3]);

            string five = Xor(one, three);
            string six = Xor(two, four);
            string seven = MMul(five, K[4]);
            string eight = MSum(six, seven);
            string nine = MMul(eight, K[5]);
            string ten = MSum(seven, nine);

            string eleven = Xor(one, nine);
            string twelve = Xor(three, nine);
            string thirteen = Xor(two, ten);
            string fourteen = Xor(four, ten);
        }

        X[0] = MMul(X[0], Z[48]);
        X[1] = MSum(X[1], Z[49]);
        X[2] = MSum(X[2], Z[50]);
        X[3] = MMul(X[3], Z[51]);

        return string.Join("", X);
    }

    private static List<string> GenerateSubkeys(string key)
    {
        if (key.Length < 128)
            throw new ArgumentException("Ключ должен быть 128 бит");

        List<string> subkeys = new List<string>();
        string currentKey = key;

        for (int group = 0; group < 6; group++)
        {
            var parts = SplitIntoParts(currentKey, 8, 16).ToList();
            subkeys.AddRange(parts);
            currentKey = CircularLeftShift(currentKey, 25);
        }

        return subkeys.Take(52).ToList();
    }

    private static List<string> GenerateDecryptKeys(List<string> keys)
    {
        if (keys.Count < 52)
            throw new ArgumentException("Недостаточно ключей для дешифрования");

        List<string> decryptKeys = new List<string>();

        for (int i = 0; i < 8; i++)
        {
            int step = i * 6;
            int lowerIndex = 46 - step;

            decryptKeys.Add(MMulInv(keys[lowerIndex + 2]));
            decryptKeys.Add(MSumInv(keys[lowerIndex + (i == 0 ? 3 : 4)]));
            decryptKeys.Add(MSumInv(keys[lowerIndex + (i == 0 ? 4 : 3)]));
            decryptKeys.Add(MMulInv(keys[lowerIndex + 5]));
            decryptKeys.Add(keys[lowerIndex]);
            decryptKeys.Add(keys[lowerIndex + 1]);
        }

        // Добавляем последние 4 ключа
        decryptKeys.Add(MMulInv(keys[0]));
        decryptKeys.Add(MSumInv(keys[1]));
        decryptKeys.Add(MSumInv(keys[2]));
        decryptKeys.Add(MMulInv(keys[3]));

        return decryptKeys;
    }

    private static string Xor(string a, string b)
    {
        if (a.Length != b.Length)
            throw new ArgumentException("Длины строк для XOR должны совпадать");

        return new string(a.Select((c, i) => c != b[i] ? '1' : '0').ToArray());
    }

    private static string CircularLeftShift(string binString, int k)
    {
        k %= binString.Length;
        return binString.Substring(k) + binString.Substring(0, k);
    }

    private static IEnumerable<string> SplitIntoParts(string str, int parts, int partLength)
    {
        for (int i = 0; i < parts; i++)
        {
            int start = i * partLength;
            if (start >= str.Length) yield break;
            int length = Math.Min(partLength, str.Length - start);
            yield return str.Substring(start, length);
        }
    }

    private static string MMul(string a, string b)
    {
        ushort aVal = Convert.ToUInt16(a, 2);
        ushort bVal = Convert.ToUInt16(b, 2);
        
        uint product = (uint)aVal * bVal;
        if (product == 0)
        {
            product = TWO_SIXTEEN_PLUS_1 - 1;
        }
        else
        {
            product %= TWO_SIXTEEN_PLUS_1;
            if (product == 0) product = TWO_SIXTEEN_PLUS_1 - 1;
        }
        
        return Convert.ToString((ushort)product, 2).PadLeft(16, '0');
    }

    private static string MSum(string a, string b)
    {
        ushort aVal = Convert.ToUInt16(a, 2);
        ushort bVal = Convert.ToUInt16(b, 2);
        ushort result = (ushort)((aVal + bVal) % TWO_SIXTEEN);
        return Convert.ToString(result, 2).PadLeft(16, '0');
    }

    private static string MMulInv(string a)
    {
        ushort aVal = Convert.ToUInt16(a, 2);
        return Convert.ToString(MulInv(aVal), 2).PadLeft(16, '0');
    }

    private static ushort MulInv(ushort a)
    {
        if (a == 0) return 0;
        
        uint m = TWO_SIXTEEN_PLUS_1;
        uint m0 = m;
        int y = 0, x = 1;

        while (a > 1)
        {
            uint q = (uint)(a / m);
            uint t = m;

            m = a % m;
            a = (ushort)t;
            t = (uint)y;

            y = x - (int)q * y;
            x = (int)t;
        }

        if (x < 0)
            x += (int)m0;

        return (ushort)x;
    }

    private static string MSumInv(string a)
    {
        ushort aVal = Convert.ToUInt16(a, 2);
        ushort result = (ushort)((TWO_SIXTEEN - aVal) % TWO_SIXTEEN);
        return Convert.ToString(result, 2).PadLeft(16, '0');
    }

    private static string BytesToBinaryString(byte[] bytes)
    {
        return string.Join("", bytes.Select(b => Convert.ToString(b, 2).PadLeft(8, '0')));
    }

    private static byte[] BinaryStringToBytes(string binary)
    {
        int numBytes = binary.Length / 8;
        byte[] bytes = new byte[numBytes];
        for (int i = 0; i < numBytes; i++)
        {
            bytes[i] = Convert.ToByte(binary.Substring(i * 8, 8), 2);
        }
        return bytes;
    }
    
    static byte[] GenerateKey()
    {
        byte[] key = new byte[16];
        new Random().NextBytes(key);
        return key;
    }
}