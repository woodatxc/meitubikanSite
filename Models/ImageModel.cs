using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;

namespace meitubikanSite.Models
{
    public class ImageModel
    {
        public string SaveImage(string encodedImageUrl)
        {
            try
            {
                string decodedImageUrl = StorageModel.UrlDecode(encodedImageUrl);
                HttpWebRequest imageRequest = (HttpWebRequest)WebRequest.Create(decodedImageUrl);
                WebResponse imageResponse = imageRequest.GetResponse();
                Stream imageFileStream = imageResponse.GetResponseStream();

                using (imageFileStream)
                {
                    var imageBlob = StorageModel.GetBlobContainer(StorageModel.ImageContainerName).GetBlockBlobReference(decodedImageUrl);
                    imageBlob.UploadFromStream(imageFileStream);
                    //imageBlob.Properties.ContentType = "image/jpeg";
                }

                return Path.Combine(StorageModel.GetBlobEndPoint(), StorageModel.ImageContainerName, decodedImageUrl);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}