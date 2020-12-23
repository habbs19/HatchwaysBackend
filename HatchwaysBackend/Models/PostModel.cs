using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HatchwaysBackend.Models
{
    public class PostModel
    {
        [JsonProperty("posts")]
        public IEnumerable<Post> Posts { get; set; }
        
    }

    public class Post
    {
        [JsonProperty("id")]
        public int Id { get; set; }
        [JsonProperty("author")]
        public string Author { get; set; }
        public int AuthorId { get; set; }
        public int Likes { get; set; }
        public decimal Popularity { get; set; }
        public int Reads { get; set; }
        public string[] Tags { get; set; }
    }
}
