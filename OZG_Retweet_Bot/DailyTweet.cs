using System.Diagnostics;
using System.Reflection;
using System.Resources;
using System.Runtime.Versioning;
using Tweetinvi;
using Tweetinvi.Models;
using Tweetinvi.Parameters;

namespace OZG_Retweet_Bot
{
  internal class DailyTweet
  {
    #region Attributes

    private readonly ITwitterClient _twitterClient;
    private readonly TwitterConfig _twitterConfig;
    private DateTime _dateTime;

    private string[] _quotes;

    #endregion

    #region Constructor

    public DailyTweet()
    {
      _twitterConfig = new TwitterConfig();
      _twitterClient = new TwitterClient(_twitterConfig.ConsumerKey, _twitterConfig.ConsumerKeySecret, _twitterConfig.AccessToken, _twitterConfig.AccessTokenSecret);
      _dateTime = DateTime.Now;

      Assembly assembly = Assembly.GetExecutingAssembly();

      string resourceName = assembly.GetName().Name + ".Properties.Resources";
      ResourceManager resource = new ResourceManager(resourceName, assembly);

      _quotes = ((string)resource.GetObject("quotes")!).Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
    }

    #endregion

    #region Tasks

    [SupportedOSPlatform("windows")]
    public async Task TweetDaily()
    {
      Random random = new Random();

      int randomQuote = random.Next(0, _quotes.Length);

      string[] dailyQuote = _quotes[randomQuote].Split("|");

      Quote2Image quote2Image = new Quote2Image(dailyQuote[0], dailyQuote[1]);

      byte[] tweetPic = quote2Image.DrawToImage();

      try
      {
        IMedia uploadedImage = await _twitterClient.Upload.UploadTweetImageAsync(tweetPic);

        await _twitterClient.Tweets.PublishTweetAsync(new PublishTweetParameters("")
        {
          Medias = { uploadedImage }
        });
      }
      catch (Exception ex)
      {
        Debug.Print(ex.Message.ToString());
      }
    }

    #endregion
  }
}
