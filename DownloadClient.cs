using System.CommandLine.Invocation;

namespace DotnetUpgrade {
    public class MacOSSdkDownloadClient {
        private HttpClient _client = new HttpClient() { Timeout = TimeSpan.FromMinutes(5)};
        public async Task<string> DownloadSdk(string version, string arch) {
            // https://learn.microsoft.com/en-us/dotnet/core/install/remove-runtime-sdk-versions?pivots=os-macos
            // need to download SDK, prompt and ask
            // two options, let's do the user facing one first
            // TODO determine right sdk to download
            // https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.403-macos-arm64-installer
            // TODO put in temp folder
            // Need to randomly parse for right download path here...

            var response = await _client.GetStringAsync($"https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-{version}-macos-{arch}-installer");

            var searchString = "window.open(\"";
            var startIndex = response.IndexOf(searchString);

            var endIndex = response.IndexOf("\", \"_self\"");

            var url = response.Substring(startIndex + searchString.Length, endIndex - startIndex - searchString.Length);

            var file = await _client.GetAsync(url);
            // TODO there should be an easy way to create a temp file with extension type
            var pathName = System.IO.Path.GetTempPath() + Path.GetRandomFileName() + ".pkg";
            using (var fs = new FileStream(pathName, 
                FileMode.OpenOrCreate))
            {
                // TODO make sure we cleanup file
                await file.Content.CopyToAsync(fs);
            }
            return pathName;
        }
// public async Task<string> DownloadSdk(string version, string arch) {
//     // https://learn.microsoft.com/en-us/dotnet/core/install/remove-runtime-sdk-versions?pivots=os-macos
//     // need to download SDK, prompt and ask
//     // two options, let's do the user facing one first
//     // TODO determine right sdk to download
//     // https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-6.0.403-macos-arm64-installer
//     // TODO put in temp folder
//     // Need to randomly parse for right download path here...

//     var response = await _client.GetStringAsync($"https://dotnet.microsoft.com/en-us/download/dotnet/thank-you/sdk-{version}-macos-{arch}-installer");

//     var urlRegex = new Regex("window.open\\(\"([^\"]+)\"");
//     var urlMatch = urlRegex.Match(response);
//     var url = urlMatch.Groups[1].Value;

//     var file = await _client.GetAsync(url);
//     var tempFileName = Path.GetTempFileName();
//     File.WriteAllBytes(tempFileName, await file.Content.ReadAsByteArrayAsync());
//     return tempFileName;
// }

// // ...

// // Delete the temporary file when it is no longer needed
// File.Delete(tempFileName);


    }
}