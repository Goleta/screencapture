//
// Copyright © 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.ScreenCapture.Drawing.Imaging
{
	using System;
	using System.IO;
	using System.Drawing;
	using System.Drawing.Imaging;

    public sealed class PngFileType : FileType
    {
        private static readonly Guid _FormatId = new Guid(0x3691c8f2, 0xdafd, 0x4cfe, 0x8b, 0x92, 0x9e, 0x5d, 0xd0, 0xf9, 0xe2, 0x56); // {3691C8F2-DAFD-4cfe-8B92-9E5DD0F9E256}
        private static readonly FileExtensionCollection _Extensions = new FileExtensionCollection("png");

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
                return ApplicationManager.GetString("Png");
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
            using (Bitmap pic = new Bitmap(bitmap.Width, bitmap.Height, PixelFormat.Format24bppRgb))
            {
                using (Graphics dc = Graphics.FromImage(pic))
                {
                    dc.DrawImage(bitmap, 0, 0);
                }

                pic.Save(stream, GetImageCodecInfo(ImageFormat.Png), null);
            }
        }

        internal PngFileType()
        {

        }

    }
}