using CommandDotNet;
using phat.Interface.CMD;

Console.Title = "Phat";


new AppRunner<ConsoleCommand>()
    .UseTypoSuggestions()
    .Run(args);


