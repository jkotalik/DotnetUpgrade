
// Step 1, check what sdks are currently installed
// Step 2, get latest sdks available
// Step 3, install latest sdk
// Step 4, $$$

// get existing info from current runtime (arch, channel, installdir, etc)
// version
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.Text.RegularExpressions;

namespace DotnetUpgrade
{
    public class Program
    {
        // Things to do in priority order
        // For macos experience
        // How can you wait for a package to be installed?
        // All different OS and ARCH options
        // Optimize for interactive and silent option
        // prompt for user input to delete old sdks
        // https://learn.microsoft.com/en-us/dotnet/core/additional-tools/uninstall-tool?tabs=macos
        // can uninstall with this tool

        public static Task<int> Main(string[] args)
        {
            // dotnet sdk check
            var command = CreateUpgradeCommand();
            // command.Add(CreateUpgradeCommand());

            var builder = new CommandLineBuilder(command);
            builder.UseHelp();
            builder.UseVersionOption();
            builder.CancelOnProcessTermination();

            var parser = builder.Build();
            return parser.InvokeAsync(args);
        }

        private static Regex _patchRegex = new Regex("Patch (.*) is available.");

        private static Command CreateUpgradeCommand()
        {
            // dotnet 
            var command = new RootCommand("upgrade current sdks")
            {
            };

            command.Handler = CommandHandler.Create<UpgradeArgs>(async args =>
            {

                Console.WriteLine("Capturing SDKs and Runtimes...");
                var check = await Checker.GetDotnetCheckOutput();

                HashSet<string> sdksToDownload = new HashSet<string>();
                HashSet<string> sdksToRemove = new HashSet<string>();

                // Get SDKs to upgrade to rather than the list of all ones that need to be upgraded
                foreach (var sdk in check.Sdks) {
                    var version = sdk.Key;
                    var status = sdk.Value;
                    MatchCollection collection = null;
                    if ((collection = _patchRegex.Matches(status)).Count > 0) {
                        // Patch {0} is available.
                        // time to try to patch!
                        var downloadVersion = collection[0].Groups[1].Value;

                        if (!check.Sdks.ContainsKey(downloadVersion)) {
                            sdksToDownload.Add(downloadVersion);
                        }
                            // silent stuff
                            // if (!sdks.ContainsKey(downloadVersion)) {
                            // need to download SDK, prompt and ask
                            // two options, let's do the user facing one first
                            // TODO determine right sdk to download
                            // https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.403-macos-arm64-installer
                            // TODO put in temp folder
                            // var client = new HttpClient();
                            // var response = await client.GetAsync($"https://dotnet.microsoft.com/download/dotnet/scripts/v1/dotnet-install.sh");

                            // using (var fs = new FileStream("temp.sh", 
                            //     FileMode.Create))
                            // {
                            //     await response.Content.CopyToAsync(fs);
                            // }
                            // await ShellHelper.Bash("/Users/justinkotalik/code/sdkdownload/temp.sh");
                        
                        // check if the version exists
                    } else if (status == "Up to date.") {
                        // nothing to upgrade, we good!
                    } else if (status == "") {
                        
                    }
                }

                var client = new MacOSSdkDownloadClient();

                foreach (var sdk in sdksToDownload)
                {
                    Console.WriteLine($"Detected newer SDK version {sdk}, downloading...");
                    var binary = await client.DownloadSdk(sdk, "arm64");

                    Console.WriteLine($"Prompting download of MacOS pkg...");
                    // this will be per os
                    await Process.ExecuteAsync("open",$"{binary} --wait-apps");
                }
                // check for feature band afterwards:
                // Try out the newest .NET SDK features with .NET 7.0.100.

                Console.WriteLine("Successfully updated .NET SDKs");
            });

            return command;
        }


        private class UpgradeArgs
        {
            public IConsole Console { get; set; } = default!;

            public FileInfo Path { get; set; } = default!;
        }
    }
}