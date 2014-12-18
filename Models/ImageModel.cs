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
        public static int MinimumImageSizeInBytes = 1024;

        public string SaveImage(string encodedImageUrl)
        {
            try
            {
                string decodedImageUrl = StorageModel.UrlDecode(encodedImageUrl);
                var imageBlob = StorageModel.GetBlobContainer(StorageModel.ImageContainerName).GetBlockBlobReference(decodedImageUrl);
                if (!imageBlob.Exists())
                {
                    HttpWebRequest imageRequest = (HttpWebRequest)WebRequest.Create(decodedImageUrl);
                    WebResponse imageResponse = imageRequest.GetResponse();
                    Stream imageFileStream = imageResponse.GetResponseStream();

                    using (imageFileStream)
                    {
                        imageBlob.UploadFromStream(imageFileStream);
                        //imageBlob.Properties.ContentType = "image/jpeg";
                    }
                }

                // Too small image may has issue, delete it
                if (imageBlob.StreamWriteSizeInBytes < MinimumImageSizeInBytes)
                {
                    imageBlob.Delete();
                    return string.Empty;
                }

                return Path.Combine(StorageModel.GetBlobEndPoint(), StorageModel.ImageContainerName, decodedImageUrl);
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        // Get image from blob
        public MemoryStream GetImageFromBlob(string encodedImageUrl)
        {
            MemoryStream ms = new MemoryStream();
            string decodedImageUrl = StorageModel.UrlDecode(encodedImageUrl);
            var imageBlob = StorageModel.GetBlobContainer(StorageModel.ImageContainerName).GetBlockBlobReference(decodedImageUrl);
            imageBlob.DownloadToStream(ms);
            return ms;
        }
    }
}