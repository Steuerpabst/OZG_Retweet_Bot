using System.Diagnostics;
using Tweetinvi;
using Tweetinvi.Events;
using Tweetinvi.Models;
using Tweetinvi.Streaming;
using Tweetinvi.Streaming.Events;

namespace OZG_Retweet_Bot
{
  internal class TwitterStream
  {

    #region Attributes

    private IFilteredStream _filteredStream;
    private ITwitterClient _twitterClient;
    private readonly TwitterConfig _twitterConfig;

    #endregion

    #region Constructor

    public TwitterStream()
    {
      _twitterConfig = new TwitterConfig();
    }

    #endregion

    #region Tasks

    public Task Initialize()
    {
      _twitterClient = new TwitterClient(_twitterConfig.ConsumerKey, _twitterConfig.ConsumerKeySecret, _twitterConfig.AccessToken, _twitterConfig.AccessTokenSecret);

      _filteredStream = _twitterClient.Streams.CreateFilteredStream();

      if (_twitterConfig.TermsToRetweet != null)
      {
        foreach (string term in _twitterConfig.TermsToRetweet)
        {
          _filteredStream.AddTrack(term);
        }
      }

      if (_twitterConfig.UsersToRetweet != null)
      {
        foreach (long users in _twitterConfig.UsersToRetweet)
        {
          _filteredStream.AddFollow(users);
        }
      }

      return Task.CompletedTask;
    }

    public async Task StartTwitterStream()
    {

      _filteredStream.MatchingTweetReceived += OnTweetReceived;

      _filteredStream.StreamStopped += (s, e) =>
      {
        Exception exc = e.Exception;
        IDisconnectMessage disconnectMessage = e.DisconnectMessage;

        if (exc != null)
        {
          LogWriter.WriteToLog(exc, Log.LogLevel.ERROR);
        }

        if (disconnectMessage != null)
        {
          LogWriter.WriteToLog("Stream stopped:  {disconnectMessage.Code} {disconnectMessage.Reason}", Log.LogLevel.ERROR);
        }
      };

      do
      {
        try
        {
          LogWriter.WriteToLog("Connecting to stream", Log.LogLevel.INFO);

          await _filteredStream.StartMatchingAnyConditionAsync();
        }
        catch (Exception exc)
        {
          LogWriter.WriteToLog(exc, Log.LogLevel.ERROR);

          if (_filteredStream.StreamState != StreamState.Stop)
          {
            _filteredStream.Stop();
          }

          await Task.Delay(5000);
        }
      }
      while (true);
    }

    #endregion

    #region Methods

    private async void OnTweetReceived(object? sender, MatchedTweetReceivedEventArgs receivedEventArgs)
    {
      if (CheckTweetForRetweet(receivedEventArgs))
      {
        try
        {
          await _twitterClient.Tweets.PublishRetweetAsync(receivedEventArgs.Tweet);
          Debug.Print(receivedEventArgs.Tweet.CreatedBy.ScreenName + ": " + receivedEventArgs.Tweet.Text);
          LogWriter.WriteToLog(receivedEventArgs.Tweet, Log.LogLevel.TWEET);
        }
        catch (Exception ex)
        {
          LogWriter.WriteToLog(ex, Log.LogLevel.ERROR);
        }
      }
    }

    private static bool CheckTweetForRetweet(MatchedTweetReceivedEventArgs receivedEventArgs)
    {
      int count = 0;
      int countAggregate = 0;
      var config = new TwitterConfig();

      HashSet<string> receivedHashtags = new HashSet<string>();

      for (int i = 0; i < receivedEventArgs.MatchingTracks.Length; i++)
      {
        receivedHashtags.Add(receivedEventArgs.MatchingTracks[i].ToLower());
      }

      ITweet tweet = receivedEventArgs.Tweet;

      for (int i = 0; i < config.UsersToRetweet.Length; i++)
      {
        if (tweet.CreatedBy.Id == config.UsersToRetweet[i])
        {
          return true;
        }
      }

      for (int i = 0; i < config.TermsToRetweet.Length; i++)
      {
        if (receivedHashtags.Contains(config.TermsToRetweet[i]))
        {
          if (config.TermsToRetweet[i] != "#opendata" && config.TermsToRetweet[i] != "#opensource")
          {
            count++;
          }
        }
      }

      if (receivedHashtags.Contains("#opendata"))
      {
        countAggregate++;
      }
      if (receivedHashtags.Contains("#opensource"))
      {
        countAggregate++;
      }

      if ((!tweet.IsRetweet) && (tweet.Language == Language.German) && (countAggregate > 1 || (countAggregate >= 0 && count > 0)))
      {
        return true;
      }

      return false;
    }

    #endregion
  }
}
