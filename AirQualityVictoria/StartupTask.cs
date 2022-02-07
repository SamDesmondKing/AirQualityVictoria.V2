using System;
using System.Diagnostics;
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
        private static string[] replyKeywords = { "hi ", "hey ", "hello ", "thanks ", "@" };
        private static string lastTweet = "";

        const string customerKey = "";
        const string customerKeySecret = "";
        const string accessToken = "";
        const string accessTokenSecret = "";
        private static readonly TwitterService service = new TwitterService(customerKey, customerKeySecret, accessToken, accessTokenSecret);

        public void Run(IBackgroundTaskInstance taskInstance)
        {

            while (true)
            {
                //Get tweet to send as string 
                var tweet = GetTweet(accountToWatch);

                //Reply, keyword and duplicate check
                var keywordCheck = ContainsAny(tweet, keywords);
                var replyCheck = ContainsAny(tweet, replyKeywords);
                
                if (tweet != lastTweet && keywordCheck && !replyCheck)
                {
                    //Send tweet and log to file. 
                    SendTweet("RT @EPA_Victoria: " + tweet);
                    lastTweet = tweet;
                }
                else
                {
                    FormattableString message = $"{tweet} - NOT SENT. KeywordCheck: {keywordCheck} ReplyCheck: {replyCheck} Time: {DateTime.Today}";
                    //AddLog(message.ToString());
                    Debug.WriteLine(message);
                }

                //Check every two minutes
                Thread.Sleep(120000);
            }
        }

        //Sends tweet and logs HTTP response to file. 
        private static void SendTweet(string status, bool keywordCheck = true, bool replyCheck = false)
        {
            service.SendTweet(new SendTweetOptions { Status = status }, (tweet, response) =>
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    FormattableString message = $"{status} - SENT. KeywordCheck: {keywordCheck} ReplyCheck: {replyCheck} Time: {DateTime.Today}";
                    //AddLog(message.ToString());
                    Debug.WriteLine(message);
                }
                else
                {
                    FormattableString message = $"{status} - FAILED. KeywordCheck: {keywordCheck} ReplyCheck: {replyCheck} HTTP response: {response.Error.Message} Time: {DateTime.Today}";
                    //AddLog(message.ToString());
                    Debug.WriteLine(message);
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

