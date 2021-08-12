using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SupportTicketSystem.Models
{
    public class UserEmp
    {
        public int UserID { get; set; }
        public string Type { get; set; }
        [Display(Name ="First Name")]
        public string FirstName { get; set; }
        [Display(Name = "Middle Name")]
        public string MiddleName { get; set; }
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
        public string Email { get; set; }
        [Display(Name = "Password Hash")]
        public string PasswordHash { get; set; }
        public string Department { get; set; }
    }

    public enum UserType
    {
        Admin,
        Support,
        Client
    }

    public enum Department
    {
        Engg,
        DevOps,
        IT,
        TechSupport,
        GroundStaff,
        KitchenStaff,
        HR,
        Security
    }
}
