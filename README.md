# Unity TMP Parameter Mover

一个用于移动JSON文件中字体参数的工具。

## 功能特性

- 🔄 自动移动JSON文件中的字体参数
- 🛡️ 智能保留特定字段（`Name`、`m_FamilyName`、`m_StyleName`）
- 📁 在原文件目录创建专用输出文件夹
- 📄 使用原文件名保存处理后的文件
- 🔍 完整的错误处理和用户友好提示
- 💻 支持命令行参数和交互式模式
- ⚡ 支持批处理脚本集成

## 使用方法

### 命令行模式（推荐）

使用命令行参数直接指定文件路径：

```cmd
UnityTMPParameterMover.exe -o "原始文件路径" -f "来源文件路径"
```

#### 命令行参数

- `-o, --origin <文件路径>` - 指定原始文件路径
- `-f, --from <文件路径>` - 指定来源文件路径  
- `-h, --help` - 显示帮助信息

#### 命令行示例

```cmd
UnityTMPParameterMover.exe -o "D:\Game\font_original.json" -f "D:\Game\font_source.json"
UnityTMPParameterMover.exe --origin D:\Game\font_original.json --from D:\Game\font_source.json
```

### 交互式模式

如果不提供命令行参数，程序将以交互式模式运行：

1. 双击运行 `UnityTMPParameterMover.exe`
2. 根据提示输入原始JSON文件路径
3. 输入来源JSON文件路径（参数来源）
4. 程序将自动处理并在原文件目录创建 `Moved_Parameters` 文件夹
5. 处理后的文件将保存在该文件夹中，使用原文件名

#### 交互式输入示例

```
请输入原始文件路径: D:\Game\font_original.json
请输入来源文件路径: D:\Game\font_source.json
```

### 批处理集成

项目包含了一个批处理脚本示例 `example_usage.bat`，展示如何在自动化流程中使用该程序：

```cmd
rem 设置文件路径
set ORIGIN_FILE=original_font.json
set FROM_FILE=source_font.json

rem 执行参数移动
UnityTMPParameterMover.exe -o "%ORIGIN_FILE%" -f "%FROM_FILE%"
```

### 输出

程序将在 `D:\Game\` 目录下创建 `Moved_Parameters` 文件夹，并将处理后的文件保存为 `D:\Game\Moved_Parameters\font_original.json`。

## 处理的字段

程序会处理以下JSON字段：

- `m_GlyphTable` - 字形表
- `m_FaceInfo` - 字体信息（保留 `m_FamilyName` 和 `m_StyleName`）
- `m_AtlasWidth` - 图集宽度
- `m_AtlasHeight` - 图集高度
- `m_CharacterTable` - 字符表
- `m_UsedGlyphRects` - 已使用字形矩形
- `m_FreeGlyphRects` - 可用字形矩形

## 构建说明

### 环境要求

- .NET 8.0 SDK
- Windows 10/11 x64

### 构建步骤

1. 克隆或下载项目
2. 在项目根目录运行：
   ```bash
   dotnet build
   ```

### 发布exe程序

运行批处理脚本：
```cmd
publish.bat
```

或手动执行：
```bash
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:DebugType=none -o .\publish
```

发布的exe文件将位于 `publish\` 目录中。

## 项目结构

```
UnityTMPParameterMover/
├── Program.cs                    # 主程序文件
├── CustomJsonFormatting.cs       # JSON格式化工具
├── UnityTMPParameterMover.csproj # 项目配置文件
├── publish.bat                   # 发布脚本
├── example_usage.bat             # 批处理使用示例
├── .gitignore                    # Git忽略文件
└── README.md                     # 项目自述文件
```

## 技术特性

- **平台**: .NET 8.0
- **依赖**: Newtonsoft.Json 13.0.3
- **目标平台**: Windows x64
- **发布方式**: 独立exe程序（包含完整.NET运行时）
- **文件大小**: 约60-70MB
- **兼容性**: Windows 10/11 x64系统

## 错误处理

程序包含完整的错误处理机制：

- 文件存在性检查
- JSON格式验证
- 文件读写权限检查
- 路径格式清理（支持带引号路径）
- 用户友好的错误提示