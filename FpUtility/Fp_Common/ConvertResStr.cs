using System.Text;

namespace FpUtility.Fp_Common
{
    /// <summary>
    /// 将返回的数据转换成中文
    /// </summary>
    public class ConvertResStr
    {
        public static string ConvertRes(string resStr)
        {
            StringBuilder result = new StringBuilder();
            if (string.IsNullOrEmpty(resStr))
            {
                return resStr;
            }
            else
            {
                result = result.Append(resStr);
                result = result.Replace("method", "方法");
                result = result.Replace("is not found.", "没有找到").Replace("parameter is missing", "参数没找到");
                result = result.Replace("Test Data Type", "临床数据类型");
                result = result.Replace("Sample Source Type", "样品源类型");
                result = result.Replace("Sample Type", "样品类型");
                result = result.Replace("Please specify Box Type. Example: box_type='10 x 10'", "请指定样品和类型比如：10x10");
                result = result.Replace("Box could not be found", "没找到盒子");
                result = result.Replace("Container could not be found", "存储结构没找到");
                result = result.Replace("Not enough permission", "没有足够的权限");
                result = result.Replace("does not exist in your database", "不在数据库中");
                result = result.Replace("should be unique", "必须唯一");
                result = result.Replace("The value for User Field", "这个字段的值：");
                return result.ToString().Trim();
            }
        }
    }
}