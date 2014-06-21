//
// Copyright © 2006 - 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.ScreenCapture.Drawing.Imaging
{
    using System;
	using System.IO;
	using System.Drawing;
	using System.Drawing.Imaging;

    using Goletas.ScreenCapture;

    public sealed class JpegFileType : FileType
    {
        private static readonly Guid _FormatId = new Guid(0x2cd20838, 0x55a6, 0x4193, 0xad, 0x7d, 0xe4, 0xe8, 0x83, 0x90, 0x35, 0x5); // {2CD20838-55A6-4193-AD7D-E4E883903505}
        private static readonly FileExtensionCollection _Extensions = new FileExtensionCollection("jpg");

        private byte _ImageQuality;

        public byte ImageQuality
        {
            get
            {
                return this._ImageQuality;
            }
            set
            {
                if (value > 100)
                {
                    throw new ArgumentOutOfRangeException();
                }

                this._ImageQuality = value;
            }
        }

        public override Guid FormatId
        {
            get
            {
                return _FormatId;
            }
        }

		public override string Name
		{
			get
			{
                return ApplicationManager.GetString("Jpg");
			}
		}

        public override FileExtensionCollection Extensions
        {
            get
            {
                return _Extensions;
            }
        }

        protected override void SaveImage(Bitmap bitmap, Stream stream)
        {
			using (EncoderParameters Parameters = new EncoderParameters(1))
			{
                using (Parameters.Param[0] = new EncoderParameter(Encoder.Quality, (long)this._ImageQuality))
				{
					bitmap.Save(stream, GetImageCodecInfo(ImageFormat.Jpeg), Parameters);
				}
			}
        }

        public override bool IsEditable
        {
            get
            {
                return true;
            }
        }

        public override System.Windows.Forms.DialogResult EditSettings(System.Windows.Forms.IWin32Window parent)
        {
            using (SettingsForm Form = new SettingsForm(this))
            {
                return Form.ShowDialog(parent);
            }
        }

        public override byte[] Serialize()
        {
            return new byte[] { this._ImageQuality };
        }

        public override bool Deserialize(byte[] settings)
        {
            bool IsDeserializable = ((settings != null) && (settings.Length == 1) && (settings[0] <= 100));
            
            if (IsDeserializable)
            {
                this._ImageQuality = settings[0];
            }

            return IsDeserializable;
        }

        internal JpegFileType()
        {
            this._ImageQuality = 100;
        }


        private sealed class SettingsForm : System.Windows.Forms.Form
        {
            private JpegFileType _Source;
            private int _OriginalQualityIndex;

            private System.Windows.Forms.TabControl _Pages;
            private System.Windows.Forms.TabPage _General;
            private System.Windows.Forms.Button _Cancel;
            private System.Windows.Forms.Button _OK;
            private Goletas.ScreenCapture.GroupLabel _ImageQuality;
            private System.Windows.Forms.ComboBox _ImageQualitySelector;

            public SettingsForm(JpegFileType fileType)
            {
                this._Source = fileType;

                this._Pages = new System.Windows.Forms.TabControl();
                this._General = new System.Windows.Forms.TabPage();
                this._ImageQualitySelector = new System.Windows.Forms.ComboBox();
                this._ImageQuality = new Goletas.ScreenCapture.GroupLabel();
                this._Cancel = new System.Windows.Forms.Button();
                this._OK = new System.Windows.Forms.Button();

                this.SuspendLayout();
                this._Pages.SuspendLayout();
                this._General.SuspendLayout();


                this._Pages.Controls.Add(this._General);
                this._Pages.Bounds = new System.Drawing.Rectangle(6, 6, 320, 348);
                this._Pages.SelectedIndex = 0;


                this._General.Controls.Add(this._ImageQuality);
                this._General.UseVisualStyleBackColor = true;
                this._General.Text = ApplicationManager.GetString("General.Page");


                this._ImageQualitySelector.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
                this._ImageQualitySelector.Bounds = new System.Drawing.Rectangle(14, 20, 280, 21);
                this._ImageQualitySelector.Items.AddRange(new object[] { ApplicationManager.GetString("Lowest.Item"), ApplicationManager.GetString("Low.Item"), ApplicationManager.GetString("Normal.Item"), ApplicationManager.GetString("High.Item"), ApplicationManager.GetString("Highest.Item") });
                ScreenCapture.SettingsForm.SetHelp(this._ImageQualitySelector, "ImageQuality.Help");

                if (fileType.ImageQuality > 95) this._ImageQualitySelector.SelectedIndex = 4;
                else if (fileType.ImageQuality > 90) this._ImageQualitySelector.SelectedIndex = 3;
                else if (fileType.ImageQuality > 85) this._ImageQualitySelector.SelectedIndex = 2;
                else if (fileType.ImageQuality > 80) this._ImageQualitySelector.SelectedIndex = 1;
                else this._ImageQualitySelector.SelectedIndex = 0;

                this._OriginalQualityIndex = this._ImageQualitySelector.SelectedIndex;
                this._ImageQualitySelector.SelectedIndexChanged += new EventHandler(this.OnImageQualitySelected);


                this._ImageQuality.Controls.Add(this._ImageQualitySelector);
                this._ImageQuality.Bounds = new System.Drawing.Rectangle(2, 8, 308, 48);
                this._ImageQuality.Text = ApplicationManager.GetString("ImageQuality.Group");


                this._Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
                this._Cancel.Bounds = new System.Drawing.Rectangle(252, 360, 74, 24);
                this._Cancel.UseVisualStyleBackColor = true;
                this._Cancel.Text = ApplicationManager.GetString("Cancel");
                ScreenCapture.SettingsForm.SetHelp(this._Cancel, "Cancel.Help");


                this._OK.DialogResult = System.Windows.Forms.DialogResult.OK;
                this._OK.Bounds = new System.Drawing.Rectangle(172, 360, 74, 24);
                this._OK.UseVisualStyleBackColor = true;
                this._OK.Enabled = false;
                this._OK.Text = ApplicationManager.GetString("OK");
                ScreenCapture.SettingsForm.SetHelp(this._OK, "OK.Help");
                this._OK.Click += new EventHandler(this.OnOK);


                this.Controls.Add(this._Cancel);
                this.Controls.Add(this._Pages);
                this.Controls.Add(this._OK);
                this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
                this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
                this.ClientSize = new System.Drawing.Size(332, 392);
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
                this.HelpButton = true;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.ShowIcon = false;
                this.ShowInTaskbar = false;
                this.CancelButton = this._Cancel;
                this.Text = fileType.Name;


                this._General.ResumeLayout(false);
                this._Pages.ResumeLayout(false);
                this.ResumeLayout(false);
            }

            private void OnImageQualitySelected(object sender, EventArgs e)
            {
                this._OK.Enabled = (this._ImageQualitySelector.SelectedIndex != this._OriginalQualityIndex);
            }

            private void OnOK(object sender, EventArgs e)
            {
                this._Source.ImageQuality = (byte)(80 + this._ImageQualitySelector.SelectedIndex * 5);
            }
        }

    }
}