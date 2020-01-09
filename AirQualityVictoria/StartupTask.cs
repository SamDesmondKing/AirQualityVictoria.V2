using System;
using System.Diagnostics;
using System.Linq;
using Windows.ApplicationModel.Background;
using System.Net;
using System.Threading;
using TweetSharp;

namespace AirQualityVictoria
{
    public sealed class StartupTask : IBackgroundTask
    {

        private static string accountToWatch = "EPA_Victoria";
        private static string[] keywords = new string[] { "#AirQuality", "air quality", "Air quality", "smoke", "Smoke" };
        private static string[] replyKeywords = new string[] { "Hi", "hi", "Thanks", "thanks", "@" };

        private static string customerKey = "gQFaDYO5a59nf3AGWo6ZCCIaJ";
        private static string customerKeySecret = "bbmjE1aU1fQeyvNUR1MpghhyqaUwzWiNlNG4u6HIAcCdD3pJRY";
        private static string accessToken = "1212970147939381250-7aj2fMVVd5yq5HElbElncVOVJkUVJp";
        private static string accessTokenSecret = "joz6Ruap7aV2URSSp7CvGiD6wCs1aZeiCj1iDr5PHcDrr";
        private static TwitterService service = new TwitterService(customerKey, customerKeySecret, accessToken, accessTokenSecret);
        private static string lastTweet = "";

        public void Run(IBackgroundTaskInstance taskInstance)
        {

            while (true)
            {

                //Get tweet to send as string 
                var tweet = GetTweet(accountToWatch);

                bool keywordCheck = ContainsAny(tweet, keywords);
                bool replyCheck = ContainsAny(tweet, replyKeywords);

                //Duplicate and keyword check
                if (tweet != lastTweet && keywordCheck == true && replyCheck == false)
                {
                    //Send it
                    SendTweet("RT @EPA_Victoria: " + tweet);
                    lastTweet = tweet;
                }

                //Cycle every thirty minutes
                Thread.Sleep(1800000);
            }
        }

        private static bool ContainsAny(string tweet, string[] keywords)
        {
            foreach (string keyword in keywords)
            {
                if (tweet.Contains(keyword))
                    return true;
            }

            return false;
        }

        private static void SendTweet(string _status)
        {
            service.SendTweet(new SendTweetOptions { Status = _status }, (tweet, response) =>
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Debug.WriteLine("Tweet sent");
                }
                else
                {
                    Debug.WriteLine("Tweet failed" + response);
                }
            });
        }

        private static String GetTweet(string target)
        {
            var currentTweet = service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions
            {
                ScreenName = target,
                Count = 1,
            });

            return currentTweet.First().Text;
        }
    }
}

