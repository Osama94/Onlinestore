using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Controllers
{
    public class ShopController : Controller
    {
        // GET: Shop
        public ActionResult Index()
        {
            return RedirectToAction("Index","Pages");
        }


        public ActionResult CategoryMenuPartial()
        {

            //declare list of categoryVM
            List<CategoryVM> categoryVMList;
            //Initialize List
            using (Db db = new Db())
            {
                categoryVMList = db.Categories.ToArray().OrderBy(x => x.Sorting).Select(x => new CategoryVM(x)).ToList();

            }
            //Return partial view with list
            return PartialView(categoryVMList);
        }


        ////GET: Shop/Category/name

        public ActionResult Category(string name)
        {
            //Declare A list of ProductVM
            List<ProductVM> ProductVMList;

            using (Db db = new Db())
            {
                //Get Category ID
                CategoryDTO categoryDTO = db.Categories.Where(x => x.Slug == name).FirstOrDefault();
                int catId = categoryDTO.Id;
                //Initialize The List
                ProductVMList = db.Products.ToArray().Where(x => x.CategoryId == catId).Select(x => new ProductVM(x)).ToList();
                //Get Category Name
                var productCat = db.Products.Where(x => x.CategoryId == catId).FirstOrDefault();
                ViewBag.CategoryName = productCat.CategoryName;
            }
            //Return View With List

            return View(ProductVMList);
        }




        // GET: Shop/ProductDetails/name
        [ActionName("product-details")]
        public ActionResult ProductDetails(string name)
        {

            //Declare VM and DTO
            ProductVM model;
            ProductDTO dto;
            //Initialize Product id
            int id = 0;
            using (Db db = new Db())
            {
                //check if product exist
                if (!db.Products.Any(x=>x.Slug.Equals(name)))
                {
                    return RedirectToAction("Index","Shop");
                }
                //Initialize ProductDTO
                dto = db.Products.Where(x => x.Slug == (name)).FirstOrDefault();
                //Get  Id
                id = dto.Id;
                //Initialize Model
                model = new ProductVM(dto);
            }
            //Get Gallery Images
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                  .Select(fn => Path.GetFileName(fn));
            //Return View With model

            return View("ProductDetails",model);
        }
    }
}