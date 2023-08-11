using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace TL2_Mikuro
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
 


        private bool editorDLLInitSucess;

        private string preBuildModDatFilePath = "";
        private bool buildResult;

        private string preUnPackModFilePath = "";
        private string preUnpackTargetDir = "";


        Task unpackTask;
        Task packTask;

        public MainWindow()
        {
            InitializeComponent();
            InitModProjectComboxList();
            editorDLLInitSucess = false;
            InitEditorDLL();
        }

        private void InitModProjectComboxList()
        {
            DirectoryInfo modsDirectory = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory + @"\mods");
            if (modsDirectory.Exists)
            {
                DirectoryInfo[] subfolders = modsDirectory.GetDirectories();
                // Sort subfolders by last write time (most recent first)
                var sortedSubfolders = subfolders.OrderByDescending(folder => folder.LastWriteTime);

                List<string> modList = new List<string>();

                foreach (var subfolder in sortedSubfolders)
                {
                    modList.Add(subfolder.FullName);
                }
                _UI_ModProjectList.ItemsSource = modList.ToArray();
            }
            else
            {
                UpdateStatusBar("Default mods folder not found");
            }
        }

        private void ModProjectSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string selectedModProject = (string)_UI_ModProjectList.SelectedItem;
            if (!string.IsNullOrEmpty(selectedModProject))
            {
                preBuildModDatFilePath = selectedModProject + "\\MOD.DAT";
                _UI_PreBuildModDatFilePath.Text = preBuildModDatFilePath;
            }
        }

        private void FileDropStackPanel_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);

                if (files != null && files.Length == 1)
                {
                    preUnPackModFilePath = files[0];
                    string fileExt = Path.GetExtension(preUnPackModFilePath);
                    if (fileExt.ToUpper() == ".MOD")
                    {
                        _UI_PreUnpackModFilePath.Text = "MOD Path: " + preUnPackModFilePath;
                        _UI_TargetUnPackDirectory.Text = AppDomain.CurrentDomain.BaseDirectory + @"\mods\" + Path.GetFileNameWithoutExtension(preUnPackModFilePath);
                    }
                    else
                    {
                        UpdateStatusBar("Invalid MOD File");
                    }
                }
            }
        }
        private void UpdateStatusBar(string text)
        {
            Dispatcher.Invoke(() => StatusBarText.Text = text);
        }

        private void InitEditorDLL()
        {
            Task initTask = Task.Run(() =>
            {
                try
                {
                    UpdateStatusBar("Initilizing the dll");
                    Stopwatch stopwatch = new Stopwatch();
                    stopwatch.Start();
                    // Get the thread ID of the current thread
                    int threadId = ExternalMethod.GetCurrentThreadId();

                    // Get the handle of the current thread
                    IntPtr threadHandle = ExternalMethod.OpenThread(ExternalMethod.ThreadAccessRights.All, false, (uint)threadId);

                    //int rsCode = EditorDLL.InitEditor(threadId, threadHandle.ToInt32());
                    int rsCode = EditorDLL.InitEditor(Marshal.GetHINSTANCE(typeof(Task).Module).ToInt32(), threadHandle.ToInt32());
                    if (rsCode == 1)
                    {
                        UpdateStatusBar("DLL init finished, wave your spell now");
                        editorDLLInitSucess = true;
                        stopwatch.Stop();
                        UpdateConsoleText("Init DLL cost: " + stopwatch.ElapsedMilliseconds + "ms", false);
                    }
                    else
                    {
                        UpdateStatusBar("DLL init fail:(");
                    }
                }
                catch (Exception e)
                {
                    UpdateStatusBar(e.Message);

                }
                //finally
                //{
                //    ExternalMethod.CloseHandle(threadHandle);
                //}
            });
        }

        private void UpdateConsoleText(string newText, bool clean)
        {
            if (clean)
            {
                Dispatcher.Invoke(() => _UI_ConsoleTextBox.Text = "");
            }

            if (!string.IsNullOrEmpty(newText))
            {
                string preUpdateText = _UI_ConsoleTextBox.Text + newText + "\n";
                Dispatcher.Invoke(() => _UI_ConsoleTextBox.Text = preUpdateText);
            }

        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (editorDLLInitSucess)
            {
                _ = EditorDLL.ShutdownEditor();
            }
        }


        private async void TriggerPackMod(object sender, RoutedEventArgs e)
        {
            packTask = Task.Run(() =>
            {
                Dispatcher.Invoke(() => _UI_BuildButton.IsEnabled = false);
                buildResult = EditorDLL.CreateMod(preBuildModDatFilePath, true);
            });
            await Task.Run(async () =>
            {
                string dots = "";

                while (!packTask.IsCompleted)
                {
                    dots = (dots.Length < 6 ? dots + "." : ".");
                    UpdateStatusBar("Building MOD" + dots);
                    await Task.Delay(200);
                }
                UpdateStatusBar("Build " + (buildResult ? "sucess" : "fail"));
                Dispatcher.Invoke(() => _UI_BuildButton.IsEnabled = true);
            });
        }

        private async void TriggerUnpackMod(object sender, RoutedEventArgs e)
        {
            unpackTask = Task.Run(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    _UI_UnpackButton.IsEnabled = false;
                    _UI_ConsoleTextBox.Text = "";
                });
                EditorDLL.EditorUncompressPak(preUnPackModFilePath, preUnpackTargetDir);
            });

            await Task.Run(async () =>
            {
                while (!unpackTask.IsCompleted)
                {
                    await Task.Delay(50);
                    int max = EditorDLL.EditorGetUncompressFileCountMax();
                    int current = EditorDLL.EditorGetUncompressFileCount();
                    UpdateStatusBar("Unpacking: " + current.ToString() + "/" + max.ToString());
                }
                UpdateStatusBar("Unpack finished");
                Dispatcher.Invoke(() => _UI_UnpackButton.IsEnabled = true);
            });
        }

        private void TargetDirectory_TextChanged(object sender, TextChangedEventArgs e)
        {
            preUnpackTargetDir = _UI_TargetUnPackDirectory.Text;
        }
    }
}
