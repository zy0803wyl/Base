using Fp_Part.Fp_DTOModel.Fp_Model;
using System;
using System.Collections.Generic;

namespace Fp_Part
{
    internal class Post
    {
        internal virtual string GetAll(Dictionary<string, string> dataDic, Fp_Common.FpMethod method)
        {
            NameAndPwd up = new NameAndPwd();
            string result = string.Empty;
            try
            {
                up = Fp_Common.SessionHelper.GetSession(up.GetType().Name) as NameAndPwd;
                if (!string.IsNullOrEmpty(up.UserName) && !string.IsNullOrEmpty(up.PassWord))
                {
                    Dictionary<string, string> temDic = new Dictionary<string, string>();
                    temDic.Add("username", up.UserName);
                    temDic.Add("password", up.PassWord);
                    temDic.Add("method", method.ToString());
                    if (dataDic != null && dataDic.Count > 0)
                    {
                        foreach (KeyValuePair<string, string> item in dataDic)
                        {
                            if (!temDic.ContainsKey(item.Key))
                            {
                                temDic.Add(item.Key.Trim(), item.Value.Trim());
                            }
                        }
                    }
                    CallApi api = new CallApi();
                    api.PostData = Newtonsoft.Json.JsonConvert.SerializeObject(temDic);
                    result = api.Post();
                }
            }
            catch (Exception)
            {
                throw;
            }
            return result;
        }
    }
}