namespace OZG_Retweet_Bot
{
  internal class Log
  {
    #region Attributes

    private DateTime _logTime;
    private string? _logMessage;
    private TweetToRetweet? _logTweet;

    private LogLevel _logLevel;

    #endregion

    #region Enumerations

    public enum LogLevel
    {
      OFF,
      ERROR,
      WARNING,
      INFO,
      TWEET,
      ALL
    }

    #endregion

    #region Constructor

    public Log()
    {
      _logTime = DateTime.Now;
    }

    public Log(string message, LogLevel level)
    {
      _logMessage = message;
      _logLevel = level;
      _logTime = DateTime.Now;
      _logTweet = null;
    }

    public Log(TweetToRetweet tweet, LogLevel level)
    {
      _logTweet = tweet;
      _logLevel= level;
      _logTime= DateTime.Now;
      _logMessage = null;
    }

    #endregion

    #region Properties

    public string? LogMessage
    {
      get { return _logMessage; }
      set { _logMessage = value; }
    }

    public TweetToRetweet? LogTweet
    {
      get { return _logTweet; }
      set { _logTweet = value; }
    }

    public LogLevel Level
    {
      get { return _logLevel; }
      set { _logLevel = value; }
    }

    public string GetDate => _logTime.ToString("yyyy-MM-dd");

    public string GetTime => _logTime.ToString("HH:mm:ss.fff");

    #endregion
  }
}
