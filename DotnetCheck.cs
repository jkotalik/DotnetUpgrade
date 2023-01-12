namespace DotnetUpgrade
{
    public class DotnetCheck {
        public string LatestDotnetSdkVersion {get;set;}
        public Dictionary<string, string> Sdks {get;set;} = new Dictionary<string, string>();
        public Dictionary<string, string> Runtimes {get;set;} = new Dictionary<string, string>();
    }
}