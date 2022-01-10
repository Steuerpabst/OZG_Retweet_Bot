using System.Runtime.Versioning;

namespace OZG_Retweet_Bot
{
  static class Program
  {
    [SupportedOSPlatform("windows")]
    public async static Task Main()
    {
      _ = LogWriter.GetInstance;
      _ = DailyTweet.GetInstance;

      TwitterStream twitterStream = new();
     
      await twitterStream.Initialize();
      await twitterStream.StartTwitterStream();
    }
  }
}