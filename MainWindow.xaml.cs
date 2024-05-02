using Microsoft.Win32;
using System;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Patcher
{
  /// <summary>
  /// MainWindow.xaml 的交互逻辑
  /// </summary>
  public partial class MainWindow : Window
  {
    public MainWindow()
    {
      string[] args = Environment.GetCommandLineArgs();
      if (args.Length == 3)
      {
        PatchHelper.PatchIt(args[1], args[2]);
        Environment.Exit(0);
      }
      else
      {
        InitializeComponent();
      }
    }

    private void Button1_Click(object sender, RoutedEventArgs e)
    {
      var originalPath = "";
      try { originalPath = Path.GetDirectoryName(textBox1.Text); } catch { }
      OpenFileDialog ofd = new OpenFileDialog
      {
        Title = "游戏可执行文件",
        Filter = "MurderMysteryParadox.exe|MurderMysteryParadox.exe|Exe 文件|*.exe|所有文件|*.*",
        InitialDirectory = originalPath,
      };
      ofd.ShowDialog();
      if (!string.IsNullOrEmpty(ofd.FileName)) { textBox1.Text = ofd.FileName; }
    }

    private void Button2_Click(object sender, RoutedEventArgs e)
    {
      var originalPath = "";
      try { originalPath = Path.GetDirectoryName(textBox2.Text); } catch { }
      OpenFileDialog ofd = new OpenFileDialog
      {
        Title = "补丁包",
        Filter = "补丁包|*.zip;*.xzp|所有文件|*.*",
        InitialDirectory = originalPath,
      };
      ofd.ShowDialog();
      if (!string.IsNullOrEmpty(ofd.FileName)) { textBox2.Text = ofd.FileName; }
    }

    private void ButtonConfirm_Click(object sender, RoutedEventArgs e)
    {
      string buttonConfirmText = (string)buttonConfirm.Content;
      buttonConfirm.Content = "……";
      buttonConfirm.IsEnabled = false;
      string originalPath = textBox1.Text;
      string patchPath = textBox2.Text;
      Thread thread = new Thread(() =>
      {
        try
        {
          PatchHelper.PatchIt(originalPath, patchPath);
        }
        catch (Exception ex)
        {
          MessageBox.Show($"错误：{ex.Message}\n{ex.StackTrace}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
        buttonConfirm.Dispatcher.Invoke(DispatcherPriority.Normal, (ThreadStart)delegate ()
        {
          buttonConfirm.Content = buttonConfirmText;
          buttonConfirm.IsEnabled = true;
        });
      });
      thread.Start();
    }

    private void TextBox_DragDrop(object sender, DragEventArgs e)
    {
      string filePath = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
      if (PatchHelper.CheckIfFileExists(filePath))
      {
        ((TextBox)sender).Text = filePath;
      }
    }

    private void TextBox_DragEnter(object sender, DragEventArgs e)
    {
      if (e.Data.GetDataPresent(DataFormats.FileDrop) && ((Array)e.Data.GetData(DataFormats.FileDrop)).Length == 1)
      {
        e.Effects = DragDropEffects.Link;
      }
      else
      {
        e.Effects = DragDropEffects.None;
      }
    }

    private void TextBox_PreviewDragOver(object sender, DragEventArgs e)
    {
      e.Handled = true;
    }
  }
}
