using System.Collections.Generic;

namespace MVC_CSS_EF.Models
{
    public partial class Product
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public decimal Price { get; set; }

        public int? CategoryID { get; set; }

        public virtual ICollection<ProductImageMapping> ProductImageMappings { get; set; }

        public virtual Category Category { get; set; }
    }


}