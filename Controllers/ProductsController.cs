﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using MVC_CSS_EF.DAL;
using MVC_CSS_EF.Models;
using MVC_CSS_EF.ViewModel;
using PagedList;

namespace MVC_CSS_EF.Controllers
{
    public class ProductsController : Controller
    {
        private StoreContext db = new StoreContext();

        // GET: Products
        public ActionResult Index(string category, string search, string sortBy, int? page)
        {
            ProductIndexViewModel viewmodel = new ProductIndexViewModel();

            var products = db.Products.Include(p => p.Category);
    
            if (!String.IsNullOrEmpty(search))
            {
                products = products.Where(p => p.Name.Contains(search) 
                                                    || p.Description.Contains(search) 
                                                    || p.Category.Name.Contains(search));
                viewmodel.Search = search;
            }

            //viewmodel.CatsWithCount = from matchingProducts in products 
            //                          where matchingProducts.CategoryID != null 
            //                          group matchingProducts by matchingProducts.Category.Name 
            //                          into catGroup 
            //                          select new CategoryWithCount() { CategoryName = catGroup.Key, ProductCount = catGroup.Count() };

            viewmodel.CatsWithCount = products.Where(p => p.CategoryID != null).GroupBy(p => p.Category.Name).Select(p => new CategoryWithCount() { CategoryName = p.Key, ProductCount = p.Count() });

            if (!String.IsNullOrEmpty(category))
            {
                products = products.Where(p => p.Category.Name == category);
                viewmodel.Category = category;
            }

            viewmodel.Sorts = new Dictionary<string, string>
            {
                {"Price low to high", "price_lowest" },
                {"Price high to low", "price_highest" }
            };

            switch (sortBy)
            {
                case "price_lowest":
                    products = products.OrderBy(p => p.Price);
                    break;
                case "price_highest":
                    products = products.OrderByDescending(p => p.Price);
                    break;
                default:
                    products = products.OrderBy(p => p.Name);
                    break;
            }

            
            int currentpage = (page ?? 1);
            viewmodel.Products = products.ToPagedList(currentpage, Constants.PageItems);

            return View(viewmodel);
        }

        // GET: Products/Details/5 
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create
        public ActionResult Create(string category)
        {
            ViewModel.ProductViewModel viewmodel = new ViewModel.ProductViewModel();
            viewmodel.CategoryList = new SelectList(db.Categories, "ID", "Name");

            viewmodel.ImageLists = new List<SelectList>();

            for (int i = 0; i < Constants.NumberOfProductImages; i++)
            {
                viewmodel.ImageLists.Add(new SelectList(db.ProductImages, "ID", "FileName"));
            }

            return View(viewmodel);
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ViewModel.ProductViewModel viewModel)
        {
            Product product = new Product();
            product.Name = viewModel.Name;
            product.Description = viewModel.Description;
            product.Price = viewModel.Price;
            product.CategoryID = viewModel.CategoryID;
            product.ProductImageMappings = new List<ProductImageMapping>();

            string[] productImages = viewModel.ProductImages.Where(pi => !string.IsNullOrEmpty(pi)).ToArray();

            for (int i = 0; i < productImages.Length; i++)
            {
                product.ProductImageMappings.Add(new ProductImageMapping
                {
                    ProductImage = db.ProductImages.Find(int.Parse(productImages[i])),
                    ImageNumber = i
                });
            }


            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "ID", "Name", product.CategoryID);
            return View(product);
        }

        // GET: Products/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }

            ProductViewModel viewModel = new ProductViewModel();
            viewModel.CategoryList = new SelectList(db.Categories, "ID", "Name", product.CategoryID);
            viewModel.ImageLists = new List<SelectList>();

            foreach(var imageMapping in product.ProductImageMappings.OrderBy(pim=> pim.ImageNumber))
            {
                viewModel.ImageLists.Add(new SelectList(db.ProductImages, "ID", "FileName", imageMapping.ProductImageID));
            }

            for (int i = viewModel.ImageLists.Count; i < Constants.NumberOfProductImages; i++)
            {
                viewModel.ImageLists.Add(new SelectList(db.ProductImages, "ID", "FileName"));
            }

            viewModel.ID = product.ID;
            viewModel.Name = product.Name;
            viewModel.Description = product.Description;
            viewModel.Price = product.Price;

            return View(viewModel);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(ProductViewModel viewModel)
        {
            var productToUpdate = db.Products.Include(p => p.ProductImageMappings).Where(p => p.ID == viewModel.ID).Single();
            
            if (TryUpdateModel(productToUpdate,"", new string[] { "Name", "Description", "Price", "CategoryID"}))
            {
                if (productToUpdate.ProductImageMappings == null) 
                { 
                    productToUpdate.ProductImageMappings = new List<ProductImageMapping>(); 
                }                  
                //get a list of selected images without any blanks                 
                string[] productImages = viewModel.ProductImages.Where(pi => !string.IsNullOrEmpty(pi)).ToArray();
                for (int i = 0; i < productImages.Length; i++)   
                {
                    //get the image currently stored              
                    var imageMappingToEdit = productToUpdate.ProductImageMappings.Where(pim => pim.ImageNumber == i).FirstOrDefault();
                    //find the new image              
                    var image = db.ProductImages.Find(int.Parse(productImages[i]));
                    //if there is nothing stored then we need to add a new mapping                
                    if (imageMappingToEdit == null)
                    {
                        //add image to the imagemappings              
                        productToUpdate.ProductImageMappings.Add(new ProductImageMapping
                        {
                            ImageNumber = i,
                            ProductImage = image,
                            ProductImageID = image.ID
                        });
                    }
                    //else it's not a new file so edit the current mapping                
                    else
                    {
                        //if they are not the same              
                        if (imageMappingToEdit.ProductImageID != int.Parse(productImages[i]))
                        {
                            //assign image property of the image mapping              
                            imageMappingToEdit.ProductImage = image;
                        }
                    }
                }
                for (int i = productImages.Length; i < Constants.NumberOfProductImages; i++)
                {
                    var imageMappingToEdit = productToUpdate.ProductImageMappings.Where(pim =>
                        pim.ImageNumber == i).FirstOrDefault();
                    //if there is something stored in the mapping              
                    if (imageMappingToEdit != null)
                    {
                        //delete the record from the mapping table directly.              
                        //just calling productToUpdate.ProductImageMappings.Remove(imageMappingToEdit)  
                        //results in a FK error              
                        db.ProductImageMappings.Remove(imageMappingToEdit);
                    }
                }

                db.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(viewModel);



            //if (ModelState.IsValid)
            //{
            //    db.Entry(viewModel).State = EntityState.Modified;
            //    db.SaveChanges();
            //    return RedirectToAction("Index");
            //}

            return View(viewModel);
        }

        // GET: Products/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
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
