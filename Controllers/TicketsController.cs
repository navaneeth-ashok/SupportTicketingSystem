using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SupportTicketSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace SupportTicketSystem.Controllers
{
    public class TicketsController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;

        public TicketsController(UserManager<IdentityUser> userManager)
        {
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            IList<Ticket> tickets = new List<Ticket>(); //list of tikets

            string path = Request.PathBase + "App_Data/tickets.xml";
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(path))
            {
                //only load if the file exists
                doc.Load(path); //exists, so load
                XmlNodeList ticketList = doc.GetElementsByTagName("ticket"); //load user elements
                foreach (XmlElement ticketItem in ticketList)
                {
                    // get cc list
                    XmlNodeList ccList = ticketItem.GetElementsByTagName("ccUser");
                    List<CCList> listCC = new List<CCList>();
                    foreach(XmlElement user in ccList)
                    {
                        CCList ccUser = new CCList
                        {
                            UserID = Convert.ToInt32(user.InnerText)
                        };
                        listCC.Add(ccUser);
                    }

                    
                    UserController userController = new UserController(userManager);
                    
                    Ticket ticket = new Ticket
                    {
                        TicketID = Convert.ToInt32(ticketItem.GetElementsByTagName("ticketID")[0].InnerText),
                        LastUpdated = Convert.ToDateTime(ticketItem.Attributes["lastUpdated"].Value),
                        IssueDate = Convert.ToDateTime(ticketItem.GetElementsByTagName("issueDate")[0].InnerText),
                        Deadline = Convert.ToDateTime(ticketItem.GetElementsByTagName("ticketDeadline")[0].InnerText),
                        Subject = ticketItem.GetElementsByTagName("ticketSubject")[0].InnerText,
                        Owner = Convert.ToInt32(ticketItem.GetElementsByTagName("ticketOwner")[0].InnerText),
                        OwnerName = userController.FetchUserName(Convert.ToInt32(ticketItem.GetElementsByTagName("ticketOwner")[0].InnerText)),
                        Assignee = Convert.ToInt32(ticketItem.GetElementsByTagName("ticketAssignee")[0].InnerText),
                        AssigneeName = userController.FetchUserName(Convert.ToInt32(ticketItem.GetElementsByTagName("ticketAssignee")[0].InnerText)),
                        Status = ticketItem.GetElementsByTagName("ticketStatus")[0].InnerText,
                        CCUsers = listCC,
                    };
                    tickets.Add(ticket);
                }
            }
            return View(tickets); // pass the value to the View
        }

        [HttpGet]
        public IActionResult Details(int ID)
        {
            UserController userController = new UserController(userManager); 
            
            Ticket ticket = new Ticket(); // ticket
            string path = Request.PathBase + "App_Data/tickets.xml";
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(path))
            {
                //only load if the file exists
                doc.Load(path); //exists, so load
                XmlNodeList ticketList = doc.SelectNodes("//ticket[ticketID/text()='"+ ID +"']"); //load ticket elements
                // Load Xml

                // nodes.Count == 2
                foreach (XmlElement ticketItem in ticketList)
                {
                    // get cc list
                    XmlNodeList ccList = ticketItem.GetElementsByTagName("ccUser");
                    List<CCList> listCC = new List<CCList>();
                    foreach (XmlElement user in ccList)
                    {
                        CCList ccUser = new CCList
                        {
                            UserID = Convert.ToInt32(user.InnerText)
                        };
                        listCC.Add(ccUser);
                    }

                    // get comments
                    XmlNodeList comments = ticketItem.GetElementsByTagName("comment");
                    List<Comment> commentsList = new List<Comment>();

                    foreach (XmlElement comment in comments)
                    {
                        string author = userController.FetchUserName(Convert.ToInt32(comment.GetElementsByTagName("commentUser")[0].InnerText));
                        if (comment.GetElementsByTagName("attachment").Count > 0)
                        {
                            Comment commentItem = new Comment
                            {
                                CommentID = Convert.ToInt32(comment.GetElementsByTagName("commentID")[0].InnerText),
                                Author = Convert.ToInt32(comment.GetElementsByTagName("commentUser")[0].InnerText),
                                Time = Convert.ToDateTime(comment.GetElementsByTagName("commentDate")[0].InnerText),
                                AuthorName = author,
                                Content = comment.GetElementsByTagName("commentString")[0].InnerText,
                                AttachmentLink = comment.GetElementsByTagName("attachment")[0].InnerText,
                            };
                            commentsList.Add(commentItem);
                        }
                        else
                        {
                            
                            
                            Comment commentItem = new Comment
                            {
                                CommentID = Convert.ToInt32(comment.GetElementsByTagName("commentID")[0].InnerText),
                                Author = Convert.ToInt32(comment.GetElementsByTagName("commentUser")[0].InnerText),
                                AuthorName = author,
                                Time = Convert.ToDateTime(comment.GetElementsByTagName("commentDate")[0].InnerText),
                                Content = comment.GetElementsByTagName("commentString")[0].InnerText,
                            };
                            commentsList.Add(commentItem);
                        }
                    }

                    ticket = new Ticket
                    {
                        TicketID = Convert.ToInt32(ticketItem.GetElementsByTagName("ticketID")[0].InnerText),
                        LastUpdated = Convert.ToDateTime(ticketItem.Attributes["lastUpdated"].Value),
                        IssueDate = Convert.ToDateTime(ticketItem.GetElementsByTagName("issueDate")[0].InnerText),
                        Deadline = Convert.ToDateTime(ticketItem.GetElementsByTagName("ticketDeadline")[0].InnerText),
                        Subject = ticketItem.GetElementsByTagName("ticketSubject")[0].InnerText,
                        Owner = Convert.ToInt32(ticketItem.GetElementsByTagName("ticketOwner")[0].InnerText),
                        OwnerName = userController.FetchUserName(Convert.ToInt32(ticketItem.GetElementsByTagName("ticketOwner")[0].InnerText)),
                        Assignee = Convert.ToInt32(ticketItem.GetElementsByTagName("ticketAssignee")[0].InnerText),
                        AssigneeName = userController.FetchUserName(Convert.ToInt32(ticketItem.GetElementsByTagName("ticketAssignee")[0].InnerText)),
                        Status = ticketItem.GetElementsByTagName("ticketStatus")[0].InnerText,
                        CCUsers = listCC,
                        Comments = commentsList,
                    };
                }
            }

            List<UserEmp> users = userController.FetchUserList();

            // list of users according to XML
            ViewBag.userList = users.Select(x => new SelectListItem() { Value = Convert.ToString(x.UserID), Text = x.FirstName + " " + x.LastName }).ToList();

            var statusList = (from action in (TicketStatus[])Enum.GetValues(typeof(TicketStatus)) select action.ToString()).ToList();
            var list = statusList.Select(x => new SelectListItem() { Value = x, Text = x }).ToList();
            ViewBag.statusList = list;
            System.Diagnostics.Debug.WriteLine(LastCommentId(Convert.ToInt32(ticket.TicketID)));

            return View(ticket); // pass the value to the View
        }

        [HttpGet]
        public Ticket FetchTicket(int ID)
        {
            Ticket ticket = new Ticket(); // ticket
            string path = "App_Data/tickets.xml";
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(path))
            {
                //only load if the file exists
                doc.Load(path); //exists, so load
                XmlNodeList ticketList = doc.SelectNodes("//ticket[ticketID/text()='" + ID + "']"); //load ticket elements
                // Load Xml

                // nodes.Count == 2
                foreach (XmlElement ticketItem in ticketList)
                {
                    // get cc list
                    XmlNodeList ccList = ticketItem.GetElementsByTagName("ccUser");
                    List<CCList> listCC = new List<CCList>();
                    foreach (XmlElement user in ccList)
                    {
                        CCList ccUser = new CCList
                        {
                            UserID = Convert.ToInt32(user.InnerText)
                        };
                        listCC.Add(ccUser);
                    }

                    // get comments
                    XmlNodeList comments = ticketItem.GetElementsByTagName("comment");
                    List<Comment> commentsList = new List<Comment>();
                    UserController userController = new UserController(userManager);
                    foreach (XmlElement comment in comments)
                    {
                        if (comment.GetElementsByTagName("attachment").Count > 0)
                        {
                            Comment commentItem = new Comment
                            {
                                CommentID = Convert.ToInt32(comment.GetElementsByTagName("commentID")[0].InnerText),
                                Author = Convert.ToInt32(comment.GetElementsByTagName("commentUser")[0].InnerText),
                                Time = Convert.ToDateTime(comment.GetElementsByTagName("commentDate")[0].InnerText),
                                Content = comment.GetElementsByTagName("commentString")[0].InnerText,
                                AttachmentLink = comment.GetElementsByTagName("attachment")[0].InnerText,
                            };
                            commentsList.Add(commentItem);
                        }
                        else
                        {

                            string author = userController.FetchUserName(Convert.ToInt32(comment.GetElementsByTagName("commentUser")[0].InnerText));
                            Comment commentItem = new Comment
                            {
                                CommentID = Convert.ToInt32(comment.GetElementsByTagName("commentID")[0].InnerText),
                                Author = Convert.ToInt32(comment.GetElementsByTagName("commentUser")[0].InnerText),
                                AuthorName = author,
                                Time = Convert.ToDateTime(comment.GetElementsByTagName("commentDate")[0].InnerText),
                                Content = comment.GetElementsByTagName("commentString")[0].InnerText,
                            };
                            commentsList.Add(commentItem);
                        }
                    }

                    ticket = new Ticket
                    {
                        TicketID = Convert.ToInt32(ticketItem.GetElementsByTagName("ticketID")[0].InnerText),
                        LastUpdated = Convert.ToDateTime(ticketItem.Attributes["lastUpdated"].Value),
                        IssueDate = Convert.ToDateTime(ticketItem.GetElementsByTagName("issueDate")[0].InnerText),
                        Deadline = Convert.ToDateTime(ticketItem.GetElementsByTagName("ticketDeadline")[0].InnerText),
                        Subject = ticketItem.GetElementsByTagName("ticketSubject")[0].InnerText,
                        Owner = Convert.ToInt32(ticketItem.GetElementsByTagName("ticketOwner")[0].InnerText),
                        OwnerName = userController.FetchUserName(Convert.ToInt32(ticketItem.GetElementsByTagName("ticketOwner")[0].InnerText)),
                        Assignee = Convert.ToInt32(ticketItem.GetElementsByTagName("ticketAssignee")[0].InnerText),
                        AssigneeName = userController.FetchUserName(Convert.ToInt32(ticketItem.GetElementsByTagName("ticketAssignee")[0].InnerText)),
                        Status = ticketItem.GetElementsByTagName("ticketStatus")[0].InnerText,
                        CCUsers = listCC,
                        Comments = commentsList,
                    };
                }
                return ticket;
            }
            return ticket;
        }

        private void UpdateTicketTime(string ticketID)
        {
            string path = "App_Data/tickets.xml";
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(path))
            {
                //only load if the file exists
                doc.Load(path); //exists, so load
                XmlNode ticket = doc.SelectNodes("//ticket[ticketID/text()='" + ticketID + "']")[0]; //load ticket elements
                ticket.Attributes["lastUpdated"].Value = Convert.ToString(Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM-ddThh:mm:ss"));
                // save the file
                doc.Save(path);
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult Edit(string ticketID, string Assignee, string Status)
        {
            string path = "App_Data/tickets.xml";
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(path))
            {
                //only load if the file exists
                doc.Load(path); //exists, so load
                XmlNodeList ticketList = doc.SelectNodes("//ticket[ticketID/text()='" + ticketID + "']"); //load ticket elements
                // Load Xml

                // nodes.Count == 2
                foreach (XmlElement ticketItem in ticketList)
                {
                    var assignee = ticketItem.GetElementsByTagName("ticketAssignee")[0];
                    assignee.InnerText = Assignee;
                    var status = ticketItem.GetElementsByTagName("ticketStatus")[0];
                    status.InnerText = Status;
                }
                //UpdateTicketTime(ticketID);
                // save the file
                doc.Save(path);
            }
            UpdateTicketTime(ticketID);
            return RedirectToAction("Details", new { id = ticketID });
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> AddComment(string ticketID, string Content, string AttachmentLink)
        {
            var user = await userManager.GetUserAsync(HttpContext.User);
            UserController userController = new UserController(userManager);
            ViewBag.user = user;
            Comment newComment = new Comment
            {
                CommentID = LastCommentId(Convert.ToInt32(ticketID)),
                Author = userController.FetchUserID(user.Email),
                Time = DateTime.Now

            };
            string xmlPath = "App_Data/tickets.xml";
            XmlDocument doc = new XmlDocument();

            //if commentID = 1 : create comments tag else add just a new comment

            if (System.IO.File.Exists(xmlPath))
            {
                // load the file
                doc.Load(xmlPath);


                // fetch the ticket
                XmlNode ticket = doc.SelectNodes("//ticket[ticketID/text()='" + ticketID + "']")[0];

                XmlNode comment = doc.CreateElement("comment");

                XmlNode commentID = doc.CreateElement("commentID");
                commentID.InnerText = Convert.ToString(newComment.CommentID);
                XmlNode commentUser = doc.CreateElement("commentUser");
                commentUser.InnerText = Convert.ToString(newComment.Author);
                XmlNode commentDate = doc.CreateElement("commentDate");
                commentDate.InnerText = Convert.ToString(Convert.ToDateTime(newComment.Time).ToString("yyyy-MM-ddThh:mm:ss"));
                XmlNode commentString = doc.CreateElement("commentString");
                commentString.InnerText = Content;
                
                comment.AppendChild(commentID);
                comment.AppendChild(commentUser);
                comment.AppendChild(commentDate);
                comment.AppendChild(commentString);
                if (AttachmentLink != null)
                {
                    XmlNode attachment = doc.CreateElement("attachment");
                    attachment.InnerText = AttachmentLink;
                    comment.AppendChild(attachment);
                }
                ticket["comments"].AppendChild(comment);
                doc.Save(xmlPath);
                
            }
            UpdateTicketTime(ticketID);
            return RedirectToAction("Details", new { id = ticketID });
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            // this is to send the last ID + 1 to the create view
            Ticket newTicket = new()
            {
                TicketID = LastId(),
            };
            var user = await userManager.GetUserAsync(HttpContext.User);
            ViewBag.user = user;

            UserController userController = new UserController(userManager);
            List<UserEmp> users = userController.FetchUserList();

            // list of users according to XML
            ViewBag.userList = users.Select(x => new SelectListItem() { Value = Convert.ToString(x.UserID), Text = x.FirstName + " " + x.LastName }).ToList();

            var statusList = (from action in (TicketStatus[])Enum.GetValues(typeof(TicketStatus)) select action.ToString()).ToList();
            var list = statusList.Select(x => new SelectListItem() { Value = x, Text = x }).ToList();
            ViewBag.statusList = list;

            return View(newTicket);

        }

        public int LastId()
        {
            string path = "App_Data/tickets.xml";
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(path))
            {
                doc.Load(path);
                //only load if the file exists
                System.Diagnostics.Debug.WriteLine(doc.GetElementsByTagName("ticketID"));

                foreach(XmlElement a in doc.GetElementsByTagName("ticketID"))
                {
                    System.Diagnostics.Debug.WriteLine(a.InnerText);
                }
                var lastID = Convert.ToInt32(doc.GetElementsByTagName("ticketID")[^1].InnerText);
                return lastID + 1;
            }
            else
            {
                return 1;
            }
        }

        public int LastCommentId(int ticketID)
        {
            string path = "App_Data/tickets.xml";
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(path))
            {
                doc.Load(path);
                //only load if the file exists

                XmlNode ticketList = doc.SelectNodes("//ticket[ticketID/text()='" + ticketID + "']")[0];
                if (!ticketList["comments"].IsEmpty)
                {
                    return Convert.ToInt32(ticketList["comments"].GetElementsByTagName("commentID")[^1].InnerText) + 1;
                }
                else
                {
                    return 1;
                }
            }
            else
            {
                return 1;
            }
        }

        [HttpPost]
        [Authorize]
        public IActionResult Create(Ticket newTicket, string ccList)
        {
            var ticketID = LastId();
            // load the xml
            string xmlPath = Request.PathBase + "App_Data/tickets.xml";
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(xmlPath))
            {
                // load the file
                doc.Load(xmlPath);

                // create the person
                XmlElement ticket = _CreateTicketElement(doc, newTicket, ccList);

                // append to root
                doc.DocumentElement.AppendChild(ticket);
            }
            else
            {
                // create new file
                XmlNode dec = doc.CreateXmlDeclaration("1.0", "utf-8", "");
                doc.AppendChild(dec);
                XmlNode root = doc.CreateElement("ticket");

                // create a person
                XmlElement ticket = _CreateTicketElement(doc, newTicket, ccList);
                root.AppendChild(ticket);
                doc.AppendChild(root);
            }

            // save the file
            doc.Save(xmlPath);
            return RedirectToAction("Details", new { id = ticketID });
        }

        private XmlElement _CreateTicketElement(XmlDocument doc, Ticket newTicket, string ccList)
        {
            UserController userController = new UserController(userManager);

            XmlElement ticket = doc.CreateElement("ticket");

            XmlNode id = doc.CreateElement("ticketID");
            // Preventing random ID injection by server validation
            id.InnerText = Convert.ToString(LastId());

            XmlNode issueDate = doc.CreateElement("issueDate");
            issueDate.InnerText = Convert.ToString(Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM-ddThh:mm:ss"));
            XmlNode deadline = doc.CreateElement("ticketDeadline");
            deadline.InnerText = Convert.ToString(Convert.ToDateTime(newTicket.Deadline).ToString("yyyy-MM-ddThh:mm:ss"));
            XmlNode subject = doc.CreateElement("ticketSubject");
            subject.InnerText = newTicket.Subject;
            XmlNode owner = doc.CreateElement("ticketOwner");

            owner.InnerText = Convert.ToString(userController.FetchUserID(newTicket.OwnerName));
            XmlNode assignee = doc.CreateElement("ticketAssignee");
            assignee.InnerText = Convert.ToString(newTicket.Assignee);
            XmlNode status = doc.CreateElement("ticketStatus");
            status.InnerText = newTicket.Status;
            
            XmlAttribute lastUpdated = doc.CreateAttribute("lastUpdated");
            lastUpdated.Value = Convert.ToString(Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM-ddThh:mm:ss"));
            ticket.Attributes.Append(lastUpdated);

            XmlNode comments = doc.CreateElement("comments");


            ticket.AppendChild(id);
            ticket.AppendChild(issueDate);
            ticket.AppendChild(deadline);
            ticket.AppendChild(subject);
            ticket.AppendChild(owner);
            ticket.AppendChild(assignee);
            ticket.AppendChild(status);
            if (ccList != null)
            {
                XmlNode ccListNode = _CreateCCList(doc, ccList);
                ticket.AppendChild(ccListNode);
            }
            ticket.AppendChild(comments);

            return ticket;
        }

        private XmlNode _CreateCCList(XmlDocument doc, string CCList)
        {
            XmlNode ccList = doc.CreateElement("ccList");
            List<string> users = CCList.Split(',').ToList();
            foreach(var userString in users)
            {
                XmlNode user = doc.CreateElement("ccUser");
                user.InnerText = userString;
                ccList.AppendChild(user);
            }
            return ccList;
        }

        private XmlElement _CreateUserElement(XmlDocument doc, UserEmp newUser)
        {
            XmlElement user = doc.CreateElement("user");

            XmlNode id = doc.CreateElement("userID");
            // Preventing random ID injection by server validation
            id.InnerText = Convert.ToString(LastId());

            XmlNode email = doc.CreateElement("userEmail");
            email.InnerText = newUser.Email;
            XmlNode hash = doc.CreateElement("userPasswordHash");
            hash.InnerText = newUser.PasswordHash;
            XmlNode dept = doc.CreateElement("department");
            dept.InnerText = newUser.Department;

            XmlNode username = doc.CreateElement("userName");
            XmlNode firstname = doc.CreateElement("firstName");
            firstname.InnerText = newUser.FirstName;

            XmlNode middlename = doc.CreateElement("middleName");
            middlename.InnerText = newUser.MiddleName;

            XmlNode lastname = doc.CreateElement("lastName");
            lastname.InnerText = newUser.LastName;
            username.AppendChild(firstname);
            username.AppendChild(middlename);
            username.AppendChild(lastname);

            XmlAttribute userType = doc.CreateAttribute("type");
            userType.Value = newUser.Type;
            user.Attributes.Append(userType);

            user.AppendChild(id);
            user.AppendChild(username);
            user.AppendChild(email);
            user.AppendChild(hash);
            user.AppendChild(dept);
            return user;
        }

        //private XmlElement _CreateCommentElement(XmlDocument doc, Comment newComment)
        //{
        //    UserController userController = new UserController(userManager);

        //    XmlElement ticket = doc.CreateElement("ticket");

        //    XmlNode id = doc.CreateElement("ticketID");
        //    // Preventing random ID injection by server validation
        //    id.InnerText = Convert.ToString(LastId());

        //    XmlNode issueDate = doc.CreateElement("issueDate");
        //    issueDate.InnerText = Convert.ToString(Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM-ddThh:mm:ss"));
        //    XmlNode deadline = doc.CreateElement("ticketDeadline");
        //    deadline.InnerText = Convert.ToString(Convert.ToDateTime(newTicket.Deadline).ToString("yyyy-MM-ddThh:mm:ss"));
        //    XmlNode subject = doc.CreateElement("ticketSubject");
        //    subject.InnerText = newTicket.Subject;
        //    XmlNode owner = doc.CreateElement("ticketOwner");

        //    owner.InnerText = Convert.ToString(userController.FetchUserID(newTicket.OwnerName));
        //    XmlNode assignee = doc.CreateElement("ticketAssignee");
        //    assignee.InnerText = Convert.ToString(newTicket.Assignee);
        //    XmlNode status = doc.CreateElement("ticketStatus");
        //    status.InnerText = newTicket.Status;

        //    XmlAttribute lastUpdated = doc.CreateAttribute("lastUpdated");
        //    lastUpdated.Value = Convert.ToString(Convert.ToDateTime(DateTime.Now).ToString("yyyy-MM-ddThh:mm:ss"));
        //    ticket.Attributes.Append(lastUpdated);

        //    ticket.AppendChild(id);
        //    ticket.AppendChild(issueDate);
        //    ticket.AppendChild(deadline);
        //    ticket.AppendChild(subject);
        //    ticket.AppendChild(owner);
        //    ticket.AppendChild(assignee);
        //    ticket.AppendChild(status);
        //    if (ccList != null)
        //    {
        //        XmlNode ccListNode = _CreateCCList(doc, ccList);
        //        ticket.AppendChild(ccListNode);
        //    }

        //    return ticket;
        //}
    }
}
