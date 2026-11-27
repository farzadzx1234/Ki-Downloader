using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.Win32;

namespace KiDownloaderSetup
{
    public class SetupForm : Form
    {
        private Label _statusLabel;
        private ProgressBar _progress;
        private CheckBox _desktopCheck;
        private CheckBox _launchCheck;
        private Button _installBtn;
        private string _installDir;

        public SetupForm()
        {
            _installDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programs", "Ki-Downloader");

            Text = "Ki-Downloader Installer";
            Width = 460;
            Height = 290;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Color.FromArgb(13, 13, 15);
            ForeColor = Color.White;
            Font = new Font("Segoe UI", 9.5f);

            try { Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath); } catch { }

            Label title = new Label
            {
                Text = "Ki-Downloader v1.0",
                Font = new Font("Segoe UI", 16f, FontStyle.Bold),
                Location = new Point(24, 22),
                AutoSize = true
            };
            Label sub = new Label
            {
                Text = "Fast parallel Windows download manager\nInstall path: " + _installDir,
                ForeColor = Color.FromArgb(160, 165, 175),
                Location = new Point(26, 58),
                Size = new Size(400, 40)
            };

            _desktopCheck = new CheckBox
            {
                Text = "Create Desktop shortcut",
                Checked = true,
                Location = new Point(26, 108),
                AutoSize = true
            };
            _launchCheck = new CheckBox
            {
                Text = "Launch Ki-Downloader after installation",
                Checked = true,
                Location = new Point(26, 134),
                AutoSize = true
            };

            _progress = new ProgressBar
            {
                Location = new Point(26, 170),
                Size = new Size(392, 16),
                Style = ProgressBarStyle.Continuous
            };

            _statusLabel = new Label
            {
                Text = "Ready to install.",
                ForeColor = Color.FromArgb(160, 165, 175),
                Location = new Point(26, 194),
                Size = new Size(270, 22)
            };

            _installBtn = new Button
            {
                Text = "Install",
                BackColor = Color.FromArgb(10, 132, 255),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Location = new Point(318, 198),
                Size = new Size(100, 34),
                Cursor = Cursors.Hand
            };
            _installBtn.FlatAppearance.BorderSize = 0;
            _installBtn.Click += delegate { RunInstall(); };

            Controls.AddRange(new Control[] { title, sub, _desktopCheck, _launchCheck, _progress, _statusLabel, _installBtn });
        }

        private void RunInstall()
        {
            _installBtn.Enabled = false;
            try
            {
                foreach (Process p in Process.GetProcessesByName("Ki-Downloader"))
                {
                    try { p.Kill(); p.WaitForExit(1500); } catch { }
                }

                _statusLabel.Text = "Extracting application files...";
                _progress.Value = 30;
                Application.DoEvents();

                if (!Directory.Exists(_installDir)) Directory.CreateDirectory(_installDir);

                using (Stream resStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("payload.zip"))
                {
                    if (resStream == null) throw new Exception("Embedded payload.zip missing.");
                    using (ZipArchive archive = new ZipArchive(resStream, ZipArchiveMode.Read))
                    {
                        int total = Math.Max(1, archive.Entries.Count);
                        int done = 0;
                        foreach (ZipArchiveEntry entry in archive.Entries)
                        {
                            if (string.IsNullOrEmpty(entry.Name)) continue;
                            string dest = Path.Combine(_installDir, entry.FullName);
                            string dir = Path.GetDirectoryName(dest);
                            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                            entry.ExtractToFile(dest, true);
                            done++;
                            _progress.Value = Math.Min(90, 30 + (done * 60 / total));
                            Application.DoEvents();
                        }
                    }
                }

                string exePath = Path.Combine(_installDir, "Ki-Downloader.exe");
                string icoPath = Path.Combine(_installDir, "assets", "ki-downloader.ico");

                string uninstallExe = Path.Combine(_installDir, "Uninstall.exe");
                try { File.Copy(Application.ExecutablePath, uninstallExe, true); } catch { }

                string startMenu = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Ki-Downloader.lnk");
                CreateShortcut(startMenu, exePath, _installDir, icoPath);

                if (_desktopCheck.Checked)
                {
                    string desktop = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Ki-Downloader.lnk");
                    CreateShortcut(desktop, exePath, _installDir, icoPath);
                }

                using (RegistryKey key = Registry.CurrentUser.CreateSubKey(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\Ki-Downloader"))
                {
                    key.SetValue("DisplayName", "Ki-Downloader");
                    key.SetValue("DisplayVersion", "1.0.0");
                    key.SetValue("Publisher", "Farzad Alizadeh");
                    key.SetValue("DisplayIcon", icoPath);
                    key.SetValue("InstallLocation", _installDir);
                    key.SetValue("UninstallString", "\"" + uninstallExe + "\" /uninstall");
                    key.SetValue("NoModify", 1, RegistryValueKind.DWord);
                    key.SetValue("NoRepair", 1, RegistryValueKind.DWord);
                }

                _progress.Value = 100;
                _statusLabel.Text = "Installed successfully!";
                if (_launchCheck.Checked && File.Exists(exePath))
                {
                    Process.Start(exePath);
                }
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Installation failed: " + ex.Message, "Ki-Downloader Setup", MessageBoxButtons.OK, MessageBoxIcon.Error);
                _installBtn.Enabled = true;
            }
        }

        private static void CreateShortcut(string lnkPath, string targetPath, string workDir, string iconPath)
        {
            Type shellType = Type.GetTypeFromProgID("WScript.Shell");
            dynamic shell = Activator.CreateInstance(shellType);
            dynamic shortcut = shell.CreateShortcut(lnkPath);
            shortcut.TargetPath = targetPath;
            shortcut.WorkingDirectory = workDir;
            if (File.Exists(iconPath)) shortcut.IconLocation = iconPath;
            shortcut.Save();
        }

        public static void RunUninstall()
        {
            string installDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "Programs", "Ki-Downloader");
            foreach (Process p in Process.GetProcessesByName("Ki-Downloader"))
            {
                try { p.Kill(); p.WaitForExit(1500); } catch { }
            }
            try { File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Programs), "Ki-Downloader.lnk")); } catch { }
            try { File.Delete(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "Ki-Downloader.lnk")); } catch { }
            try { Registry.CurrentUser.DeleteSubKeyTree(@"Software\Microsoft\Windows\CurrentVersion\Uninstall\Ki-Downloader", false); } catch { }
            MessageBox.Show("Ki-Downloader has been uninstalled.\nShortcuts and registry entries removed.", "Ki-Downloader", MessageBoxButtons.OK, MessageBoxIcon.Information);
            try { Process.Start("cmd.exe", "/c timeout /t 2 >nul & rmdir /s /q \"" + installDir + "\""); } catch { }
        }

        [STAThread]
        private static void Main(string[] args)
        {
            if (args != null && args.Length > 0 && args[0].Equals("/uninstall", StringComparison.OrdinalIgnoreCase))
            {
                RunUninstall();
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SetupForm());
        }
    }
}
