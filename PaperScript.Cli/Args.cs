namespace PaperScript.Cli;

public class Args
{
    public string? InputFile { get; set; }
    public string? OutputFile { get; set; }

    public static Args Parse(string[] args)
    {
        var result = new Args();
        int i = 0;

        // First positional argument = input file
        if (i < args.Length && !args[i].StartsWith("-"))
        {
            result.InputFile = args[i++];
        }
        else
        {
            throw new ArgumentException("Missing input file");
        }

        // Parse flags
        while (i < args.Length)
        {
            switch (args[i])
            {
                case "-o":
                case "--output":
                    if (++i < args.Length)
                        result.OutputFile = args[i++];
                    else
                        throw new ArgumentException("Missing value for -o");
                    break;

                default:
                    throw new ArgumentException($"Unknown argument: {args[i]}");
            }
        }

        return result;
    }
}