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

    public sealed class TiffFileType : FileType
    {
        private static readonly Guid _FormatId = new Guid(0xab9583a1, 0xf521, 0x4b2b, 0xa6, 0xf3, 0xe2, 0x89, 0x55, 0x72, 0xad, 0xe8); // {AB9583A1-F521-4b2b-A6F3-E2895572ADE8}
        private static readonly FileExtensionCollection _Extensions = new FileExtensionCollection("tif");

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
                return ApplicationManager.GetString("Tif");
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
                using (Parameters.Param[0] = new EncoderParameter(Encoder.ColorDepth, 24L))
                {
                    bitmap.Save(stream, GetImageCodecInfo(ImageFormat.Tiff), Parameters);
                }
            }
        }

        internal TiffFileType()
        {

        }

    }
}