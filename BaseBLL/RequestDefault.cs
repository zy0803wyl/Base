using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseBLL
{
    public class RequestDefault : BaseIBLL.IBaseRequest
    {
        public string SampleSourceType
        {
            get;
            set;
        }
        public List<BaseIBLL.IBaseModel> GetResquestModel()
        {
            List<ResquestDefaultModel> defaultModelList = new List<ResquestDefaultModel>();
            return null;
        }
    }
}
