using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HatchwaysBackend.Models
{
    public class AuthorModel
    {
        public IEnumerable<Author> Authors { get; set; }
    }
    public class Author
    {
        public int Id { get; set;}
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public IEnumerable<Post> Posts { get; set; }
        public IEnumerable<string> Tags { get; set; }
        public int TotalLikeCount { get; set; } = 0;
        public int TotalReadCount { get; set; } = 0;
       
    }
}
