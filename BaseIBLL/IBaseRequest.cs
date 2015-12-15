using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseIBLL
{
    public interface IBaseRequest
    {
        /// <summary>
        /// 样品源类型
        /// </summary>
        string SampleSourceType { get; set; }
        /// <summary>
        /// 根据样品源类型获取对象模型
        /// </summary>
        /// <returns></returns>
        List<IBaseModel> GetResquestModel();
    }
}
