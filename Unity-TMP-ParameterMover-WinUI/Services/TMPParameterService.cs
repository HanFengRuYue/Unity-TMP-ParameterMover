using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Unity_TMP_ParameterMover_WinUI.Models;
using Unity_TMP_ParameterMover_WinUI.Utilities;

namespace Unity_TMP_ParameterMover_WinUI.Services
{
    /// <summary>
    /// TMP 参数处理服务
    /// </summary>
    public class TMPParameterService
    {
        /// <summary>
        /// 进度报告委托
        /// </summary>
        public Action<string>? OnLogMessage { get; set; }

        /// <summary>
        /// 处理单个文件
        /// </summary>
        /// <param name="originFile">原始文件路径</param>
        /// <param name="fromFile">来源文件路径</param>
        /// <returns>处理结果</returns>
        public ProcessResult ProcessSingleFile(string originFile, string fromFile)
        {
            var result = new ProcessResult
            {
                FileName = Path.GetFileName(originFile)
            };

            try
            {
                // 检查文件是否存在
                if (!File.Exists(originFile))
                {
                    result.Success = false;
                    result.ErrorMessage = $"原始文件不存在: {originFile}";
                    OnLogMessage?.Invoke($"  ✗ 错误: {result.ErrorMessage}");
                    return result;
                }

                if (!File.Exists(fromFile))
                {
                    result.Success = false;
                    result.ErrorMessage = $"来源文件不存在: {fromFile}";
                    OnLogMessage?.Invoke($"  ✗ 错误: {result.ErrorMessage}");
                    return result;
                }

                // 读取JSON文件
                string originJson;
                string fromJson;

                try
                {
                    originJson = File.ReadAllText(originFile);
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"无法读取原始文件: {ex.Message}";
                    OnLogMessage?.Invoke($"  ✗ 错误: {result.ErrorMessage}");
                    return result;
                }

                try
                {
                    fromJson = File.ReadAllText(fromFile);
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"无法读取来源文件: {ex.Message}";
                    OnLogMessage?.Invoke($"  ✗ 错误: {result.ErrorMessage}");
                    return result;
                }

                // 解析JSON
                JsonObject originFileNode;
                JsonObject fromFileNode;

                try
                {
                    originFileNode = JsonNode.Parse(originJson)!.AsObject();
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"原始文件JSON格式无效: {ex.Message}";
                    OnLogMessage?.Invoke($"  ✗ 错误: {result.ErrorMessage}");
                    return result;
                }

                try
                {
                    fromFileNode = JsonNode.Parse(fromJson)!.AsObject();
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"来源文件JSON格式无效: {ex.Message}";
                    OnLogMessage?.Invoke($"  ✗ 错误: {result.ErrorMessage}");
                    return result;
                }

                // 用于记录变化的信息
                List<string> changedFields = new List<string>();

                // 处理 m_fontInfo
                JsonObject? fromFile_m_fontInfoNode = fromFileNode["m_fontInfo"]?.AsObject();
                JsonObject? originFile_m_fontInfoNode = originFileNode["m_fontInfo"]?.AsObject();
                if (fromFile_m_fontInfoNode != null && originFile_m_fontInfoNode != null)
                {
                    var fieldsToMove = GetFontInfoFields(fromFile_m_fontInfoNode);
                    var movedFields = MoveFieldsInObjectNodeWithTracking(fromFile_m_fontInfoNode, originFile_m_fontInfoNode, fieldsToMove);
                    changedFields.AddRange(movedFields.Select(f => $"m_fontInfo.{f}"));
                }

                // 处理 m_glyphInfoList
                JsonObject? fromFile_m_glyphInfoList = fromFileNode["m_glyphInfoList"]?.AsObject();
                JsonObject? originFile_m_glyphInfoList = originFileNode["m_glyphInfoList"]?.AsObject();
                if (fromFile_m_glyphInfoList != null && originFile_m_glyphInfoList != null)
                {
                    var fieldsToMove = GetAllFieldsInNode(fromFile_m_glyphInfoList);
                    var movedFields = MoveFieldsInObjectNodeWithTracking(fromFile_m_glyphInfoList, originFile_m_glyphInfoList, fieldsToMove);
                    changedFields.AddRange(movedFields.Select(f => $"m_glyphInfoList.{f}"));
                }

                // 处理 m_FaceInfo
                JsonObject? fromFile_m_FaceInfo = fromFileNode["m_FaceInfo"]?.AsObject();
                JsonObject? originFile_m_FaceInfo = originFileNode["m_FaceInfo"]?.AsObject();
                if (fromFile_m_FaceInfo != null && originFile_m_FaceInfo != null)
                {
                    var fieldsToMove = GetFieldsFaceInfo(fromFile_m_FaceInfo);
                    var movedFields = MoveFieldsInObjectNodeWithTracking(fromFile_m_FaceInfo, originFile_m_FaceInfo, fieldsToMove);
                    changedFields.AddRange(movedFields.Select(f => $"m_FaceInfo.{f}"));
                }

                // 处理 m_AtlasWidth 和 m_AtlasHeight
                if (fromFileNode["m_AtlasWidth"] != null && !JsonNode.DeepEquals(fromFileNode["m_AtlasWidth"], originFileNode["m_AtlasWidth"]))
                {
                    originFileNode["m_AtlasWidth"] = fromFileNode["m_AtlasWidth"]?.DeepClone();
                    changedFields.Add("m_AtlasWidth");
                }
                if (fromFileNode["m_AtlasHeight"] != null && !JsonNode.DeepEquals(fromFileNode["m_AtlasHeight"], originFileNode["m_AtlasHeight"]))
                {
                    originFileNode["m_AtlasHeight"] = fromFileNode["m_AtlasHeight"]?.DeepClone();
                    changedFields.Add("m_AtlasHeight");
                }

                // 处理 m_GlyphTable
                JsonObject? fromFile_m_GlyphTable = fromFileNode["m_GlyphTable"]?.AsObject();
                JsonObject? originFile_m_GlyphTable = originFileNode["m_GlyphTable"]?.AsObject();
                if (fromFile_m_GlyphTable != null && originFile_m_GlyphTable != null)
                {
                    var fieldsToMove = GetAllFieldsInNode(fromFile_m_GlyphTable);
                    var movedFields = MoveFieldsInObjectNodeWithTracking(fromFile_m_GlyphTable, originFile_m_GlyphTable, fieldsToMove);
                    changedFields.AddRange(movedFields.Select(f => $"m_GlyphTable.{f}"));
                }

                // 处理 m_CharacterTable
                JsonObject? fromFile_m_CharacterTable = fromFileNode["m_CharacterTable"]?.AsObject();
                JsonObject? originFile_m_CharacterTable = originFileNode["m_CharacterTable"]?.AsObject();
                if (fromFile_m_CharacterTable != null && originFile_m_CharacterTable != null)
                {
                    var fieldsToMove = GetAllFieldsInNode(fromFile_m_CharacterTable);
                    var movedFields = MoveFieldsInObjectNodeWithTracking(fromFile_m_CharacterTable, originFile_m_CharacterTable, fieldsToMove);
                    changedFields.AddRange(movedFields.Select(f => $"m_CharacterTable.{f}"));
                }

                // 处理 m_UsedGlyphRects
                JsonObject? fromFile_m_UsedGlyphRects = fromFileNode["m_UsedGlyphRects"]?.AsObject();
                JsonObject? originFile_m_UsedGlyphRects = originFileNode["m_UsedGlyphRects"]?.AsObject();
                if (fromFile_m_UsedGlyphRects != null && originFile_m_UsedGlyphRects != null)
                {
                    var fieldsToMove = GetAllFieldsInNode(fromFile_m_UsedGlyphRects);
                    var movedFields = MoveFieldsInObjectNodeWithTracking(fromFile_m_UsedGlyphRects, originFile_m_UsedGlyphRects, fieldsToMove);
                    changedFields.AddRange(movedFields.Select(f => $"m_UsedGlyphRects.{f}"));
                }

                // 处理 m_FreeGlyphRects
                JsonObject? fromFile_m_FreeGlyphRects = fromFileNode["m_FreeGlyphRects"]?.AsObject();
                JsonObject? originFile_m_FreeGlyphRects = originFileNode["m_FreeGlyphRects"]?.AsObject();
                if (fromFile_m_FreeGlyphRects != null && originFile_m_FreeGlyphRects != null)
                {
                    var fieldsToMove = GetAllFieldsInNode(fromFile_m_FreeGlyphRects);
                    var movedFields = MoveFieldsInObjectNodeWithTracking(fromFile_m_FreeGlyphRects, originFile_m_FreeGlyphRects, fieldsToMove);
                    changedFields.AddRange(movedFields.Select(f => $"m_FreeGlyphRects.{f}"));
                }

                // 显示参数对比结果
                if (changedFields.Count > 0)
                {
                    OnLogMessage?.Invoke($"  移动了 {changedFields.Count} 个参数:");
                    foreach (string field in changedFields)
                    {
                        OnLogMessage?.Invoke($"    ✓ {field}");
                    }
                    result.HasChanges = true;
                }
                else
                {
                    OnLogMessage?.Invoke("  没有参数被移动");
                    result.HasChanges = false;
                }

                result.ChangedFields = changedFields;

                // 使用自定义格式化器写入文件
                string newJSON = originFileNode.ToJsonString(CustomJsonFormatting.Options);

                // 清理空数组中的多余空格
                newJSON = CustomJsonFormatting.CleanEmptyArrays(newJSON);

                // 获取原文件的目录路径和文件名
                string originFileDir = Path.GetDirectoryName(originFile) ?? "";
                string originFileName = Path.GetFileName(originFile);

                // 在原文件目录下创建新的文件夹
                string outputDir = Path.Combine(originFileDir, "Moved_Parameters");
                Directory.CreateDirectory(outputDir);

                // 构建输出文件的完整路径，使用原文件名
                string outputFilePath = Path.Combine(outputDir, originFileName);

                try
                {
                    File.WriteAllText(outputFilePath, newJSON, Encoding.UTF8);
                    result.OutputFilePath = outputFilePath;
                    result.Success = true;
                    OnLogMessage?.Invoke($"  ✓ 输出文件: {outputFilePath}");
                }
                catch (Exception ex)
                {
                    result.Success = false;
                    result.ErrorMessage = $"无法写入输出文件: {ex.Message}";
                    OnLogMessage?.Invoke($"  ✗ 错误: {result.ErrorMessage}");
                }

                return result;
            }
            catch (Exception ex)
            {
                result.Success = false;
                result.ErrorMessage = $"处理过程中发生未知错误: {ex.Message}";
                OnLogMessage?.Invoke($"  ✗ 错误: {result.ErrorMessage}");
                return result;
            }
        }

