using System.Reflection;
using System.Resources;
using System.Runtime.Versioning;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace OZG_Retweet_Bot
{
  [SupportedOSPlatform("windows")]
  internal class DailyTweet
  {
    #region Attributes

    private static DailyTweet? Instance;

    private readonly ITwitterClient? _twitterClient;
    private readonly TwitterConfig? _twitterConfig;

    private readonly DateTime _dateTime;

    private static string[]? _quotes;

    private readonly PeriodicTimer _timer;

    #endregion

    #region Constructor

    private DailyTweet()
    {
      _twitterConfig = new TwitterConfig();
      _twitterClient = new TwitterClient(_twitterConfig.ConsumerKey, _twitterConfig.ConsumerKeySecret, _twitterConfig.AccessToken, _twitterConfig.AccessTokenSecret);

      _dateTime = DateTime.Now;

      _timer = new PeriodicTimer(TimeSpan.FromHours(24));

      _ = WaitingTask();
    }

    public static DailyTweet GetInstance
    {
      get
      {
        if (Instance == null)
        {

          Instance = new DailyTweet();

          Assembly assembly = Assembly.GetExecutingAssembly();

          string resourceName = assembly.GetName().Name + ".Properties.Resources";
          ResourceManager resource = new ResourceManager(resourceName, assembly);

          _quotes = ((string)resource.GetObject("quotes")!).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
        }

        return Instance;
      }
    }

    #endregion

    #region Tasks

    // Thanks to: İLKAY İLKNUR (https://www.ilkayilknur.com/a-new-modern-timer-api-in-dotnet-6-periodictimer)
    private async Task WaitingTask()
    {
      while(await _timer.WaitForNextTickAsync())
      {
        await TweetDaily();
      }
    }
    
    
    public async Task TweetDaily()
    {
      Random random = new Random();

      int randomQuote = random.Next(0, _quotes!.Length);

      string[] dailyQuote = _quotes[randomQuote].Split("|");

      Quote2Image quote2Image = new Quote2Image(dailyQuote[0], dailyQuote[1]);

      byte[] tweetPic = quote2Image.DrawToImage(randomQuote)!;

      string motivationText = "#" + _dateTime.DayOfWeek + "Motivation";

      if (tweetPic != null)
      {
        try
        {

          LogWriter.WriteToLog("Zitat " + randomQuote + " gesendet", Log.LogLevel.INFO);
          
          IMedia uploadedImage = await _twitterClient!.Upload.UploadTweetImageAsync(tweetPic);

          await _twitterClient.Tweets.PublishTweetAsync(new PublishTweetParameters(motivationText + "\n" + "#OZGLounge #Twitterverwaltung")
          {
            Medias = { uploadedImage }
          });
          
        }
        catch (Exception ex)
        {
          LogWriter.WriteToLog(ex, Log.LogLevel.ERROR);
        }
      }
      else
      {
        LogWriter.WriteToLog("Es wurde kein Pictures übergeben!", Log.LogLevel.ERROR);
      }
    }

    #endregion
  }
}
