try
{
    Console.Clear();
}
catch (IOException)
{
    throw new IOException("Debug console (internal terminal) in VS Code is not supported, please use integrated terminal or external terminal.");
}

Console.WriteLine("Welcome to Simple Math Game, created by womzil.");
Console.WriteLine();
Console.WriteLine(@"In this game, you have to guess results of some 
simple math equations (addition, subtraction, division, multiplication)
on whole numbers from range 1-100. Results of division also should be whole numbers.");
Console.WriteLine();
Console.WriteLine();
