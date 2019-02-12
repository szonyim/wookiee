using ExoticWookieeChat.Models;
using ExoticWookieeChat.ViewModel;
using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace ExoticWookieeChat.Controllers
{
    /// <summary>
    /// Authenticate the user
    /// </summary>
    public class AuthController : Controller
    {
        private readonly DataContext dc;

        public AuthController()
        {
            dc = DataContext.CreateContext();
        }

        [HttpGet]
        public ActionResult Login()
        {
            return View(new LoginViewModel());
        }

        [HttpPost]
        public ActionResult Login(LoginViewModel loginViewModel)
        {
            bool LoginError = false;

            try
            {
                if (ModelState.IsValid)
                {
                    using (DataContext dc = DataContext.CreateContext())
                    {
                        string hashedPwd = loginViewModel.GetSHA512Password();
                        User user = dc.Users.FirstOrDefault(e => e.UserName == loginViewModel.UserName && e.Password == hashedPwd);

                        if (user != null)
                        {
                            FormsAuthenticationTicket authTicket =
                            new FormsAuthenticationTicket(1, loginViewModel.UserName, DateTime.Now, DateTime.Now.AddMinutes(60), false, user.Role);

                            string encryptedTicket = FormsAuthentication.Encrypt(authTicket);
                            HttpCookie authCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);

                            Response.Cookies.Add(authCookie);
                            Response.Redirect(FormsAuthentication.GetRedirectUrl(loginViewModel.UserName, false));
                        }
                        else
                        {
                            LoginError = true;
                        }
                    }
                }
                else
                {
                    LoginError = true;
                }
            }catch(Exception e)
            {
                LoginError = true;
            }

            ViewBag.LoginError = LoginError;
            return View(loginViewModel);
        }

        [HttpGet]
        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Home");
        }
    }
}