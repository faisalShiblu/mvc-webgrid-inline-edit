using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using InlineEditingWebgrid.Models;
using InlineEditingWebgrid.Models.ViewModels;
using Newtonsoft.Json.Linq;

namespace InlineEditingWebgrid.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index()
        {
            List<SiteUserModel> list = new List<SiteUserModel>();
            using (EventSchedulerEntities db = new EventSchedulerEntities())
            {
                var v = (from a in db.SiteUsers
                         join b in db.UserRoles on a.RoleID equals b.ID
                         select new SiteUserModel
                         {
                             ID = a.ID,
                             FirstName = a.FirstName,
                             LastName = a.LastName,
                             DOB = a.DOB,
                             RoleID = a.RoleID,
                             RoleName = b.RoleName
                         });
                list = v.ToList();
            }
            return View(list);
        }

        [HttpPost]
        public ActionResult Saveuser(int id, string propertyName, string value)
        {
            var status = false;
            var message = "";

            //Update data to database 
            using (EventSchedulerEntities db = new EventSchedulerEntities())
            {
                var user = db.SiteUsers.Find(id);

                object updateValue = value;
                bool isValid = true;

                if (propertyName == "RoleID")
                {
                    int newRoleID = 0;
                    if (int.TryParse(value, out newRoleID))
                    {
                        updateValue = newRoleID;
                        //Update value field
                        value = db.UserRoles.Where(a => a.ID == newRoleID).First().RoleName;
                    }
                    else
                    {
                        isValid = false;
                    }

                }
                else if (propertyName == "DOB")
                {
                    DateTime dob;
                    if (DateTime.TryParseExact(value, "dd-MM-yyyy", new CultureInfo("en-US"), DateTimeStyles.None, out dob))
                    {
                        updateValue = dob;
                    }
                    else
                    {
                        isValid = false;
                    }
                }

                if (user != null && isValid)
                {
                    db.Entry(user).Property(propertyName).CurrentValue = updateValue;
                    db.SaveChanges();
                    status = true;
                }
                else
                {
                    message = "Error!";
                }
            }

            var response = new { value = value, status = status, message = message };
            JObject o = JObject.FromObject(response);
            return Content(o.ToString());
        }

        public ActionResult GetUserRoles(int id)
        {
            //Allowed response content format
            //{'E':'Letter E','F':'Letter F','G':'Letter G', 'selected':'F'}
            int selectedRoleID = 0;
            StringBuilder sb = new StringBuilder();
            using (EventSchedulerEntities db = new EventSchedulerEntities())
            {
                var roles = db.UserRoles.OrderBy(a => a.RoleName).ToList();
                foreach (var item in roles)
                {
                    sb.Append(string.Format("'{0}':'{1}',", item.ID, item.RoleName));
                }

                selectedRoleID = db.SiteUsers.Where(a => a.ID == id).First().RoleID;

            }

            sb.Append(string.Format("'selected': '{0}'", selectedRoleID));
            return Content("{" + sb.ToString() + "}");
        }
    }
}