using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SupportTicketSystem.Areas.Identity.Pages.Account
{
    public class AccessDeniedModel : PageModel
    {
        public void OnGet()
        {
            System.Diagnostics.Debug.WriteLine("Denied");
        }
    }
}

