try
{
    Console.Clear();
}
catch (IOException)
{
    throw new IOException("Debug console (internal terminal) in VS Code is not supported, please use integrated terminal or external terminal.");
}

char[] equations = ['+', '-', '/', '*', '^'];
Random rand = new Random();
int score = 0;

Console.WriteLine("Welcome to Simple Math Game, created by womzil.");
Console.WriteLine();
Console.WriteLine(@"In this game, you have to guess results of some 
simple math equations (addition, subtraction, division, multiplication)
on whole numbers from range 1-100. Results of division also should be whole numbers.");
Console.WriteLine();

ConsoleKey key = ContinueOrExit("Press \"Enter\" to start playing or \"Escape\" to exit.");
PlayAgainOrLeave();

void PlayAgainOrLeave()
{
    do
    {
        if (key == ConsoleKey.Enter)
        {
            StartGame();
            key = ContinueOrExit("Do you wish to play again? Press \"Enter\" to continue or \"Escape\" to exit.");
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

    } while (key != ConsoleKey.Enter && key != ConsoleKey.Escape);
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

    for (int i = 1; i <= numberOfRounds; i++)
    {
        StartRound(i, numberOfRounds);
    }

    Console.Clear();
    Console.WriteLine("Game completed!");
    Console.WriteLine($"Your score: {score}/{numberOfRounds}.");
    Console.WriteLine();
}

void StartRound(int numberOfRound, int numberOfRounds)
{
    Console.Clear();
    Console.WriteLine($"Round number {numberOfRound}/{numberOfRounds}");
    Console.WriteLine();

    int result;
    char equation = equations[rand.Next(0, equations.Length)];
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
    string instructions = "Press \"Enter\" to continue or \"Escape\" to exit.", 
    string wrongKey = "Wrong key. You need to press \"Enter\" or \"Escape\".")
{
    Console.WriteLine(instructions);
    ConsoleKey key = Console.ReadKey().Key;
    Console.WriteLine();

    while (key != ConsoleKey.Enter && key != ConsoleKey.Escape)
    {
        Console.WriteLine(wrongKey);
        Console.Write("Your input: ");
        key = Console.ReadKey().Key;
        Console.WriteLine();
    }

    return key;
}