using System.Configuration;
using System.Diagnostics;
using Tweetinvi.Models;
using static OZG_Retweet_Bot.Log;

namespace OZG_Retweet_Bot
{
  internal class LogWriter
  {

    #region Attributes

    private static LogWriter? Instance;

    private static Queue<Log> _logQueue;

    private static readonly string _baseDirectory = AppDomain.CurrentDomain.BaseDirectory;

    private string? _subDirectory;
    private string? _logDirectory;
    private string? _logFile;

    private DateTime _logTime;

    private FileTypes _fileTypes;

    private static readonly int _flushAtAge = Convert.ToInt32(ConfigurationManager.AppSettings["flush_at_age"]);
    private static readonly int _flushAtQty = Convert.ToInt32(ConfigurationManager.AppSettings["flush_at_quantity"]);

    private static DateTime _flushedAt;

    #endregion

    #region Constructor

    private LogWriter()
    {
    }

    public static LogWriter GetInstance
    {
      get
      {
        if (Instance == null)
        {
          Instance = new LogWriter();
          _logQueue = new Queue<Log>();
          _flushedAt = DateTime.Now;
        }

        return Instance;
      }
    }

    #endregion

    #region Enumerations

    public enum FileTypes
    {
      Log,
      Error,
      Tweet
    }

    #endregion

    #region Properties

    public FileTypes FileType
    {
      get { return _fileTypes; }
      set
      {
        _fileTypes = value;

        switch (value)
        {
          case FileTypes.Log:
            SubDirectory = "\\logs\\";
            break;
          case FileTypes.Error:
            SubDirectory = "\\errorlogs\\";
            break;
          case FileTypes.Tweet:
            SubDirectory = "\\tweetlogs\\";
            break;
        }
      }
    }

    public static bool CheckTimeToFlush()
    {
      TimeSpan timeSpan = DateTime.Now - _flushedAt;

      if (timeSpan.TotalSeconds > _flushAtAge)
      {
        _flushedAt = DateTime.Now;
        return true;
      }

      return false;
    }

    public string SubDirectory
    {
      get
      {
        if (_subDirectory == null)
        {
          _subDirectory = "\\logs\\";
        }
        return _subDirectory;
      }
      set
      {
        _subDirectory = "\\" + value + "\\";
        _logDirectory = null;
        _logFile = null;
      }
    }

    public string LogDirectory
    {
      get
      {
        if (_logDirectory == null)
        {
          _logTime = DateTime.Now;
          _logDirectory = _baseDirectory + SubDirectory + _logTime.ToString("yyyyMMdd") + "\\";

          if (!Directory.Exists(_logDirectory))
          {
            Directory.CreateDirectory(_logDirectory);
          }
        }
        return _logDirectory;
      }
    }

    public string LogFile
    {
      get
      {
        if (_logFile == null)
        {
          _logFile = LogDirectory + _logTime.ToString("yyyyMMdd") + ".log";
        }
        return (_logFile);
      }
    }

    #endregion

    #region Methods

    public static void WriteToLog(string message, LogLevel level)
    {
      lock (_logQueue)
      {
        // Create log
        Log log = new Log(message, level);
        _logQueue.Enqueue(log);

        // Check if should flush
        if (_logQueue.Count >= _flushAtQty || CheckTimeToFlush())
        {
          FlushLogToFile();
        }
      }
    }

    public static void WriteToLog(Exception exc, LogLevel level)
    {
      lock (_logQueue)
      {
        // Create log
        Log msg = new Log(exc.Message.ToString().Trim(), level);
        _logQueue.Enqueue(msg);

        // Check if should flush
        if (_logQueue.Count >= _flushAtQty || CheckTimeToFlush())
        {
          FlushLogToFile();
        }
      }
    }

