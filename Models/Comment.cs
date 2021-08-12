using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SupportTicketSystem.Models
{
    public class Comment
    {
        public int CommentID { get; set; }
        public int Author{ get; set; }
        public string AuthorName { get; set; }
        public DateTime Time{ get; set; }
        public string Content { get; set; }
        public string AttachmentLink { get; set; }
    }
}
