using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Storage;
using Windows.Storage.Pickers;
using Unity_TMP_ParameterMover_WinUI.Services;
using Unity_TMP_ParameterMover_WinUI.Models;

namespace Unity_TMP_ParameterMover_WinUI
{
    /// <summary>
    /// 文件信息类，用于数据绑定
    /// </summary>
    public class FileItem
    {
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
    }

    /// <summary>
    /// 主窗口
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private ObservableCollection<FileItem> _originFiles = new ObservableCollection<FileItem>();
        private string _sourceFilePath = string.Empty;

        public MainWindow()
        {
            InitializeComponent();
            FileListView.ItemsSource = _originFiles;

            // 自定义标题栏
            ExtendsContentIntoTitleBar = true;
            SetTitleBar(AppTitleBar);

            // 设置窗口大小
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
            var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
            appWindow.Resize(new Windows.Graphics.SizeInt32(1200, 900));
        }

        /// <summary>
        /// 选择原始文件
        /// </summary>
        private async void BtnSelectOriginFiles_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.ViewMode = PickerViewMode.List;
            openPicker.FileTypeFilter.Add(".json");

            var files = await openPicker.PickMultipleFilesAsync();
            if (files != null && files.Count > 0)
            {
                foreach (var file in files)
                {
                    // 检查是否已存在
                    if (!_originFiles.Any(f => f.FilePath == file.Path))
                    {
                        _originFiles.Add(new FileItem
                        {
                            FileName = file.Name,
                            FilePath = file.Path
                        });
                    }
                }

                UpdateOriginCount();
                AppendLog($"添加了 {files.Count} 个原始文件");
            }
        }

        /// <summary>
        /// 选择原始文件夹
        /// </summary>
        private async void BtnSelectOriginFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderPicker = new FolderPicker();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(folderPicker, hWnd);

            folderPicker.FileTypeFilter.Add("*");

            var folder = await folderPicker.PickSingleFolderAsync();
            if (folder != null)
            {
                var files = await folder.GetFilesAsync();
                var jsonFiles = files.Where(f => f.FileType.ToLower() == ".json").ToList();

                int addedCount = 0;
                foreach (var file in jsonFiles)
                {
                    if (!_originFiles.Any(f => f.FilePath == file.Path))
                    {
                        _originFiles.Add(new FileItem
                        {
                            FileName = file.Name,
                            FilePath = file.Path
                        });
                        addedCount++;
                    }
                }

                UpdateOriginCount();
                AppendLog($"从文件夹 '{folder.Name}' 添加了 {addedCount} 个 JSON 文件");
            }
        }

        /// <summary>
        /// 选择来源文件
        /// </summary>
        private async void BtnSelectSourceFile_Click(object sender, RoutedEventArgs e)
        {
            var openPicker = new FileOpenPicker();
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            WinRT.Interop.InitializeWithWindow.Initialize(openPicker, hWnd);

            openPicker.ViewMode = PickerViewMode.List;
            openPicker.FileTypeFilter.Add(".json");

            var file = await openPicker.PickSingleFileAsync();
            if (file != null)
            {
                _sourceFilePath = file.Path;
                TxtSourceFile.Text = file.Name;
                AppendLog($"选择了来源文件: {file.Name}");
            }
        }

        /// <summary>
        /// 从列表中移除文件
        /// </summary>
        private void BtnRemoveFile_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string filePath)
            {
                var item = _originFiles.FirstOrDefault(f => f.FilePath == filePath);
                if (item != null)
                {
                    _originFiles.Remove(item);
                    UpdateOriginCount();
                    AppendLog($"移除了文件: {item.FileName}");
                }
            }
        }

        /// <summary>
        /// 清空文件列表
        /// </summary>
        private void BtnClearList_Click(object sender, RoutedEventArgs e)
        {
            _originFiles.Clear();
            UpdateOriginCount();
            AppendLog("已清空文件列表");
        }

        /// <summary>
        /// 开始处理
        /// </summary>
        private async void BtnStartProcess_Click(object sender, RoutedEventArgs e)
        {
            // 验证输入
            if (_originFiles.Count == 0)
            {
                await ShowMessageAsync("错误", "请至少选择一个原始文件！");
                return;
            }

            if (string.IsNullOrEmpty(_sourceFilePath))
            {
                await ShowMessageAsync("错误", "请选择来源文件！");
                return;
            }

            // 禁用按钮防止重复点击
            BtnStartProcess.IsEnabled = false;
            BtnSelectOriginFiles.IsEnabled = false;
            BtnSelectOriginFolder.IsEnabled = false;
            BtnSelectSourceFile.IsEnabled = false;
            BtnClearList.IsEnabled = false;

            // 显示进度卡片
            ProgressCard.Visibility = Visibility.Visible;
            ProcessProgressBar.Value = 0;
            TxtProgressStatus.Text = "准备处理...";

            AppendLog("=================================================");
            AppendLog("开始处理文件...");
            AppendLog($"原始文件数量: {_originFiles.Count}");
            AppendLog($"来源文件: {Path.GetFileName(_sourceFilePath)}");
            AppendLog("=================================================");

            // 创建服务实例
            var service = new TMPParameterService();

            // 设置日志回调
            service.OnLogMessage = (message) =>
            {
                DispatcherQueue.TryEnqueue(() =>
                {
                    AppendLog(message);
                });
            };

            // 处理统计
            int successCount = 0;
            int failCount = 0;
            int changedCount = 0;

            // 在后台线程处理文件
            await Task.Run(() =>
            {
                for (int i = 0; i < _originFiles.Count; i++)
                {
                    var file = _originFiles[i];

                    // 更新UI（在UI线程）
                    DispatcherQueue.TryEnqueue(() =>
                    {
                        ProcessProgressBar.Value = (i + 1) * 100.0 / _originFiles.Count;
                        TxtProgressStatus.Text = $"正在处理: {file.FileName} ({i + 1}/{_originFiles.Count})";
                        AppendLog($"\n[{i + 1}/{_originFiles.Count}] 处理: {file.FileName}");
                    });

                    // 调用服务处理文件
                    var result = service.ProcessSingleFile(file.FilePath, _sourceFilePath);

                    // 统计结果
                    if (result.Success)
                    {
                        successCount++;
                        if (result.HasChanges)
                        {
                            changedCount++;
                        }
                    }
                    else
                    {
                        failCount++;
                    }
                }
            });

            // 处理完成
            TxtProgressStatus.Text = "处理完成！";
            AppendLog("\n=================================================");
            AppendLog($"处理完成统计:");
            AppendLog($"  总文件数: {_originFiles.Count}");
            AppendLog($"  成功: {successCount}");
            AppendLog($"  失败: {failCount}");
            AppendLog($"  有变化: {changedCount}");
            AppendLog("=================================================");

            // 重新启用按钮
            BtnStartProcess.IsEnabled = true;
            BtnSelectOriginFiles.IsEnabled = true;
            BtnSelectOriginFolder.IsEnabled = true;
            BtnSelectSourceFile.IsEnabled = true;
            BtnClearList.IsEnabled = true;

            // 显示完成消息
            string message = $"处理完成！\n\n总文件数: {_originFiles.Count}\n成功: {successCount}\n失败: {failCount}\n有参数变化: {changedCount}";
            if (failCount > 0)
            {
                message += "\n\n请查看日志了解失败详情。";
            }

            await ShowMessageAsync("处理完成", message);
        }

        /// <summary>
        /// 清空日志
        /// </summary>
        private void BtnClearLog_Click(object sender, RoutedEventArgs e)
        {
            TxtLog.Text = "等待操作...";
        }

        /// <summary>
        /// 更新原始文件计数显示
        /// </summary>
        private void UpdateOriginCount()
        {
            TxtOriginCount.Text = $"已选择: {_originFiles.Count} 个文件";
        }

        /// <summary>
        /// 追加日志
        /// </summary>
        private void AppendLog(string message)
        {
            if (TxtLog.Text == "等待操作...")
            {
                TxtLog.Text = $"[{DateTime.Now:HH:mm:ss}] {message}";
            }
            else
            {
                TxtLog.Text += $"\n[{DateTime.Now:HH:mm:ss}] {message}";
            }
        }

        /// <summary>
        /// 显示消息对话框
        /// </summary>
        private async System.Threading.Tasks.Task ShowMessageAsync(string title, string message)
        {
            var dialog = new ContentDialog
            {
                Title = title,
                Content = message,
                CloseButtonText = "确定",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
    }
}
