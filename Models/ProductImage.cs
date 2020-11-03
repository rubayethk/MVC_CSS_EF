using System.Collections.Generic;

namespace MVC_CSS_EF.Models
{
    public partial class ProductImage
    {
        public int ID { get; set; }
        public string FileName { get; set; }

        public virtual ICollection<ProductImageMapping> ProductImageMappings { get; set; }

    }
}