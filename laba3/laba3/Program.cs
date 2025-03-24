using MathNet.Numerics;

var text = File.ReadAllText("text.txt").ToLower();

if (string.IsNullOrWhiteSpace(text))
    return;

Console.WriteLine($"Исходный текст: {text}");

List<int> spaceIndexes = new();
for (int i = 0; i < text.Length; i++)
{
    if (text[i] == ' ')
        spaceIndexes.Add(i);
}

text = text.Replace(" ", "");

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
encryptedVigener = InsertSpacesAtIndexes(encryptedVigener, spaceIndexes);
Console.WriteLine($"Зашифрованный текст (Виженер): {encryptedVigener}");

encryptedVigener = encryptedVigener.Replace(" ", "");
var decryptedVigener = Vigener.Decryption(encryptedVigener, secretWord);
decryptedVigener = InsertSpacesAtIndexes(decryptedVigener, spaceIndexes);
Console.WriteLine($"Расшифрованный текст (Виженер): {decryptedVigener}");

Console.WriteLine();
Console.WriteLine("Тест Касиски для метода Виженера:");
var secretWordLength = Kasiski.FindSecretWordLength(encryptedVigener);

encryptedVigener = encryptedVigener.Replace(" ", "");
var foundSecretWord = Kasiski.FindVigenereKey(encryptedVigener, secretWordLength);
Console.WriteLine($"Вычисленное кодовое слово Виженера: {foundSecretWord}");

static string InsertSpacesAtIndexes(string text, List<int> indexes)
{
    foreach (var index in indexes)
    {
        if (index < text.Length)
            text = text.Insert(index, " ");
        else
            text += " ";
    }
    return text;
}

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
    static public int FindSecretWordLength(string encryptedText)
    {
        var repetitions = FindRepetitionsWithThreeLetters(encryptedText);
        List<long> repDistances;

        if (repetitions is not null)
            repDistances = CalculateRepetitionsDistances(encryptedText, repetitions);
        else
        {
            Console.WriteLine("Не удалось высчитать длину кодового слова");
            return 0;
        }

        var length = Euclid.GreatestCommonDivisor(repDistances);

        if (length != 0)
        {
            Console.WriteLine($"Длина кодового слова: {length}");
            return (int)length;
        }
        else
        {
            repetitions = FindRepetitionsWithTwoLetters(encryptedText);
            repDistances = CalculateRepetitionsDistances(encryptedText, repetitions!);

            length = Euclid.GreatestCommonDivisor(repDistances);

            if (length != 0)
                Console.WriteLine($"Длина кодового слова: {length}");
            else
                Console.WriteLine("Не удалось высчитать длину кодового словаю. Вероятно, текст слишком короткий");
            return (int)length;
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

    static public string FindVigenereKey(string encryptedText, int secretWordLength)
    {
        string secretWord = string.Empty;
        var dict = new Dictionary<char, int>();

        for (int i = 0; i < secretWordLength; i++)
        {
            for (int j = 0; ; j++)
            {
                int index = i + j * secretWordLength;

                if (index <= encryptedText.Length - 1)
                {
                    if (dict.ContainsKey(encryptedText[index]))
                        dict[encryptedText[index]]++;
                    else
                        dict.Add(encryptedText[index], 1);
                }
                else
                    break;
            }

            char ch = ' ';
            double frequency = 0;

            foreach (var pair in dict)
            {
                if ((double)((double)pair.Value / (double)secretWordLength) > frequency)
                {
                    frequency = (double)((double)pair.Value / (double)secretWordLength);
                    ch = pair.Key;
                }
            }

            int shift = (int)Math.Abs(ch - 'e');


            secretWord += (char)((int)('a' + shift));
        }

        return secretWord;
    }
}
