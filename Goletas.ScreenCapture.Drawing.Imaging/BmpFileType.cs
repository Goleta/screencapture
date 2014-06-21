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

    public sealed class BmpFileType : FileType
    {
        private static readonly Guid _FormatId = new Guid(0x2cb9558c, 0xb03, 0x41b2, 0xba, 0x8f, 0xff, 0x3d, 0x2e, 0x8c, 0x6d, 0x32);  // {2CB9558C-0B03-41b2-BA8F-FF3D2E8C6D32}
        private static readonly FileExtensionCollection _Extensions = new FileExtensionCollection("bmp");

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
                return ApplicationManager.GetString("Bmp");
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
            using (Bitmap pic = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb)) // BMP supports 1 - 24 bit color depth.
            {
                using (Graphics dc = Graphics.FromImage(pic))
                {
                    dc.DrawImage(bitmap, 0, 0);
                }

                pic.Save(stream, GetImageCodecInfo(ImageFormat.Bmp), null);
            }
        }

        internal BmpFileType()
        {

        }

    }
}