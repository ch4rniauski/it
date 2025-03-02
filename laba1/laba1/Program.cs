Console.Write("Введите строку: ");

string? str = Console.ReadLine();

if (str is null || str == "")
    return;

str = str.ToLower();

var symbolsRepetitions = new Dictionary<char, int>();
var charCodes = new Dictionary<char, string>();

foreach (char c in str)
{
    if (symbolsRepetitions.ContainsKey(c))
        symbolsRepetitions[c]++;
    else
        symbolsRepetitions.Add(c, 1);
}

foreach(var pair in symbolsRepetitions)
{
    var node = new Node()
    {
        Element = pair.Key,
        Repetitions = pair.Value
    };

    Huffman.Tree.Add(node);
}

Huffman.SortNodes();

while (Huffman.Tree.Count > 1)
{
    Huffman.CreateNode();
    Huffman.SortNodes();
}

Huffman.CreateTree(ref charCodes, symbolsRepetitions.Count);

Console.WriteLine();
foreach (var pair in charCodes)
    Console.WriteLine($"{pair.Key} = {pair.Value}");
Console.WriteLine();

foreach (char c in str)
    Console.Write(charCodes[c]);
Console.WriteLine();

static class Huffman
{
    public static List<Node> Tree = new();

    public static void CreateNode()
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

    public static void CreateTree(ref Dictionary<char, string> charCodes, int n)
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
}

class Node
{
    public char? Element { get; set; } = null;
    public int Repetitions { get; set; }
    public Node? Right { get; set; } = null;
    public Node? Left { get; set; } = null;
    public bool IsChecked { get; set; } = false;
}
