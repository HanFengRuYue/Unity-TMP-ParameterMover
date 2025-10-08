using System.Collections.Generic;

namespace Unity_TMP_ParameterMover_WinUI.Models
{
    /// <summary>
    /// 文件处理结果
    /// </summary>
    public class ProcessResult
    {
        /// <summary>
        /// 处理是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 是否有参数变化
        /// </summary>
        public bool HasChanges { get; set; }

        /// <summary>
        /// 变更的字段列表
        /// </summary>
        public List<string> ChangedFields { get; set; } = new List<string>();

        /// <summary>
        /// 输出文件路径
        /// </summary>
        public string OutputFilePath { get; set; } = string.Empty;

        /// <summary>
        /// 错误信息
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;

        /// <summary>
        /// 原始文件名
        /// </summary>
        public string FileName { get; set; } = string.Empty;
    }
}
