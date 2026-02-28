using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;

class Program
{
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    private const int SW_HIDE = 0;

    [STAThread]
    static void Main()
    {
        var consoleWindow = GetConsoleWindow();
        if (consoleWindow != IntPtr.Zero)
        {
#if !DEBUG
            ShowWindow(consoleWindow, SW_HIDE);
#endif
        }

        string appPath = AppDomain.CurrentDomain.BaseDirectory;
        string dataFolderPath = Path.Combine(appPath, "Data");

        if (!Directory.Exists(dataFolderPath))
        {
            Directory.CreateDirectory(dataFolderPath);
        }

        string defaultJsonPath = Path.Combine(dataFolderPath, "Default.json");

        if (!File.Exists(defaultJsonPath))
        {
            File.WriteAllText(defaultJsonPath, string.Empty);
        }
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new TrayApplicationContext(appPath, dataFolderPath));
    }
}

class TrayApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _trayIcon;
    private readonly string _dataFolderPath;
    private readonly string _appPath;

    public TrayApplicationContext(string appPath, string dataFolderPath)
    {
        _appPath = appPath;
        _dataFolderPath = dataFolderPath;

        var menu = new ContextMenuStrip();
        
        menu.Items.Add("设置", null, OpenExamSettings);
        menu.Items.Add("打开数据文件夹", null, OpenDataFolder);
        menu.Items.Add("重启", null, Restart);
        menu.Items.Add("退出", null, Exit);

        _trayIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
            Text = "ExamHelper",
            ContextMenuStrip = menu,
            Visible = true
        };

        _trayIcon.DoubleClick += OpenDataFolder;
    }

    private void OpenDataFolder(object? sender, EventArgs e)
    {
        try
        {
            Console.WriteLine($"[ExamHelper.Services] 打开数据文件夹: {_dataFolderPath}");
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = _dataFolderPath,
                UseShellExecute = true,
                Verb = "open"
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExamHelper.Services] 打开数据文件夹失败: {ex.Message}");
        }
    }

    private void OpenExamSettings(object? sender, EventArgs e)
    {
        string[] candidates = new[]
        {
            Path.Combine(_appPath, "ExamSettings.exe"),
            Path.Combine(_appPath, "ExamSettings", "ExamSettings.exe")
        };

        foreach (var c in candidates)
        {
            try
            {
                Console.WriteLine($"[ExamHelper.Services] 尝试打开设置文件: {c}");
                var full = Path.GetFullPath(c);
                if (File.Exists(full))
                {
                    Console.WriteLine($"[ExamHelper.Services] 找到设置文件: {full}");
                    StartExe(full);
                    return;
                }
                Console.WriteLine($"[ExamHelper.Services] 设置文件不存在: {full}");
            }
            catch { }
        }

        string[] projectRoots = new[]
        {
            Path.GetFullPath(Path.Combine(_appPath, "..", "ExamSettings")),
            Path.GetFullPath(Path.Combine(_appPath, "..", "..", "ExamSettings")),
            Path.GetFullPath(Path.Combine(_appPath, "ExamSettings")),
            Path.GetFullPath(Path.Combine(_appPath, ".."))
        };

        foreach (var projRoot in projectRoots.Distinct())
        {
            try
            {
                if (!Directory.Exists(projRoot))
                    continue;

                foreach (var conf in new[] { "Debug", "Release" })
                {
                    var binConf = Path.Combine(projRoot, "bin", conf);
                    if (!Directory.Exists(binConf))
                        continue;

                    foreach (var tfmDir in Directory.GetDirectories(binConf))
                    {
                        var exe = Path.Combine(tfmDir, "ExamSettings.exe");
                        if (File.Exists(exe))
                        {
                            StartExe(exe);
                            return;
                        }
                    }
                }
            }
            catch { }
        }

        string? found = FindInDescendants(_appPath, 3) ?? FindInDescendants(Path.GetFullPath(Path.Combine(_appPath, "..")), 4);
        if (found != null)
        {
            StartExe(found);
            return;
        }

        try
        {
            _trayIcon.ShowBalloonTip(5000, "未能打开设置", "找不到 ExamSettings 可执行文件。请确认应用程序是否完整。", ToolTipIcon.Warning);
        }
        catch { }
    }

    private void StartExe(string fullPath)
    {
        try
        {
            Console.WriteLine($"[ExamHelper.Services] 启动设置程序: {fullPath}");
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = fullPath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ExamHelper.Services] 启动设置程序失败: {ex.Message}");
            try
            {
                _trayIcon.ShowBalloonTip(5000, "启动失败", "无法启动 ExamSettings。", ToolTipIcon.Error);
            }
            catch { }
        }
    }

    private string? FindInDescendants(string root, int maxDepth)
    {
        try
        {
            root = Path.GetFullPath(root);
        }
        catch { return null; }

        var q = new Queue<(string dir, int depth)>();
        q.Enqueue((root, 0));

        while (q.Count > 0)
        {
            var (dir, depth) = q.Dequeue();
            try
            {
                foreach (var file in Directory.GetFiles(dir, "ExamSettings.exe"))
                {
                    return file;
                }
            }
            catch { }

            if (depth >= maxDepth) continue;

            try
            {
                foreach (var d in Directory.GetDirectories(dir))
                {
                    q.Enqueue((d, depth + 1));
                }
            }
            catch { }
        }

        return null;
    }

    private void Restart(object? sender, EventArgs e)
    {
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Application.Restart();
    }

    private void Exit(object? sender, EventArgs e)
    {
        _trayIcon.Visible = false;
        _trayIcon.Dispose();
        Application.Exit();
    }
}