using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text;

namespace UnityTMPParameterMover;

class Program
{
    static void Main(string[] args)
    {
        try
        {
            Console.Write("Print origin file: ");
            string? originFile = Console.ReadLine();
            
            Console.Write("Print from file copy parameters: ");
            string? fromFile = Console.ReadLine();
            
            if (string.IsNullOrEmpty(originFile) || string.IsNullOrEmpty(fromFile))
            {
                Console.WriteLine("Origin File or From File copy parameters NOT have been entered!");
                return;
            }
            
            // 清理文件路径，去掉可能的引号
            originFile = CleanFilePath(originFile);
            fromFile = CleanFilePath(fromFile);
            
            // 检查文件是否存在
            if (!File.Exists(originFile))
            {
                Console.WriteLine($"错误: 原始文件不存在: {originFile}");
                return;
            }
            
            if (!File.Exists(fromFile))
            {
                Console.WriteLine($"错误: 来源文件不存在: {fromFile}");
                return;
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
                Console.WriteLine($"错误: 无法读取原始文件 '{originFile}': {ex.Message}");
                return;
            }
            
            try
            {
                fromJson = File.ReadAllText(fromFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: 无法读取来源文件 '{fromFile}': {ex.Message}");
                return;
            }
            
            JObject originFileNode;
            JObject fromFileNode;
            
            try
            {
                originFileNode = JObject.Parse(originJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: 原始文件JSON格式无效 '{originFile}': {ex.Message}");
                return;
            }
            
            try
            {
                fromFileNode = JObject.Parse(fromJson);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: 来源文件JSON格式无效 '{fromFile}': {ex.Message}");
                return;
            }
            
            // 处理 m_fontInfo
            JObject? fromFile_m_fontInfoNode = fromFileNode["m_fontInfo"] as JObject;
            JObject? originFile_m_fontInfoNode = originFileNode["m_fontInfo"] as JObject;
            if (fromFile_m_fontInfoNode != null && originFile_m_fontInfoNode != null)
            {
                MoveFieldsInObjectNode(fromFile_m_fontInfoNode, originFile_m_fontInfoNode, GetFontInfoFields(fromFile_m_fontInfoNode));
            }
            
            // 处理 m_glyphInfoList
            JObject? fromFile_m_glyphInfoList = fromFileNode["m_glyphInfoList"] as JObject;
            JObject? originFile_m_glyphInfoList = originFileNode["m_glyphInfoList"] as JObject;
            if (fromFile_m_glyphInfoList != null && originFile_m_glyphInfoList != null)
            {
                MoveFieldsInObjectNode(fromFile_m_glyphInfoList, originFile_m_glyphInfoList, GetAllFieldsInNode(fromFile_m_glyphInfoList));
            }
            
            // 处理 m_FaceInfo
            JObject? fromFile_m_FaceInfo = fromFileNode["m_FaceInfo"] as JObject;
            JObject? originFile_m_FaceInfo = originFileNode["m_FaceInfo"] as JObject;
            if (fromFile_m_FaceInfo != null && originFile_m_FaceInfo != null)
            {
                MoveFieldsInObjectNode(fromFile_m_FaceInfo, originFile_m_FaceInfo, GetFieldsFaceInfo(fromFile_m_FaceInfo));
            }
            
            // 处理 m_AtlasWidth 和 m_AtlasHeight
            if (fromFileNode["m_AtlasWidth"] != null)
            {
                originFileNode["m_AtlasWidth"] = fromFileNode["m_AtlasWidth"];
            }
            if (fromFileNode["m_AtlasHeight"] != null)
            {
                originFileNode["m_AtlasHeight"] = fromFileNode["m_AtlasHeight"];
            }
            
            // 处理 m_GlyphTable
            JObject? fromFile_m_GlyphTable = fromFileNode["m_GlyphTable"] as JObject;
            JObject? originFile_m_GlyphTable = originFileNode["m_GlyphTable"] as JObject;
            if (fromFile_m_GlyphTable != null && originFile_m_GlyphTable != null)
            {
                MoveFieldsInObjectNode(fromFile_m_GlyphTable, originFile_m_GlyphTable, GetAllFieldsInNode(fromFile_m_GlyphTable));
            }
            
            // 处理 m_CharacterTable
            JObject? fromFile_m_CharacterTable = fromFileNode["m_CharacterTable"] as JObject;
            JObject? originFile_m_CharacterTable = originFileNode["m_CharacterTable"] as JObject;
            if (fromFile_m_CharacterTable != null && originFile_m_CharacterTable != null)
            {
                MoveFieldsInObjectNode(fromFile_m_CharacterTable, originFile_m_CharacterTable, GetAllFieldsInNode(fromFile_m_CharacterTable));
            }
            
            // 处理 m_UsedGlyphRects
            JObject? fromFile_m_UsedGlyphRects = fromFileNode["m_UsedGlyphRects"] as JObject;
            JObject? originFile_m_UsedGlyphRects = originFileNode["m_UsedGlyphRects"] as JObject;
            if (fromFile_m_UsedGlyphRects != null && originFile_m_UsedGlyphRects != null)
            {
                MoveFieldsInObjectNode(fromFile_m_UsedGlyphRects, originFile_m_UsedGlyphRects, GetAllFieldsInNode(fromFile_m_UsedGlyphRects));
            }
            
            // 处理 m_FreeGlyphRects
            JObject? fromFile_m_FreeGlyphRects = fromFileNode["m_FreeGlyphRects"] as JObject;
            JObject? originFile_m_FreeGlyphRects = originFileNode["m_FreeGlyphRects"] as JObject;
            if (fromFile_m_FreeGlyphRects != null && originFile_m_FreeGlyphRects != null)
            {
                MoveFieldsInObjectNode(fromFile_m_FreeGlyphRects, originFile_m_FreeGlyphRects, GetAllFieldsInNode(fromFile_m_FreeGlyphRects));
            }
            
            // 使用自定义格式化器写入文件
            string newJSON = JsonConvert.SerializeObject(originFileNode, CustomJsonFormatting.Settings);
            
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
                Console.WriteLine($"参数移动完成！输出文件: {outputFilePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"错误: 无法写入输出文件 '{outputFilePath}': {ex.Message}");
                return;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: {ex.Message}");
        }
        finally
        {
            // 等待用户按任意键才关闭程序
            Console.WriteLine();
            Console.WriteLine("按任意键关闭程序...");
            
            // 检查是否有可用的控制台输入
            try
            {
                // 如果输入被重定向，Console.KeyAvailable 会抛出异常
                if (Console.KeyAvailable || !Console.IsInputRedirected)
                {
                    Console.ReadKey(true);
                }
                else
                {
                    // 如果输入被重定向，使用Read()方法
                    Console.Read();
                }
            }
            catch (InvalidOperationException)
            {
                // 如果没有控制台或输入被重定向，使用Read()方法
                Console.Read();
            }
        }
    }
    
    private static List<string> GetFieldsFaceInfo(JObject m_FaceInfo)
    {
        List<string> fieldsFaceInfo = new List<string>();
        
        foreach (var property in m_FaceInfo.Properties())
        {
            string fieldName = property.Name;
            if (fieldName != "m_FamilyName" && fieldName != "m_StyleName")
            {
                fieldsFaceInfo.Add(fieldName);
            }
        }
        
        return fieldsFaceInfo;
    }
    
    public static List<string> GetFontInfoFields(JObject m_fontInfoNode)
    {
        List<string> fieldsFontInfo = new List<string>();
        
        foreach (var property in m_fontInfoNode.Properties())
        {
            string fieldName = property.Name;
            if (fieldName != "Name")
            {
                fieldsFontInfo.Add(fieldName);
            }
        }
        
        return fieldsFontInfo;
    }
    
    public static List<string> GetAllFieldsInNode(JObject node)
    {
        List<string> fields = new List<string>();
        
        foreach (var property in node.Properties())
        {
            fields.Add(property.Name);
        }
        
        return fields;
    }
    
    public static void MoveFieldsInObjectNode(JObject fromNode, JObject toNode, List<string> moveFields)
    {
        foreach (string field in moveFields)
        {
            if (fromNode[field] != null)
            {
                toNode.Remove(field);
                toNode[field] = fromNode[field];
            }
        }
    }
    
    private static string CleanFilePath(string filePath)
    {
        if (string.IsNullOrEmpty(filePath))
            return filePath;
        
        // 去掉开头和结尾的引号
        filePath = filePath.Trim();
        if (filePath.StartsWith("\"") && filePath.EndsWith("\""))
        {
            filePath = filePath.Substring(1, filePath.Length - 2);
        }
        
        // 去掉单引号
        if (filePath.StartsWith("'") && filePath.EndsWith("'"))
        {
            filePath = filePath.Substring(1, filePath.Length - 2);
        }
        
        return filePath.Trim();
    }
}
