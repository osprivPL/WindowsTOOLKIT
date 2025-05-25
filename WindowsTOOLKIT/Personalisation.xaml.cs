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
            new List<((string Key, string valueName)registry, string type)>
            {
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "SystemUsesLightTheme"),
                    "dword"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Themes\Personalize", "AppsUseLightTheme"),
                    "dword"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowTaskViewButton"),
                    "dword"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "SearchboxTaskbarMode"),
                    "dword"),
                ((@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", "VerboseStatus"),
                    "dword"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\ContentDeliveryManager",
                    "SystemPaneSuggestionsEnabled"), "dword"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced",
                    "ShowSecondsInSystemClock"), "dword"),
                ((@"HKEY_CURRENT_USER\Control Panel\Mouse", "MouseSpeed"), "REG_SZ"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "HideFileExt"),
                    "dword"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "Hidden"), "dword"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "ShowSuperHidden"),
                    "dword"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced", "TaskbarAl"),
                    "dword"),
                ((
                    @"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Explorer\Advanced\TaskbarDeveloperSettings",
                    "TaskbarEndTask"), "dword"),
                ((@"HKEY_CURRENT_USER\Software\Microsoft\Windows\CurrentVersion\Search", "BingSearchEnabled"), "dword"),
                ((@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\Explorer",
                    "SettingsPageVisibility"), "REG_SZ")
            };

        List<bool> cbBefore = new List<bool>();


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
                    if (cb.Name == "CbHidden")
                    {
                        int[] values = new int[2] { -1, -1 };
                        try
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                proc = new Process
                                {
                                    StartInfo = new ProcessStartInfo
                                    {
                                        FileName = "reg",
                                        Arguments =
                                            $"query \"{Keys[index].Item1.Item1}\" /v \"{Keys[index].Item1.Item2}\"",
                                        RedirectStandardOutput = true,
                                        UseShellExecute = false,
                                        CreateNoWindow = true
                                    }
                                };
                                proc.Start();
                                output = proc.StandardOutput.ReadToEnd();
                                proc.WaitForExit();
                                parts = output.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                                values[i] = int.Parse(parts[parts.Length - 1][2].ToString());
                                // MessageBox.Show(cb.Name+ ": " + parts[parts.Length - 1]);
                                index++;
                            }
                        }
                        catch (Exception)
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                proc = new Process
                                {
                                    StartInfo = new ProcessStartInfo
                                    {
                                        FileName = "reg",
                                        Arguments =
                                            $"add \"{Keys[index].Item1.Item1}\" /v \"{Keys[index].Item1.Item2}\" /t {Keys[index].Item2} /d 0 /f",
                                        RedirectStandardOutput = true,
                                        UseShellExecute = false,
                                        CreateNoWindow = true
                                    }
                                };
                                proc.Start();
                                proc.WaitForExit();
                            }
                        }


                        if (values[0] == 1 && values[1] == 1)
                        {
                            cb.IsChecked = true;
                        }
                        else
                        {
                            cb.IsChecked = false;
                        }

                        if (values[0] + values[1] > 0)
                        {
                            cbBefore.Add(cb.IsChecked == true);
                            continue;
                        }
                    }
                    else
                    {
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
                        }
                        catch (Exception)
                        {
                            proc = new Process
                            {
                                StartInfo = new ProcessStartInfo
                                {
                                    FileName = "reg",
                                    Arguments = $"add \"{Keys[index].Item1.Item1}\" /v \"{Keys[index].Item1.Item2}\" /t {Keys[index].Item2} /d 0 /f",
                                    RedirectStandardOutput = true,
                                    UseShellExecute = false,
                                    CreateNoWindow = true
                                }
                            };
                            proc.Start();
                            output = proc.StandardOutput.ReadToEnd();
                            proc.WaitForExit();
                        }
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

        private void BtnSave_click(object sender, RoutedEventArgs e)
        {
            int index = 0;
            foreach (var elem in GPersonalisation.Children)
            {
                if (elem is CheckBox cb)
                {
                    bool state = cb.IsChecked == true;
                    if (state != cbBefore[index])
                    {
                        if (cb.Name == "CbHidden")
                        {
                            for (int i = 0; i < 2; i++)
                            {
                                Process proc = new Process
                                {
                                    StartInfo = new ProcessStartInfo
                                    {
                                        FileName = "reg",
                                        Arguments =
                                            $"add \"{Keys[index].Item1.Item1}\" /v \"{Keys[index].Item1.Item2}\" /t {Keys[index].Item2} /d" +
                                            (state ? "1" : "0") + " /f",
                                        RedirectStandardOutput = false,
                                        UseShellExecute = false,
                                        CreateNoWindow = true
                                    }
                                };
                                proc.Start();
                                proc.WaitForExit();
                                index++;
                            }
                        }
                    }
                }
            }
        }
    }
}