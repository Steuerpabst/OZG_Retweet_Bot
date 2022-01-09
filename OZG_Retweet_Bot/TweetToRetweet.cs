namespace OZG_Retweet_Bot
{
  internal class TweetToRetweet
  {

    #region Attributes

    private string? _tweetId;
    private string? _tweetText;
    private string? _tweetCreatedBy;
    private string? _tweetLanguage;

    private DateTimeOffset _tweetCreatedAt;

    private int? _tweetHashtagCount;
    private List<string>? _tweetHashtags;

    private string? _tweetUserId;
    private long? _tweetUserFollowers;
    private long? _tweetUserFollowing;
    private long? _tweetUserTweets;
    private bool _tweetUserVerified;

    #endregion

    #region Constructor

    public TweetToRetweet()
    {
    }

    #endregion

    #region Properties

    public string? TweetId
    {
      get { return _tweetId; }
      set { _tweetId = value; }
    }

    public string? TweetText
    {
      get { return _tweetText; }
      set { _tweetText = value; }
    }

    public string? TweetCreatedBy
    {
      get { return _tweetCreatedBy; }
      set { _tweetCreatedBy = value; }
    }

    public string? TweetLanguage
    {
      get { return _tweetLanguage; }
      set { _tweetLanguage = value; }
    }

    public DateTimeOffset TweetCreatedAt
    {
      get { return _tweetCreatedAt; }
      set { _tweetCreatedAt = value; }
    }

    public int? TweetHashtagCount
    {
      get { return _tweetHashtagCount; }
      set { _tweetHashtagCount = value;}
    }

    public List<string>? TweetHashtags
    {
      get { return _tweetHashtags; }
      set { _tweetHashtags = value;}
    }

    public string? TweetUserId
    {
      get { return _tweetUserId; }
      set { _tweetUserId = value; }
    }

    public long? TweetUserFollower
    {
      get { return _tweetUserFollowers; }
      set { _tweetUserFollowers = value;}
    }

    public long? TweetUserFollowing
    {
      get { return _tweetUserFollowing; }
      set { _tweetUserFollowing = value; }
    }

    public long? TweetUserTweets
    {
      get { return _tweetUserTweets; }
      set { _tweetUserTweets = value; }
    }

    public bool TweetUserVerified
    {
      get { return _tweetUserVerified; }
      set { _tweetUserVerified = value; }
    }
      
    #endregion
  }
}
