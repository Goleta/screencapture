//
// Copyright © 2005 - 2008 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.ScreenCapture
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.ComponentModel;
    using System.Security.Permissions;
    using System.Runtime.InteropServices;

    using Goletas.Win32;
	using Goletas.ScreenCapture;
	using Goletas.ScreenCapture.Drawing.Imaging;

	/// <summary>
	/// The settings form of the Screen Capture program.
	/// </summary>
	public sealed class SettingsForm : System.Windows.Forms.Form
	{
        [Flags]
        private enum SettingsItems : uint
        {
            None = 0,
            IncludeCursor = 0x1,
            AutoStartup = 0x2,
            AlwaysOnTop = 0x4,
            ImageDestinations = 0x8,
            WorkingDirectory = 0x10,
            FileName = 0x20,
            UseFileOverwrite = 0x40,
            UseExternalApp = 0x80,
            ExternalApp = 0x100,
            FileType = 0x200,
            ImageQuality = 0x400
        }

		private Settings _Settings;
		private SettingsItems _ChangedItems;
        private Guid _CurrentFormatId;

        private System.Windows.Forms.CheckBox _UseFileOverwrite;
        private System.Windows.Forms.Label _ForegroundWindowKey;
        private System.Windows.Forms.ComboBox _ForegroundWindowKeySelector;
        private System.Windows.Forms.Button _WorkingDirectorySelector;
        private System.Windows.Forms.TextBox _WorkingDirectoryPath;
        private System.Windows.Forms.Label _WorkingDirectory;
        private System.Windows.Forms.Button _Cancel;
        private System.Windows.Forms.Button _OK;
        private System.Windows.Forms.Button _Apply;
        private System.Windows.Forms.Button _FileTypeSettings;
        private System.Windows.Forms.TabControl _Pages;
        private System.Windows.Forms.TabPage _GeneralPage;
        private System.Windows.Forms.TabPage _PreferencesPage;
        private System.Windows.Forms.TabPage _DestinationPage;
        private System.Windows.Forms.TabPage _ImagePage;
        private System.Windows.Forms.TabPage _AboutPage;
        private Goletas.ScreenCapture.GroupLabel _ImageCompositionGroup;
        private System.Windows.Forms.CheckBox _IncludeCursor;
        private System.Windows.Forms.CheckBox _AutoStartup;
        private Goletas.ScreenCapture.GroupLabel _ImageDestinationGroup;
        private System.Windows.Forms.Label _FileName;
        private System.Windows.Forms.TextBox _FileNameSelector;
        private System.Windows.Forms.Button _ExternalAppSelector;
        private System.Windows.Forms.TextBox _ExternalAppPath;
        private System.Windows.Forms.CheckBox _UseExternalApp;
        private Goletas.ScreenCapture.GroupLabel _HotKeysGroup;
        private Goletas.ScreenCapture.GroupLabel _FileFormatGroup;
        private Goletas.ScreenCapture.GroupLabel _AppGroup;
        private System.Windows.Forms.CheckBox _AlwaysOnTop;
        private System.Windows.Forms.ComboBox _SettingsKeySelector;
        private System.Windows.Forms.Label _SettingsKey;
        private System.Windows.Forms.ComboBox _VirtualScreenKeySelector;
        private System.Windows.Forms.Label _VirtualScreenKey;
        private System.Windows.Forms.ComboBox _CurrentScreenKeySelector;
        private System.Windows.Forms.Label _CurrentScreenKey;
        private System.Windows.Forms.ComboBox _ImageDestinationSelector;
        private System.Windows.Forms.ComboBox _FileFormatSelector;
        private System.Windows.Forms.Label _HowToUseLabel;
        private System.Windows.Forms.Label _LegalInfo;
        private Goletas.ScreenCapture.GroupLabel _FileGroup;
        private System.Windows.Forms.LinkLabel _RemoveApp;

        private static void OnHelpRequested(object sender, System.Windows.Forms.HelpEventArgs hlpevent)
        {
            System.Windows.Forms.Control parent = (System.Windows.Forms.Control)sender;
            string message = ApplicationManager.GetString((string)parent.Tag); // "No help message is associated with this item."

            if ((message != null) && (message.Length != 0))
            {
                NativeMethods.ShowHelp(parent, message);
            }

            hlpevent.Handled = true;
        }

        internal static void SetHelp(System.Windows.Forms.Control control, string resourceId)
        {
            control.Tag = resourceId;
            control.HelpRequested += new System.Windows.Forms.HelpEventHandler(OnHelpRequested);
        }

        protected override void OnLoad(System.EventArgs e)
        {
            base.OnLoad(e);

            // This brings the form on top when the window is hidden
            // and the user clicks on the executable within Windows explorer
            NativeMethods.BringToTop(new HandleRef(this, this.Handle), true);

            if (this._Settings.AlwaysOnTop)
            {
                // TopMost is not reliable, the window must be active for the TopMost to work!
                this.TopMost = true;
            }
        }

        [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
        protected override void WndProc(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == ApplicationManager.WM_APP_BRING_TO_TOP)
            {
                this.BringToTop();
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        private void BringToTop()
        {
            IntPtr Handle;

            if (!(this.IsHandleCreated && ((Handle = this.Handle) != NativeMethods.GetForegroundWindow()) && NativeMethods.BringToTop(new HandleRef(this, Handle), true)))
            {
                NativeMethods.MessageBeep(NativeMethods.MB_DEFAULT);
            }
        }

        private void OnTrackChanges(SettingsItems item, bool turnOn)
        {
            if (turnOn)
            {
                this._ChangedItems |= item;
            }
            else
            {
                this._ChangedItems &= ~item;
            }

            this._Apply.Enabled = (this._ChangedItems != SettingsItems.None);
        }

        private ImageDestinations _ImageDestinations
        {
            get
            {
                return (ImageDestinations)(this._ImageDestinationSelector.SelectedIndex + 1);
            }
            set
            {
                this._ImageDestinationSelector.SelectedIndex = (int)value  - 1;
            }
        }

        private void OnOK(object sender, System.EventArgs e)
        {
            this.OnTryApplySettings(true);
        }

        private void OnCancel(object sender, System.EventArgs e)
        {
            this.Close();
        }

		private void OnApply(object sender, System.EventArgs e)
		{
			this.OnTryApplySettings(false);
		}

		private void OnIncludeCursorChanged(object sender, System.EventArgs e)
		{
			this.OnTrackChanges(SettingsItems.IncludeCursor, this._IncludeCursor.Checked != this._Settings.IncludeCursor);
		}

		private void OnAutoStartupChanged(object sender, System.EventArgs e)
		{
			this.OnTrackChanges(SettingsItems.AutoStartup, this._AutoStartup.Checked != this._Settings.AutoStartup);
		}

		private void OnAlwaysOnTopChanged(object sender, System.EventArgs e)
		{
			this.OnTrackChanges(SettingsItems.AlwaysOnTop, this._AlwaysOnTop.Checked != this._Settings.AlwaysOnTop);
		}

        private void OnImageDestinationChanged(object sender, System.EventArgs e)
        {
            ImageDestinations Destinations = this._ImageDestinations;
            bool IsFileOutput = (Destinations & ImageDestinations.File) == ImageDestinations.File;

            if (!IsFileOutput)
            {
                this._WorkingDirectoryPath.Text = this._Settings.WorkingDirectory;
                this._FileNameSelector.Text = this._Settings.FileNameFormat;
                this._ExternalAppPath.Text = this._Settings.ExternalApp;
            }

            this._FileGroup.Enabled = IsFileOutput;
            this._FileFormatGroup.Enabled = IsFileOutput;

            this.OnTrackChanges(SettingsItems.ImageDestinations, Destinations != this._Settings.ImageDestinations);
        }

        private void OnWorkingDirectoryChanged(object sender, System.EventArgs e)
        {
            this.OnTrackChanges(SettingsItems.WorkingDirectory, this._WorkingDirectoryPath.Text != this._Settings.WorkingDirectory);
        }

		private void OnSelectWorkingDirectory(object sender, System.EventArgs e)
		{
            using (System.Windows.Forms.FolderBrowserDialog Dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                Dialog.Description = ApplicationManager.GetString("WorkingDirectory.Description");
                string WorkingDirectory = this._WorkingDirectoryPath.Text;

                if (Settings.IsValidPath(WorkingDirectory) && Directory.Exists(WorkingDirectory))
                {
                    Dialog.SelectedPath = WorkingDirectory;
                }
                else if (!((this._Settings.WorkingDirectory.Length != 0) && Directory.Exists(this._Settings.WorkingDirectory)))
                {
                    Dialog.SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                }
                else
                {
                    Dialog.SelectedPath = this._Settings.WorkingDirectory;
                }

                try
                {
                    if (Dialog.ShowDialog(this) == System.Windows.Forms.DialogResult.OK)
                    {
                        this._WorkingDirectoryPath.Text = Dialog.SelectedPath;
                    }
                }
                catch (NotSupportedException)
                {

                }
            }
		}

		private void OnFileNameChanged(object sender, System.EventArgs e)
		{
			this.OnTrackChanges(SettingsItems.FileName, !this._FileNameSelector.Text.Equals(this._Settings.FileNameFormat));
		}

		private void OnUseFileOverwriteChanged(object sender, System.EventArgs e)
		{
			this.OnTrackChanges(SettingsItems.UseFileOverwrite, this._UseFileOverwrite.Checked != this._Settings.UseFileOverwrite);
		}

        private void OnUseExternalAppChanged(object sender, System.EventArgs e)
        {
            bool UseExternalApp = this._UseExternalApp.Checked;

            if (!UseExternalApp)
            {
                this._ExternalAppPath.Text = this._Settings.ExternalApp;
            }

            this._ExternalAppPath.Enabled = UseExternalApp;
            this._ExternalAppSelector.Enabled = UseExternalApp;

            this.OnTrackChanges(SettingsItems.UseExternalApp, UseExternalApp != this._Settings.UseExternalApp);
        }

		private void OnExternalAppChanged(object sender, System.EventArgs e)
		{
			this.OnTrackChanges(SettingsItems.ExternalApp, this._ExternalAppPath.Text != this._Settings.ExternalApp);
		}

        private void OnSelectExternalApp(object sender, System.EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();

            dialog.Filter = ApplicationManager.GetString("Exe") + " (*.exe)|*.exe";
            dialog.Title = ApplicationManager.GetString("ExternalApp.Title");
            dialog.DefaultExtension = "exe";

            string AppPath = this._ExternalAppPath.Text;

            if (Settings.IsValidPathToExecutable(AppPath) && File.Exists(AppPath))
            {
                dialog.FileName = AppPath;
            }
            else if (!((this._Settings.ExternalApp.Length != 0) && File.Exists(this._Settings.ExternalApp)))
            {
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
                dialog.FileName = "*.exe";
            }
            else
            {
                dialog.FileName = this._Settings.ExternalApp;
            }

            if (dialog.ShowDialog(this.Handle))
            {
                this._ExternalAppPath.Text = dialog.FileName;
            }
        }

        private void OnRemoveApp(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            if ((e.Button == System.Windows.Forms.MouseButtons.Left) && (Goletas.ScreenCapture.MessageBox.Show(this, ApplicationManager.GetString("RemoveApp.Dialog"), System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Question, System.Windows.Forms.MessageBoxDefaultButton.Button2) == System.Windows.Forms.DialogResult.Yes))
            {
                Configuration.Remove();
            }
        }

        private void OnChangeFileTypeSettings(object sender, System.EventArgs e)
        {
            FileType item = Configuration.Current.FileTypes[this._FileFormatSelector.SelectedIndex];

            if ((item.EditSettings(this) == System.Windows.Forms.DialogResult.OK) && (Configuration.Current.FileTypes.Replace(item)))
            {
                if (this._CurrentFormatId.Equals(item.FormatId))
                {
                    Configuration.Current.Settings.FileType = item;
                }

                Configuration.Save(item);
            }
        }

        private void OnFileFormatChanged(object sender, System.EventArgs e)
        {
            FileType item = Configuration.Current.FileTypes[this._FileFormatSelector.SelectedIndex];
            this._FileTypeSettings.Enabled = item.IsEditable;
            this._Settings.FileType = item;

            this.OnTrackChanges(SettingsItems.FileType, !this._CurrentFormatId.Equals(item.FormatId));
        }

        private void OnTryApplySettings(bool closeForm)
        {
            if (this._ChangedItems != SettingsItems.None)
            {
                if ((this._ImageDestinations & ImageDestinations.File) == ImageDestinations.File)
                {
                    // working directory check
                    if (!Settings.IsValidPath(this._WorkingDirectoryPath.Text))
                    {
                        MessageBox.Show(this, ApplicationManager.GetString("InvalidDirectoryName"), System.Windows.Forms.MessageBoxIcon.Warning);

                        return;
                    }

                    if (!Directory.Exists(this._WorkingDirectoryPath.Text))
                    {
                        MessageBox.Show(this, ApplicationManager.GetString("DirectoryNotExist"), System.Windows.Forms.MessageBoxIcon.Warning);

                        return;
                    }

                    // file name check
                    if (!Settings.IsValidFileNameFormat(this._FileNameSelector.Text))
                    {
                        MessageBox.Show(this, ApplicationManager.GetString("InvalidFileName"), System.Windows.Forms.MessageBoxIcon.Warning);

                        return;
                    }

                    // external app path check
                    if (this._UseExternalApp.Checked)
                    {
                        if (!Settings.IsValidPathToExecutable(this._ExternalAppPath.Text))
                        {
                            MessageBox.Show(this, ApplicationManager.GetString("InvalidExecutablePath"), System.Windows.Forms.MessageBoxIcon.Warning);

                            return;
                        }

                        if (!File.Exists(this._ExternalAppPath.Text))
                        {
                            MessageBox.Show(this, ApplicationManager.GetString("ExternalAppNotFound"), System.Windows.Forms.MessageBoxIcon.Warning);

                            return;
                        }
                    }

                    this._Settings.WorkingDirectory = this._WorkingDirectoryPath.Text;
                    this._Settings.UseFileOverwrite = this._UseFileOverwrite.Checked;
                    this._Settings.UpdateFileNameFormat(this._FileNameSelector.Text);

                    if (this._Settings.UseExternalApp = this._UseExternalApp.Checked)
                    {
                        this._Settings.ExternalApp = this._ExternalAppPath.Text;
                    }
                }

                this._Settings.IncludeCursor = this._IncludeCursor.Checked;
                this._Settings.AutoStartup = this._AutoStartup.Checked;
                this._Settings.AlwaysOnTop = this._AlwaysOnTop.Checked;
                this._Settings.ImageDestinations = this._ImageDestinations;

                this._CurrentFormatId = this._Settings.FileType.FormatId;
                this._ChangedItems = SettingsItems.None;
                this._Apply.Enabled = false;
                this.TopMost = this._Settings.AlwaysOnTop;

                Configuration.Current.Settings.FromSettings(this._Settings);
                Configuration.Save(this._Settings);
            }

            if (closeForm)
            {
                this.Close();
            }
        }


        internal SettingsForm()
        {
            this._Settings = Configuration.Current.Settings.Clone();
            this._CurrentFormatId = this._Settings.FileType.FormatId;

            this._Cancel = new System.Windows.Forms.Button();
            this._OK = new System.Windows.Forms.Button();
            this._Apply = new System.Windows.Forms.Button();
            this._FileTypeSettings = new System.Windows.Forms.Button();
            this._Pages = new System.Windows.Forms.TabControl();
            this._GeneralPage = new System.Windows.Forms.TabPage();
            this._HowToUseLabel = new System.Windows.Forms.Label();
            this._PreferencesPage = new System.Windows.Forms.TabPage();
            this._AlwaysOnTop = new System.Windows.Forms.CheckBox();
            this._AppGroup = new Goletas.ScreenCapture.GroupLabel();
            this._AutoStartup = new System.Windows.Forms.CheckBox();
            this._HotKeysGroup = new Goletas.ScreenCapture.GroupLabel();
            this._VirtualScreenKeySelector = new System.Windows.Forms.ComboBox();
            this._VirtualScreenKey = new System.Windows.Forms.Label();
            this._SettingsKeySelector = new System.Windows.Forms.ComboBox();
            this._SettingsKey = new System.Windows.Forms.Label();
            this._ForegroundWindowKeySelector = new System.Windows.Forms.ComboBox();
            this._ForegroundWindowKey = new System.Windows.Forms.Label();
            this._CurrentScreenKeySelector = new System.Windows.Forms.ComboBox();
            this._CurrentScreenKey = new System.Windows.Forms.Label();
            this._ImageCompositionGroup = new Goletas.ScreenCapture.GroupLabel();
            this._IncludeCursor = new System.Windows.Forms.CheckBox();
            this._DestinationPage = new System.Windows.Forms.TabPage();
            this._FileGroup = new Goletas.ScreenCapture.GroupLabel();
            this._WorkingDirectorySelector = new System.Windows.Forms.Button();
            this._WorkingDirectoryPath = new System.Windows.Forms.TextBox();
            this._WorkingDirectory = new System.Windows.Forms.Label();
            this._ExternalAppSelector = new System.Windows.Forms.Button();
            this._UseExternalApp = new System.Windows.Forms.CheckBox();
            this._ExternalAppPath = new System.Windows.Forms.TextBox();
            this._FileNameSelector = new System.Windows.Forms.TextBox();
            this._FileName = new System.Windows.Forms.Label();
            this._UseFileOverwrite = new System.Windows.Forms.CheckBox();
            this._ImageDestinationGroup = new Goletas.ScreenCapture.GroupLabel();
            this._ImageDestinationSelector = new System.Windows.Forms.ComboBox();
            this._ImagePage = new System.Windows.Forms.TabPage();
            this._FileFormatGroup = new Goletas.ScreenCapture.GroupLabel();
            this._FileFormatSelector = new System.Windows.Forms.ComboBox();
            this._AboutPage = new System.Windows.Forms.TabPage();
            this._LegalInfo = new System.Windows.Forms.Label();
            this._RemoveApp = new System.Windows.Forms.LinkLabel();


            this.SuspendLayout();
            this._Pages.SuspendLayout();
            this._GeneralPage.SuspendLayout();
            this._PreferencesPage.SuspendLayout();
            this._DestinationPage.SuspendLayout();
            this._ImagePage.SuspendLayout();
            this._AboutPage.SuspendLayout();
            this._ImageCompositionGroup.SuspendLayout();
            this._HotKeysGroup.SuspendLayout();
            this._AppGroup.SuspendLayout();
            this._ImageDestinationGroup.SuspendLayout();
            this._FileGroup.SuspendLayout();
            this._FileFormatGroup.SuspendLayout();


            this._Pages.Controls.Add(this._GeneralPage);
            this._Pages.Controls.Add(this._PreferencesPage);
            this._Pages.Controls.Add(this._DestinationPage);
            this._Pages.Controls.Add(this._ImagePage);
            this._Pages.Controls.Add(this._AboutPage);
            this._Pages.Bounds = new System.Drawing.Rectangle(6, 6, 320, 348);
            this._Pages.SelectedIndex = 0;


            this._GeneralPage.Controls.Add(this._HowToUseLabel);
            this._GeneralPage.Text = ApplicationManager.GetString("General.Page");
            this._GeneralPage.UseVisualStyleBackColor = true;

            this._PreferencesPage.Controls.Add(this._ImageCompositionGroup);
            this._PreferencesPage.Controls.Add(this._HotKeysGroup);
            this._PreferencesPage.Controls.Add(this._AppGroup);
            this._PreferencesPage.Controls.Add(this._RemoveApp);
            this._PreferencesPage.Text = ApplicationManager.GetString("Preferences.Page");
            this._PreferencesPage.UseVisualStyleBackColor = true;


            this._DestinationPage.Controls.Add(this._ImageDestinationGroup);
            this._DestinationPage.Controls.Add(this._FileGroup);
            this._DestinationPage.Text = ApplicationManager.GetString("Destination.Page");
            this._DestinationPage.UseVisualStyleBackColor = true;

            this._ImagePage.Controls.Add(this._FileFormatGroup);
            this._ImagePage.Text = ApplicationManager.GetString("Image.Page");
            this._ImagePage.UseVisualStyleBackColor = true;

            this._AboutPage.Controls.Add(this._LegalInfo);
            this._AboutPage.Text = ApplicationManager.GetString("About.Page");
            this._AboutPage.UseVisualStyleBackColor = true;


            this._HowToUseLabel.Bounds = new System.Drawing.Rectangle(2, 8, 308, 308);

            if (Environment.OSVersion.Version.Major >= 6) // Vista and later
            {
                this._HowToUseLabel.Text = ApplicationManager.GetString("HowToUse") + Environment.NewLine + Environment.NewLine + ApplicationManager.GetString("OsNotFullySupported");
            }
            else
            {
                this._HowToUseLabel.Text = ApplicationManager.GetString("HowToUse");
            }

            this._HowToUseLabel.UseMnemonic = false;


            this._ImageCompositionGroup.Controls.Add(this._IncludeCursor);
            this._ImageCompositionGroup.Bounds = new System.Drawing.Rectangle(2, 8, 308, 44);
            this._ImageCompositionGroup.Text = ApplicationManager.GetString("ImageComposition.Group");


            this._HotKeysGroup.Controls.Add(this._CurrentScreenKey);
            this._HotKeysGroup.Controls.Add(this._CurrentScreenKeySelector);
            this._HotKeysGroup.Controls.Add(this._VirtualScreenKey);
            this._HotKeysGroup.Controls.Add(this._VirtualScreenKeySelector);
            this._HotKeysGroup.Controls.Add(this._ForegroundWindowKey);
            this._HotKeysGroup.Controls.Add(this._ForegroundWindowKeySelector);
            this._HotKeysGroup.Controls.Add(this._SettingsKey);
            this._HotKeysGroup.Controls.Add(this._SettingsKeySelector);
            this._HotKeysGroup.Bounds = new System.Drawing.Rectangle(2, 64, 308, 116);
            this._HotKeysGroup.Text = ApplicationManager.GetString("HotKeys.Group");


            this._AppGroup.Controls.Add(this._AutoStartup);
            this._AppGroup.Controls.Add(this._AlwaysOnTop);
            this._AppGroup.Bounds = new System.Drawing.Rectangle(2, 188, 308, 64);
            this._AppGroup.Text = ApplicationManager.GetString("App.Group");



            this._RemoveApp.Bounds = new System.Drawing.Rectangle(2, 272, 308, 36);
            this._RemoveApp.Text = ApplicationManager.GetString("RemoveApp");
            this._RemoveApp.LinkArea = new System.Windows.Forms.LinkArea(ApplicationManager.GetInt32("RemoveApp.Start"), ApplicationManager.GetInt32("RemoveApp.Length"));
            this._RemoveApp.DisabledLinkColor = System.Drawing.SystemColors.GrayText;
            this._RemoveApp.VisitedLinkColor = System.Drawing.SystemColors.HotTrack;
            this._RemoveApp.ActiveLinkColor = System.Drawing.SystemColors.HotTrack;
            this._RemoveApp.LinkColor = System.Drawing.SystemColors.HotTrack;
            this._RemoveApp.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(OnRemoveApp);
            SetHelp(this._RemoveApp, "RemoveApp.Help");


            this._IncludeCursor.Bounds = new System.Drawing.Rectangle(14, 20, 280, 18);
            this._IncludeCursor.Text = ApplicationManager.GetString("IncludeCursor");
            this._IncludeCursor.Checked = this._Settings.IncludeCursor;
            this._IncludeCursor.CheckedChanged += new System.EventHandler(this.OnIncludeCursorChanged);
            SetHelp(this._IncludeCursor, "IncludeCursor.Help");


            this._CurrentScreenKey.Bounds = new System.Drawing.Rectangle(14, 16, 134, 20);
            this._CurrentScreenKey.Text = ApplicationManager.GetString("CurrentScreenKey");
            this._CurrentScreenKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;


            this._CurrentScreenKeySelector.Bounds = new System.Drawing.Rectangle(14, 36, 134, 21);
            this._CurrentScreenKeySelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._CurrentScreenKeySelector.Items.Add(ApplicationManager.GetString("PrintScreen.Item"));
            this._CurrentScreenKeySelector.SelectedIndex = 0;
            SetHelp(this._CurrentScreenKeySelector, "CurrentScreenKey.Help");


            this._VirtualScreenKey.Bounds = new System.Drawing.Rectangle(160, 16, 134, 20);
            this._VirtualScreenKey.Text = ApplicationManager.GetString("VirtualScreenKey");
            this._VirtualScreenKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;


            this._VirtualScreenKeySelector.Bounds = new System.Drawing.Rectangle(160, 36, 134, 21);
            this._VirtualScreenKeySelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._VirtualScreenKeySelector.Items.Add(ApplicationManager.GetString("CtrlPrintScreen.Item"));
            this._VirtualScreenKeySelector.SelectedIndex = 0;
            SetHelp(this._VirtualScreenKeySelector, "VirtualScreenKey.Help");


            this._ForegroundWindowKey.Bounds = new System.Drawing.Rectangle(14, 64, 134, 20);
            this._ForegroundWindowKey.Text = ApplicationManager.GetString("ForegroundWindowKey");
            this._ForegroundWindowKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;


            this._ForegroundWindowKeySelector.Bounds = new System.Drawing.Rectangle(14, 84, 134, 21);
            this._ForegroundWindowKeySelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._ForegroundWindowKeySelector.Items.Add(ApplicationManager.GetString("ShiftPrintScreen.Item"));
            this._ForegroundWindowKeySelector.SelectedIndex = 0;
            SetHelp(this._ForegroundWindowKeySelector, "ForegroundWindowKey.Help");


            this._SettingsKey.Bounds = new System.Drawing.Rectangle(160, 64, 134, 20);
            this._SettingsKey.Text = ApplicationManager.GetString("SettingsKey");
            this._SettingsKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;


            this._SettingsKeySelector.Bounds = new System.Drawing.Rectangle(160, 84, 134, 21);
            this._SettingsKeySelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._SettingsKeySelector.Items.Add(ApplicationManager.GetString("AltPrintScreen.Item"));
            this._SettingsKeySelector.SelectedIndex = 0;
            SetHelp(this._SettingsKeySelector, "SettingsKey.Help");


            this._AutoStartup.Bounds = new System.Drawing.Rectangle(14, 20, 280, 18);
            this._AutoStartup.Text = ApplicationManager.GetString("AutoStartup");
            this._AutoStartup.Checked = this._Settings.AutoStartup;
            this._AutoStartup.CheckedChanged += new System.EventHandler(this.OnAutoStartupChanged);
            SetHelp(this._AutoStartup, "AutoStartup.Help");


            this._AlwaysOnTop.Bounds = new System.Drawing.Rectangle(14, 40, 280, 18);
            this._AlwaysOnTop.Text = ApplicationManager.GetString("AlwaysOnTop");
            this._AlwaysOnTop.Checked = this._Settings.AlwaysOnTop;
            this._AlwaysOnTop.CheckedChanged += new System.EventHandler(this.OnAlwaysOnTopChanged);
            SetHelp(this._AlwaysOnTop, "AlwaysOnTop.Help");


            this._ImageDestinationGroup.Controls.Add(this._ImageDestinationSelector);
            this._ImageDestinationGroup.Bounds = new System.Drawing.Rectangle(2, 8, 308, 48);
            this._ImageDestinationGroup.Text = ApplicationManager.GetString("ImageDestination.Group");


            this._FileGroup.Controls.Add(this._WorkingDirectory);
            this._FileGroup.Controls.Add(this._WorkingDirectoryPath);
            this._FileGroup.Controls.Add(this._WorkingDirectorySelector);
            this._FileGroup.Controls.SetChildIndex(this._WorkingDirectorySelector, 0);
            this._FileGroup.Controls.Add(this._FileName);
            this._FileGroup.Controls.Add(this._FileNameSelector);
            this._FileGroup.Controls.Add(this._UseFileOverwrite);
            this._FileGroup.Controls.Add(this._UseExternalApp);
            this._FileGroup.Controls.Add(this._ExternalAppPath);
            this._FileGroup.Controls.Add(this._ExternalAppSelector);
            this._FileGroup.Controls.SetChildIndex(this._ExternalAppSelector, 0);
            this._FileGroup.Bounds = new System.Drawing.Rectangle(2, 64, 308, 220);
            this._FileGroup.Text = ApplicationManager.GetString("File.Group");
            this._FileGroup.Enabled = ((this._Settings.ImageDestinations & ImageDestinations.File) == ImageDestinations.File);


            this._ImageDestinationSelector.Bounds = new System.Drawing.Rectangle(14, 20, 280, 21);
            this._ImageDestinationSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._ImageDestinationSelector.Items.AddRange(new object[] { ApplicationManager.GetString("Clipboard.Item"), ApplicationManager.GetString("File.Item"), ApplicationManager.GetString("ClipboardAndFile.Item") });
            this._ImageDestinations = this._Settings.ImageDestinations;
            this._ImageDestinationSelector.SelectedIndexChanged += new System.EventHandler(OnImageDestinationChanged);
            SetHelp(this._ImageDestinationSelector, "ImageDestination.Help");


            this._WorkingDirectory.Bounds = new System.Drawing.Rectangle(14, 16, 280, 20);
            this._WorkingDirectory.Text = ApplicationManager.GetString("WorkingDirectory");
            this._WorkingDirectory.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;


            this._WorkingDirectoryPath.Bounds = new System.Drawing.Rectangle(14, 36, 248, 20);
            this._WorkingDirectoryPath.MaxLength = NativeMethods.MAX_PATH - 1; // one for null terminator
            this._WorkingDirectoryPath.Text = this._Settings.WorkingDirectory;
            this._WorkingDirectoryPath.TextChanged += new System.EventHandler(this.OnWorkingDirectoryChanged);
            NativeMethods.SetTextualCue(this._WorkingDirectoryPath, ApplicationManager.GetString("WorkingDirectory.Cue"));
            SetHelp(this._WorkingDirectoryPath, "WorkingDirectory.Help");


            this._WorkingDirectorySelector.Bounds = new System.Drawing.Rectangle(268, 34, 28, 24);
            this._WorkingDirectorySelector.Text = ApplicationManager.GetString("Select");
            this._WorkingDirectorySelector.Click += new System.EventHandler(this.OnSelectWorkingDirectory);
            SetHelp(this._WorkingDirectorySelector, "SelectWorkingDirectory.Help");

            this._FileName.Bounds = new System.Drawing.Rectangle(14, 64, 280, 20);
            this._FileName.Text = ApplicationManager.GetString("FileName");
            this._FileName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;


            this._FileNameSelector.Bounds = new System.Drawing.Rectangle(14, 84, 280, 20);
            this._FileNameSelector.MaxLength = NativeMethods.MAX_PATH - 4;
            this._FileNameSelector.Text = this._Settings.FileNameFormat;
            this._FileNameSelector.TextChanged += new System.EventHandler(this.OnFileNameChanged);
            NativeMethods.SetTextualCue(this._FileNameSelector, ApplicationManager.GetString("FileName.Cue"));
            SetHelp(this._FileNameSelector, "FileName.Help");


            this._UseFileOverwrite.Bounds = new System.Drawing.Rectangle(14, 116, 280, 18);
            this._UseFileOverwrite.Text = ApplicationManager.GetString("UseFileOverwrite");
            this._UseFileOverwrite.Checked = this._Settings.UseFileOverwrite;
            this._UseFileOverwrite.CheckedChanged += new System.EventHandler(this.OnUseFileOverwriteChanged);
            SetHelp(this._UseFileOverwrite, "UseFileOverwrite.Help");


            this._UseExternalApp.Bounds = new System.Drawing.Rectangle(14, 136, 280, 18);
            this._UseExternalApp.Text = ApplicationManager.GetString("UseExternalApp");
            this._UseExternalApp.Checked = this._Settings.UseExternalApp;
            this._UseExternalApp.CheckedChanged += new System.EventHandler(this.OnUseExternalAppChanged);
            SetHelp(this._UseExternalApp, "UseExternalApp.Help");


            this._ExternalAppPath.Bounds = new System.Drawing.Rectangle(14, 154, 248, 20);
            this._ExternalAppPath.MaxLength = NativeMethods.MAX_PATH - 1; // one for null terminator
            this._ExternalAppPath.Text = this._Settings.ExternalApp;            
            this._ExternalAppPath.Enabled = this._Settings.UseExternalApp;
            this._ExternalAppPath.TextChanged += new System.EventHandler(this.OnExternalAppChanged);
            NativeMethods.SetTextualCue(this._ExternalAppPath, ApplicationManager.GetString("ExternalApp.Cue"));
            SetHelp(this._ExternalAppPath, "ExternalApp.Help");


            this._ExternalAppSelector.Bounds = new System.Drawing.Rectangle(268, 152, 28, 24);
            this._ExternalAppSelector.Text = ApplicationManager.GetString("Select");
            this._ExternalAppSelector.Enabled = this._Settings.UseExternalApp;
            this._ExternalAppSelector.Click += new System.EventHandler(this.OnSelectExternalApp);
            SetHelp(this._ExternalAppSelector, "SelectExternalApp.Help");


            this._FileFormatGroup.Controls.Add(this._FileFormatSelector);
            this._FileFormatGroup.Controls.Add(this._FileTypeSettings);
            this._FileFormatGroup.Bounds = new System.Drawing.Rectangle(2, 8, 308, 48);
            this._FileFormatGroup.Text = ApplicationManager.GetString("FileFormat.Group");
            this._FileFormatGroup.Enabled = ((this._Settings.ImageDestinations & ImageDestinations.File) == ImageDestinations.File);


            this._FileFormatSelector.Bounds = new System.Drawing.Rectangle(14, 20, 248, 21);
            this._FileFormatSelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this._FileFormatSelector.Items.AddRange(Configuration.Current.FileTypes.GetDisplayNames());
            this._FileFormatSelector.SelectedIndex = Configuration.Current.FileTypes.IndexOf(this._Settings.FileType.FormatId);
            this._FileFormatSelector.SelectedIndexChanged += new System.EventHandler(OnFileFormatChanged);
            SetHelp(this._FileFormatSelector, "FileFormat.Help");


            this._FileTypeSettings.Bounds = new System.Drawing.Rectangle(268, 18, 28, 24);
            this._FileTypeSettings.Text = ApplicationManager.GetString("Select");
            this._FileTypeSettings.UseVisualStyleBackColor = true;
            this._FileTypeSettings.Enabled = this._Settings.FileType.IsEditable;
            this._FileTypeSettings.Click += new System.EventHandler(OnChangeFileTypeSettings);
            SetHelp(this._FileTypeSettings, "FileType.Help");


            this._LegalInfo.Bounds = new System.Drawing.Rectangle(2, 8, 308, 308);
            this._LegalInfo.Text = ApplicationManager.ProductFullName + Environment.NewLine + ApplicationManager.LegalInfo;
            this._LegalInfo.UseMnemonic = false;


            this._OK.Bounds = new System.Drawing.Rectangle(92, 360, 74, 24);
            this._OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this._OK.Text = ApplicationManager.GetString("OK");
            this._OK.UseVisualStyleBackColor = true;
            this._OK.Click += new System.EventHandler(this.OnOK);
            SetHelp(this._OK, "AppOK.Help");


            this._Cancel.Bounds = new System.Drawing.Rectangle(172, 360, 74, 24);
            this._Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this._Cancel.Text = ApplicationManager.GetString("Cancel");
            this._Cancel.UseVisualStyleBackColor = true;
            this._Cancel.Click += new System.EventHandler(this.OnCancel);
            SetHelp(this._Cancel, "AppCancel.Help");


            this._Apply.Bounds = new System.Drawing.Rectangle(252, 360, 74, 24);
            this._Apply.Text = ApplicationManager.GetString("Apply");
            this._Apply.UseVisualStyleBackColor = true;
            this._Apply.Enabled = false;
            this._Apply.Click += new System.EventHandler(this.OnApply);
            SetHelp(this._Apply, "AppApply.Help");


            this.Controls.Add(this._OK);
            this.Controls.Add(this._Cancel);
            this.Controls.Add(this._Apply);
            this.Controls.Add(this._Pages);
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(332, 392);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.HelpButton = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;

            this.ShowIcon = false;
            this.Icon = System.Drawing.Icon.FromHandle(NativeMethods.GetApplicationIcon());

#if DEBUG
            this.Text = ApplicationManager.ProductName + " Debug Build";
#else
            this.Text = ApplicationManager.ProductName;
#endif

            this._FileFormatGroup.ResumeLayout(false);
            this._FileGroup.ResumeLayout(false);
            this._ImageDestinationGroup.ResumeLayout(false);
            this._AppGroup.SuspendLayout();
            this._HotKeysGroup.ResumeLayout(false);
            this._ImageCompositionGroup.ResumeLayout(false);
            this._AboutPage.ResumeLayout(false);
            this._ImagePage.ResumeLayout(false);
            this._DestinationPage.ResumeLayout(false);
            this._PreferencesPage.ResumeLayout(false);
            this._GeneralPage.ResumeLayout(false);
            this._Pages.ResumeLayout(false);
            this.ResumeLayout(false);
        }

	}
}