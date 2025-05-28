using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;

namespace WindowsTOOLKIT
{
    public partial class Personalisation : Window
    {
        private static readonly List<((string, string), string)> Keys =
            new List<((string Key, string valueName) registry, string type)>
            {
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme"),
                    "REG_DWORD"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme"),
                    "REG_DWORD"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton"),
                    "REG_DWORD"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode"),
                    "REG_DWORD"),
                ((@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "VerboseStatus"),
                    "REG_DWORD"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
                    "SystemPaneSuggestionsEnabled"), "REG_DWORD"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                    "ShowSecondsInSystemClock"), "REG_DWORD"),
                ((@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSpeed"), "REG_SZ"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt"),
                    "REG_DWORD"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden"),
                    "REG_DWORD"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl"),
                    "REG_DWORD"),
                ((
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings",
                    "TaskbarEndTask"), "REG_DWORD"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled"), "dword"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                    "SettingsPageVisibility"), "REG_SZ")
            };

        List<bool?> cbBefore = new List<bool?>();
        bool closedByProgram = false;


        public Personalisation()
        {
            InitializeComponent();
        }

        private void WPersonalisation_Loaded(object sender, RoutedEventArgs e)
        {
            int index = 0;
            foreach (UIElement elem in GPersonalisation.Children)
            {
                if (elem is CheckBox cb)
                {
                    string output = "";
                    Process proc;
                    // string parts[] = null;
                    string[] parts;
                    try
                    {
                        proc = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "reg",
                                Arguments = $"query \"{Keys[index].Item1.Item1}\" /v \"{Keys[index].Item1.Item2}\"",
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };
                        proc.Start();
                        output = proc.StandardOutput.ReadToEnd();
                        proc.WaitForExit();

                        if (output == "ERROR: The system was unable to find the specified registry key or value." ||
                            output == "\r\n\r\n")
                        {
                            throw new Exception();
                        }
                    }
                    catch (Exception)
                    {
                        proc = new Process
                        {
                            StartInfo = new ProcessStartInfo
                            {
                                FileName = "reg",
                                Arguments =
                                    $"add \"{Keys[index].Item1.Item1}\" /v \"{Keys[index].Item1.Item2}\" /t {Keys[index].Item2} /d " +
                                    (Keys[index].Item2 == "REG_DWORD" ? "0" : "\"\"") + " /f",
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            }
                        };
                        proc.Start();
                        output = proc.StandardOutput.ReadToEnd();
                        proc.WaitForExit();
                    }


                    parts = output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    // MessageBox.Show(cb.Name+ ": " + parts[parts.Length - 1]);
                    if (cb.Name == "CbHomePage")
                    {
                        if (parts[parts.Length - 1].Contains("hide:home"))
                        {
                            cb.IsChecked = false;
                        }
                        else
                        {
                            cb.IsChecked = true;
                        }
                    }
                    else if (parts[parts.Length - 1].Contains("0x1") || parts[parts.Length - 1].Contains("1"))
                    {
                        cb.IsChecked = true;
                    }
                    else if (parts[parts.Length - 1].Contains("0x0") || parts[parts.Length - 1].Contains("0"))
                    {
                        cb.IsChecked = false;
                    }

                    cbBefore.Add(cb.IsChecked == true);


                    index++;
                }
            }
        }


        private void BtnBack_click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void BtnSave_click(object sender, RoutedEventArgs e)
        {
            var cbIndex = cbBefore.Count - 1;
            List<bool?> cbAfter = new List<bool?>();

            // Przygotowanie listy do porownania nowego stanu z poprzednim
            for (int i = 0; i < cbBefore.Count; i++)
            {
                cbAfter.Add(null);
            }

            for (int i = GPersonalisation.Children.Count - 1; i >= 0; i--)
            {
                var elem = GPersonalisation.Children[i];

                if (!(elem is CheckBox) && !(elem is Label))
                {
                    continue;
                }

                if (elem is CheckBox cb)
                {
                    cbAfter[cbIndex] = cb.IsChecked == true;
                    cbIndex--;
                }

                GPersonalisation.Children.Remove(elem);
            }


            int index = 0;
            foreach (var elem in cbAfter)
            {
                if (cbAfter[index] != cbBefore[index])
                {
                    if (index == 13)
                    {
                        await Task.Run(() =>
                        {
                            Process proc = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "reg",
                                    Arguments = $"add \"{Keys[index].Item1.Item1}\" /v \"{Keys[index].Item1.Item2}\" /t {Keys[index].Item2} /d " +
                                                ((bool)cbAfter[index] ? "hide: home" : "") + " /f",
                                    RedirectStandardOutput = false,
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                }
                            };
                            proc.Start();
                            proc.WaitForExit();
                        });
                    }
                    else
                    {
                        await Task.Run(() =>
                        {
                            Process proc = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "reg",
                                    Arguments =
                                        $"add \"{Keys[index].Item1.Item1}\" /v \"{Keys[index].Item1.Item2}\" /t {Keys[index].Item2} /d " +
                                        ((bool)cbAfter[index] ? "1" : "0") + " /f",
                                    RedirectStandardOutput = false,
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                }
                            };
                            proc.Start();
                            proc.WaitForExit();
                        });
                    }
                }
                index++;
            }

            this.Topmost = true;
            this.Activate();
            MessageBox.Show("Zapisano zmiany. Zrestartuj komputer, aby zmiany zostały zastosowane");
            closedByProgram = true;
            this.Topmost = false;
            this.Close();
        }

        private void WPersonalisation_Closed(object sender, EventArgs e)
        {
            if (!closedByProgram)
            {
                closedByProgram = false;
                MessageBox.Show("Zamknięcie okna będzie możliwe po zakończeniu operacji");
            }
        }
    }
}