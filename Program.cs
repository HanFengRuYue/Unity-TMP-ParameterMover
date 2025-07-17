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
            // 解析命令行参数
            var (originPaths, fromFile, showHelp) = ParseCommandLineArgs(args);
            
            // 显示帮助信息
            if (showHelp)
            {
                ShowHelp();
                return;
            }
            
            // 如果没有通过命令行参数提供文件路径，则使用交互式模式
            if (originPaths.Count == 0 || string.IsNullOrEmpty(fromFile))
            {
                if (args.Length > 0)
                {
                    Console.WriteLine("错误: 请提供有效的文件路径参数，或使用 -h 查看帮助信息。");
                    return;
                }
                
                // 交互式模式
                Console.WriteLine("Unity TMP Parameter Mover - 交互式模式");
                Console.WriteLine("提示: 使用 -h 参数查看命令行使用方法");
                Console.WriteLine();
                
                Console.Write("请输入原始文件路径或文件夹路径（支持多个路径，用分号分隔）: ");
                string? originInput = Console.ReadLine();
                
                Console.Write("请输入来源文件路径: ");
                fromFile = Console.ReadLine();
                
                if (string.IsNullOrEmpty(originInput) || string.IsNullOrEmpty(fromFile))
                {
                    Console.WriteLine("错误: 原始文件或来源文件路径不能为空!");
                    return;
                }
                
                // 解析原始文件路径
                originPaths = ParseOriginPaths(originInput);
            }
            
            // 处理文件
            ProcessMultipleFiles(originPaths, fromFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: {ex.Message}");
        }
        finally
        {
            // 只在交互式模式下等待用户输入
            if (args.Length == 0)
            {
                Console.WriteLine();
                Console.WriteLine("按任意键关闭程序...");
                
                try
                {
                    if (Console.KeyAvailable || !Console.IsInputRedirected)
                    {
                        Console.ReadKey(true);
                    }
                    else
                    {
                        Console.Read();
                    }
                }
                catch (InvalidOperationException)
                {
                    Console.Read();
                }
            }
        }
    }
    
    private static (List<string> originPaths, string? fromFile, bool showHelp) ParseCommandLineArgs(string[] args)
    {
        List<string> originPaths = new List<string>();
        string? fromFile = null;
        bool showHelp = false;
        
        for (int i = 0; i < args.Length; i++)
        {
            switch (args[i].ToLower())
            {
                case "-h":
                case "--help":
                    showHelp = true;
                    break;
                    
                case "-o":
                case "--origin":
                    if (i + 1 < args.Length)
                    {
                        string originInput = args[i + 1];
                        originPaths.AddRange(ParseOriginPaths(originInput));
                        i++; // 跳过下一个参数
                    }
                    else
                    {
                        Console.WriteLine("错误: -o/--origin 参数需要指定文件路径或文件夹路径");
                        Environment.Exit(1);
                    }
                    break;
                    
                case "-f":
                case "--from":
                    if (i + 1 < args.Length)
                    {
                        fromFile = args[i + 1];
                        i++; // 跳过下一个参数
                    }
                    else
                    {
                        Console.WriteLine("错误: -f/--from 参数需要指定文件路径");
                        Environment.Exit(1);
                    }
                    break;
                    
                default:
                    if (args[i].StartsWith("-"))
                    {
                        Console.WriteLine($"错误: 未知的参数 '{args[i]}'");
                        Console.WriteLine("使用 -h 查看帮助信息");
                        Environment.Exit(1);
                    }
                    break;
            }
        }
        
        return (originPaths, fromFile, showHelp);
    }
    
    private static List<string> ParseOriginPaths(string originInput)
    {
        List<string> originPaths = new List<string>();
        
        // 分号分隔的多个路径
        string[] paths = originInput.Split(';', StringSplitOptions.RemoveEmptyEntries);
        
        foreach (string path in paths)
        {
            string cleanPath = CleanFilePath(path);
            
            if (Directory.Exists(cleanPath))
            {
                // 如果是文件夹，扫描所有JSON文件
                string[] jsonFiles = Directory.GetFiles(cleanPath, "*.json", SearchOption.TopDirectoryOnly);
                originPaths.AddRange(jsonFiles);
            }
            else if (File.Exists(cleanPath))
            {
                // 如果是文件，直接添加
                originPaths.Add(cleanPath);
            }
            else
            {
                Console.WriteLine($"警告: 路径不存在: {cleanPath}");
            }
        }
        
        return originPaths;
    }
    
    private static void ShowHelp()
    {
        Console.WriteLine("Unity TMP Parameter Mover v1.2.0");
        Console.WriteLine("Unity TextMeshPro字体参数移动工具");
        Console.WriteLine();
        Console.WriteLine("用法:");
        Console.WriteLine("  UnityTMPParameterMover.exe [选项]");
        Console.WriteLine();
        Console.WriteLine("选项:");
        Console.WriteLine("  -o, --origin <文件路径或文件夹路径>    指定原始文件路径或文件夹路径");
        Console.WriteLine("                                        支持多个路径，用分号分隔");
        Console.WriteLine("  -f, --from <文件路径>                  指定来源文件路径");
        Console.WriteLine("  -h, --help                            显示此帮助信息");
        Console.WriteLine();
        Console.WriteLine("示例:");
        Console.WriteLine("  UnityTMPParameterMover.exe -o \"original.json\" -f \"source.json\"");
        Console.WriteLine("  UnityTMPParameterMover.exe -o \"file1.json;file2.json\" -f \"source.json\"");
        Console.WriteLine("  UnityTMPParameterMover.exe -o \"fonts/\" -f \"source.json\"");
        Console.WriteLine("  UnityTMPParameterMover.exe -o \"fonts/;file1.json\" -f \"source.json\"");
        Console.WriteLine();
        Console.WriteLine("注意:");
        Console.WriteLine("  - 如果不提供命令行参数，程序将以交互式模式运行");
        Console.WriteLine("  - 输出文件将保存在原始文件目录下的 'Moved_Parameters' 文件夹中");
        Console.WriteLine("  - 支持的文件格式: JSON");
        Console.WriteLine("  - 指定文件夹时，将处理该文件夹中的所有JSON文件");
    }
    
    private static void ProcessMultipleFiles(List<string> originPaths, string fromFile)
    {
        if (originPaths.Count == 0)
        {
            Console.WriteLine("错误: 没有找到有效的原始文件");
            return;
        }
        
        Console.WriteLine($"找到 {originPaths.Count} 个原始文件需要处理:");
        for (int i = 0; i < originPaths.Count; i++)
        {
            Console.WriteLine($"  {i + 1}. {originPaths[i]}");
        }
        Console.WriteLine();
        
        int successCount = 0;
        int failCount = 0;
        
        foreach (string originFile in originPaths)
        {
            Console.WriteLine($"正在处理: {Path.GetFileName(originFile)}");
            Console.WriteLine(new string('=', 50));
            
            try
            {
                var changes = ProcessSingleFile(originFile, fromFile);
                if (changes.HasChanges)
                {
                    successCount++;
                    Console.WriteLine($"✓ 处理成功: {Path.GetFileName(originFile)}");
                }
                else
                {
                    Console.WriteLine($"- 无变化: {Path.GetFileName(originFile)}");
                    successCount++;
                }
            }
            catch (Exception ex)
            {
                failCount++;
                Console.WriteLine($"✗ 处理失败: {Path.GetFileName(originFile)} - {ex.Message}");
            }
            
            Console.WriteLine();
        }
        
        Console.WriteLine($"处理完成! 成功: {successCount}, 失败: {failCount}");
    }
    
    private static (JObject OriginFileNode, JObject FromFileNode, bool HasChanges) ProcessSingleFile(string originFile, string fromFile)
    {
        // 清理文件路径，去掉可能的引号
        originFile = CleanFilePath(originFile);
        fromFile = CleanFilePath(fromFile);
        
        // 检查文件是否存在
        if (!File.Exists(originFile))
        {
            Console.WriteLine($"错误: 原始文件不存在: {originFile}");
            return (new JObject(), new JObject(), false); // 返回空的JObject表示失败
        }
        
        if (!File.Exists(fromFile))
        {
            Console.WriteLine($"错误: 来源文件不存在: {fromFile}");
            return (new JObject(), new JObject(), false); // 返回空的JObject表示失败
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
            return (new JObject(), new JObject(), false); // 返回空的JObject表示失败
        }
        
        try
        {
            fromJson = File.ReadAllText(fromFile);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: 无法读取来源文件 '{fromFile}': {ex.Message}");
            return (new JObject(), new JObject(), false); // 返回空的JObject表示失败
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
            return (new JObject(), new JObject(), false); // 返回空的JObject表示失败
        }
        
        try
        {
            fromFileNode = JObject.Parse(fromJson);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: 来源文件JSON格式无效 '{fromFile}': {ex.Message}");
            return (new JObject(), new JObject(), false); // 返回空的JObject表示失败
        }
        
        // 创建原始文件的副本用于对比
        JObject originalCopy = JObject.Parse(originJson);
        
        // 用于记录变化的信息
        List<string> changedFields = new List<string>();
        
        // 处理 m_fontInfo
        JObject? fromFile_m_fontInfoNode = fromFileNode["m_fontInfo"] as JObject;
        JObject? originFile_m_fontInfoNode = originFileNode["m_fontInfo"] as JObject;
        if (fromFile_m_fontInfoNode != null && originFile_m_fontInfoNode != null)
        {
            var fieldsToMove = GetFontInfoFields(fromFile_m_fontInfoNode);
            var movedFields = MoveFieldsInObjectNodeWithTracking(fromFile_m_fontInfoNode, originFile_m_fontInfoNode, fieldsToMove);
            changedFields.AddRange(movedFields.Select(f => $"m_fontInfo.{f}"));
        }
        
        // 处理 m_glyphInfoList
        JObject? fromFile_m_glyphInfoList = fromFileNode["m_glyphInfoList"] as JObject;
        JObject? originFile_m_glyphInfoList = originFileNode["m_glyphInfoList"] as JObject;
        if (fromFile_m_glyphInfoList != null && originFile_m_glyphInfoList != null)
        {
            var fieldsToMove = GetAllFieldsInNode(fromFile_m_glyphInfoList);
            var movedFields = MoveFieldsInObjectNodeWithTracking(fromFile_m_glyphInfoList, originFile_m_glyphInfoList, fieldsToMove);
            changedFields.AddRange(movedFields.Select(f => $"m_glyphInfoList.{f}"));
        }
        
        // 处理 m_FaceInfo
        JObject? fromFile_m_FaceInfo = fromFileNode["m_FaceInfo"] as JObject;
        JObject? originFile_m_FaceInfo = originFileNode["m_FaceInfo"] as JObject;
        if (fromFile_m_FaceInfo != null && originFile_m_FaceInfo != null)
        {
            var fieldsToMove = GetFieldsFaceInfo(fromFile_m_FaceInfo);
            var movedFields = MoveFieldsInObjectNodeWithTracking(fromFile_m_FaceInfo, originFile_m_FaceInfo, fieldsToMove);
            changedFields.AddRange(movedFields.Select(f => $"m_FaceInfo.{f}"));
        }
        
        // 处理 m_AtlasWidth 和 m_AtlasHeight
        if (fromFileNode["m_AtlasWidth"] != null && !JToken.DeepEquals(fromFileNode["m_AtlasWidth"], originFileNode["m_AtlasWidth"]))
        {
            originFileNode["m_AtlasWidth"] = fromFileNode["m_AtlasWidth"];
            changedFields.Add("m_AtlasWidth");
        }
        if (fromFileNode["m_AtlasHeight"] != null && !JToken.DeepEquals(fromFileNode["m_AtlasHeight"], originFileNode["m_AtlasHeight"]))
        {
            originFileNode["m_AtlasHeight"] = fromFileNode["m_AtlasHeight"];
            changedFields.Add("m_AtlasHeight");
        }
        
        // 处理 m_GlyphTable
        JObject? fromFile_m_GlyphTable = fromFileNode["m_GlyphTable"] as JObject;
        JObject? originFile_m_GlyphTable = originFileNode["m_GlyphTable"] as JObject;
        if (fromFile_m_GlyphTable != null && originFile_m_GlyphTable != null)
        {
            var fieldsToMove = GetAllFieldsInNode(fromFile_m_GlyphTable);
            var movedFields = MoveFieldsInObjectNodeWithTracking(fromFile_m_GlyphTable, originFile_m_GlyphTable, fieldsToMove);
            changedFields.AddRange(movedFields.Select(f => $"m_GlyphTable.{f}"));
        }
        
        // 处理 m_CharacterTable
        JObject? fromFile_m_CharacterTable = fromFileNode["m_CharacterTable"] as JObject;
        JObject? originFile_m_CharacterTable = originFileNode["m_CharacterTable"] as JObject;
        if (fromFile_m_CharacterTable != null && originFile_m_CharacterTable != null)
        {
            var fieldsToMove = GetAllFieldsInNode(fromFile_m_CharacterTable);
            var movedFields = MoveFieldsInObjectNodeWithTracking(fromFile_m_CharacterTable, originFile_m_CharacterTable, fieldsToMove);
            changedFields.AddRange(movedFields.Select(f => $"m_CharacterTable.{f}"));
        }
        
        // 处理 m_UsedGlyphRects
        JObject? fromFile_m_UsedGlyphRects = fromFileNode["m_UsedGlyphRects"] as JObject;
        JObject? originFile_m_UsedGlyphRects = originFileNode["m_UsedGlyphRects"] as JObject;
        if (fromFile_m_UsedGlyphRects != null && originFile_m_UsedGlyphRects != null)
        {
            var fieldsToMove = GetAllFieldsInNode(fromFile_m_UsedGlyphRects);
            var movedFields = MoveFieldsInObjectNodeWithTracking(fromFile_m_UsedGlyphRects, originFile_m_UsedGlyphRects, fieldsToMove);
            changedFields.AddRange(movedFields.Select(f => $"m_UsedGlyphRects.{f}"));
        }
        
        // 处理 m_FreeGlyphRects
        JObject? fromFile_m_FreeGlyphRects = fromFileNode["m_FreeGlyphRects"] as JObject;
        JObject? originFile_m_FreeGlyphRects = originFileNode["m_FreeGlyphRects"] as JObject;
        if (fromFile_m_FreeGlyphRects != null && originFile_m_FreeGlyphRects != null)
        {
            var fieldsToMove = GetAllFieldsInNode(fromFile_m_FreeGlyphRects);
            var movedFields = MoveFieldsInObjectNodeWithTracking(fromFile_m_FreeGlyphRects, originFile_m_FreeGlyphRects, fieldsToMove);
            changedFields.AddRange(movedFields.Select(f => $"m_FreeGlyphRects.{f}"));
        }
        
        // 显示参数对比结果
        if (changedFields.Count > 0)
        {
            Console.WriteLine($"移动的参数 ({changedFields.Count} 个):");
            foreach (string field in changedFields)
            {
                Console.WriteLine($"  ✓ {field}");
            }
            Console.WriteLine();
        }
        else
        {
            Console.WriteLine("没有参数被移动");
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
            Console.WriteLine($"输出文件: {outputFilePath}");
            return (originFileNode, fromFileNode, changedFields.Count > 0); // 表示有变化
        }
        catch (Exception ex)
        {
            Console.WriteLine($"错误: 无法写入输出文件 '{outputFilePath}': {ex.Message}");
            return (originFileNode, fromFileNode, false); // 表示失败
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
    
    private static List<string> MoveFieldsInObjectNodeWithTracking(JObject fromNode, JObject toNode, List<string> moveFields)
    {
        List<string> movedFields = new List<string>();
        
        foreach (string field in moveFields)
        {
            if (fromNode[field] != null)
            {
                // 检查是否有实际变化
                if (!JToken.DeepEquals(fromNode[field], toNode[field]))
                {
                    toNode.Remove(field);
                    toNode[field] = fromNode[field];
                    movedFields.Add(field);
                }
            }
        }
        
        return movedFields;
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
