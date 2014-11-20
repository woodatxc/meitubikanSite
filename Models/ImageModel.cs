using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace meitubikanSite.Models
{
    public class ImageModel
    {
        // All letters in container name must be lowercase
        public static readonly string ImageContainerName = "meitubikanimages";

        public string SaveTagImage(string imageUrl, string blobfileName)
        {
            // TODO: crawl image file.
            Stream imageFileStream = null;
            using (imageFileStream)
            {
                StorageModel.GetBlobContainer(ImageModel.ImageContainerName)
                            .GetBlockBlobReference(blobfileName)
                            .UploadFromStream(imageFileStream);
            }

            return Path.Combine(StorageModel.GetBlobEndPoint(), ImageModel.ImageContainerName, blobfileName);
        }
    }
}