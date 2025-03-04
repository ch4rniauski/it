var str = File.ReadAllText("text.txt");

if (str is null || str == "")
    return;

Console.WriteLine($"Исходная строка: {str}");

str = str.ToLower();

var strLength = str.Length;

var symbolsRepetitions = new Dictionary<char, int>();
var charCodes = new Dictionary<char, string>();
var codedStr = string.Empty;

foreach (char c in str) // количесвто повторений символа
{
    if (symbolsRepetitions.ContainsKey(c))
        symbolsRepetitions[c]++;
    else
        symbolsRepetitions.Add(c, 1);
}

foreach(var pair in symbolsRepetitions) // создание списка узлов из символов
{
    var node = new Node()
    {
        Element = pair.Key,
        Repetitions = pair.Value
    };

    Huffman.Tree.Add(node);
}

Huffman.SortNodes();

while (Huffman.Tree.Count > 1) // создание дерева из узлов
{
    Huffman.CreateTreeFromNodes();
    Huffman.SortNodes();
}

Huffman.CreateCharCodes(ref charCodes, symbolsRepetitions.Count);

Console.WriteLine(); // табличные данные
foreach (var pair in charCodes)
{

    Console.WriteLine($"{pair.Key} ({((double)symbolsRepetitions[pair.Key] / (double)strLength) * 100}%) = {pair.Value} ({pair.Value.Length} символов)");
}
Console.WriteLine();

Console.Write("Строка в закодированном виде: "); // закодированная строка
foreach (char c in str)
{
    codedStr += charCodes[c];
    Console.Write(charCodes[c]);
}
Console.WriteLine();
Console.WriteLine();

Huffman.Decoder(codedStr);


double entropy = 0; // энтропия
foreach (var pair in symbolsRepetitions)
{
    double probability = (double)pair.Value / strLength;
    entropy += probability * Math.Log2(probability);
}
Console.WriteLine($"Энтропия: {-entropy}");

double averageCodeLength = 0; // ср. длина кодового слова
foreach (var pair in charCodes)
{
    double probability = (double)symbolsRepetitions[pair.Key] / strLength;
    averageCodeLength += probability * pair.Value.Length;
}
Console.WriteLine($"Средняя длина кодового слова: {averageCodeLength}");

static class Huffman
{
    public static List<Node> Tree = new();

    public static void CreateTreeFromNodes()
    {
        Node node1 = Tree[0];
        Node node2 = Tree[1];

        Node newNode = new()
        {
            Repetitions = node1.Repetitions + node2.Repetitions,
            Right = node1,
            Left = node2,
        };

        Tree[0] = newNode;
        Tree.Remove(node2);
    }

    public static void SortNodes()
    {
        for (int i = 0; i < Tree.Count - 1; i++)
        {
            for (int j = 0; j < Tree.Count - (i + 1); j++)
            {
                if (Tree[j].Repetitions > Tree[j + 1].Repetitions)
                    (Tree[j], Tree[j + 1]) = (Tree[j + 1], Tree[j]);
            }
        }
    }

    public static void CreateCharCodes(ref Dictionary<char, string> charCodes, int n)
    {
        for (int i = 0; i < n; i++)
        {
            string code = string.Empty;
            Node tempNode = Huffman.Tree[0];

            while (true)
            {
                if (tempNode.Left != null && tempNode.Left.IsChecked == false)
                {
                    tempNode = tempNode.Left;
                    code += "0";
                }
                else if (tempNode.Right != null && tempNode.Right.IsChecked == false)
                {
                    tempNode = tempNode.Right;
                    code += "1";
                }
                else if (tempNode.Element != null)
                {
                    charCodes.Add(Convert.ToChar(tempNode.Element), code);
                    tempNode.IsChecked = true;
                    break;
                }
                else
                {
                    tempNode.IsChecked = true;
                    i--;
                    break;
                }
            }
        }
    }

    public static void Decoder(string codedStr)
    {
        int j = 0;
        int i = 0;

        Console.Write("Декодированная строка: ");

        while (j == 0)
        {
            var node = Tree[0];

            for (; i < codedStr.Length; i++)
            {
                if (node.Left != null && codedStr[i] == '0')
                    node = node.Left;
                else if (node.Right != null && codedStr[i] == '1')
                    node = node.Right;                
                else
                {
                    Console.Write(node.Element);
                    break;
                }

                if (i == codedStr.Length - 1)
                    Console.WriteLine(node.Element);
            }

            if (i == codedStr.Length)
                break;
        }

        Console.WriteLine();
    }
}

class Node
{
    public char? Element { get; set; } = null;
    public int Repetitions { get; set; }
    public Node? Right { get; set; } = null;
    public Node? Left { get; set; } = null;
    public bool IsChecked { get; set; } = false;
}
