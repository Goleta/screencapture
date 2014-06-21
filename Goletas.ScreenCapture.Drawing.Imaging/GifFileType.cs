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

    public sealed class GifFileType : FileType
    {
        private static readonly Guid _FormatId = new Guid(0x89fbf58, 0x2ff9, 0x4169, 0xa6, 0xad, 0x7a, 0x8b, 0xe4, 0xae, 0xd9, 0x75); // {089FBF58-2FF9-4169-A6AD-7A8BE4AED975}
        private static readonly FileExtensionCollection _Extensions = new FileExtensionCollection("gif");

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
                return ApplicationManager.GetString("Gif");
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
			bitmap.Save(stream, GetImageCodecInfo(ImageFormat.Gif), null);
		}

        internal GifFileType()
        {

        }

    }
}