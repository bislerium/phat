using CommandDotNet;
using CommandDotNet.DataAnnotations;
using phat.Interface.CMD;

Console.Title = Settings.AppTitle;


new AppRunner<ConsoleCommand>()
    .UseTypoSuggestions()
    .UseDataAnnotationValidations()
    .UseVersionMiddleware()
    .UseTimeDirective()
    .Run(args);


