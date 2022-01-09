using System.Configuration;

namespace OZG_Retweet_Bot
{
  internal class TwitterConfig
  {

		#region Attributes

		private readonly string? _accessToken = ConfigurationManager.AppSettings["access_token"];
		private readonly string? _accessTokenSecret = ConfigurationManager.AppSettings["access_token_secret"];
		private readonly string? _consumerKey = ConfigurationManager.AppSettings["consumer_key"];
		private readonly string? _consumerKeySecret = ConfigurationManager.AppSettings["consumer_key_secret"];

		private readonly string? _termsToRetweet = ConfigurationManager.AppSettings["terms_to_retweet"];
		private readonly string? _termsNotToRetweet = ConfigurationManager.AppSettings["terms_not_to_retweet"];
		private readonly string? _usersToRetweet = ConfigurationManager.AppSettings["users_to_retweet"];
		private readonly string? _usersNotToRetweet = ConfigurationManager.AppSettings["users_not_to_retweet"];
		private readonly string? _languagesToRetweet = ConfigurationManager.AppSettings["languages_to_retweet"];
		private readonly string? _languagesNotToRetweet = ConfigurationManager.AppSettings["languages_not_to_retweet"];

    #endregion

    #region Constructor

		public TwitterConfig()
    {
    }

		#endregion

		#region Properties

		public string? ConsumerKey => _consumerKey;
		public string? ConsumerKeySecret => _consumerKeySecret;
		public string? AccessToken => _accessToken;
		public string? AccessTokenSecret => _accessTokenSecret;

		public string[]? TermsToRetweet => _termsToRetweet == "" ? null : _termsToRetweet!.Split(",");

		public string[]? TermsNotToRetweet => _termsNotToRetweet == "" ? null : _termsNotToRetweet!.Split(",");

		public long[]? UsersToRetweet
    {
			get
      {
				if (_usersToRetweet == "")
        {
					return null;
        }
        else
        {
					string[] usersToRetweet = _usersToRetweet!.Split(",");

					long[] usersToRetweetID = new long[usersToRetweet.Length];

					for (int i = 0; i < usersToRetweet.Length; i++)
          {
						usersToRetweetID[i] = Convert.ToInt64(usersToRetweet[i]);
          }
					return usersToRetweetID;
        }
      }
    }

		public string[]? UsersNotToRetweet => _usersNotToRetweet == "" ? null : _usersNotToRetweet!.Split(",");

		public string[]? LangsToRetweet => _languagesToRetweet == "" ? null : _languagesToRetweet!.Split(",");

		public string[]? LangsNotToRetweet => _languagesNotToRetweet == "" ? null : _languagesNotToRetweet!.Split(",");

		#endregion
	}
}
