using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fp_Part.Fp_DTOModel.Fp_Model;

namespace Fp_Part
{
    public class Login
    {
        NameAndPwd up = new NameAndPwd();
        public Login(string username, string password)
        {
            up.UserName = username;
            up.PassWord = password;
        }
        public bool CheckLogin()
        {
            bool result = false;
            Dictionary<string, string> dicData = new Dictionary<string, string>();
            dicData.Add("username", up.UserName);
            dicData.Add("password", up.PassWord);
            dicData.Add("method", "gen_token");
            ICallApi api = new CallApi();
            api.PostData = Newtonsoft.Json.JsonConvert.SerializeObject(dicData);
            string strResult = api.Post();
            if (!string.IsNullOrEmpty(strResult) && strResult.Contains("auth_token"))
            {
                result = true;
                Fp_Common.SessionHelper.SetSession(up.GetType().Name.ToString() + "_" + up.UserName, dicData);
            }
            else
            {
                Fp_Common.SessionHelper.Del(up.GetType().Name.ToString() + "_" + up.UserName);
            }
            return result;
        }
    }
}
