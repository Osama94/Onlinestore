using CmsShoppingCart.Models.Cart;
using CmsShoppingCart.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;

namespace CmsShoppingCart.Controllers
{
    public class CartController : Controller
    {
        // GET: Cart
        public ActionResult Index()
        {
            //Initialize cart list
            var cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();
            //Check IF Cart Is Empty
            if (cart.Count==0 || Session["cart"]==null)
            {
                ViewBag.Message = "Your Cart Is Empty";
                return View();
            }
            //Calculate Total And Save To ViewBag
            decimal total = 0m;

            foreach (var item in cart)
            {
                total += item.Total;
            }
            ViewBag.GrandTotal = total;
            //Return View With List

            return View(cart);
        }


        public ActionResult CartPartial()
        {

            //Initialize CartVm
            CartVM model = new CartVM();
            //Initialize Quantity
            int qty = 0;
            //Initialize Price
            decimal price = 0m;
            //Check For Cart Session
            if (Session["cart"] !=null)
            {
                //Get Total Quantity And Price
                var list = (List<CartVM>)Session["cart"];

                foreach (var item in list)
                {
                    qty += item.Quantity;
                    price += item.Quantity * item.Price;
                }
                model.Quantity = qty;
                model.Price = price;
            }

            else
            {
                //Or Set Quantity And Price to 0
                model.Quantity = 0;
                model.Price = 0m;
            }
            //Return Partial View With Model
            return PartialView(model);
        }

        public ActionResult AddToCartPartial(int id)
        {
            //Initialize CartVm List
            List<CartVM> cart = Session["cart"] as List<CartVM> ?? new List<CartVM>();

            //Initialize CartVm
            CartVM model = new CartVM();

            using(Db db = new Db())
            {

                //Get The Product
                ProductDTO product = db.Products.Find(id);
                //Check If Product Already In Cart
                var productInCart = cart.FirstOrDefault( x=>x.ProductId==id);
                //If Not , Add New
                if (productInCart==null)
                {
                    cart.Add(new CartVM()
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        Quantity = 1,
                        Price=product.Price,
                        Image=product.ImageName
                       
                    });
                }
                else
                {
                    //if it is, increment
                    productInCart.Quantity++;
                }
            }

            //get toatal qty and price and add to model
            int qty = 0;
            decimal price = 0m;

            foreach (var item in cart)
            {
                qty += item.Quantity;
                price += item.Quantity * item.Price;
            }
            model.Quantity = qty;
            model.Price = price;
            //save cart back to session
            Session["cart"] = cart;
            //Return Partial View With Model

            return PartialView(model);
        }

        // GET: /Cart/IncrementProduct

        public JsonResult IncrementProduct(int productId)
        {

            //Initialize Cart List
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {

                //Get CartVM Form List
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                //Increment Qty
                model.Quantity++;
                //store neede data
                var result = new { qty=model.Quantity , price=model.Price};
                //return json with data
                return Json(result,JsonRequestBehavior.AllowGet);
          }

        }



        // GET: /Cart/DecrementProduct
        public JsonResult DecrementProduct(int productId)
        {

            //Initialize Cart List
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {

                //Get CartVM Form List
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                //Decrement Qty
                if (model.Quantity>1)
                {
                    model.Quantity--;

                }
                else
                {
                    model.Quantity = 0;
                    cart.Remove(model);
                }
                //store neede data
                var result = new { qty = model.Quantity, price = model.Price };
                //return json with data
                return Json(result, JsonRequestBehavior.AllowGet);
            }

        }



        // GET: /Cart/RemoveProduct
        public void RemoveProduct(int productId)
        {

            //Initialize Cart List
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            using (Db db = new Db())
            {

                //Get CartVM Form List
                CartVM model = cart.FirstOrDefault(x => x.ProductId == productId);
                //Remove model from list
               
                    cart.Remove(model);
                }
               
            

        }



        public ActionResult PaybalPartial()
        {

            List<CartVM> cart = Session["cart"] as List<CartVM>;


            return PartialView(cart);
        }


        // POST: /Cart/PlaceOrder
        [HttpPost]
        public void PlaceOrder()
        {
            //get cart list
            List<CartVM> cart = Session["cart"] as List<CartVM>;

            //get username
            string username = User.Identity.Name;

            int orderId = 0;
            using (Db db = new Db())
            {

                //initialize orderDTO
                OrderDTO orderDTO = new OrderDTO();

                //get userid
                var q = db.Users.FirstOrDefault(x=>x.Username==username);
                int userId = q.Id;
                //add to orderDTO and save
                orderDTO.UserId = userId;
                orderDTO.CreatedAt = DateTime.Now;

                db.Orders.Add(orderDTO);
                db.SaveChanges();
                //get inserted id
                 orderId = orderDTO.OrderId;
                //initialize OrderDeatailsDTO
                OrderDetailsDTO orderDetailsDTO = new OrderDetailsDTO();
                //add to OrderDeatailsDTO
                foreach (var item in cart)
                {
                    orderDetailsDTO.OrderId = orderId;
                    orderDetailsDTO.UserId = userId;
                    orderDetailsDTO.ProductId = item.ProductId;
                    orderDetailsDTO.Quantity = item.Quantity;

                    db.OrderDetails.Add(orderDetailsDTO);
                    db.SaveChanges();
                }
            }


            //Email admin

            var client = new SmtpClient("mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("21f57cbb94cf88", "e9d7055c69f02d"),
                EnableSsl = true
            };
            client.Send("admin@example.com", "admin@example.com", "New Order", "You have new order, order number is:"+ orderId );

            //reset session
            Session["cart"] = null;

        }
    }
}