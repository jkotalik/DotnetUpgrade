
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

namespace DotnetSdk
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
            var command = new RootCommand()
            {
                Description = "Tool to upgrade current SDKs.",
            };
            command.Add(CreateUpgradeCommand());

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
            var command = new Command("upgrade", "upgrade current sdks")
            {

            };

            command.Handler = CommandHandler.Create<UpgradeArgs>(async args =>
            {
                // invoking command
                // so what does dotnet sdk check do?
                // It doesn't work for me?
                // invoke dotnet sdk check first to get each of them
                // parse each line, determine if status is up to date or not
                var res = new List<string>();
                var result = await Process.ExecuteAsync("dotnet", "sdk check", stdOut: (s =>
                {
                    res.Add(s);
                }));

                var sdks = new Dictionary<string, Sdk>();

                var index = 0;

                var line = res[index];

                if (line != ".NET SDKs:") {
                    // skip next two 
                    Console.WriteLine("no sdks");
                }

                // skipping table indexes
                index += 3;
                for (; index < res.Count; index++) {
                    line = res[index];
                    if (line == "") {
                        break;
                    }
                    // get index of first space, then trim around
                    var split = line.IndexOf(' ');
                    var version = line.Substring(0, split);
                    var status = line.Substring(split + 1).Trim();
                    sdks[version] = new Sdk{Version = version, Status = status};
                }
                foreach (var sdk in sdks) {
                    var version = sdk.Key;
                    var status = sdk.Value.Status;
                    MatchCollection collection = null;
                    if ((collection = _patchRegex.Matches(status)).Count > 0) {
                        // Patch {0} is available.
                        // time to try to patch!
                        var downloadVersion = collection[0].Groups[1].Value;
                        if (!sdks.ContainsKey(downloadVersion)) {
                            // https://learn.microsoft.com/en-us/dotnet/core/install/remove-runtime-sdk-versions?pivots=os-macos
                            // need to download SDK, prompt and ask
                            // two options, let's do the user facing one first
                            // TODO determine right sdk to download
                            // https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.403-macos-arm64-installer
                            // TODO put in temp folder
                            var client = new HttpClient();
                            // Need to randomly parse for right download path here...
                            var response = await client.GetStringAsync($"https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-{downloadVersion}-macos-arm64-installer");

                            var searchString = "window.open(\"";
                            var startIndex = response.IndexOf(searchString);


                            var endIndex = response.IndexOf("\", \"_self\"");

                            var url = response.Substring(startIndex + searchString.Length, endIndex - startIndex - searchString.Length);

                            // I actually really like this experience for MacOS...

                            var file = await client.GetAsync(url);
                            using (var fs = new FileStream("temp.pkg", 
                                FileMode.OpenOrCreate))
                            {
                                await file.Content.CopyToAsync(fs);
                            }
                            await Process.ExecuteAsync("open","temp.pkg --wait-apps");

                        }

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
                    } 
                }

                // check for feature band afterwards:
                // Try out the newest .NET SDK features with .NET 7.0.100.
                // 
            });

            return command;
        }

        private class Sdk {
            public string Version { get; set; }
            public string Status { get; set; }
        }

        private class UpgradeArgs
        {
            public IConsole Console { get; set; } = default!;

            public FileInfo Path { get; set; } = default!;
        }
    }
}