using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Name
{
    public class Book
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int AuthorID { get; set; }
        public int YearPublished { get; set; }
        public string Genre { get; set; }
        public bool IsAvailable { get; set; }
        public string AuthorName { get; set; }

    }
}
