# Unity TMP Parameter Mover

一个现代化的 WinUI 3 桌面应用程序，用于批量处理 Unity TextMeshPro 字体资产文件，自动迁移字体参数。

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET](https://img.shields.io/badge/.NET-9.0-purple.svg)
![Platform](https://img.shields.io/badge/platform-Windows-blue.svg)

## ✨ 功能特性

- 🎨 **现代化 UI** - 采用 WinUI 3 设计，支持 Mica 材质背景
- 🔄 **批量处理** - 支持多文件和文件夹批量处理
- 🛡️ **智能保留** - 自动保留特定字段（`Name`、`m_FamilyName`、`m_StyleName`）
- 📁 **智能输出** - 在原文件目录创建 `Moved_Parameters` 文件夹
- 📊 **实时日志** - 显示详细的处理进度和参数变化信息
- 🔍 **完整验证** - 全面的错误处理和用户友好提示
- ⚡ **高性能** - 使用 `System.Text.Json` 确保快速处理
- 🌐 **多平台支持** - 支持 x64、x86 和 ARM64 架构

## 📋 系统要求

### 运行环境

- **操作系统**: Windows 10 (1809) 或更高版本
- **.NET Runtime**: [.NET 9.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Windows App SDK**: [Windows App SDK Runtime 1.8](https://learn.microsoft.com/windows/apps/windows-app-sdk/downloads)

### 开发环境

- **IDE**: Visual Studio 2022 或更高版本
- **SDK**: .NET 9.0 SDK
- **Windows SDK**: 10.0.26100.0 或更高版本

## 🚀 快速开始

### 下载和运行

1. 从 [Releases](../../releases) 页面下载最新版本的 `Unity_TMP_ParameterMover_WinUI.exe`
2. 确保已安装 [.NET 9.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0)
3. 双击运行程序

### 使用步骤

1. **选择原始文件**
   - 点击「选择文件」按钮选择一个或多个需要更新的 JSON 文件
   - 或点击「选择文件夹」按钮批量导入文件夹中的所有 JSON 文件

2. **选择来源文件**
   - 点击「选择来源文件」按钮选择提供新参数的 JSON 文件

3. **开始处理**
   - 点击「开始处理」按钮
   - 查看实时日志了解处理进度

4. **查看结果**
   - 处理完成后，在原文件目录下的 `Moved_Parameters` 文件夹中找到处理后的文件

### 输出示例

```
原始文件: D:\Game\Fonts\MyFont.asset
输出文件: D:\Game\Fonts\Moved_Parameters\MyFont.asset
```

## 📦 处理的参数

程序会从来源文件复制以下参数到原始文件：

| 参数名称 | 说明 | 保留字段 |
|---------|------|---------|
| `m_fontInfo` | 字体信息 | `Name` |
| `m_glyphInfoList` | 字形信息列表 | - |
| `m_FaceInfo` | 字体面信息 | `m_FamilyName`, `m_StyleName` |
| `m_AtlasWidth` | 图集宽度 | - |
| `m_AtlasHeight` | 图集高度 | - |
| `m_GlyphTable` | 字形表 | - |
| `m_CharacterTable` | 字符表 | - |
| `m_UsedGlyphRects` | 已使用字形矩形 | - |
| `m_FreeGlyphRects` | 可用字形矩形 | - |

## 🔨 构建说明

### 使用构建脚本（推荐）

```powershell
# 构建 x64 版本（默认）
.\Build.ps1

# 构建其他平台
.\Build.ps1 -Platform x86
.\Build.ps1 -Platform ARM64
```

构建完成后，单文件 EXE（约 38 MB）将输出到 `Release` 文件夹。

### 手动构建

```powershell
cd Unity-TMP-ParameterMover-WinUI

# 恢复依赖
dotnet restore

# 构建
dotnet build --configuration Release -p:Platform=x64

# 发布单文件
dotnet publish `
    --configuration Release `
    --runtime win-x64 `
    --self-contained false `
    -p:Platform=x64 `
    -p:PublishSingleFile=true
```

## 📂 项目结构

```
Unity-TMP-ParameterMover-WinUI/
├── App.xaml(.cs)                    # 应用程序入口点
├── MainWindow.xaml(.cs)             # 主窗口和 UI 逻辑
├── Services/
│   └── TMPParameterService.cs       # 核心参数迁移逻辑
├── Models/
│   └── ProcessResult.cs             # 处理结果数据模型
├── Utilities/
│   └── CustomJsonFormatting.cs      # JSON 格式化工具
├── Unity-TMP-ParameterMover-WinUI.csproj
└── ICON.ico                         # 应用程序图标
```

## 🛠️ 技术栈

- **框架**: .NET 9.0 + WinUI 3
- **Windows App SDK**: 1.8
- **JSON 处理**: System.Text.Json
- **架构模式**: MVVM-lite
- **部署方式**: 框架依赖单文件发布
- **文件大小**: 约 38 MB

## 🔧 高级配置

### 自定义输出目录

默认情况下，处理后的文件会保存到原文件目录下的 `Moved_Parameters` 文件夹。这个行为在 `TMPParameterService.cs` 中定义。

### JSON 格式化

程序使用 `CustomJsonFormatting` 工具保持 JSON 文件的可读性，确保输出格式与 Unity 编辑器生成的格式一致。

## ❓ 常见问题

**Q: 程序无法启动，提示缺少运行时？**  
A: 请确保已安装 [.NET 9.0 Desktop Runtime](https://dotnet.microsoft.com/download/dotnet/9.0) 和 [Windows App SDK Runtime](https://learn.microsoft.com/windows/apps/windows-app-sdk/downloads)。

**Q: 支持哪些文件格式？**  
A: 程序处理 Unity TextMeshPro 字体资产的 JSON 格式文件（通常是 `.asset` 文件）。

**Q: 原始文件会被修改吗？**  
A: 不会。程序会在原文件目录创建 `Moved_Parameters` 文件夹，并将处理后的文件保存到该文件夹中，原文件不会被修改。

**Q: 如何验证处理结果？**  
A: 查看日志窗口中的详细信息，程序会列出所有被修改的参数。您也可以使用文本编辑器或 Unity 编辑器对比原文件和处理后的文件。

## 📄 许可证

本项目采用 MIT 许可证。详见 [LICENSE](LICENSE) 文件。

## 🤝 贡献

欢迎提交 Issue 和 Pull Request！

## 📮 联系方式

如有问题或建议，请通过 GitHub Issues 联系。