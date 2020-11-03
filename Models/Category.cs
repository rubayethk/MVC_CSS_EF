using System.Collections.Generic;


namespace MVC_CSS_EF.Models
{
    public partial class Category
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Product> Products { get; set; }

    }
}