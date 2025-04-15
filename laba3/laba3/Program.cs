using MathNet.Numerics;

var originalText = File.ReadAllText("text.txt");

Console.WriteLine("Исходный текст:");
Console.WriteLine(originalText);
Console.WriteLine();
Console.WriteLine();

var key = "";
while (true)
{
    Console.Write("Введите ключ: ");
    key = Console.ReadLine();

    if (!string.IsNullOrWhiteSpace(key))
        break;
}

Console.WriteLine();
Console.WriteLine();
key = key.ToLower();

var encryptedTextWithoutChanging = Vigenere.EncryptWithoutChanging(originalText, key!);
Console.WriteLine("Зашифрованный текст:");
Console.WriteLine(encryptedTextWithoutChanging);

var encryptedText = Vigenere.Encrypt(originalText, key!);

Vigenere.KasiskiMethod(encryptedText);

static class Vigenere
{
    public static string EncryptWithoutChanging(string text, string key)
    {
        var encryptedText = "";
        var j = 0;

        foreach (var c in text)
        {
            if (char.IsLetter(c))
            {
                bool isUpper = char.IsUpper(c);
                char baseChar = isUpper ? 'A' : 'a';

                encryptedText += (char)((c - baseChar + (key[j] - 'a')) % 26 + baseChar);

                j = (j + 1) % key.Length;
            }
            else
                encryptedText += c;
        }

        return encryptedText;
    }

    public static string Encrypt(string text, string key)
    {
        text = text.ToUpper();
        key = key.ToUpper();
        string encryptedText = "";

        for (int i = 0, j = 0; i < text.Length; i++)
        {
            char c = text[i];

            if (c < 'A' || c > 'Z')
                continue;

            encryptedText += (char)((c - 'A' + (key[j] - 'A')) % 26 + 'A');
            j = (j + 1) % key.Length;
        }

        return encryptedText;
    }

    private static List<int> FindRepeatingSequences(string text, int sequenceLength)
    {
        var sequences = new Dictionary<string, List<int>>();

        for (int i = 0; i <= text.Length - sequenceLength; i++)
        {
            string sequence = text.Substring(i, sequenceLength);

            if (!sequences.ContainsKey(sequence))
                sequences[sequence] = new List<int>();

            sequences[sequence].Add(i);
        }
        sequences = sequences.OrderByDescending(x => x.Value.Count).ToDictionary(x => x.Key, x => x.Value);
        return sequences.Values.First();
    }

    public static void KasiskiMethod(string encryptedText, int sequenceLength = 3)
    {
        encryptedText = encryptedText.ToUpper();
        var positions = FindRepeatingSequences(encryptedText, sequenceLength);

        var distances = new List<long>();
        for (int i = 1; i < positions.Count; i++)
            distances.Add(positions[i] - positions[i - 1]);

        long gcd = Euclid.GreatestCommonDivisor(distances);
        Console.WriteLine();
        Console.WriteLine();
        Console.WriteLine($"Длина ключа: {gcd}");
        Console.WriteLine();
        Console.WriteLine("Ключ: " + FindKey(encryptedText, (int)gcd));
    }

    private static string FindKey(string encryptedText, int gcd)
    {
        string found = "";

        for (int i = 0; i < gcd; i++)
        {
            var dictionary = new Dictionary<char, int>();
            for (var j = i; j < encryptedText.Length; j += gcd)
            {
                if (!dictionary.TryAdd(encryptedText[j], 1))
                    dictionary[encryptedText[j]]++;
            }

            dictionary = dictionary.OrderByDescending(x => x.Value)
                .ToDictionary(x => x.Key, x => x.Value);

            int shift = dictionary.Keys.First() - 'E';
            
            if (shift < 0) 
                shift += 26;

            found += (char)(shift + 'a');
        }

        return found;
    }
}