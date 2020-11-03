using MVC_CSS_EF.Models;
using PagedList;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;


namespace MVC_CSS_EF.ViewModel
{
    public class ViewModels
    {

    }

    public class ProductIndexViewModel
    {
        public IPagedList<Product> Products { get; set; }
        public string Search { get; set; }

        public IEnumerable<CategoryWithCount> CatsWithCount { get; set; }
        public string Category { get; set; }
        public string SortBy { get; set; }
        public Dictionary<string,string> Sorts { get; set; }

        public IEnumerable<SelectListItem>  CatFilterItems 
        { get
            {
                var allCats = CatsWithCount.Select(cc => new SelectListItem
                {
                    Value = cc.CategoryName,
                    Text = cc.CatNameWithCount

                });

                return allCats;
            }
        
        
        }
    }

    public class ProductCreateViewModel
    {
        public Product Product { get; set; }
        public string Category { get; set; }
    }

    public partial class ProductViewModel
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public int CategoryID { get; set; }
        public SelectList CategoryList { get; set; }
        public List<SelectList> ImageLists { get; set; }
        public string[] ProductImages { get; set; }
    }
    public class CategoryWithCount
    {
        public int ProductCount { get; set; }
        public string CategoryName { get; set; }
        public string CatNameWithCount 
        { 
            get 
            {
                return CategoryName + " (" + ProductCount.ToString() + ")"; 
            } 
        }
    }
}