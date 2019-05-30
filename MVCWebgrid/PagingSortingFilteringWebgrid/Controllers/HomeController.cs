using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PagingSortingFilteringWebgrid.Models;
using System.Linq.Dynamic;


namespace PagingSortingFilteringWebgrid.Controllers
{
    public class HomeController : Controller
    {
        // GET: Home
        public ActionResult Index(int page = 1, string sort = "FirstName", string sortdir = "asc", string search = "")
        {
            int pageSize = 10;
            int totalRecord = 0;
            if (page < 1) page = 1;
            int skip = (page * pageSize) - pageSize;
            var data = GetEmployees(search, sort, sortdir, skip, pageSize, out totalRecord);
            ViewBag.TotalRows = totalRecord;
            ViewBag.search = search;
            return View(data);
        }

        public List<Employee> GetEmployees(string search, string sort, string sortdir, int skip, int pageSize, out int totalRecord)
        {
            using (EventSchedulerEntities dc = new EventSchedulerEntities())
            {
                var v = (from a in dc.Employees
                         where
                                 a.FirstName.Contains(search) ||
                                 a.LastName.Contains(search) ||
                                 a.EmailID.Contains(search) ||
                                 a.City.Contains(search) ||
                                 a.Country.Contains(search)
                         select a);
                totalRecord = v.Count();
                v = v.OrderBy(sort + " " + sortdir);
                if (pageSize > 0)
                {
                    v = v.Skip(skip).Take(pageSize);
                }
                return v.ToList();
            }
        }
    }
}