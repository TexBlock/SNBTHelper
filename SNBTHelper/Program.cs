// See https://aka.ms/new-console-template for more information

using System.CommandLine;
using System.CommandLine.Invocation;

namespace Snbt.Helper
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var rootCommand = new RootCommand("SNBT files helper");

            var snbtToJsonCommand = new Command("snbt2json", "Convert SNBT files to JSON");

            var jsonToSnbtCommand = new Command("json2snbt", "Convert JSON files to SNBT");

            var inputDirectoryArgument = new Argument<string>("input-dir", "The directory containing SNBT files.")
            {
                Arity = ArgumentArity.ExactlyOne
            };

            var outputDirectoryArgument = new Argument<string>("output-dir", "The directory where JSON files will be saved.")
            {
                Arity = ArgumentArity.ExactlyOne
            };

            
            ConvertSnbtToJsonCommand(rootCommand, snbtToJsonCommand, inputDirectoryArgument, outputDirectoryArgument);
            ConvertJsonToSnbtCommand(rootCommand, jsonToSnbtCommand, inputDirectoryArgument, outputDirectoryArgument);

            rootCommand.InvokeAsync(args).Wait();
        }

        private static void ConvertSnbtToJsonCommand(RootCommand rootCommand, Command snbtToJsonCommand, Argument<string> inputDirectoryArgument, Argument<string> outputDirectoryArgument)
        {
            snbtToJsonCommand.AddArgument(inputDirectoryArgument);
            snbtToJsonCommand.AddArgument(outputDirectoryArgument);

            snbtToJsonCommand.SetHandler((context) =>
            {
                var inputDirectory = context.ParseResult.GetValueForArgument(inputDirectoryArgument);
                var outputDirectory = context.ParseResult.GetValueForArgument(outputDirectoryArgument);

                SnbtConverter.ConvertAllSnbtToJson(inputDirectory, outputDirectory);
            });

            rootCommand.AddCommand(snbtToJsonCommand);
        }

        private static void ConvertJsonToSnbtCommand(RootCommand rootCommand, Command jsonToSnbtCommand, Argument<string> inputDirectoryArgument, Argument<string> outputDirectoryArgument)
        {
            jsonToSnbtCommand.AddArgument(inputDirectoryArgument);
            jsonToSnbtCommand.AddArgument(outputDirectoryArgument);

            jsonToSnbtCommand.SetHandler((context) =>
            {
                var inputDirectory = context.ParseResult.GetValueForArgument(inputDirectoryArgument);
                var outputDirectory = context.ParseResult.GetValueForArgument(outputDirectoryArgument);

                SnbtConverter.ConvertAllJsonToSnbt(inputDirectory, outputDirectory);
            });

            rootCommand.AddCommand(jsonToSnbtCommand);
        }
    }
}
