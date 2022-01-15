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

    private System.Timers.Timer _startTimer;
    private PeriodicTimer? _timer;

    #endregion

    #region Constructor

    private DailyTweet()
    {
      _twitterConfig = new TwitterConfig();
      _twitterClient = new TwitterClient(_twitterConfig.ConsumerKey, _twitterConfig.ConsumerKeySecret, _twitterConfig.AccessToken, _twitterConfig.AccessTokenSecret);

      _dateTime = DateTime.Now;

      TimeSpan day = new TimeSpan(24, 0, 0);
      TimeSpan now = TimeSpan.Parse(_dateTime.ToString("HH:mm"));
      TimeSpan fireEvent = new TimeSpan(9, 00, 0);

      TimeSpan timeLeftUntilFirstRun =(day - now + fireEvent);

      if(timeLeftUntilFirstRun.TotalHours > 24)
      {
        timeLeftUntilFirstRun -= new TimeSpan(24, 0, 0);
      }

      _startTimer = new System.Timers.Timer();
      _startTimer.Interval = timeLeftUntilFirstRun.TotalMilliseconds;
      _startTimer.Elapsed += OnTimeEvent;
      _startTimer.Start();
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
          ResourceManager resource = new(resourceName, assembly);

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
      while(await _timer!.WaitForNextTickAsync())
      {
        await TweetDaily();
      }
    }
    
    
    public async Task TweetDaily()
    {
      Random random = new();

      int randomQuote = random.Next(0, _quotes!.Length);

      string[] dailyQuote = _quotes[randomQuote].Split("|");

      Quote2Image quote2Image = new(dailyQuote[0], dailyQuote[1]);

      byte[] tweetPic = quote2Image.DrawToImage(randomQuote)!;

      string motivationText = "#" + DateTime.Now.DayOfWeek + "Motivation";

      if (tweetPic != null)
      {
        try
        {

          LogWriter.WriteToLog("Zitat " + randomQuote + " gesendet, " + motivationText, Log.LogLevel.INFO);
          
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
        LogWriter.WriteToLog("Es wurde kein Picture übergeben!", Log.LogLevel.ERROR);
      }
    }

    #endregion

    #region Methods

    private async void OnTimeEvent(object? source, System.Timers.ElapsedEventArgs eventArgs)
    {
      await TweetDaily();

      _startTimer.Stop();

      _timer = new PeriodicTimer(TimeSpan.FromHours(24)) ;

      _ = WaitingTask();
    }

    #endregion
  }
}
