using System.Collections.Generic;

public static class ErrorHandler
{
    private static List<string> errors = new List<string>();

    public static void Add(string message)
    {
        errors.Add(message);
    }

    public static bool HasErrors => errors.Count > 0;

    public static void PrintAll()
    {
        foreach (var err in errors)
            System.Console.WriteLine(err);
    }

    public static void Clear()
    {
        errors.Clear();
    }
}
