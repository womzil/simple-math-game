using System.Diagnostics;
using System.Drawing;
using System.Text.Json;
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
char[] modes = new char[equations.Length + 1];
Array.Copy(equations, modes, equations.Length);
modes[^1] = 'R';

Random rand = new Random();
List<Game> games = new List<Game>();
int score = 0;
int currentLeaderboardModeIndex = 0;

bool exit = false;
bool playAgain = false;
while (!exit)
{
    Console.Clear();
    Console.WriteLine("Welcome to Simple Math Game, created by womzil.");
    Console.WriteLine();
    Console.WriteLine(@"In this game, you have to guess results of some 
simple math equations (addition, subtraction, division, multiplication, powering)
on whole numbers from range 1-100. Results of division also should be whole numbers.");
    Console.WriteLine();
    ConsoleKey key = GetKey(
            playAgain
                ? "Do you wish to play again?\nPress:\n- \"Enter\" to continue\n- \"L\" to see top scores\n- \"Tab\" to change the settings\n- \"Escape\" to exit"
                : "Press:\n- \"Enter\" to start playing\n- \"L\" to see top scores\n- \"Tab\" to change the settings\n- \"Escape\" to exit");
    
    if (key == ConsoleKey.Enter)
    {
        StartGame();
        playAgain = true;
    }

    if (key == ConsoleKey.L)
    {
        DisplayLeaderboard();

        ConsoleKey leaderboardKey = Console.ReadKey().Key;
        while (leaderboardKey != ConsoleKey.Escape)
        {
            if (leaderboardKey == ConsoleKey.LeftArrow || leaderboardKey == ConsoleKey.A)
                currentLeaderboardModeIndex++;
            else if (leaderboardKey == ConsoleKey.RightArrow || leaderboardKey == ConsoleKey.D)
                currentLeaderboardModeIndex--;

            if (currentLeaderboardModeIndex >= modes.Length)
                currentLeaderboardModeIndex = 0;
            else if (currentLeaderboardModeIndex < 0)
                currentLeaderboardModeIndex = modes.Length - 1;

            DisplayLeaderboard();

            leaderboardKey = Console.ReadKey().Key;
        }
        Console.Write("x");
        continue;
    }

    if (key == ConsoleKey.Tab)
    {
        ChangeSettings();
        continue;
    }

    if (key == ConsoleKey.Escape)
    {
        key = GetKey("xAre you sure? Press \"Escape\" again to confirm or \"Enter\" to cancel.");
        if (key == ConsoleKey.Escape)
        {
            Console.WriteLine("xLeaving...");
            exit = true;
        }
    }
}

return 0;

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

    float percentage = (float)score / numberOfRounds;
    Console.Clear();
    Console.WriteLine("Game completed!");
    Console.WriteLine($"Your score: {score}/{numberOfRounds} [{percentage:P}].");
    Console.WriteLine();

    string? userName = null;
    while (string.IsNullOrEmpty(userName))
    {
        Console.Write("Please, enter your name: ");
        userName = Console.ReadLine();

        if (string.IsNullOrEmpty(userName))
            Console.WriteLine("You have to input your name!");
    }

    games.Add(new Game
    {
        PlayerName = userName,
        GameMode = GetSettingValue("RandomEquation") ? 'R' : equation,
        Score = score,
        Rounds = numberOfRounds,
        Time = DateTime.Now
    });
    string fileName = "TopScores.json";
    string jsonString = JsonSerializer.Serialize(games, new JsonSerializerOptions { WriteIndented = true });
    File.WriteAllText(fileName, jsonString);
    score = 0;
}

void StartRound(char equation, int numberOfRound, int numberOfRounds)
{
    float percentage = (float)score / numberOfRound;
    Console.Clear();
    Console.WriteLine($"Round number {numberOfRound}/{numberOfRounds}\tScore: {score} [{percentage:P}]");
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

    result = equation switch
    {
        '+' => num1 + num2,
        '-' => num1 - num2,
        '/' => num1 / num2,
        '*' => num1 * num2,
        '^' => (int)Math.Pow(num1, num2),
        _ => throw new ArithmeticException(
            "The program tried to access nonexistent type of equation in \"equations\" array.")
    };

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

        Console.BackgroundColor = ConsoleColor.Green;
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(" \u2713 "); //Check mark
        Console.ResetColor();
        Console.WriteLine($" Correct answer!");
        Console.WriteLine($"Your score is {score} now!");
    }
    else
    {
        Console.BackgroundColor = ConsoleColor.Red;
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(" X ");
        Console.ResetColor();
        Console.WriteLine($" Wrong answer! The correct answer is {result}.");
        Console.WriteLine($"Your score is still {score}.");
    }

    Console.WriteLine();
    Console.WriteLine("Press any key to continue.");
    Console.ReadKey();
}

void DisplayLeaderboard()
{
    try
    {
        games = JsonSerializer.Deserialize<List<Game>>(File.ReadAllText("TopScores.json"))!;
    }
    catch (IOException) { }

    string mode = modes[currentLeaderboardModeIndex] switch
    {
        '+' => "Addition",
        '-' => "Subtraction",
        '*' => "Multiplication",
        '/' => "Division",
        '^' => "Powering",
        'R' => "Random",
        _ => throw new InvalidDataException("Wrong equation type."),
    };
    List<Game> gamesCurrentGameMode = games.FindAll(FindGamesWithCurrentGameMode);
    Console.Clear();
    Console.WriteLine($"Top scores - {mode}");
    Console.WriteLine("You can change the mode using arrows (left/right) or A/D.");
    Console.WriteLine();

    if (gamesCurrentGameMode.Count == 0)
        Console.WriteLine("No games played yet in this mode!");

    for (int i = 0; i < gamesCurrentGameMode.Count; i++)
    {
        float percentage = (float)gamesCurrentGameMode[i].Score / gamesCurrentGameMode[i].Rounds;
        Console.WriteLine($"{i + 1}. {gamesCurrentGameMode[i].PlayerName} - {gamesCurrentGameMode[i].Score}/{gamesCurrentGameMode[i].Rounds} [{percentage:P}]");
    }
}

bool FindGamesWithCurrentGameMode(Game game)
{
    return game.GameMode == modes[currentLeaderboardModeIndex];
}

ConsoleKey GetKey(
    string instructions = "Press \"Enter\" to continue.",
    string wrongKeyMessage = "Wrong key. You need to press \"Enter\".")
{
    Console.WriteLine(instructions);
    ConsoleKey key = Console.ReadKey().Key;
    Console.WriteLine();

    while (key != ConsoleKey.Enter && key != ConsoleKey.Escape && key != ConsoleKey.L && key != ConsoleKey.Tab)
    {
        Console.WriteLine(wrongKeyMessage);
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

// Game object used for storing scores
public class Game
{
    public string PlayerName { get; set; }
    public int Score { get; set; }
    public int Rounds { get; set; }
    public char GameMode { get; set; }
    public DateTime Time { get; set; }
}