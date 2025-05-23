using System;
using System.Collections.Generic;

public class SymbolTable
{
    private Dictionary<string, string> variables = new Dictionary<string, string>();

    public bool AddVariable(string name, string type)
    {
        if (variables.ContainsKey(name))
            return false; // Переменная уже объявлена

        variables[name] = type;
        return true;
    }

    public bool IsDeclared(string name)
    {
        return variables.ContainsKey(name);
    }

    public string GetType(string name)
    {
        if (variables.TryGetValue(name, out string type))
            return type;
        return null;
    }
}
