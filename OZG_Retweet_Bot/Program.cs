namespace OZG_Retweet_Bot
{
  static class Program
  {
    public static async Task Main()
    {
      _ = LogWriter.GetInstance;
      
      TwitterStream twitterStream = new TwitterStream();

      await twitterStream.Initialize();
      await twitterStream.StartTwitterStream();
    }
  }
}