using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MVC_CSS_EF.DAL;
using MVC_CSS_EF.Models;

namespace MVC_CSS_EF.Controllers
{
    public class ProductImagesController : Controller
    {
        private StoreContext db = new StoreContext();

        // GET: ProductImages
        public ActionResult Index()
        {
            return View(db.ProductImages.ToList());
        }

        // GET: ProductImages/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductImage productImage = db.ProductImages.Find(id);
            if (productImage == null)
            {
                return HttpNotFound();
            }
            return View(productImage);
        }

        // GET: ProductImages/Create
        public ActionResult Upload()
        {
            return View();
        }

        // POST: ProductImages/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Upload(HttpPostedFileBase[] files)
        {
            bool allValid = true;
            string inValidFiles = "";
            db.Database.Log = sql => Trace.WriteLine(sql);
            //check the user has entered a file
            if (files[0] != null)
            {
                //if the user has entered less than ten files 
                if (files.Length < 10)
                {
                    foreach (var file in files)
                    {
                        if (!ImgFileUploadHelper.ValidateFile(file))
                        {
                            allValid = false;
                            inValidFiles += ", " + file.FileName;
                        }
                    }
                    //Save if all the files are valid
                    if (allValid)
                    {
                        foreach (var file in files)
                        {
                            try
                            {
                                ImgFileUploadHelper.SaveFileToDisk(file);
                            }
                            catch (Exception ex)
                            {
                                ModelState.AddModelError("FileName", "Sorry an error occurred saving the files to disk, please try again");
                            }
                        }
                    }
                    else //show error that there are invalid files selected
                    {
                        ModelState.AddModelError("FileName", "All files must be gif, png, jpeg or jpg and less than 2MB in size.The following files" + inValidFiles + " are not valid");
                    }

                }
                //user has entered more than 10 files
                else
                {
                    ModelState.AddModelError("FileName", "Please only upload upto ten files at a time");
                }
            }
            else
            {
                //if the user has not entered a file return an error message
                ModelState.AddModelError("FileName", "Please choose a file");
            }

            if (ModelState.IsValid)
            {
                bool duplicates = false;
                bool otherDbError = false;
                string duplicateFiles = "";

                foreach (var file in files)
                {
                    var productToAdd = new ProductImage { FileName = file.FileName };

                    try
                    {
                        db.ProductImages.Add(productToAdd);
                        db.SaveChanges();
                    }
                    catch (DbUpdateException ex)
                    {
                        SqlException innerException = ex.InnerException.InnerException as SqlException;
                        if (innerException != null && innerException.Number == 2601)
                        {
                            duplicateFiles += ", " + file.FileName;
                            duplicates = true;
                            //have to remove the duplicated file from the dbcontext
                            db.Entry(productToAdd).State = EntityState.Detached;
                        }
                        else
                        {
                            otherDbError = true;
                        }
                    }
                }
                if (duplicates)
                {
                    ModelState.AddModelError("FileName", "All files uploaded except the files" + duplicateFiles + ", which already exist in the system." + " Please delete them and try again if you wish to re - add them");
                    return View();
                }
                else if (otherDbError)
                {
                    ModelState.AddModelError("FileName", "Sorry an error has occurred saving to the  database, please try again");
                    return View();
                }
                return RedirectToAction("Index");
            }

            return View();
        }

        // GET: ProductImages/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductImage productImage = db.ProductImages.Find(id);
            if (productImage == null)
            {
                return HttpNotFound();
            }
            return View(productImage);
        }

        // POST: ProductImages/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,FileName")] ProductImage productImage)
        {
            if (ModelState.IsValid)
            {
                db.Entry(productImage).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(productImage);
        }

        // GET: ProductImages/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            ProductImage productImage = db.ProductImages.Find(id);
            if (productImage == null)
            {
                return HttpNotFound();
            }
            return View(productImage);
        }

        // POST: ProductImages/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            ProductImage productImage = db.ProductImages.Find(id);
            //find all the mappings for this image    
            var mappings = productImage.ProductImageMappings.Where(pim => pim.ProductImageID == id);
            foreach (var mapping in mappings)                
            {                      
                //find all mappings for any product containing this image      
                var mappingsToUpdate = db.ProductImageMappings.Where(pim => pim.ProductID ==  mapping.ProductID); 
                //for each image in each product change its imagenumber to one lower if it is higher         
                //than the current image        
                foreach (var mappingToUpdate in mappingsToUpdate)    
                {   if (mappingToUpdate.ImageNumber > mapping.ImageNumber)  
                    {                              
                        mappingToUpdate.ImageNumber--;                          
                    }                     
                }                
            }         
            System.IO.File.Delete(Request.MapPath(Constants.ProductImagePath + productImage.FileName)); 
            System.IO.File.Delete(Request.MapPath(Constants.ProductThumbnailPath + productImage.FileName)); 
            db.ProductImages.Remove(productImage); 
            db.SaveChanges(); return RedirectToAction("Index");
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
