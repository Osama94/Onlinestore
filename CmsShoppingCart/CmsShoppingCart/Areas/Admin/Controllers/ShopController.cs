using CmsShoppingCart.Areas.Admin.Models.ViewModels.Shop;
using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Shop;
using PagedList;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;

namespace CmsShoppingCart.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin")]

    public class ShopController : Controller
    {
        // GET: Admin/Shop/Categories
        public ActionResult Categories()
        {

            //Declare a List Of Models
            List<CategoryVM> categoryVMList;
            using (Db db = new Db())
            {
                //Intialize The List
                categoryVMList = db.Categories
                                .ToArray()
                                .OrderBy(x=>x.Sorting)
                                .Select(x=>new CategoryVM(x))
                                .ToList();
            }
                //Return View With List
                return View(categoryVMList);
        }

        // POST: Admin/Shop/AddNewCategory
        [HttpPost]
        public string AddNewCategory(string catName)
        {
            //Declare id
            string id;
            using (Db db = new Db())
            {
                //check that the category name is unique
                if (db.Categories.Any(x=>x.Name == catName))
                {
                    return "titletaken";
                }
                //Intialize DTO
                CategoryDTO dto = new CategoryDTO();
                //Add To DTO
                dto.Name = catName;
                dto.Slug = catName.Replace(" ", "-").ToLower();
                dto.Sorting = 100;
                //Save DTO
                db.Categories.Add(dto);
                db.SaveChanges();
                //Get The ID
                id = dto.Id.ToString() ;

            }
            //Return Id
            return id;
            }


        // POST: Admin/Shop/Categories
        [HttpPost]
        public void ReorderCategories(int[] id)
        {
            using (Db db = new Db())
            {
                //set initial count
                int count = 1;
                //Declare pageDTO
                CategoryDTO dto;
                //set sorting for each Category
                foreach (var catId in id)
                {
                    dto = db.Categories.Find(catId);
                    dto.Sorting = count;
                    db.SaveChanges();
                    count++;

                }
            }
        }

        // GET: Admin/shop/DeleteCategory/id

        public ActionResult DeleteCategory(int id)
        {
            using (Db db = new Db())
            {
                //get the page
             CategoryDTO dto = db.Categories.Find(id);

                //remove the Category
                db.Categories.Remove(dto);
                //save
                db.SaveChanges();
            }
            //redirect
            return RedirectToAction("Categories");
        }

        // POST: Admin/Shop/RenameCategory
        [HttpPost]
        public string RenameCategory(string newCatName,int id)
        {
            using (Db db = new Db())
            {
                //check category name is unique
                if (db.Categories.Any(x=>x.Name==newCatName))
                {
                    return "titletaken";
                }
                //Get DTO
                CategoryDTO dto = db.Categories.Find(id);

                //Edit DTO
                dto.Name = newCatName;
                dto.Slug = newCatName.Replace(" ","-").ToLower();
                //Save
                db.SaveChanges();
            }
            //Return
            return "ok";

        }


        // GET: Admin/shop/AddProduct
        [HttpGet]
        public ActionResult AddProduct()
        {
            //Intialize Model
            ProductVM model = new ProductVM();
            //Add Select List Of Categories to Model
            using (Db db = new Db())
            {

                model.Categories = new SelectList(db.Categories.ToList(),"Id","Name");
            }

            //Return view With Model
                return View(model);
        }



        //POST: Admin/shop/AddProduct
        [HttpPost]
        public ActionResult AddProduct(ProductVM model,HttpPostedFileBase file)
        {
            //check model state
            if (!ModelState.IsValid)
            {
                using (Db db = new Db())
                {

                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    return View(model);
                }

            }
            //make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Any(x=>x.Name==model.Name))
                {
                    model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                    ModelState.AddModelError("", "That Product Name Is Taken !");
                    return View(model);
                }
                
            }
            //declare product id
            int id;
            //initilize and save productDTO
            using (Db db = new Db())
            {
                ProductDTO product = new ProductDTO();
                product.Name = model.Name;
                product.Slug = model.Name.Replace(" ","-").ToLower();
                product.Description = model.Description;
                product.Price = model.Price;
                product.CategoryId = model.CategoryId;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                product.CategoryName = catDTO.Name;

                db.Products.Add(product);
                db.SaveChanges();

                //Get the id
                id = product.Id;
            }

            //set tempdata messsage
            TempData["Success Message"] = "You Have Added A Product!";

            #region Upload Image

            //create necessary directories
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads",Server.MapPath(@"\")));
            var pathString1 = Path.Combine(originalDirectory.ToString(),"Products");
            var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\"+id.ToString());
            var pathString3 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString()+"\\Thumbs");
            var pathString4 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery");
            var pathString5 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

            if (!Directory.Exists(pathString1))
            {
                Directory.CreateDirectory(pathString1);
            }

            if (!Directory.Exists(pathString2))
            {
                Directory.CreateDirectory(pathString2);
            }
            if (!Directory.Exists(pathString3))
            {
                Directory.CreateDirectory(pathString3);
            }
            if (!Directory.Exists(pathString4))
            {
                Directory.CreateDirectory(pathString4);
            }
            if (!Directory.Exists(pathString5))
            {
                Directory.CreateDirectory(pathString5);
            }

            //check if a file was  uploaded
            if (file != null && file.ContentLength > 0)
            {
                //get file extension
                string ext = file.ContentType.ToLower();
                //verify extension
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png" )
                {
                    using (Db db = new Db())
                    {
                        
                            model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                            ModelState.AddModelError("", "The Image Was Not Uploaded - Wrong Image Extension !");
                            return View(model);
                        

                    }
                }
                //initilize image name
                string imageName = file.FileName;
                //save mage name to DTO
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }
                //set orignial and thumb image path
                var path = string.Format("{0}\\{1}",pathString2,imageName);
                var path2 = string.Format("{0}\\{1}", pathString3, imageName);

                //save original image
                file.SaveAs(path);
                //create and save thumb
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);
            }
            #endregion

            //Redirect
            return RedirectToAction("AddProduct");
        }


        // GET: Admin/shop/Products
        public ActionResult Products(int? page, int? catId)
        {

            //Declare List Of ProductVM
            List<ProductVM> listOfProductVM;
            //Set Page Number
            var pageNumber = page ?? 1;
            using (Db db = new Db())
            {
                //Intialize List
                listOfProductVM = db.Products.ToArray()
                                .Where(x => catId == null || catId == 0 || x.CategoryId == catId)
                                .Select(x => new ProductVM(x))
                                .ToList();
                //Populate Categories select list
                ViewBag.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");
                //set selected category
                ViewBag.SelectedCat = catId.ToString();
            }

            //set pagination
            var onePageOfProducts = listOfProductVM.ToPagedList(pageNumber, 3);
            ViewBag.OnePageOfProducts = onePageOfProducts;
                //return view with list

                return View(listOfProductVM);
        }


        // GET: Admin/shop/EditProduct/id
        [HttpGet]
        public ActionResult EditProduct(int id)
        {
            //Declare productVM
            ProductVM model;
            using (Db db = new Db())
            {
                //Get The product
                ProductDTO dto = db.Products.Find(id);
                //Make Sure Product exist
                if (dto==null)
                {
                    return Content("That Product Does Not Exist !");
                }
                //Intialize Model
                model = new ProductVM(dto);
                //Make A select List
                model.Categories = new SelectList(db.Categories.ToList(),"Id","Name");
                //Get All Gallery Images
                model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id+"/Gallery/Thumbs"))
                                       .Select(fn=>Path.GetFileName(fn));
            }
                //return View with model
                return View(model);
        }


        // POST: Admin/shop/EditProduct/id
        [HttpPost]
        public ActionResult EditProduct(ProductVM model , HttpPostedFileBase file)
        {
            //get product id
            int id = model.Id;
            //populate categories select list and gallery images
            using (Db db = new Db())
            {
                model.Categories = new SelectList(db.Categories.ToList(), "Id", "Name");

            }
            model.GalleryImages = Directory.EnumerateFiles(Server.MapPath("~/Images/Uploads/Products/" + id + "/Gallery/Thumbs"))
                                     .Select(fn => Path.GetFileName(fn));
            //check model state
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            //make sure product name is unique
            using (Db db = new Db())
            {
                if (db.Products.Where(x => x.Id != id).Any(x => x.Name == model.Name))
                {
                    ModelState.AddModelError("", "That Product Is Already Taken !");
                    return View(model);

                }

            }
            //update product
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);

                dto.Name = model.Name;
                dto.Slug = model.Name.Replace(" ", "-").ToLower();
                dto.Description = model.Description;
                dto.Price = model.Price;
                dto.CategoryId = model.CategoryId;
                dto.ImageName = model.ImageName;

                CategoryDTO catDTO = db.Categories.FirstOrDefault(x => x.Id == model.CategoryId);
                dto.CategoryName = catDTO.Name;
                db.SaveChanges();
            }
                //Set TempData message
                TempData["Success Message"] = "You Have Edit The Product !";


            #region Image Upload

            //check if a file was  uploaded
            if (file != null && file.ContentLength > 0)
            {
                //get  extension
                string ext = file.ContentType.ToLower();
                //verify extension
                if (ext != "image/jpg" &&
                    ext != "image/jpeg" &&
                    ext != "image/pjpeg" &&
                    ext != "image/gif" &&
                    ext != "image/x-png" &&
                    ext != "image/png")
                {
                    using (Db db = new Db())
                    {

                        ModelState.AddModelError("", "The Image Was Not Uploaded - Wrong Image Extension !");
                        return View(model);


                    }
                }
                //Set upload Directory Paths
                var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));
                var pathString1 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString());
                var pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Thumbs");

                //Delete Files From Directories
                DirectoryInfo di1 = new DirectoryInfo(pathString1);

                DirectoryInfo di2 = new DirectoryInfo(pathString2);

                foreach (FileInfo file2 in di1.GetFiles())
                    file2.Delete();

                foreach (FileInfo file3 in di2.GetFiles())
                    file3.Delete();
                //save Image Name

                string imageName = file.FileName;
                using (Db db = new Db())
                {
                    ProductDTO dto = db.Products.Find(id);
                    dto.ImageName = imageName;
                    db.SaveChanges();
                }
                //Save orignial and thumb images

                var path = string.Format("{0}\\{1}", pathString1, imageName);
                var path2 = string.Format("{0}\\{1}", pathString2, imageName);

                file.SaveAs(path);
                  
                WebImage img = new WebImage(file.InputStream);
                img.Resize(200, 200);
                img.Save(path2);

            }



            #endregion

            //Redirect
            return RedirectToAction("EditProduct");
        }

        // GET: Admin/shop/DeleteProduct/id

        public ActionResult DeleteProduct(int id)
        {
            //delete product from DB
            using (Db db = new Db())
            {
                ProductDTO dto = db.Products.Find(id);
                db.Products.Remove(dto);
                db.SaveChanges();

            }

            //delete Product Folder
            var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads",Server.MapPath(@"\")));
            string pathString = Path.Combine(originalDirectory.ToString(),"Products\\"+id.ToString());

            if (Directory.Exists(pathString))
            {
                Directory.Delete(pathString,true);

            }
            //redirect

            return RedirectToAction("Products");
        }

        // POST: Admin/shop/SaveGalleryImages
        [HttpPost]
        public ActionResult SaveGalleryImages(int id)
        {

            //Loop Through Files
            foreach (string fileName in Request.Files)
            {

                //initialize the file
                HttpPostedFileBase file = Request.Files[fileName];

                //Check it's not null
                if (file != null && file.ContentLength > 0)
                {

                    //set directory psths
                    var originalDirectory = new DirectoryInfo(string.Format("{0}Images\\Uploads", Server.MapPath(@"\")));

                    string pathString1 = Path.Combine(originalDirectory.ToString(),"Products\\"+id.ToString()+"\\Gallery");
                    string pathString2 = Path.Combine(originalDirectory.ToString(), "Products\\" + id.ToString() + "\\Gallery\\Thumbs");

                    //set image paths
                    var path = string.Format("{0}\\{1}",pathString1,file.FileName);
                    var path2 = string.Format("{0}\\{1}", pathString2, file.FileName);

                    //save original and thumb
                    file.SaveAs(path);
                    WebImage img = new WebImage(file.InputStream);
                    img.Resize(200, 200);
                    img.Save(path2);
                }

            }
            return View();
        }

        // POST: Admin/shop/DeleteImage
        [HttpPost]
        public void DeleteImage(int id , string imageName)
        {

            
            string fullpath1 = Request.MapPath("~/Images/Uploads/Products/"+id.ToString()+"/Gallery/"+ imageName);
            string fullpath2 = Request.MapPath("~/Images/Uploads/Products/" + id.ToString() + "/Gallery/Thumbs/" + imageName);

            if (System.IO.File.Exists(fullpath1))
                System.IO.File.Delete(fullpath1);
            

            if (System.IO.File.Exists(fullpath2))
            System.IO.File.Delete(fullpath2);
            
        }


        // GET: Admin/shop/Orders
        public ActionResult Orders()
        {

            //Intialize List Of OrdersForAdminVM
            List<OrdersForAdminVM> ordersForAdmin = new List<OrdersForAdminVM>(); 

            using (Db db = new Db())
            {

                //Intialize List Of OrderVM
                List<OrderVM> orders = db.Orders.ToArray().Select(x => new OrderVM(x)).ToList();

                //Loop through List Of OrderVM
                foreach (var order in orders)
                {
                    //Intialize Product Dictionarry
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();
                    //declare total
                    decimal total = 0m;
                    //Intialize List Of OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsList = db.OrderDetails.Where(x => x.OrderId==order.OrderId).ToList();
                    //Get Username
                    UserDTO user = db.Users.Where(x=>x.Id==order.UserId).FirstOrDefault();
                    string username = user.Username;
                    //Loop Through List Of OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsList)
                    {
                        //Get The Product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();
                        //Get The Product Price
                        decimal price = product.Price;
                        //Get The Product Name
                        string productName = product.Name;
                        //Add to Product dictionary
                        productsAndQty.Add(productName,orderDetails.Quantity);
                        //Get Total
                        total += orderDetails.Quantity * price;
                    }

                    //Add to ordersforadminVM List
                    ordersForAdmin.Add(new OrdersForAdminVM() {

                        OrderNumber = order.OrderId,
                        Username = username,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt

                    });
                }
            }

            //Return View With OrdersForAdminVM List

            return View(ordersForAdmin);
        }
    }
}