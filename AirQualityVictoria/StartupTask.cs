using System;
using System.Linq;
using Windows.ApplicationModel.Background;
using System.Net;
using System.Threading;
using TweetSharp;

namespace AirQualityVictoria
{
    public sealed class StartupTask : IBackgroundTask
    {

        private string accountToWatch = "EPA_Victoria";
        private static string[] keywords = new string[] { "#AirQuality", "smoke", "smokey", "air quality" };
        private static string lastTweet = "";

        private static string customerKey = "gQFaDYO5a59nf3AGWo6ZCCIaJ";
        private static string customerKeySecret = "bbmjE1aU1fQeyvNUR1MpghhyqaUwzWiNlNG4u6HIAcCdD3pJRY";
        private static string accessToken = "1212970147939381250-7aj2fMVVd5yq5HElbElncVOVJkUVJp";
        private static string accessTokenSecret = "joz6Ruap7aV2URSSp7CvGiD6wCs1aZeiCj1iDr5PHcDrr";
        private static TwitterService service = new TwitterService(customerKey, customerKeySecret, accessToken, accessTokenSecret);

        public void Run(IBackgroundTaskInstance taskInstance)
        {

            while (true)
            {
                Console.WriteLine("Test");

                //Get tweet to send as string 
                var tweet = GetTweet(accountToWatch);

                bool keywordCheck = ContainsAny(tweet, keywords);

                //Duplicate and keyword check
                if (tweet != lastTweet && keywordCheck == true)
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
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"<{DateTime.Now}> - Tweet Sent!");
                    Console.ResetColor();
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"<ERROR>" + response.Error.Message);
                    Console.ResetColor();
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

