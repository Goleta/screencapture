//
// Copyright © 2005 - 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.ScreenCapture
{
	using System.Drawing;
	using System.Drawing.Text;
	using System.Windows.Forms;
    using System.Windows.Forms.VisualStyles;
	using System.Security.Permissions;

	/// <summary>
	/// Represents a Windows control that displays the caption
    /// with the line separating a group of controls.
	/// </summary>
	/// <remarks>
	/// Use the <see cref="GroupLabel"/> to logically group a collection of controls on a form.
	/// The <see cref="GroupLabel"/> is a container control that can be used to define groups
	/// of controls.
	/// </remarks>
    public sealed class GroupLabel : Control
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="GroupLabel"/> class. 
		/// </summary>
		public GroupLabel()
		{
			this.SetStyle(ControlStyles.ContainerControl | ControlStyles.SupportsTransparentBackColor | ControlStyles.ResizeRedraw | ControlStyles.UserPaint, true);
			this.SetStyle(ControlStyles.Selectable, false);
			this.TabStop = false;
		}

		/// <summary>
		/// Processes a mnemonic character.
		/// </summary>
        /// <param name="charCode">
		/// The character to process.
		/// </param>
		/// <returns>
		/// <c>true</c> if the character was processed as a mnemonic by the control;
		/// otherwise, <c>false</c>.
		/// </returns>
		[UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
		protected override bool ProcessMnemonic(char charCode)
		{
			if (this.Enabled && this.Visible && Control.IsMnemonic(charCode, this.Text))
			{
				return base.SelectNextControl(null, true, true, true, false);
			}

			return false;
		}

        /// <summary>
        /// Raises the Paint event.
        /// </summary>
        /// <param name="e">
        /// A <see cref="PaintEventArgs"/> that contains the event data.
        /// </param>
        protected override void OnPaint(PaintEventArgs e)
        {
            int width = this.ClientRectangle.Width;
            string text = this.Text;
            Font font = this.Font;
            bool isRightToLeft = (this.RightToLeft == RightToLeft.Yes);
            TextFormatFlags formatFlags = TextFormatFlags.SingleLine | TextFormatFlags.WordEllipsis | TextFormatFlags.GlyphOverhangPadding;

            if (!this.ShowKeyboardCues)
            {
                formatFlags |= TextFormatFlags.HidePrefix;
            }

            if (isRightToLeft)
            {
                formatFlags |= TextFormatFlags.Right | TextFormatFlags.RightToLeft;
            }

            Size TextSize = TextRenderer.MeasureText(e.Graphics, text, font, new Size(width, 18), formatFlags);
            int TextMidline = TextSize.Height >> 1;

            bool DrawLine = (TextSize.Width + 32) < width;

            Rectangle TextBounds;

            if (isRightToLeft)
            {
                TextBounds = new Rectangle(width - TextSize.Width, 0, TextSize.Width, TextSize.Height);
            }
            else
            {
                TextBounds = new Rectangle(0, 0, TextSize.Width, TextSize.Height);
            }

            if (Application.RenderWithVisualStyles)
            {
                VisualStyleRenderer Renderer = new VisualStyleRenderer((this.Enabled) ? VisualStyleElement.Button.GroupBox.Normal : VisualStyleElement.Button.GroupBox.Disabled);

                TextRenderer.DrawText(e.Graphics, text, font, TextBounds, Renderer.GetColor(ColorProperty.TextColor), formatFlags);

                if (DrawLine)
                {
                    if (isRightToLeft)
                    {
                        e.Graphics.DrawLine(SystemPens.ActiveBorder, 4, TextMidline, width - TextSize.Width, TextMidline);
                    }
                    else
                    {
                        e.Graphics.DrawLine(SystemPens.ActiveBorder, TextSize.Width, TextMidline, width - 4, TextMidline);
                    }
                }
            }
            else
            {
                if (this.Enabled)
                {
                    TextRenderer.DrawText(e.Graphics, text, font, TextBounds, SystemColors.ControlText, formatFlags);
                }
                else
                {
                    TextRenderer.DrawText(e.Graphics, text, font, new Rectangle(TextBounds.X + 1, TextBounds.Y + 1, TextBounds.Width, TextBounds.Height), SystemColors.ControlLightLight, formatFlags);
                    TextRenderer.DrawText(e.Graphics, text, font, TextBounds, SystemColors.GrayText, formatFlags);
                }

                if (DrawLine)
                {
                    if (isRightToLeft)
                    {
                        e.Graphics.DrawLine(SystemPens.GrayText, 4, TextMidline, width - TextSize.Width, TextMidline);
                        e.Graphics.DrawLine(SystemPens.ControlLightLight, 4, TextMidline + 1, width - TextSize.Width, TextMidline + 1);
                    }
                    else
                    {
                        e.Graphics.DrawLine(SystemPens.GrayText, TextSize.Width, TextMidline, width - 4, TextMidline);
                        e.Graphics.DrawLine(SystemPens.ControlLightLight, TextSize.Width, TextMidline + 1, width - 4, TextMidline + 1);
                    }
                }
            }

            base.OnPaint(e);
        }

	}
}