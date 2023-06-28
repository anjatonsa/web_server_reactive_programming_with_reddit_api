using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Reddit;
using Reddit.Controllers;
using Reddit.Things;
using uhttpsharp.Handlers;

namespace TreciProjekat
{
    internal class WebServer
    {
        private HttpListener listener;
        private string listenUrl;
        private string clientId;
        private string secret;


        private RedditAPI redditAPI;
        private RedditClient redditClient;
        private string accessToken;



        public WebServer(string[] arg)
        {
            Console.WriteLine("Server thread started.");
            this.listenUrl = arg[0];
            clientId = arg[1];
            secret= arg[2];
        }

        private async Task SetUpRedditClient()
        {
            try
            {
                redditAPI = new RedditAPI();
                accessToken = await redditAPI.getAccessToken(clientId, secret);
                redditClient = new RedditClient(accessToken: accessToken);
            }
            catch (Exception ex) 
            {
                Console.WriteLine(ex.ToString());
            }
        }
        public async Task Start()
        {
            try
            {
                await SetUpRedditClient();


                listener = new HttpListener();
                listener.Prefixes.Add(listenUrl);
                listener.Start();
                Console.WriteLine("Server is listening.\n");

                while (listener.IsListening)
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    HandleRequest(context);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private async Task HandleRequest(HttpListenerContext context)
        {
            HttpListenerResponse response = context.Response;

            try
            {
                HttpListenerRequest request = context.Request;
                string url = request.Url.ToString();
                string ignore = $"{listenUrl}favicon.ico";
                if (url == ignore)
                {
                    return;
                }
                
                    Console.WriteLine($"Received request at {url}.");


                if (request.HttpMethod != "GET")
                {
                    await HandleError(response, "method");
                }
                else
                {
                    string fileExt = Path.GetExtension(url);
                    string imageName = Path.GetFileName(url);
                    Console.WriteLine();

                    int placeOfQuestionMark=url.IndexOf('?');
                    string urlParams = url.Substring(placeOfQuestionMark+1);
                    string [] subbredditArray = urlParams.Split('&');

                    if (placeOfQuestionMark == -1 || subbredditArray.Length == 1)
                    {
                        HandleError(response, "missing-parametars");
                    }
                    else
                    {
                        List<Task> completionTasks = new List<Task>();

                        foreach (string subR in subbredditArray)
                        {
                            completionTasks.Add(ProcessSubreddit(response, subR));
                        }
                        await Task.WhenAll(completionTasks);
                        ReturnResponse(response);
                    }
                }
            }
            catch (Exception ex)
            {
                await HandleError(response, "error");
                Console.WriteLine(ex.Message);
            }

        }

        private async Task ProcessSubreddit(HttpListenerResponse res, string subR)
        {
            Console.WriteLine($"Started processing subreddit-{subR}.");
            string subbreditName = subR;
            var commentStream = new SubbreditStream(subbreditName, redditClient);
            var observer = new SubbreditObserver(subbreditName);
            var scheduler = TaskPoolScheduler.Default;
            var subscription = commentStream.ObserveOn(scheduler).Subscribe(observer);
            await commentStream.getComments();
            Console.WriteLine($"Completed processing subreddit {subR}.");
        }
        private async Task HandleError(HttpListenerResponse res, string error)
        {
            string ret = "";
            if (error == "method")
            {
                //vraca se informacija o gresci
                ret = "<h2>Error - only GET request is valid.</h2>";
                res.StatusCode = (int)HttpStatusCode.BadRequest;
                res.StatusDescription = "Bad request";
                Console.WriteLine("Error - only GET request is valid.\n");
            }
            else
            {
                if (error == "missing-parametars")
                {
                    ret = "<h2>Error - missing parametars.</h2>";
                    res.StatusCode = (int)HttpStatusCode.BadRequest;
                    res.StatusDescription = "Bad request.";
                    Console.WriteLine("Error - bad request.\n");
                }
                else
                {
                    ret = "<h2>Error - bad request.</h2>";
                    res.StatusCode = (int)HttpStatusCode.BadRequest;
                    res.StatusDescription = "Bad request.";
                    Console.WriteLine("Error - bad request.\n");
                }
            }

            res.Headers.Set("Content-Type", "text/html");
            byte[] buf = Encoding.UTF8.GetBytes(ret);
            using Stream output = res.OutputStream;
            res.ContentLength64 = buf.Length;
            await output.WriteAsync(buf, 0, buf.Length);
            output.Close();
        }

        private async Task ReturnResponse(HttpListenerResponse res)
        {
            string ret = $"<h2>Successfully processed subreddit comments.</h2>";
            res.Headers.Set("Content-Type", "text/html");
            byte[] buf = Encoding.UTF8.GetBytes(ret);
            using Stream output = res.OutputStream;
            res.ContentLength64 = buf.Length;
            await output.WriteAsync(buf, 0, buf.Length);
            output.Close();
        }

    }
}
