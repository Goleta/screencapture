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
    using System.Globalization;
    using System.Windows.Forms;

    public abstract class FileType : ICloneable
    {
        /// <summary>
        /// Retrieves all the codec information for the installed image encoder
        /// from the specified <paramref name="imageFormat"/>.
        /// </summary>
        /// <param name="imageFormat">
        /// The image format for which to return the codec information.
        /// </param>
        /// <returns>
        /// The codec infomation for the specified <paramref name="imageFormat"/>,
        /// or <c>null</c> if no match was found.
        /// </returns>
        internal static ImageCodecInfo GetImageCodecInfo(ImageFormat imageFormat)
        {
            ImageCodecInfo[] Encoders = ImageCodecInfo.GetImageEncoders();

            for (int i = 0; i < Encoders.Length; i++)
            {
                if (Encoders[i].FormatID.Equals(imageFormat.Guid))
                {
                    return Encoders[i];
                }
            }

            return null;
        }

        public abstract Guid FormatId
        {
            get;
        }

        public abstract string Name
        {
            get;
        }

        public abstract FileExtensionCollection Extensions
        {
            get;
        }

        public void Save(Bitmap bitmap, Stream stream)
        {
            if ((bitmap == null) || (stream == null))
            {
                throw new ArgumentNullException();
            }

            this.SaveImage(bitmap, stream);
        }

        protected abstract void SaveImage(Bitmap bitmap, Stream stream);

        public virtual bool IsEditable
        {
            get
            {
                return false;
            }
        }

        public virtual DialogResult EditSettings(IWin32Window parent)
        {
            throw new NotSupportedException();
        }

        public virtual byte[] Serialize()
        {
            throw new NotSupportedException();
        }

        public virtual bool Deserialize(byte[] settings)
        {
            throw new NotSupportedException();
        }

        public virtual FileType Clone()
        {
            if (this.IsEditable)
            {
                return (FileType)this.MemberwiseClone();
            }

            return this;
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public override string ToString()
        {
            return this.Name + " (*." + this.Extensions[0] + ")";
        }

        protected FileType()
        {

        }

    }
}