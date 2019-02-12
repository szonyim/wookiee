using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using ExoticWookieeChat.Constants;
using ExoticWookieeChat.Models;
using ExoticWookieeChat.Util;
using ExoticWookieeChat.ViewModel;

namespace ExoticWookieeChat.Controllers
{
    public class UsersController : Controller
    {
        private DataContext db = new DataContext();

        // GET: Users
        public ActionResult Index()
        {
            return View(db.Users.ToList());
        }

        // GET: Users/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Users/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "DisplayName,UserName,Password,ConfirmPassword")] User user)
        {
            if (ModelState.IsValid)
            {
                if(user.Password == user.ConfirmPassword)
                {
                    String sha512Password = PasswordUtil.GenerateSHA512String(user.Password);
                    user.Password = sha512Password;
                    user.CreatedAt = DateTime.Now;
                    user.Role = UserRoleConstants.ROLE_EMPLOYEE;

                    db.Users.Add(user);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }

                return View(user);
            }

            return View(user);
        }

        // GET: Users/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            return View(user);
        }

        // POST: Users/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,DisplayName,UserName")] User user)
        {
            if (ModelState.IsValid)
            {
                db.Entry(user).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(user);
        }

        // GET: Users/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            User user = db.Users.Find(id);
            db.Users.Remove(user);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult ChangePassword(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            User user = db.Users.Find(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            PasswordChangeViewModel pcvm = new PasswordChangeViewModel()
            {
                UserId = user.Id
            };

            return View(pcvm);
        }

        [HttpPost, ActionName("ChangePassword")]
        public ActionResult ChangePassword([Bind(Include = "UserId, OldPassword, NewPassword, ConfirmNewPassword")]PasswordChangeViewModel pcvm)
        {
            User user = db.Users.Find(pcvm.UserId);

            if(user == null)
            {
                return HttpNotFound();
            }

            if (ModelState.IsValid && pcvm.ValidateOldaPassword(user.Password) && pcvm.ValidateNewPassword())
            {
                user.Password = pcvm.GetHashedNewPassword();
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(pcvm);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
