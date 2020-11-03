﻿using System.Linq;
using System.Web;
using System.Web.Helpers;

namespace MVC_CSS_EF
{
    public static class ImgFileUploadHelper
    {
        public static bool ValidateFile(HttpPostedFileBase file)
        {
            string fileExtension = System.IO.Path.GetExtension(file.FileName).ToLower();
            
            string[] allowedFileTypes = { ".gif", ".png", ".jpg", ".jpeg" };
            if ((file.ContentLength > 0 && file.ContentLength < 2097152) && allowedFileTypes.Contains(fileExtension))
            {
                return true;
            }
            return false;
        }

        public static void SaveFileToDisk (HttpPostedFileBase file)
        {
            WebImage img = new WebImage(file.InputStream);
            string fileName = System.IO.Path.GetFileName(file.FileName);
            if (img.Width > 190)
            {
                img.Resize(190, img.Height);
            }

            img.Save(System.IO.Path.Combine(Constants.ProductImagePath, fileName));

            if (img.Width > 100)
            {
                img.Resize(100, img.Height);
            }

            img.Save(System.IO.Path.Combine(Constants.ProductThumbnailPath, fileName));
        }
    }
}