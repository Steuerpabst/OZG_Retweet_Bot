using System.Runtime.Versioning;

namespace OZG_Retweet_Bot
{
  static class Program
  {
    [SupportedOSPlatform("windows")]
    public static async Task Main()
    {
      _ = LogWriter.GetInstance;
      
      TwitterStream twitterStream = new TwitterStream();
      DailyTweet dailyTweet = new DailyTweet();

      await twitterStream.Initialize();
      await twitterStream.StartTwitterStream();
      //await dailyTweet.TweetDaily();
    }
  }
}