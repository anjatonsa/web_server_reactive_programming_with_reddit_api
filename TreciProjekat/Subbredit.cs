using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Reactive;
using System.Reactive.Subjects;
using System.Net.Http.Headers;
using System.Xml.Linq;
using Reddit;
using Reddit.Controllers;
using VaderSharp;
using System.Reactive.Linq;
using RestSharp;
using System.Net;
using System.Text;

namespace TreciProjekat
{
    public class SubbreditComment
    {
        public string Text { get; set; }
        public string Sentiment { get; set; }

    }

    public class SubbreditStream : IObservable<SubbreditComment> 
    { 
    
        private Subject<SubbreditComment> subject;
        private string subredditName;
        private RedditClient redditClient;
 

        public SubbreditStream(string subbreditName, RedditClient reddit)
        {
            subject = new Subject<SubbreditComment>();
            this.subredditName = subbreditName;
            redditClient = reddit;
        }

        public async Task getComments()
        {
                try
                {
                    Subreddit subreddit = redditClient.Subreddit(subredditName).About();

                    List<Post> posts = subreddit.Posts.GetTop(limit: 5);

                    foreach (Post post in posts)
                    {
                        List<Comment> comments = post.Comments.GetComments();

                        foreach (Comment comment in comments)
                        {
                            SubbreditComment newComment = new SubbreditComment();
                            newComment.Text = comment.Body;

                            if (comment.Body != null)
                            {
                                SentimentIntensityAnalyzer analyzer = new SentimentIntensityAnalyzer();
                                var sentiment = analyzer.PolarityScores(comment.Body);
                                double compoundScore = sentiment.Compound;
                                if (compoundScore >= 0.05)
                                {
                                    newComment.Sentiment = "positive";
                                }
                                else if (compoundScore <= -0.05)
                                {
                                    newComment.Sentiment = "negative";
                                }
                                else
                                {
                                    newComment.Sentiment = "neutral";
                                }
                                subject.OnNext(newComment);
                            }
                        }
                    }
                    subject.OnCompleted();
                    //Task.Delay(5000);
                }
                catch (Exception ex) { subject.OnError(ex); }
        }

        public IDisposable Subscribe(IObserver<SubbreditComment> observer) 
        {
            return subject.Subscribe(observer);
        }
    }


    public class SubbreditObserver : IObserver<SubbreditComment>
    {
        public string subbreditName;
        private int totalCount;
        private int positiveCount;
        private int negativeCount;
        private int neutralCount;
        private string response;
        public SubbreditObserver(string name)
        {
            subbreditName = name;
            totalCount = 0;
            positiveCount = 0;   
            negativeCount = 0;
            neutralCount= 0;
            response = "";
        }
        public void OnNext(SubbreditComment comment)
        {
            totalCount++;
            if (comment.Sentiment == "positive")
                positiveCount++;
            else
            {
                if(comment.Sentiment == "negative")
                    negativeCount++;
                else
                    neutralCount++;
            }
        }
        public void OnError(Exception e)
        {
            Console.WriteLine("There was an error: {e.Message}");
        }
        public string ReturnResponse()
        {
            return response;
        }
        public void OnCompleted()
        {
            string s1 = $"Total number of comments: {totalCount}";
            string s2 = $"Positive Percentage: {((double)positiveCount / totalCount * 100):0.00}%";
            string s3 = $"Negative Percentage: {((double)negativeCount / totalCount * 100):0.00}%";
            string s4 = $"Neutral Percentage: {((double)neutralCount / totalCount * 100):0.00}%";
            response = s1 + s2 + s3 + s4;

            Console.WriteLine(s1);
            Console.WriteLine(s2);
            Console.WriteLine(s3);
            Console.WriteLine(s4);
            Console.WriteLine();

        }
    }

}