        /// <summary>
        /// 获取 FaceInfo 字段（排除 m_FamilyName 和 m_StyleName）
        /// </summary>
        private List<string> GetFieldsFaceInfo(JsonObject m_FaceInfo)
        {
            List<string> fieldsFaceInfo = new List<string>();

            foreach (var property in m_FaceInfo)
            {
                string fieldName = property.Key;
                if (fieldName != "m_FamilyName" && fieldName != "m_StyleName")
                {
                    fieldsFaceInfo.Add(fieldName);
                }
            }

            return fieldsFaceInfo;
        }

        /// <summary>
        /// 获取 FontInfo 字段（排除 Name）
        /// </summary>
        private List<string> GetFontInfoFields(JsonObject m_fontInfoNode)
        {
            List<string> fieldsFontInfo = new List<string>();

            foreach (var property in m_fontInfoNode)
            {
                string fieldName = property.Key;
                if (fieldName != "Name")
                {
                    fieldsFontInfo.Add(fieldName);
                }
            }

            return fieldsFontInfo;
        }

        /// <summary>
        /// 获取节点中的所有字段
        /// </summary>
        private List<string> GetAllFieldsInNode(JsonObject node)
        {
            List<string> fields = new List<string>();

            foreach (var property in node)
            {
                fields.Add(property.Key);
            }

            return fields;
        }

        /// <summary>
        /// 移动字段并跟踪变化
        /// </summary>
        private List<string> MoveFieldsInObjectNodeWithTracking(JsonObject fromNode, JsonObject toNode, List<string> moveFields)
        {
            List<string> movedFields = new List<string>();

            foreach (string field in moveFields)
            {
                if (fromNode[field] != null)
                {
                    // 检查是否有实际变化
                    if (!JsonNode.DeepEquals(fromNode[field], toNode[field]))
                    {
                        toNode.Remove(field);
                        toNode[field] = fromNode[field]?.DeepClone();
                        movedFields.Add(field);
                    }
                }
            }

            return movedFields;
        }
    }
}
