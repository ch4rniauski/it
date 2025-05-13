using System.Numerics;
using System.Text;

const string inputFile = "input.txt";
const string encryptedFile = "encrypted.bin";
const string decryptedFile = "decrypted.bin";

Console.Write("Введите простое число p: ");
var p = ReadPrime();

Console.Write("Введите простое число q: ");
var q = ReadPrime();

var euler = (p - 1) * (q - 1);

Console.Write($"Введите число e, взаимно простое с числом {euler}: ");
var e = ReadInt();

Rsa.GenerateKeys(p, q, e, euler);

Rsa.EncryptFile(inputFile, encryptedFile);
Rsa.DecryptFile(encryptedFile, decryptedFile);

Console.WriteLine();
Console.WriteLine($"p: {p}");
Console.WriteLine($"q: {q}");
Console.WriteLine($"Открытый ключ - (E: {Rsa.E}, N: {Rsa.N})");
Console.WriteLine($"Закрытый ключ - (D: {Rsa.D}, D: {Rsa.N})");

var hackedKey = Rsa.HackPrivateKey();
Console.WriteLine($"Взломанный закрытый ключ - (D: {hackedKey}, N: {Rsa.N})");


static int ReadPrime()
{
    while (true)
    {
        if (int.TryParse(Console.ReadLine(), out var number) && IsPrime(number))
        {
            return number;
        }
        
        Console.WriteLine("Это НЕ простое число. Введите простое");
    }
}

static int ReadInt()
{
    int number;
    
    while (!int.TryParse(Console.ReadLine(), out number))
    {
        Console.WriteLine("Это НЕ целое число. Введите целое");
    }
    return number;
}

static bool IsPrime(int number)
{
    if (number < 2)
    {
        return false;
    }
    
    for (var i = 2; i * i <= number; i++)
    {
        if (number % i == 0)
        {
            return false;
        }
    }
    
    return true;
}

static class Rsa
{
    public static int N;
    public static int E;
    public static int D;

    public static int HackPrivateKey()
    {
        var p = FindFactor(N);
        var q = N / p;
        
        Console.WriteLine($"Найдены множители: p = {p}, q = {q}");
        
        int phi = (p - 1) * (q - 1);
        
        return ModInverse(E, phi);
    }

    private static int FindFactor(int n)
    {
        var maxDivisor = (int)Math.Sqrt(n);
        
        for (var i = 3; i <= maxDivisor; i += 2)
        {
            if (IsPrime(i))
            {
                if (n % i == 0)
                {
                    return i;
                }
            }
            
        }
        
        return n;
    }
    
    public static void GenerateKeys(int p, int q, int e, int phi)
    {
        N = p * q;

        if (!IsPrime(p) || !IsPrime(q) || !AreCoprime(e, phi))
        {
            throw new ArgumentException("Некорректные параметры!");
        }

        D = ModInverse(e, phi);
        E = e;
    }

    public static void EncryptFile(string inputFile, string outputFile)
    {
        if (!File.Exists(inputFile))
        {
            Console.WriteLine("Ошибка: Файл не найден!");
            return;
        }

        var fileBytes = File.ReadAllBytes(inputFile);

        Console.WriteLine();
        Console.WriteLine($"Исходный текст: {Encoding.UTF8.GetString(fileBytes)}");
        
        Console.WriteLine();
        WriteBytes("Исходный текст в байтах: ", fileBytes);
        
        var encryptedData = new List<ushort>();

        foreach (var m in fileBytes)
        {
            var encryptedByte = (ushort)BigInteger.ModPow(m, E, N);
            encryptedData.Add(encryptedByte);
        }

        File.WriteAllBytes(outputFile, encryptedData.SelectMany(BitConverter.GetBytes).ToArray());

        Console.WriteLine();
        WriteBytes("Зашифрованный текст в байтах: ", encryptedData.SelectMany(BitConverter.GetBytes).ToArray());
        Console.WriteLine();
    }

    private static void WriteBytes(string outputText, byte[] bytes)
    {
        Console.WriteLine(outputText);
        foreach (var b in bytes)
        {
            Console.Write(b);
        }
        Console.Write($" ({bytes.Length} символа)");
        Console.WriteLine();
    }

    public static void DecryptFile(string inputFile, string outputFile)
    {
        if (!File.Exists(inputFile))
        {
            Console.WriteLine("Ошибка: Файл не найден!");
            return;
        }

        var encryptedBytes = File.ReadAllBytes(inputFile);
        var decryptedData = new List<byte>();

        for (var i = 0; i < encryptedBytes.Length; i += 2)
        {
            var encryptedBlock = BitConverter.ToUInt16(encryptedBytes, i);
            var decryptedByte = (byte)BigInteger.ModPow(encryptedBlock, D, N);
            decryptedData.Add(decryptedByte);
        }

        Console.WriteLine();
        WriteBytes("Расшифрованные текст в байтах: ", decryptedData.ToArray());
        
        File.WriteAllBytes(outputFile, decryptedData.ToArray());
        var bytes = decryptedData.ToArray();
        Console.WriteLine();
        Console.WriteLine($"Расшифрованный текст: {Encoding.UTF8.GetString(bytes)}");
    }

    private static bool IsPrime(int number)
    {
        if (number < 2)
        {
            return false;
        }

        for (var i = 2; i * i <= number; i++)
        {
            if (number % i == 0)
            {
                return false;
            }
        }
        
        return true;
    }

    private static bool AreCoprime(int a, int b)
    {
        return Gcd(a, b) == 1;
    } 
        

    private static int Gcd(int a, int b)
    {
        while (b != 0)
        {
            (a, b) = (b, a % b);
        }
        
        return a;
    }

    private static int ModInverse(int a, int m)
    {
        var g = ExtendedEuclidean(a, m, out var x, out var y);

        if (g != 1)
        {
            throw new ArgumentException("Обратный элемент не существует");
        }
        
        return (x % m + m) % m;
    }

    private static int ExtendedEuclidean(int a, int b, out int x, out int y)
    {
        if (a == 0)
        {
            x = 0; y = 1; 
            return b;
        }
        
        int x1, y1;
        var g = ExtendedEuclidean(b % a, a, out x1, out y1);
        
        x = y1 - (b / a) * x1;
        y = x1;
        
        return g;
    }
}