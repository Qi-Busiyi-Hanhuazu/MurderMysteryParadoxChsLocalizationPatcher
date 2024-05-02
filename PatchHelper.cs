using Patcher.Asar;
using System;
using System.IO;
using System.IO.Compression;
using System.Windows;

namespace Patcher
{
  internal class PatchHelper
  {
    public static bool PatchIt(string originalPath, string patchPath)
    {
      originalPath = Path.GetFullPath(originalPath);
      var root = Path.GetPathRoot(originalPath);
      while (originalPath != root)
      {
        if (File.Exists(Path.Combine(originalPath, "MurderMysteryParadox.exe"))) { break; }
        originalPath = Path.GetDirectoryName(originalPath);
      }
      originalPath = Path.Combine(originalPath, "resources", "app.asar");

      var isDirectory = false;
      if (Directory.Exists(originalPath)) isDirectory = true;
      if (!CheckIfFileExists(originalPath)) return false;
      if (!CheckIfFileExists(patchPath)) return false;

      string extractPath;
      if (isDirectory)
      {
        extractPath = originalPath;
      }
      else
      {
        extractPath = originalPath + ".unpacked";
        Directory.CreateDirectory(extractPath);
      }

      // 解压补丁包
      var archiveStream = File.OpenRead(patchPath);
      var archive = new ZipArchive(archiveStream);
      foreach (ZipArchiveEntry file in archive.Entries)
      {
        string completeFileName = Path.GetFullPath(Path.Combine(extractPath, file.FullName));
        if (file.Name == "")
        {
          Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
          continue;
        }
        else
        {
          if (!Directory.Exists(Path.GetDirectoryName(completeFileName)))
          {
            Directory.CreateDirectory(Path.GetDirectoryName(completeFileName));
          }
          file.ExtractToFile(completeFileName, true);
        }
      }
      archiveStream.Close();

      if (!isDirectory)
      {
        var backupPath = originalPath + ".bak";
        if (!File.Exists(backupPath))
        {
          File.Move(originalPath, backupPath);
        }
        var asarArchive = new AsarArchive(File.OpenRead(originalPath + ".bak"));
        foreach (var fileName in asarArchive.FileList)
        {
          if (!File.Exists(Path.Combine(extractPath, fileName))) continue;
          Console.WriteLine(fileName);
          var file = asarArchive.GetFile(fileName);
          file.Unpacked = true;
          file.Offset = null;
          file.Size = new FileInfo(Path.Combine(extractPath, fileName)).Length;
        }
        asarArchive.WriteTo(File.OpenWrite(originalPath));
      }

      MessageBox.Show("已完成。", "完成");
      return true;
    }

    public static bool CheckIfFileExists(string filePath)
    {
      bool exists = File.Exists(filePath);
      if (!exists)
      {
        MessageBox.Show($"文件不存在：{filePath}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
      }
      return exists;
    }
  }
}
