using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SupportTicketSystem.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Xml;

namespace SupportTicketSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;

        public UserController(UserManager<IdentityUser> userManager)
        {
            this.userManager = userManager;
        }

        public IActionResult Index()
        {
            List<UserEmp> users = new List<UserEmp>(); //list of user obj
            users = FetchUserList();
            return View(users); // pass the value to the View
        }

        [HttpGet]
        [Authorize]
        public List<UserEmp> FetchUserList()
        {
            List<UserEmp> users = new List<UserEmp>(); //list of user obj
            string path = "App_Data/users.xml";
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(path))
            {
                //only load if the file exists
                doc.Load(path); //exists, so load
                XmlNodeList userList = doc.GetElementsByTagName("user"); //load user elements
                foreach (XmlElement userItem in userList)
                {
                    string middleName = null;
                    if (userItem.GetElementsByTagName("middleName").Count > 0)
                    {
                        middleName = userItem.GetElementsByTagName("middleName")[0].InnerText;
                    }

                    UserEmp user = new UserEmp
                    {
                        UserID = Convert.ToInt32(userItem.GetElementsByTagName("userID")[0].InnerText),
                        Type = userItem.Attributes["type"].Value,
                        FirstName = userItem.GetElementsByTagName("firstName")[0].InnerText,
                        MiddleName = middleName,
                        LastName = userItem.GetElementsByTagName("lastName")[0].InnerText,
                        Email = userItem.GetElementsByTagName("userEmail")[0].InnerText,
                        PasswordHash = userItem.GetElementsByTagName("userPasswordHash")[0].InnerText,
                        Department = userItem.GetElementsByTagName("department")[0].InnerText
                    };
                    users.Add(user);
                }
            }
            return users;
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            // this is to send the last ID + 1 to the create view
            UserEmp newUser = new()
            {
                UserID = LastId(),
            };
            var user = await userManager.GetUserAsync(HttpContext.User);
            ViewBag.user = user;

            List<UserType> userTypes = new List<UserType>();
            var userList = (from action in (UserType[])Enum.GetValues(typeof(UserType)) select action.ToString()).ToList();
            ViewBag.userList = userList.Select(x => new SelectListItem() { Value = x, Text = x }).ToList();
            
            List<Department> departments = new List<Department>();
            var deptList = (from action in (Department[])Enum.GetValues(typeof(Department)) select action.ToString()).ToList();
            var list = deptList.Select(x => new SelectListItem() { Value = x, Text = x }).ToList();
            ViewBag.deptList = list;
            
            return View(newUser);

        }

        public string FetchUserName(int UserID)
        {
            string path = "App_Data/users.xml";
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(path))
            {
                //only load if the file exists
                doc.Load(path); //exists, so load
                XmlNode user = doc.SelectSingleNode("//user[userID/text()='" + UserID + "']"); //load user elements
                
                if (user != null)
                {
                    string middleName = user["userName"]["middleName"] != null ? " " + user["userName"]["middleName"].InnerText : "";
                    return user["userName"]["firstName"].InnerText + middleName + " " + user["userName"]["lastName"].InnerText;
                } else
                {
                    return "";
                }
                

            }
            else
            {
                return "";
            }
        }

        public int FetchUserID(string userEmail)
        {
            string path = "App_Data/users.xml";
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(path))
            {
                //only load if the file exists
                doc.Load(path); //exists, so load
                XmlNode user = doc.SelectSingleNode("//user[userEmail/text()='" + userEmail + "']"); //load user elements

                if (user != null)
                {
                    return Convert.ToInt32(user["userID"].InnerText);
                }
                else
                {
                    return 0;
                }


            }
            else
            {
                return 0;
            }
        }

        public int LastId()
        {
            string path =  "App_Data/users.xml";
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(path))
            {
                //only load if the file exists
                doc.Load(path); //exists, so load
                var lastID = Convert.ToInt32(doc.GetElementsByTagName("userID")[^1].InnerText);
                return lastID + 1;
            }
            else
            {
                return 1;
            }
        }

        [HttpPost]
        public IActionResult Create(UserEmp newUser)
        {
            // load the xml
            string xmlPath = Request.PathBase + "App_Data/users.xml";
            XmlDocument doc = new XmlDocument();
            if (System.IO.File.Exists(xmlPath))
            {
                // load the file
                doc.Load(xmlPath);

                // create the person
                XmlElement user = _CreateUserElement(doc, newUser);

                // append to root
                doc.DocumentElement.AppendChild(user);
            }
            else
            {
                // create new file
                XmlNode dec = doc.CreateXmlDeclaration("1.0", "utf-8", "");
                doc.AppendChild(dec);
                XmlNode root = doc.CreateElement("user");

                // create a person
                XmlElement user = _CreateUserElement(doc, newUser);
                root.AppendChild(user);
                doc.AppendChild(root);
            }

            // save the file
            doc.Save(xmlPath);
            return RedirectToAction("Index");
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
    }
}
