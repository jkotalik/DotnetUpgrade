using System.CommandLine.Invocation;

namespace DotnetUpgrade {

    public class Checker {
        private const string NewSdkVersion = "Try out the newest .NET SDK features with .NET ";

        // output example
        /*
        .NET SDKs:
        Version      Status
        ----------------------------------------
        6.0.306      Up to date.
        6.0.402      Patch 6.0.403 is available.
        6.0.403      Up to date.

        Try out the newest .NET SDK features with .NET 7.0.100.

        .NET Runtimes:
        Name                          Version      Status
        ---------------------------------------------------------------------
        Microsoft.AspNetCore.App      6.0.10       Patch 6.0.11 is available.
        Microsoft.NETCore.App         6.0.10       Patch 6.0.11 is available.
        Microsoft.AspNetCore.App      6.0.11       Up to date.
        Microsoft.NETCore.App         6.0.11       Up to date.


        The latest versions of .NET can be installed from https://aka.ms/dotnet-core-download. For more information about .NET lifecycles, see https://aka.ms/dotnet-core-support.

        */
        public static async Task<DotnetCheck> GetDotnetCheckOutput()
        {
            List<string> res = await GetOutputFromDotnetSdkCheck();

            var check = new DotnetCheck();

            var index = 0;

            var line = res[index];

            if (line != ".NET SDKs:")
            {
                // skip next two 
                throw new InvalidOperationException("not in the right line state, expected sdks");
            }

            // skipping table indexes
            index += 3;
            for (; index < res.Count; index++)
            {
                line = res[index];
                if (line == "")
                {
                    break;
                }
                // get index of first space, then trim around
                var split = line.IndexOf(' ');
                var version = line.Substring(0, split);
                var status = line.Substring(split + 1).Trim();
                check.Sdks[version] = status;
            }

            index++;
            if (index >= res.Count) {
                return check;
            }

            // now check if next line contains info about upgrade
            line = res[index];
            if (line.StartsWith(NewSdkVersion)) {
                check.LatestDotnetSdkVersion = line.Substring(NewSdkVersion.Length).Trim().TrimEnd('.');
                index += 2;
                line = res[index];
            }

            if (line != ".NET Runtimes:") {
                throw new InvalidOperationException("not in the right line state, expected runtimes");
            }
            index += 3;

            for (; index < res.Count; index++)
            {
                line = res[index];
                if (line == "")
                {
                    break;
                }
                // get index of first space, then trim around
                var splitName = line.IndexOf(' ');
                var name = line.Substring(0, splitName);
                var remainder = line.Substring(splitName + 1).Trim();

                var splitVersion = remainder.IndexOf(' ');
                var version = remainder.Substring(0, splitVersion);
                var status = remainder.Substring(splitVersion + 1).Trim();
                check.Runtimes[name + "|" + version] = status;
            }
            
            return check;
        }

        private static async System.Threading.Tasks.Task<List<string>> GetOutputFromDotnetSdkCheck()
        {
            // Get Output from dotnet sdk check
            var res = new List<string>();
            var result = await Process.ExecuteAsync("dotnet", "sdk check", stdOut: (s =>
            {
                res.Add(s);
            }));
            return res;
        }
    }
}