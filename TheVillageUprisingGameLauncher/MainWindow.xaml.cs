﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Windows;

namespace GameLauncher
{
    enum LauncherStatus
    {
        ready,
        failed,
        downloadingGame,
        downloadingUpdate
    }

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string rootPath;
        private string versionFile;
        private string gameZip;
        private string gameExe;

        private LauncherStatus _status;
        internal LauncherStatus Status
        {
            get => _status;
            set
            {
                _status = value;
                switch (_status)
                {
                    case LauncherStatus.ready:
                        PlayButton.Content = "Play";
                        break;
                    case LauncherStatus.failed:
                        PlayButton.Content = "Update Failed - Retry";
                        break;
                    case LauncherStatus.downloadingGame:
                        PlayButton.Content = "Downloading Game";
                        break;
                    case LauncherStatus.downloadingUpdate:
                        PlayButton.Content = "Downloading Update";
                        break;
                    default:
                        break;
                }
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            rootPath = Directory.GetCurrentDirectory();
            versionFile = Path.Combine(rootPath, "Version.txt");
            gameZip = Path.Combine(rootPath, "Build.zip");
            gameExe = Path.Combine(rootPath, "CraftGame LabyrinthHerobrine Escape 3D", "labyrinth game 2.exe");
        }
        //The Village Uprising 2: Electric Boogaloo.exe

        private void CheckForUpdates()
        {
            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = localVersion.ToString();

                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("https://public.am.files.1drv.com/y4mGFKotmmP2i36zgvRrS2OqBfIR3LIv7MIcuyIcOSZj_kqDFuhOifwNvt8gAgZE_jGkTK3O5mPMirX1E3uojy2y1xCRzR-WkpEQ6GCJoC2yu0S_WLInscMm5IVv3zpydfTei6EAzD5uVbKbzS1NuMuRPK1NWAw8irRiQ58FWL7oDepqoj1xNiL4uI_1fcxp4_aTSbLkfjt37cURRnUIUPnISZFotZ76uINyeCC3eWl520?AVOverride=1"));

                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        InstallGameFiles(true, onlineVersion);
                    }
                    else
                    {
                        Status = LauncherStatus.ready;
                    }
                }
                catch (Exception ex)
                {
                    Status = LauncherStatus.failed;
                    MessageBox.Show($"Error checking for game updates: {ex}");
                }
            }
            else
            {
                InstallGameFiles(false, Version.zero);
            }
        }

        private void InstallGameFiles(bool _isUpdate, Version _onlineVersion)
        {
            try
            {
                WebClient webClient = new WebClient();
                if (_isUpdate)
                {
                    Status = LauncherStatus.downloadingUpdate;
                }
                else
                {
                    Status = LauncherStatus.downloadingGame;
                    _onlineVersion = new Version(webClient.DownloadString("https://public.am.files.1drv.com/y4mGFKotmmP2i36zgvRrS2OqBfIR3LIv7MIcuyIcOSZj_kqDFuhOifwNvt8gAgZE_jGkTK3O5mPMirX1E3uojy2y1xCRzR-WkpEQ6GCJoC2yu0S_WLInscMm5IVv3zpydfTei6EAzD5uVbKbzS1NuMuRPK1NWAw8irRiQ58FWL7oDepqoj1xNiL4uI_1fcxp4_aTSbLkfjt37cURRnUIUPnISZFotZ76uINyeCC3eWl520?AVOverride=1"));
                }
                //https://download1591.mediafire.com/dlvn6gydx7hgKkLyTZqgB_BHDcexbvbLWH16QTZaX-QUwmTWYI2MjQFQlkqcziZFHlGS0ddOHTxWp6aAnJodiOQ_427B/eyku0dcqphi1drh/CraftGame+Labyrinth+Herobrine+Escape3D.zip
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameCompletedCallback);
                webClient.DownloadFileAsync(new Uri("https://public.am.files.1drv.com/y4mDyyDOWtXzB6euy3T4dkc0JHLI68qQck79tctil1_qz-poGfSlEJf_wpv-R73BhdAaRDVALfCX0HILNU2iGCNYLV3NBx13ijhHusRPojNBOeGqaJvJ3gh3XlrRLK-_uDouYO3lVwdB6e3qWBceusHmDUtIwQ-_2F9PGdZk7dRuf022o2QpqaNkg9OOgnYcR9pDjYK9pt_2SYBmSE1RkaekevZHRklE9WEy1V_Btkcm0k?AVOverride=1"), gameZip, _onlineVersion);
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error installing game files: {ex}");
            }
        }

        private void DownloadGameCompletedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                string onlineVersion = ((Version)e.UserState).ToString();
                ZipFile.ExtractToDirectory(gameZip, rootPath, true);
                File.Delete(gameZip);

                File.WriteAllText(versionFile, onlineVersion);

                VersionText.Text = onlineVersion;
                Status = LauncherStatus.ready;
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
                MessageBox.Show($"Error finishing download: {ex}");
            }
        }

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            CheckForUpdates();
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            if (File.Exists(gameExe) && Status == LauncherStatus.ready)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                startInfo.WorkingDirectory = Path.Combine(rootPath, "CraftGame LabyrinthHerobrine Escape 3D");
                Process.Start(startInfo);

                Close();
            }
            else if (Status == LauncherStatus.failed)
            {
                CheckForUpdates();
            }
        }
    }

    struct Version
    {
        internal static Version zero = new Version(0, 0, 0);

        private short major;
        private short minor;
        private short subMinor;

        internal Version(short _major, short _minor, short _subMinor)
        {
            major = _major;
            minor = _minor;
            subMinor = _subMinor;
        }
        internal Version(string _version)
        {
            string[] versionStrings = _version.Split('.');
            if (versionStrings.Length != 3)
            {
                major = 0;
                minor = 0;
                subMinor = 0;
                return;
            }

            major = short.Parse(versionStrings[0]);
            minor = short.Parse(versionStrings[1]);
            subMinor = short.Parse(versionStrings[2]);
        }

        internal bool IsDifferentThan(Version _otherVersion)
        {
            if (major != _otherVersion.major)
            {
                return true;
            }
            else
            {
                if (minor != _otherVersion.minor)
                {
                    return true;
                }
                else
                {
                    if (subMinor != _otherVersion.subMinor)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override string ToString()
        {
            return $"{major}.{minor}.{subMinor}";
        }
    }
}
