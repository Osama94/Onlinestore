using CmsShoppingCart.Models.Data;
using CmsShoppingCart.Models.ViewModels.Account;
using CmsShoppingCart.Models.ViewModels.Shop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace CmsShoppingCart.Controllers
{

    public class AccountController : Controller
    {
        // GET: Account
        public ActionResult Index()
        {
            return Redirect("~/account/login");
        }


        // GET: account/Login
        [HttpGet]
        public ActionResult Login()
        {

            //confirm user is not logged in
            string username = User.Identity.Name;
            if (!string.IsNullOrEmpty(username))
            {
                return Redirect("user-profile");

            }

            //return view
            return View();
        }



        
        // POST: account/Login
        [HttpPost]
        public ActionResult Login(LoginUserVM model)
        {
            //check model state
            if (! ModelState.IsValid)
            {
                return View(model);
            }

            //check if user is valid
            bool isValid = false;

            using (Db db = new Db())
            {
                if (db.Users.Any(x => x.Username.Equals(model.Username) && x.Password.Equals(model.Password)))
                {
                    isValid = true;
                  
                }
               

                if (!isValid)
                {
                    ModelState.AddModelError("", "Invalid username or password");
                    return View(model);

                }

                else
                {
                    FormsAuthentication.SetAuthCookie(model.Username , model.RememberMe);
                    return Redirect(FormsAuthentication.GetRedirectUrl(model.Username, model.RememberMe));
                }
            }

           
        }


        // GET: account/Logout
        [Authorize]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return Redirect("~/account/login");
        }


        // GET: account/create-account
        [ActionName("create-account")]
        [HttpGet]
        public ActionResult CreateAccount()
        {

            return View("CreateAccount");
        }



        // Post: account/create-account
        [ActionName("create-account")]
        [HttpPost]
        public ActionResult CreateAccount(UserVM model)
        {
            //check model state
            if (!ModelState.IsValid)
            {
                return View("CreateAccount", model);
            }
            //check if password match

            if (!model.Password.Equals(model.ConfirmPassword))
            {
                ModelState.AddModelError("", "Password Does not Match");
                return View("CreateAccount", model);
            }
            using (Db db = new Db())
            {

                //make sure username is unique
                if (db.Users.Any(x => x.Username.Equals(model.Username)))
                {
                    ModelState.AddModelError("", "Username" + model.Username + "Is Taken");
                    model.Username = "";
                    return View("CreateAccount", model);
                }
                //create userDTO
                UserDTO userDTO = new UserDTO()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    EmailAddress = model.EmailAddress,
                    Username = model.Username,
                    Password = model.Password

                };
                //Add The DTO
                db.Users.Add(userDTO);
                //Save
                db.SaveChanges();
                //Add to UserRolesDTO
                int id = userDTO.Id;
                UserRoleDTO userRolesDTO = new UserRoleDTO()
                {
                    UserId = id,
                    RoleId=2

                };

                db.UserRoles.Add(userRolesDTO);
                db.SaveChanges();

            }
            //Create Temp Message
            TempData["Success Message"] = "You are now registered and can login";

            //redirect  
            return Redirect("~/account/login");
        }


        [Authorize]
        public ActionResult UserNavPartial()
        {

            //get the username
            string username = User.Identity.Name;
            //declare model
            UserNavPartialVM model;

            using (Db db = new Db())
            {

                //get the user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);
                //build the model
                model = new UserNavPartialVM()
                {
                    FirstName=dto.FirstName,
                    LastName=dto.LastName
                };
            }
            //return patrial view with  model
            return PartialView(model);
        }


        // GET: account/user-profile
        [HttpGet]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile()
        {

            //get username
            string username = User.Identity.Name;

            //declare model
            UserProfileVM model;

            using (Db db = new Db())
            {
                //get user
                UserDTO dto = db.Users.FirstOrDefault(x => x.Username == username);

                //build model
                model = new UserProfileVM(dto);
               
            }
            //return view with model
            return View("UserProfile",model);
        }



        // Post: account/user-profile
        [HttpPost]
        [ActionName("user-profile")]
        [Authorize]
        public ActionResult UserProfile(UserProfileVM model)
        {
            //check model state
            if (!ModelState.IsValid)
            {
                return View("UserProfile", model);
            }

            //check if password match if need be
            if (!string.IsNullOrWhiteSpace(model.Password))
            {
                if (!model.Password.Equals(model.ConfirmPassword))
                {
                     ModelState.AddModelError("", "Password Does not Match");
                         return View("UserProfile", model);
                }
               
            }
            using (Db db = new Db())
            {
                //get username
                string username = User.Identity.Name;
                //make sure username is unique
                if (db.Users.Where(x => x.Id != model.Id).Any(x=>x.Username==username))
                {
                    ModelState.AddModelError("", "Username" + model.Username + "Already Exist");
                    model.Username = "";
                    return View("UserProfile", model);
                }
                //Edit DTO
                UserDTO dto = db.Users.Find(model.Id);

                dto.FirstName = model.FirstName;
                dto.LastName = model.LastName;
                dto.EmailAddress = model.EmailAddress;
                dto.Username = model.Username;
                if (!string.IsNullOrWhiteSpace(model.Password))
                {
                    dto.Password = model.Password;

                }
                
                //Save
                db.SaveChanges();

            }
            //Set Temp Message
            TempData["Success Message"] = "You have edit your profile";


            //redirect
            return Redirect("~/account/user-profile");

        }



        // GET: account/Orders
        [Authorize(Roles ="User")]

        public ActionResult Orders()
        {
            //Intialize List Of OrdersForUserVM
            List<OrdersForUserVM> ordersForUser = new List<OrdersForUserVM>();

            using (Db db = new Db())
            {
                //get user id
                UserDTO user = db.Users.Where(x=>x.Username==User.Identity.Name).FirstOrDefault();
                int userId = user.Id;
                //Intialize List Of OrderVM
                List<OrderVM> orders = db.Orders.Where(x =>x.UserId==userId).ToArray().Select(x => new OrderVM(x)).ToList();

                //loop through List Of OrderVM
                foreach (var order in orders)
                {
                    //Intialize Product Dictionary
                    Dictionary<string, int> productsAndQty = new Dictionary<string, int>();
                    //declare total
                    decimal total = 0m;
                    //Intialize List Of OrderDetailsDTO
                    List<OrderDetailsDTO> orderDetailsDTO = db.OrderDetails.Where(x => x.OrderId == order.OrderId).ToList();
                    //loop through List Of OrderDetailsDTO
                    foreach (var orderDetails in orderDetailsDTO)
                    {

                        //Get The Product
                        ProductDTO product = db.Products.Where(x => x.Id == orderDetails.ProductId).FirstOrDefault();
                        //Get The Product Price
                        decimal price = product.Price;
                        //Get The Product Name
                        string productName = product.Name;
                        //Add to Product dictionary
                        productsAndQty.Add(productName, orderDetails.Quantity);
                        //Get Total
                        total += orderDetails.Quantity * price;

                    }

                    //Add to ordersforuserVM List
                    ordersForUser.Add(new OrdersForUserVM()
                    {

                        OrderNumber = order.OrderId,
                        Total = total,
                        ProductsAndQty = productsAndQty,
                        CreatedAt = order.CreatedAt

                    });
                }

            }

            //Return View With List Of OrdersForUserVM

            return View(ordersForUser);
        }



    }
}