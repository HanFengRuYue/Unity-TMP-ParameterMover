using System.Text.Json;
using System.Text.RegularExpressions;

namespace Unity_TMP_ParameterMover_WinUI.Utilities
{
    /// <summary>
    /// 自定义 JSON 格式化工具
    /// </summary>
    public class CustomJsonFormatting
    {
        /// <summary>
        /// JSON 序列化选项
        /// </summary>
        public static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never
        };

        /// <summary>
        /// 清理JSON字符串中空数组的多余空格
        /// </summary>
        /// <param name="json">原始JSON字符串</param>
        /// <returns>清理后的JSON字符串</returns>
        public static string CleanEmptyArrays(string json)
        {
            // 使用正则表达式去掉空数组中的所有空白字符（包括空格、制表符、换行符）
            // 匹配 [ 任意空白字符 ] 并替换为 []
            return Regex.Replace(json, @"\[\s*\]", "[]", RegexOptions.Multiline);
        }
    }
}
