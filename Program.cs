using System.Xml;


try
{
    Console.Clear();
}
catch (IOException)
{
    throw new IOException("Debug console (internal terminal) in VS Code is not supported, please use integrated terminal or external terminal.");
}

char[] equations = { '+', '-', '/', '*', '^' };
Random rand = new Random();
int score = 0;

Console.WriteLine("Welcome to Simple Math Game, created by womzil.");
Console.WriteLine();
Console.WriteLine(@"In this game, you have to guess results of some 
simple math equations (addition, subtraction, division, multiplication)
on whole numbers from range 1-100. Results of division also should be whole numbers.");
Console.WriteLine();

ConsoleKey key = ContinueOrExit("Press \"Enter\" to start playing or \"Escape\" to exit or \"Tab\" to change the settings.");
PlayAgainOrLeave();

void PlayAgainOrLeave()
{
    do
    {
        if (key == ConsoleKey.Enter)
        {
            StartGame();
            key = ContinueOrExit("Do you wish to play again? Press \"Enter\" to continue or \"Escape\" to exit or \"Tab\" to change the settings.");
        }

        if (key == ConsoleKey.Tab)
        {
            ChangeSettings();
            Console.Clear();
            Console.Write("x");
            key = ContinueOrExit();
        }

        if (key == ConsoleKey.Escape)
        {
            key = ContinueOrExit("xAre you sure? Press \"Escape\" again to confirm or \"Enter\" to play again.");
            if (key == ConsoleKey.Escape)
            {
                Console.WriteLine("xLeaving...");
                break;
            }
        }

        PlayAgainOrLeave();

    } while (key != ConsoleKey.Enter && key != ConsoleKey.Escape && key != ConsoleKey.Tab);
}

void StartGame()
{
    Console.Clear();
    Console.WriteLine("Please, enter the number of rounds you want to play. It has to be at least 15 and at most 100.");
    Console.Write("Your choice: ");
    string userInput = Console.ReadLine();

    bool correctNumber = int.TryParse(userInput, out int numberOfRounds);
    while (!correctNumber || numberOfRounds is < 15 or > 100)
    {
        Console.WriteLine("Incorrect number! It has to be at least 15 and at most 100.");

        Console.Write("Your choice: ");
        userInput = Console.ReadLine();
        correctNumber = int.TryParse(userInput, out numberOfRounds);
    }

    char equation = GetEquation();

    for (int i = 1; i <= numberOfRounds; i++)
    {
        if (GetSettingValue("RandomEquation"))
            equation = GetEquation();

        StartRound(equation, i, numberOfRounds);
    }

    Console.Clear();
    Console.WriteLine("Game completed!");
    Console.WriteLine($"Your score: {score}/{numberOfRounds}.");
    Console.WriteLine();
}

void StartRound(char equation, int numberOfRound, int numberOfRounds)
{
    Console.Clear();
    Console.WriteLine($"Round number {numberOfRound}/{numberOfRounds}");
    Console.WriteLine();

    int result;
    int num1 = rand.Next(1, 101);
    int num2 = rand.Next(1, 101);

    if (equation == '^')
    {
        num1 = rand.Next(1, 21);
        num2 = num1 > 10 ? 2 : rand.Next(2, 4);
    }

    while (equation == '/' && (num1 % num2 != 0 || num1 < num2 || num1 == num2 || num2 == 1))
    {
        num1 = rand.Next(1, 101);
        num2 = rand.Next(1, 101);
    }

    switch (equation)
    {
        case '+':
            result = num1 + num2;
            break;
        case '-':
            result = num1 - num2;
            break;
        case '/':
            result = num1 / num2;
            break;
        case '*':
            result = num1 * num2;
            break;
        case '^':
            result = (int)Math.Pow(num1, num2);
            break;
        default:
            throw new ArithmeticException("The program tried to access nonexistent type of equation in \"equations\" array.");
    }

    Console.Write($"{num1} {equation} {num2} = ");

    bool correctNumber = int.TryParse(Console.ReadLine(), out int userAnswer);
    while (!correctNumber)
    {
        Console.WriteLine("Invalid input! Please, try again.");
        Console.Write($"{num1} {equation} {num2} = ");
        correctNumber = int.TryParse(Console.ReadLine(), out userAnswer);
    }

    if (userAnswer == result)
    {
        score++;
        Console.WriteLine($"Correct answer! Your score is {score} now!");
    }
    else
        Console.WriteLine($"Wrong asnwer! The correct answer is {result}.");

    ContinueOrExit();
}

ConsoleKey ContinueOrExit(
    string instructions = "Press \"Enter\" to continue or \"Escape\" to exit or \"Tab\" to change the settings.",
    string wrongKey = "Wrong key. You need to press \"Enter\" or \"Escape\" or \"Tab\".")
{
    Console.WriteLine(instructions);
    ConsoleKey key = Console.ReadKey().Key;
    Console.WriteLine();

    while (key != ConsoleKey.Enter && key != ConsoleKey.Escape && key != ConsoleKey.Tab)
    {
        Console.WriteLine(wrongKey);
        Console.Write("Your input: ");
        key = Console.ReadKey().Key;
        Console.WriteLine();
    }

    return key;
}

char GetEquation()
{
    char equation;

    if (GetSettingValue("RandomEquation"))
        equation = equations[rand.Next(0, equations.Length)];
    else
    {
        Console.Clear();
        Console.WriteLine(
            "Please, enter the type of equation you want to use in game. You can change that in the settings.");
        Console.Write("Your choice: ");
        string userInput = Console.ReadLine();
        bool correctEquation = char.TryParse(userInput, out equation);

        while (!correctEquation || !equations.Contains(equation))
        {
            Console.WriteLine("Incorrect equation! It has to be one of the following:");
            Console.WriteLine("+ - addition");
            Console.WriteLine("- - subtraction");
            Console.WriteLine("* - multiplication");
            Console.WriteLine("/ - division");
            Console.WriteLine("^ - powering");

            Console.Write("Your choice: ");
            userInput = Console.ReadLine();
            correctEquation = char.TryParse(userInput, out equation);
        }
    }

    return equation;
}

void DisplaySettings()
{
    char onOff = GetSettingValue("RandomEquation") ? '+' : '-';

    Console.Clear();
    Console.WriteLine("Here you can change the settings.");
    Console.WriteLine("Press \"Tab\" to turn a specific setting on/off or \"Escape\" to exit.");
    Console.WriteLine();
    Console.Write("({0}) Random equation type", onOff);
}

void ChangeSettings()
{
    DisplaySettings();

    ConsoleKey? switchKey = null;

    bool settingValue = GetSettingValue("RandomEquation");

    while (switchKey is not ConsoleKey.Escape)
    {
        switchKey = Console.ReadKey().Key;

        if (switchKey == ConsoleKey.Tab)
        {
            settingValue = !settingValue;
            UpdateSettingValue("RandomEquation", settingValue);
            DisplaySettings();
        }
    }
}

XmlDocument GetConfigFile()
{
    XmlDocument xmlDoc = new XmlDocument();
    try
    {
        xmlDoc.Load("config.xml");
        return xmlDoc;
    }
    catch (FileNotFoundException)
    {
        // Create an XmlDocument
        xmlDoc = new XmlDocument();
        xmlDoc.AppendChild(xmlDoc.CreateXmlDeclaration("1.0", "UTF-8", null));

        XmlElement rootElement = xmlDoc.CreateElement("Configuration");
        xmlDoc.AppendChild(rootElement);

        // Adding settings with their default values
        XmlElement settingElement = xmlDoc.CreateElement("RandomEquation");
        settingElement.InnerText = "false";
        rootElement.AppendChild(settingElement);

        xmlDoc.Save("config.xml");
        return xmlDoc;
    }
}

bool GetSettingValue(string key)
{
    XmlDocument xmlDoc = GetConfigFile();
    xmlDoc.Load("config.xml");
    XmlNode settingNode = xmlDoc.SelectSingleNode($"/Configuration/{key}");

    if (bool.TryParse(settingNode.InnerText, out bool value))
        return value;
    throw new InvalidOperationException("Configuration file could not be found.");
}

void UpdateSettingValue(string key, bool value)
{
    XmlDocument xmlDoc = GetConfigFile();

    // Edit or add a setting
    XmlNode settingNode = xmlDoc.SelectSingleNode($"/Configuration/{key}");

    if (settingNode != null)
    {
        settingNode.InnerText = value.ToString();
        xmlDoc.Save("config.xml");
    }
    else
    {
        throw new InvalidOperationException("Could not find that key in the config.");
    }
}