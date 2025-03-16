using MathNet.Numerics;

var text = File.ReadAllText("text.txt").ToLower();

if (string.IsNullOrWhiteSpace(text))
    return;

Console.WriteLine($"Исходный текст: {text}");

string? secretWord;

while (true)
{
    Console.Write("Введите кодовое слово для шифрования методом Виженера: ");
    secretWord = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(secretWord))
        continue;
    break;
}
Console.WriteLine();

var encryptedVigener = Vigener.Encryption(text, secretWord);
Console.WriteLine($"Зашифрованный текст (Виженер): {encryptedVigener}");

var decryptedVigener = Vigener.Decryption(encryptedVigener, secretWord);
Console.WriteLine($"Расшифрованный текст (Виженер): {decryptedVigener}");

Console.WriteLine();

int number;

while (true)
{
    Console.Write("Введите константу для сдвига в шифровании методом Цезаря: ");
    var numberStr = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(numberStr))
        continue;

    number = Convert.ToInt32(numberStr);
    break;
}

var encryptedCesar = Cesar.Encryption(text, number);
Console.WriteLine($"Зашифрованный текст (Цезарь): {encryptedCesar}");

var decryptedCesar = Cesar.Decryption(encryptedCesar, number);
Console.WriteLine($"Расшифрованный текст (Цезарь): {decryptedCesar}");

Console.WriteLine();
Console.WriteLine("Тест Касиски для метода Виженера:");
Kasiski.FindSecretWordLength(encryptedVigener);

static class Vigener
{
    public static string Encryption(string text, string secretWord)
    {
        string encryptedText = string.Empty;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == ' ')
            {
                encryptedText += ' ';
                continue;
            }

            int number = (int)text[i] + (int)secretWord[i % secretWord.Length] - 97;

            if (number > 122)
                number -= 26;

            encryptedText += (char)(number);
        }

        return encryptedText;
    }

    public static string Decryption(string encryptedText, string secretWord)
    {
        string decryptedText = string.Empty;

        for (int i = 0; i < encryptedText.Length; i++)
        {
            if (encryptedText[i] == ' ')
            {
                decryptedText += ' ';
                continue;
            }

            int number = (int)encryptedText[i] - (int)secretWord[i % secretWord.Length] + 97;

            if (number < 97)
                number += 26;

            decryptedText += (char)(number);
        }

        return decryptedText;
    }
}

static class Kasiski
{
    static public void FindSecretWordLength(string encryptedText)
    {
        var repetitions = FindRepetitionsWithThreeLetters(encryptedText);
        List<long> repDistances;

        if (repetitions is not null)
            repDistances = CalculateRepetitionsDistances(encryptedText, repetitions);
        else
        {
            Console.WriteLine("Не удалось высчитать длину кодового слова");
            return;
        }

        var length = Euclid.GreatestCommonDivisor(repDistances);

        if (length != 0)
            Console.WriteLine($"Длина кодового слова: {length}");
        else
        {
            repetitions = FindRepetitionsWithTwoLetters(encryptedText);
            repDistances = CalculateRepetitionsDistances(encryptedText, repetitions!);

            length = Euclid.GreatestCommonDivisor(repDistances);

            if (length != 0)
                Console.WriteLine($"Длина кодового слова: {length}");
            else
                Console.WriteLine("Не удалось высчитать длину кодового словаю. Вероятно, текст слишком короткий");
        }
    }

    static private Dictionary<string, List<int>>? FindRepetitionsWithThreeLetters(string encryptedText)
    {
        var repetitions = new Dictionary<string, List<int>>();

        for (int i = 0; i < encryptedText.Length - 2; i++)
        {
            var substring = encryptedText.Substring(i, 3);

            if (repetitions.ContainsKey(substring))
                repetitions[substring].Add(i);
            else
            {
                repetitions.Add(substring, new());
                repetitions[substring].Add(i);
            }
        }

        return repetitions;
    }

    static private Dictionary<string, List<int>>? FindRepetitionsWithTwoLetters(string encryptedText)
    {
        var repetitions = new Dictionary<string, List<int>>();

        for (int i = 0; i < encryptedText.Length - 1; i++)
        {
            var substring = encryptedText.Substring(i, 2);

            if (repetitions.ContainsKey(substring))
                repetitions[substring].Add(i);
            else
            {
                repetitions.Add(substring, new());
                repetitions[substring].Add(i);
            }
        }

        return repetitions;
    }

    static private List<long> CalculateRepetitionsDistances(string encryptedText, Dictionary<string, List<int>> repetitions)
    {
        var repDistances = new List<long>();

        foreach (var pair in repetitions)
        {
            if (pair.Value.Count > 1)
            {
                for (int i = 0; i < pair.Value.Count - 1; i++)
                    repDistances.Add(pair.Value[i + 1] - pair.Value[i]);
            }
        }

        return repDistances;
    }
}

static class Cesar
{
    static public string Encryption(string text, int number)
    {
        string encryptedText = string.Empty;

        for (int i = 0; i < text.Length; i++)
        {
            if (text[i] == ' ')
            {
                encryptedText += ' ';
                continue;
            }

            int charNumber = (int)text[i] + number;

            if (charNumber > 122)
                charNumber -= 26;

            encryptedText += (char)(charNumber);
        }

        return encryptedText;
    }

    public static string Decryption(string encryptedText, int number)
    {
        string decryptedText = string.Empty;

        for (int i = 0; i < encryptedText.Length; i++)
        {
            if (encryptedText[i] == ' ')
            {
                decryptedText += ' ';
                continue;
            }

            int charNumber = (int)encryptedText[i] - number;

            if (charNumber < 97)
                charNumber += 26;

            decryptedText += (char)(charNumber);
        }

        return decryptedText;
    }
}
