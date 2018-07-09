using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Pages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]

    public class PagesController : Controller
    {
        // GET: Admin/Pages
        public ActionResult Index()
        {
            //Declare List Of PageVM
            List<PageVM> PagesList;

            using (Db db = new Db())
            {
                //Initialize the List
                PagesList = db.Pages.ToArray().OrderBy(x=>x.Sorting).Select(x=>new PageVM(x)).ToList();

            }

            //Retuen View With List
            return View(PagesList);
        }

        // GET: Admin/Pages/AddPage
        [HttpGet]
        public ActionResult AddPage()
        {
            return View();
        }

        // POST: Admin/Pages/AddPage
        [HttpPost]
        public ActionResult AddPage(PageVM model)
        {
            //check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                //declare slug

                string slug;
                //Initialize pageDTO
                pageDTO dto = new pageDTO();
                //DTO Title
                dto.Title = model.Title;
                //check forand set slug if need be
                if (string.IsNullOrWhiteSpace(model.Slug))
                {
                    slug = model.Title.Replace(" ", "-").ToLower();
                }
                else
                {
                    slug = model.Slug.Replace(" ", "-").ToLower();
                }
                //make sure title and slug are unique
                if (db.Pages.Any(x=>x.Title==model.Title) || db.Pages.Any(x => x.Slug == model.Slug ))
                {
                    ModelState.AddModelError("", "That Title Or Slug Already Exist.");
                    return View(model);

                }
                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                dto.Sorting = 100;
                //Save The DTO
                db.Pages.Add(dto);
                db.SaveChanges();
            }
            //Set TempData message
            TempData["Success Message"] ="You Have Added A New Page!";
                //Redirect
                return RedirectToAction("AddPage");
        }


        // GET: Admin/Pages/EditPage/id
        [HttpGet]
        public ActionResult EditPage(int id)
        {
            //Declare PageVM
            PageVM model;
            using (Db db = new Db())
            {
                //Get The Page
                pageDTO dto = db.Pages.Find(id);
                //confirm page exist
                if (dto ==null)
                {
                    return Content("The Page Doesn't Exist.");
                }
                //initialize pageVM
                model = new PageVM(dto);
            }
                //return view with model
                return View(model);
        }


        // POST: Admin/Pages/EditPage/id
        [HttpPost]
        public ActionResult EditPage(PageVM model)
        {
            //check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            using (Db db = new Db())
            {
                //get page id
                int id = model.Id;
                //Intitialize slug
                string slug="home";
                //get the page
                pageDTO dto = db.Pages.Find(id);
                //DTO Title
                dto.Title = model.Title;
                //check for slug and set it if need be
                if (model.Slug != "home")
                {
                    if (string.IsNullOrWhiteSpace(model.Slug))
                    {
                        slug = model.Title.Replace(" ", "-").ToLower();
                    }
                    else
                    {
                        slug = model.Slug.Replace(" ", "-").ToLower();
                    }
                }
                
                //make sure title and slug are unique
                if (db.Pages.Where(x => x.Id != id).Any(x => x.Title == model.Title) ||
                    db.Pages.Where(x => x.Id != id).Any(x => x.Slug == slug))
                {
                    ModelState.AddModelError("", "That Title Or Slug Already Exist.");
                    return View(model);

                }
                //DTO the rest
                dto.Slug = slug;
                dto.Body = model.Body;
                dto.HasSidebar = model.HasSidebar;
                
                //Save The DTO
                db.SaveChanges();
            }
            //Set TempData message
            TempData["Success Message"] = "You Have Edit The Page!";
            //Redirect
            return RedirectToAction("EditPage");
        }

        // GET: Admin/Pages/PageDetails/id
        public ActionResult PageDetails(int id)
        {
            //Declare PageVM
            PageVM model;
            using (Db db = new Db())
            {
                //Get The Page
                pageDTO dto = db.Pages.Find(id);
                //confirm page exist
                if (dto == null)
                {
                    return Content("The Page Doesn't Exist.");
                }
                //initialize pageVM
                model = new PageVM(dto);
            }
            //return view with model
            return View(model);
        }


        // GET: Admin/Pages/DeletePage/id

        public ActionResult DeletePage(int id)
        {
            using (Db db = new Db())
            {
                //get the page
                pageDTO dto = db.Pages.Find(id);

                //remove the page
                db.Pages.Remove(dto);
                //save
                db.SaveChanges();
            }
            //redirect
            return RedirectToAction("Index");
        }


        // POST: Admin/Pages/ReorderPages/id
        [HttpPost]
        public void ReorderPages(int[] id)
        {
            using (Db db = new Db())
            {
                //set initial count
                int count = 1;
                //Declare pageDTO
                pageDTO dto;
                //set sorting for each page
                foreach (var pageId in id)
                {
                    dto = db.Pages.Find(pageId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;

                }
            }
        }


        // GET: Admin/Pages/EditSidebar
        [HttpGet]
        public ActionResult EditSidebar()
        {

            //Declare Model
            SidebarVM model;
            using (Db db = new Db())
            {
                //Get DTO
                SidebarDTO dto = db.Sidebar.Find(1);
                //Intialize Model
                model = new SidebarVM(dto);
            }
                //return view with model
                return View(model);
        }


        // POST: Admin/Pages/EditSidebar
        [HttpPost]
        public ActionResult EditSidebar(SidebarVM model)
        {
            using (Db db = new Db())
            {
                //get the DTO
                SidebarDTO dto = db.Sidebar.Find(1);

                //DTO the Body
                dto.Body = model.Body;
                //save
                db.SaveChanges();
            }
            //set tempdata message
            TempData["Success Message"] = "You Have Edit Sidebar !";

            //redirect
            return RedirectToAction("EditSidebar");
        }
    }
}