    public static void WriteToLog(ITweet tweet, LogLevel level)
    {
      lock (_logQueue)
      {
        // Create log       
        TweetToRetweet tweetToRetweet = new TweetToRetweet();

        tweetToRetweet.TweetId = tweet.IdStr;
        tweetToRetweet.TweetText = tweet.FullText.Replace('\n', ' ');
        tweetToRetweet.TweetCreatedBy = tweet.CreatedBy.ScreenName;
        tweetToRetweet.TweetCreatedAt = tweet.CreatedAt;
        tweetToRetweet.TweetLanguage = tweet.Language.ToString();
        tweetToRetweet.TweetHashtagCount = tweet.Hashtags.Count;

        List<string> tags = new List<string>();
        if (tweetToRetweet.TweetHashtagCount > 0)
        {
          for (int i = 0; i < tweet.Hashtags.Count; i++)
          {
            tags.Add(tweet.Hashtags[i].Text);
          }
        }

        tweetToRetweet.TweetHashtags = tags;

        tweetToRetweet.TweetUserId = tweet.CreatedBy.IdStr;
        tweetToRetweet.TweetUserFollower = tweet.CreatedBy.FollowersCount;
        tweetToRetweet.TweetUserFollowing = tweet.CreatedBy.FriendsCount;
        tweetToRetweet.TweetUserTweets = tweet.CreatedBy.StatusesCount;
        tweetToRetweet.TweetUserVerified = tweet.CreatedBy.Verified;

        Log tweetLog = new Log(tweetToRetweet, level);
        _logQueue.Enqueue(tweetLog);

        // Check if should flush
        if (_logQueue.Count >= _flushAtQty || CheckTimeToFlush())
        {
          FlushLogToFile();
        }
      }
    }

    public static void FlushLogToFile()
    {
      var myInstance = new LogWriter();

      while (_logQueue.Count > 0)
      {
        // get entry to log
        Log entry = _logQueue.Dequeue();

        int logLevel = (int)entry.Level;

        myInstance.FileType = logLevel switch
        {
          // LogLevel.OFF
          0 => FileTypes.Log,
          // LogLevel.ERROR
          1 => FileTypes.Error,
          // LogLevel.WARNING
          2 => FileTypes.Log,
          // LogLevel.INFO
          3 => FileTypes.Log,
          // LogLevel.TWEET
          4 => FileTypes.Tweet,
          // LogLevel.ALL
          5 => FileTypes.Log,
          _ => FileTypes.Log,
        };

        string? strLogLevel = ConfigurationManager.AppSettings["log_level"];
        int configLogLevel = (int)LogLevel.OFF;

        if (!String.IsNullOrEmpty(strLogLevel))
        {
          configLogLevel = (int)Enum.Parse(typeof(LogLevel), strLogLevel);
        }

        if (configLogLevel >= logLevel)
        {
          if (!File.Exists(myInstance.LogFile))
          {
            FileStream fileStream = File.Create(myInstance.LogFile);
            fileStream.Close();
          }

          try
          {
            DateTime dtLog = DateTime.Now;
            StreamWriter sWriter = File.AppendText(myInstance.LogFile);
            sWriter.WriteLine(String.Format(@"{0}" + "\t" + "{1}" + "\t" + "{2}", entry.GetDate, entry.GetTime, MakeMessage(entry)));
            sWriter.Flush();
            sWriter.Close();
          }
          catch (Exception ex)
          {
            Debug.Print(ex.Message.ToString());
            WriteToLog(ex, LogLevel.ERROR);
          }
        }
      }
    }

    private static string? MakeMessage(Log entry)
    {

      if (entry.LogMessage != null)
      {
        return entry.LogMessage.ToString();
      }
      else if (entry.LogTweet != null)
      {
        string message = entry.LogTweet.TweetId + "\t";
        message = message + entry.LogTweet.TweetText + "\t";
        message += entry.LogTweet.TweetCreatedBy + "\t";
        message += entry.LogTweet.TweetCreatedAt.ToString("dd.MM.yyyy HH:mm:ss.fff") + "\t";
        message += entry.LogTweet.TweetLanguage + "\t";
        message += entry.LogTweet.TweetUserId + "\t";
        message += entry.LogTweet.TweetUserFollower.ToString() + "\t";
        message += entry.LogTweet.TweetUserFollowing.ToString() + "\t";
        message += entry.LogTweet.TweetUserTweets.ToString() + "\t";
        message += entry.LogTweet.TweetUserVerified.ToString();

        for (int i = 0; i < entry.LogTweet.TweetHashtags.Count; i++)
        {
          message += "\t" + entry.LogTweet.TweetHashtags[i].ToLower();
        }
        return message;
      }
      return null;
    }

    #endregion
  }
}
