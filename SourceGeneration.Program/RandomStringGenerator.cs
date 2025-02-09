namespace SourceGeneration.Program;

public interface IRandomStringGenerator
{
    string Next();
}

public class RandomStringGenerator : IRandomStringGenerator
{
    private readonly Random _rnd = new(); 
    private const string Chars = "abcdefghijklmnopqrstuvwxyz0123456789";
    private const int Size = 8;

    public string Next()
    {
        var randomSize = _rnd.Next(1, Size);
        var result = "";
        for (var i = 0; i < randomSize; i++)
        {
            var x = _rnd.Next(Chars.Length);
            result += Chars[x];
        }

        return result;
    }
}