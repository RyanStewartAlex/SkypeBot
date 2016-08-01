using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using SKYPE4COMLib;

using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Upload;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System.Text.RegularExpressions;

namespace SkypeBotConsole
{
    class Program
    {

        static Skype sky;
        static bool sent = false;

        static string ytSearch;
        static string ytResult;

        const char prefix = '!';
        const string ytAPIKey = //removed for security reasons

        static void Main(string[] args)
        {
            //skype initialization
            sky = new SKYPE4COMLib.Skype();
            sky.Attach();

            while (true)
            {

                //autoresponse messages
                foreach (IChatMessage msg in sky.MissedMessages)
                {

                    string user = msg.Sender.Handle;
                    string[] cmds = { "!help", "!ping", "!yt [search query]" };

                    //help cmd
                    if (msg.Body.ToLower().Contains(prefix + "help") && msg.Body.IndexOf(prefix) == 0)
                    {
                        string cmdFormat = "Commands:";
                        foreach (string cmd in cmds)
                        {
                            cmdFormat += "\n\t\t" + cmd;
                        }
                        sky.SendMessage(user, "kek");
                        break;
                    }
                    //ping cmd
                    else if (msg.Body.ToLower().Contains(prefix + "ping") && msg.Body.IndexOf(prefix) == 0)
                    {
                        SendSMS(user, "pong");
                        break;
                    }
                    //youtube cmd
                    else if (msg.Body.ToLower().Contains(prefix + "yt") && msg.Body.IndexOf(prefix) == 0)
                    {
                        ytSearch = msg.Body.Remove(0, 3);
                        if (string.IsNullOrWhiteSpace(ytSearch))
                        {
                            SendSMS(user, "Incorrect syntax. Please do \"!yt <what you want to search>\" instead.");
                        }
                        else
                        {
                            new Program().YoutubeMethod().Wait();
                            SendSMS(user, ytResult);
                        }
                        break;
                        //invalid command
                    }
                    else if (msg.Body.ToLower().Contains(prefix.ToString()) && msg.Body.IndexOf(prefix) == 0)
                    {
                        SendSMS(user, "\"" + msg.Body + "\"" + " is not a valid command. Please type " + "\"!help\" for more info.");
                        break;
                    }
                }
            }
        }


        static void SendSMS(string user, string message)
        {
            sent = true;
            Thread.Sleep(1500);
            sky.SendMessage(user, message);
        }

        private async Task YoutubeMethod()
        {

            ytResult = null;

            var youtubeService = new YouTubeService(new BaseClientService.Initializer()
            {
                ApiKey = ytAPIKey,
                ApplicationName = "SkypeBotConsole"
            });

            var searchListRequest = youtubeService.Search.List("snippet");
            searchListRequest.Q = ytSearch; // Replace with your search term.
            searchListRequest.MaxResults = 50;

            var searchListResponse = await searchListRequest.ExecuteAsync();

            List<string> videos = new List<string>();
            List<string> channels = new List<string>();
            List<string> playlists = new List<string>();

            // Add each result to the appropriate list, and then display the lists of
            // matching videos, channels, and playlists.
            foreach (var searchResult in searchListResponse.Items)
            {
                switch (searchResult.Id.Kind)
                {
                    case "youtube#video":
                        videos.Add(searchResult.Id.VideoId);
                        break;

                    case "youtube#channel":
                        channels.Add(searchResult.Id.ChannelId);
                        break;

                    case "youtube#playlist":
                        playlists.Add(searchResult.Id.PlaylistId);
                        break;
                }
            }

            if (videos.Count == 0)
            {
                ytResult = "No video could be found with the title of \"" + ytSearch + "\".";
            }
            else
            {
                ytResult = "https://www.youtube.com/watch?v=" + videos[0];
            }
        }

    }
}
