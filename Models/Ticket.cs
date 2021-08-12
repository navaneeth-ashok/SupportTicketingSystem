using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SupportTicketSystem.Models
{
    public class Ticket
    {
        public int TicketID { get; set; }

        [Display(Name = "Ticket Last Updated on")]
        public DateTime LastUpdated { get; set; }
        [Display(Name = "Ticket Issue Date")]
        public DateTime IssueDate { get; set; }
        [Display(Name = "Ticket Deadline Date")]
        public DateTime Deadline { get; set; } = DateTime.Today.AddDays(1);
        public string Subject { get; set; }
        [Display(Name = "Ticket Owner")]
        public int Owner { get; set; }
        public string OwnerName { get; set; }
        [Display(Name = "Current Ticket Assignee")]
        public int Assignee { get; set; }
        public string AssigneeName { get; set; }
        [Display(Name = "Ticket Status")]
        public string Status { get; set; }

        public IEnumerable<CCList> CCUsers { get; set; }
        public IEnumerable<Comment> Comments { get; set; }
    }

    public enum TicketStatus
    {
        Open,
        InProgress,
        ToDo,
        Resolved,
        Backlog,
        Declined,
        WaitingForApproval
    }
}
