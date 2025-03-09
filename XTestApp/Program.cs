namespace XTestApp;

internal static class Program
{
    private static void Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Hello, World!");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine(Environment.CommandLine);
        Console.ResetColor();

        Console.WriteLine();
        foreach (string arg in args)
        {
            Console.WriteLine(arg);
            Console.WriteLine();
        }

        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write("Press any key to continue.");
        Console.ReadKey(true);
    }
}
