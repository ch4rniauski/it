using System.Text;
using System.Numerics;

uint[] Sha1_Const = 
[
    0x5A827999,
    0x6ED9EBA1,
    0x8F1BBCDC,
    0xCA62C1D6
];

const string inputFilePath = "input.txt";
Console.WriteLine("1 - Создание");
Console.WriteLine("2 - Проверка");
var choice = Console.ReadLine();

if (choice == "1")
{
    GenerateSignature();
}
else
{
    VerifySignature();
}

return;

void GenerateSignature()
{
    var sha1Hash = CalculateSha1(inputFilePath);
    Console.WriteLine($"Хэш: {sha1Hash}\n");

    Console.Write("Введите простое число p: ");
    var p = ReadPrime();
    
    Console.Write("Введите простое число q: ");
    var q = ReadPrime();

    uint d = 0, n = 0, e = 0;
    RsaInit(p, q, ref e, ref d, ref n);

    using var output = new FileStream("EDS.bin", FileMode.Create, FileAccess.Write);
    
    output.Write(BitConverter.GetBytes(e), 0, 4);
    output.Write(BitConverter.GetBytes(n), 0, 4);

    Console.Write("\nRSA (зашифрованный хэш): ");
    for (var i = 0; i < sha1Hash.Length; i += 2)
    {
        var block = Convert.ToUInt16(sha1Hash.Substring(i, 2), 16);
        
        var encrypted = (ushort)BigInteger.ModPow(block, d, n);
        Console.Write($"{encrypted:x4}");
        
        output.Write(BitConverter.GetBytes(encrypted), 0, 2);
    }
}

void VerifySignature()
{
    var sha1Hash = CalculateSha1(inputFilePath);

    using var input = new FileStream("EDS.bin", FileMode.Open, FileAccess.Read);
    
    var eBytes = new byte[4];
    var nBytes = new byte[4];
    
    input.ReadExactly(eBytes, 0, 4);
    input.ReadExactly(nBytes, 0, 4);
    
    var e = BitConverter.ToUInt32(eBytes);
    var n = BitConverter.ToUInt32(nBytes);

    Console.Write("\nРасшифрованный хеш: ");
    var isValid = true;
    
    for (var i = 0; i < 20; i++)
    {
        var blockBytes = new byte[2];
        input.ReadExactly(blockBytes, 0, 2);
        var encryptedBlock = BitConverter.ToUInt16(blockBytes);
        
        var decrypted = (ushort)BigInteger.ModPow(encryptedBlock, e, n);
        Console.Write($"{decrypted:x2}");
        
        var expected = Convert.ToUInt16(sha1Hash.Substring(i * 2, 2), 16);
        
        if (decrypted != expected)
        {
            isValid = false;
        }
    }

    Console.WriteLine();
    Console.WriteLine(isValid ? "\nПроверка пройдена" : "\nПроверка не пройдена");
}


string CalculateSha1(string filename)
{
    using var file = File.OpenRead(filename);
    var state = new uint[5];
    Sha1Init(state);
    
    var block = new byte[64];
    long totalBytes = 0;
    int bytesRead;

    while ((bytesRead = file.Read(block, 0, 64)) > 0)
    {
        totalBytes += bytesRead;
        
        if (bytesRead < 64)
        {
            Array.Resize(ref block, 64);
            block[bytesRead] = 0x80;
            
            if (bytesRead >= 56)
            {
                Sha1Transform(state, block);
                Array.Clear(block, 0, 56);
            }

            var bitLength = (ulong)totalBytes * 8;
            for (var i = 0; i < 8; i++)
            {
                block[56 + i] = (byte)(bitLength >> (56 - i * 8));
            }
        }
        
        Sha1Transform(state, block);
    }

    var hash = new StringBuilder();
    foreach (var s in state)
    {
        hash.Append(s.ToString("x8"));
    }
    
    return hash.ToString();
}

void Sha1Init(uint[] state)
{
    state[0] = 0x67452301;
    state[1] = 0xEFCDAB89;
    state[2] = 0x98BADCFE;
    state[3] = 0x10325476;
    state[4] = 0xC3D2E1F0;
}

void Sha1Transform(uint[] state, byte[] block)
{
    var a = state[0];
    var b = state[1];
    var c = state[2];
    var d = state[3];
    var e = state[4];
    
    var words = new uint[80];

    for (var i = 0; i < 16; i++)
    {
        words[i] = BitConverter.ToUInt32(block, i * 4);
    }

    for (var i = 16; i < 80; i++)
    {
        words[i] = Sha1RotL32(words[i-3] ^ words[i-8] ^ words[i-14] ^ words[i-16], 1);
    }
    
    for (var i = 0; i < 80; i++)
    {
        var temp = i switch
        {
            < 20 => Sha1RotL32(a, 5) + ((b & c) | (~b & d)) + e + words[i] + Sha1_Const[0],
            < 40 => Sha1RotL32(a, 5) + (b ^ c ^ d) + e + words[i] + Sha1_Const[1],
            < 60 => Sha1RotL32(a, 5) + ((b & c) | (b & d) | (c & d)) + e + words[i] + Sha1_Const[2],
            _ => Sha1RotL32(a, 5) + (b ^ c ^ d) + e + words[i] + Sha1_Const[3]
        };
        
        e = d;
        d = c;
        c = Sha1RotL32(b, 30);
        b = a;
        a = temp;
    }
    
    state[0] += a;
    state[1] += b;
    state[2] += c;
    state[3] += d;
    state[4] += e;
}

uint Sha1RotL32(uint value, int bits)
{
    return (value << bits) | (value >> (32 - bits));
}

void RsaInit(uint p, uint q, ref uint e, ref uint d, ref uint n)
{
    n = p * q;
    var phi = (p - 1) * (q - 1);
    

    Console.Write($"Введите число e, взаимно простое с числом {phi}: ");
    e = ReadInt();
    
    d = ModInverse(e, phi);
}

uint ModInverse(uint a, uint m)
{
    var g = ExtendedGcd(a, m, out var x, out _);
    
    if (g != 1)
    {
        throw new ArgumentException("Обратный элемент не существует");
    }
    
    return (x % m + m) % m;
}

uint ExtendedGcd(uint a, uint b, out uint x, out uint y)
{
    if (a == 0)
    {
        x = 0;
        y = 1;
        return b;
    }
    
    var gcd = ExtendedGcd(b % a, a, out var x1, out var y1);
    x = y1 - (b / a) * x1;
    y = x1;
    return gcd;
}

static uint ReadInt()
{
    uint number;
    
    while (!uint.TryParse(Console.ReadLine(), out number))
    {
        Console.WriteLine("Это НЕ целое число. Введите целое");
    }
    return number;
}

uint ReadPrime()
{
    while (true)
    {
        if (uint.TryParse(Console.ReadLine(), out var n) && IsPrime(n))
        {
            return n;
        }
        Console.WriteLine("Неверное простое число!");
    }
}

bool IsPrime(uint number)
{
    switch (number)
    {
        case < 2:
            return false;
        case 2:
            return true;
    }

    if (number % 2 == 0)
    {
        return false;
    }

    for (uint i = 3; i * i <= number; i += 2)
    {
        if (number % i == 0) return false;
    }
    
    return true;
}
