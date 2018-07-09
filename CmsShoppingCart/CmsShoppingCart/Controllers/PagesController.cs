using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Controllers
{
    public class PagesController : Controller
    {
        // GET: Index/{Page}
        public ActionResult Index(string page="")
        {

            //get & set page slug
            if (page == "")
            {
                page = "home";
            }
            //declare model and DTO
            PageVM model;
            pageDTO dto;
            //check if page exist
            using (Db db = new Db())
            {
                if (!db.Pages.Any(x => x.Slug.Equals(page)))
                {
                    return RedirectToAction("Index" ,new { page=" "});
                }
            }
            //get the page DTO
            using (Db db = new Db())
            {
                dto = db.Pages.Where(x => x.Slug == page).FirstOrDefault();
            }
            //set page title
            ViewBag.PageTitle = dto.Title;
            //check for sidebar
            if (dto.HasSidebar==true)
            {
                ViewBag.Sidebar = "Yes";
            }
            else
            {
                ViewBag.Sidebar = "No";

            }
            //Initialize model
            model = new PageVM(dto);
            //return view with model

            return View(model);
        }

        public ActionResult PagesMenuPartial()
        {

            //declare alist of pagevm
            List<PageVM> pageVMList;
            //get all page except home page
            using (Db db = new Db())
            {
                pageVMList = db.Pages.ToArray().OrderBy(x=>x.Sorting).Where(x=>x.Slug !="home").Select(x=> new Models.ViewModels.Pages.PageVM(x)).ToList();

            }
                //return partial view with list
                return PartialView(pageVMList);
        }


        public ActionResult SidebarPartial()
        {

            //declare model
            SidebarVM model;
            //Initialize Model
            using (Db db = new Db())
            {
                SidebarDTO dto =db.Sidebar.Find(1);
                model = new SidebarVM(dto);

            }
                //Return Partial View With Model
                return PartialView(model);
        }
    }
}