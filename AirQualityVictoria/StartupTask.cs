using System;
using System.Linq;
using Windows.ApplicationModel.Background;
using System.Net;
using System.Threading;
using TweetSharp;
using System.IO;

namespace AirQualityVictoria
{
    public sealed class StartupTask : IBackgroundTask
    {

        const string accountToWatch = "EPA_Victoria";
        private static string[] keywords = { "airquality", "air quality", "smoke" };
        private static string[] replyKeywords = { "hi", "hey", "hello", "thanks", "@" };
        private static string lastTweet = "";

        const string customerKey = "gQFaDYO5a59nf3AGWo6ZCCIaJ";
        const string customerKeySecret = "bbmjE1aU1fQeyvNUR1MpghhyqaUwzWiNlNG4u6HIAcCdD3pJRY";
        const string accessToken = "1212970147939381250-7aj2fMVVd5yq5HElbElncVOVJkUVJp";
        const string accessTokenSecret = "joz6Ruap7aV2URSSp7CvGiD6wCs1aZeiCj1iDr5PHcDrr";
        private static TwitterService service = new TwitterService(customerKey, customerKeySecret, accessToken, accessTokenSecret);

        public void Run(IBackgroundTaskInstance taskInstance)
        {

            while (true)
            {
                //Get tweet to send as string 
                var tweet = GetTweet(accountToWatch);
                
                //Reply, keyword and duplicate check
                bool keywordCheck = ContainsAny(tweet, keywords);
                bool replyCheck = ContainsAny(tweet, replyKeywords);
                
                if (tweet != lastTweet && keywordCheck && !replyCheck)
                {
                    //Send tweet and log to file. 
                    SendTweet("RT @EPA_Victoria: " + tweet, true, false);
                    lastTweet = tweet;
                }
                else
                {
                    //FormattableString message = $"{tweet} - NOT SENT. KeywordCheck: {keywordCheck} ReplyCheck: {replyCheck}";
                    //AddLog(message.ToString());
                }

                //Check every two minutes
                Thread.Sleep(120000);
            }
        }

        //Sends tweet and logs HTTP response to file. 
        private static void SendTweet(string status, bool keywordCheck, bool replyCheck)
        {
            service.SendTweet(new SendTweetOptions { Status = status }, (tweet, response) =>
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    //FormattableString message = $"{status} - SENT. KeywordCheck: {keywordCheck} ReplyCheck: {replyCheck}";
                    //AddLog(message.ToString());
                }
                else
                {
                    //FormattableString message = $"{status} - FAILED. KeywordCheck: {keywordCheck} ReplyCheck: {replyCheck} HTTP response: {response.Error.Message}";
                    //AddLog(message.ToString());
                }
            });
        }

        //Gets the latest tweet from our target user
        private static String GetTweet(string target)
        {
            var currentTweet = service.ListTweetsOnUserTimeline(new ListTweetsOnUserTimelineOptions
            {
                ScreenName = target,
                Count = 1,
            });

            return currentTweet.First().Text;
        }

        //Logs to text file on Raspi for easy debugging
        private static void AddLog(string message)
        {
            string filepath = @"C:\Data\Users\Administrator\Documents\AQV\" + DateTime.Today.ToString("yy-MM-dd") + "-Log.txt";

            //If log file for today doesn't already exist
            if (!File.Exists(filepath))
            {
                using (StreamWriter sw = File.CreateText(filepath))
                {
                    sw.WriteLine(message);
                }
                // Clear logs older than a week - should happen daily when new log is created. 
                ClearOldLogs();
            }
            else
            {
                using (StreamWriter sw = File.AppendText(filepath))
                {
                    sw.WriteLine(message);
                }
            }
        }

        //Clears logs older than one week
        private static void ClearOldLogs()
        {
            string[] files = Directory.GetFiles(@"C:\Data\Users\Administrator\Documents");

            foreach (string file in files)
            {
                FileInfo fi = new FileInfo(file);
                if (fi.LastAccessTime < DateTime.Now.AddDays(-7))
                {
                    fi.Delete();
                }
            }
        }

        //Performs keyword check
        private static bool ContainsAny(string tweet, string[] keywords)
        {
            foreach (string keyword in keywords)
            {
                if (tweet.ToLower().Contains(keyword.ToLower()))
                    return true;
            }
            return false;
        }
    }
}

