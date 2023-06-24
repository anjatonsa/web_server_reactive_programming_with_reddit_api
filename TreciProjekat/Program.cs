using Reddit;
using Reddit.Controllers;
using System;
namespace TreciProjekat
{
    internal class Program
    {
        static string clientId = Environment.GetEnvironmentVariable("REDDIT_API_CLIENT_ID");
        static string secret= Environment.GetEnvironmentVariable("REDDIT_API_SECRET");
        //static string accessToken3 = Environment.GetEnvironmentVariable("REDDIT_API_TOKEN");


        static async Task Main()
        {
            RedditAPI r = new RedditAPI();
            string accessToken = await r.getAccessToken(clientId, secret);
            RedditClient reddit = new RedditClient(accessToken: accessToken);


            Subreddit subreddit = reddit.Subreddit("AskAcademia").About();
            Console.WriteLine($"Subreddit Name: {subreddit.Description}");


            List<Post> posts = subreddit.Posts.GetTop("all", "10"); // Replace "10" with the number of posts you want to retrieve

            foreach (Post post in posts)
            {
                List<Comment> comments = post.Comments.GetComments();

                foreach (Comment comment in comments)
                {
                    // Process each comment as needed
                    Console.WriteLine(comment.Body);
                }
            }

        }



    }



}


