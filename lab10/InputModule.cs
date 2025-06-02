public class InputModule : IDisposable
{
    private readonly StreamReader _reader;
    public char currentChar { get; private set; }
    public bool endOfFile { get; private set; }

    public InputModule(string filename)
    {
        try
        {
            _reader = new StreamReader(filename);
        }
        catch (FileNotFoundException)
        {
            ErrorTable.Report(100);
            throw;
        }
        catch (Exception)
        {
            ErrorTable.Report(101);
            throw;
        }
    }

    public char NextChar()
    {
        int ch = _reader.Read();        
        if (ch == -1)
        {
            endOfFile = true;           
            return '\0';             
        }

        currentChar = (char)ch;         
        return currentChar;             
    }

    public void Dispose() => _reader?.Dispose();
}
