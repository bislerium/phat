using CommandDotNet;
using phat.Interface.CMD;

Console.Title = Settings.AppTitle;


new AppRunner<ConsoleCommand>()
    .UseTypoSuggestions()
    .Run(args);


