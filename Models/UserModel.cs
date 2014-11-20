using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace meitubikanSite.Models
{
    public class UserModel
    {
        public static readonly string UserTagTableName = "UserTag";
    }
    /*
    public class UserTagEntity : TableEntity
    {
        public UserTagEntity(string clientID, string url)
        {

        }
    }
    */
}