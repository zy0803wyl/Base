using FpUtility.Fp_Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FpUtility
{
    internal  class Post
    {
        internal virtual string GetAll(Dictionary<string, string> dataDic, string method)
        {
            Fp_Model.NameAndPwd up = new Fp_Model.NameAndPwd();
            string result = string.Empty;
            try
            {
                up = Fp_Common.SessionHelper.GetSession(up.GetType().Name) as Fp_Model.NameAndPwd;
            }
            catch (Exception)
            {
                throw;
            }
            if (!string.IsNullOrEmpty(up.UserName) && !string.IsNullOrEmpty(up.PassWord))
            {
                Dictionary<string, string> temDic = new Dictionary<string, string>();
                temDic.Add("username", up.UserName);
                temDic.Add("password", up.PassWord);
                temDic.Add("method", method);
                foreach (KeyValuePair<string, string> item in dataDic)
                {
                    if (!temDic.ContainsKey(item.Key))
                    {
                        temDic.Add(item.Key.Trim(), item.Value.Trim());
                    }
                }
                CallApi api = new CallApi(temDic);
                result = api.PostData();
            }
      
            return "";
        }
    }

}
