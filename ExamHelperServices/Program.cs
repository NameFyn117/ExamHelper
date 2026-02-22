using System;
using System.IO;

class Program
{
    static void Main()
    {
        string appPath = AppDomain.CurrentDomain.BaseDirectory;

        string dataFolderPath = Path.Combine(appPath, "Data");

        if (!Directory.Exists(dataFolderPath))
        {
            Directory.CreateDirectory(dataFolderPath);
            Console.WriteLine("Data 文件夹已创建。");
        }
        else
        {
            Console.WriteLine("Data 文件夹已存在。");
        }
        string defaultJsonPath = Path.Combine(dataFolderPath, "Default.json");

        if (!File.Exists(defaultJsonPath))
        {
            File.WriteAllText(defaultJsonPath, string.Empty);
            Console.WriteLine("Default.json 文件已创建。");
        }
        else
        {
            Console.WriteLine("Default.json 文件已存在。");
        }

        Console.WriteLine("操作完成。");
        Console.ReadKey();
    }
}