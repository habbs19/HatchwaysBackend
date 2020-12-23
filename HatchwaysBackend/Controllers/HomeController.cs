using HatchwaysBackend.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Net.Http;
using System.IO;
using MoreLinq;

namespace HatchwaysBackend.Controllers
{
    [Route("api/")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        readonly string host = "hatchways.io";

        [HttpGet("ping")]
        public JsonResult Index()
        {
            PingModel resp = new PingModel();
            resp.success = true;
            return new JsonResult(resp);
        }

        [HttpGet("posts")]
        public async Task<JsonResult> PostsAsync(string tags,string? sortBy,string direction)
        {
            if (tags == null)
            {
                HttpContext.Response.StatusCode = 400;
                return new JsonResult(new ErrorModel { error = "Tags parameter is required" });
            }
            if(sortBy != "id" && sortBy != "reads" && sortBy != "likes" && sortBy != "popularity" && sortBy !=null)
            {
                HttpContext.Response.StatusCode = 400;
                return new JsonResult(new ErrorModel { error = "sortBy parameter is invalid" });
            }
            if(direction == null && sortBy != null)
            {
                direction = "asc";
            }

            UriBuilder builder = new UriBuilder();
            builder.Scheme = "https";
            builder.Host = host;
            builder.Path = "/api/assessment/blog/posts";
            
            var tasks = new List<Task<HttpResponseMessage>>();
            HttpClient client = new HttpClient();
            foreach (string s in tags.Split(','))
            {
                builder.Query = $"?tag={s}";
                tasks.Add(client.GetAsync(builder.Uri));
            }
            var messages = await Task.WhenAll(tasks);
            var posts = new List<Post>();
            foreach(HttpResponseMessage message in messages)
            {
                if (message.IsSuccessStatusCode)
                {
                    var result = await message.Content.ReadAsStringAsync();
                    posts.AddRange(JsonConvert.DeserializeObject<PostModel>(result).Posts);
                }
                else
                {
                    HttpContext.Response.StatusCode = Convert.ToInt32(message.StatusCode);
                    return new JsonResult(await message.Content.ReadAsStringAsync());
                }
            }
            var distinctPosts = posts.DistinctBy(p => p.Id);
            
            System.Reflection.PropertyInfo prop = null;
            if (sortBy != null)
            {
                prop = typeof(Post).GetProperty(capitalizeFirst(sortBy));
                distinctPosts = distinctPosts.OrderBy(e=> prop.GetValue(e,null));
            }
            if (direction == "desc" && sortBy != null)
            {
                distinctPosts = distinctPosts.OrderByDescending(e => prop.GetValue(e, null));
            }
            return new JsonResult(new PostModel { Posts = distinctPosts});
        }
        [Route("authors")]
        [HttpGet]
        public async Task<JsonResult> AuthorsAsync()
        {
            HttpClient client = new HttpClient();
            UriBuilder builder = new UriBuilder();
            var tasks = new List<Task<HttpResponseMessage>>();
            var posts = new List<Post>();
            var authors = new List<Author>();

            builder.Scheme = "https";
            builder.Host = host;
            builder.Path = "/api/assessment/blog/authors";
            tasks.Add(client.GetAsync(builder.Uri));

            builder.Path = "/api/assessment/blog/posts";
            tasks.Add(client.GetAsync(builder.Uri));

            var messages = await Task.WhenAll(tasks);
            foreach (HttpResponseMessage message in messages)
            {
                if (message.IsSuccessStatusCode)
                {
                    var result = await message.Content.ReadAsStringAsync();
                    if (message.RequestMessage.RequestUri.LocalPath.Contains("posts"))
                    {
                        posts.AddRange(JsonConvert.DeserializeObject<PostModel>(result).Posts);
                    }
                    else
                    {
                        authors.AddRange(JsonConvert.DeserializeObject<AuthorModel>(result).Authors);
                    }
                }
                else
                {
                    HttpContext.Response.StatusCode = Convert.ToInt32(message.StatusCode);
                    return new JsonResult(await message.Content.ReadAsStringAsync());
                }
            }

            var mergedList = MergeAuthorsPosts(authors, posts);


            return new JsonResult(new AuthorModel { Authors = authors });

        }

        IEnumerable<Author> MergeAuthorsPosts(List<Author> authors, List<Post> posts)
        {
            authors.ForEach(a => 
            {
                var all = posts.FindAll(p => p.AuthorId == a.Id);
                var tags = new List<string>();
                foreach(string[] s in all.Select(a => a.Tags))
                {
                    tags.AddRange(s);
                }
                a.Posts = all;
                a.TotalLikeCount = all.Sum(s => s.Likes);
                a.TotalReadCount = all.Sum(s => s.Reads);
                a.Tags = tags.Distinct();
            });
            return authors;
        }

        string capitalizeFirst(string tag)
        {
            tag = tag.ToLower();
            char[] value = tag.ToCharArray();
            value[0] = char.ToUpper(value[0]);
            return new string(value);
        }
    }
}
