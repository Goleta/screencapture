//
// Copyright © 2006 - 2008 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.ScreenCapture
{
    using System;
	using Goletas.ScreenCapture.Drawing.Imaging;

	public sealed class JobInfo
	{
        private string _WorkingDirectory;

        public string WorkingDirectory
        {
            get
            {
                return this._WorkingDirectory;
            }
        }

        private string _FileNameLabel;

        public string FileNameLabel
        {
            get
            {
                return this._FileNameLabel;
            }
        }

        private string _ExternalApp;

        public string ExternalApp
        {
            get
            {
                return this._ExternalApp;
            }
        }

        private bool _UseFileOverwrite;

        public bool UseFileOverwrite
        {
            get
            {
                return this._UseFileOverwrite;
            }
        }

        private FileType _FileType;

        public FileType FileType
        {
            get
            {
                return this._FileType;
            }
        }

		private ImageDestinations _ImageDestination;

		public ImageDestinations ImageDestinations
		{
			get
			{
				return this._ImageDestination;
			}
		}

		private bool _IncludeCursor;

		public bool IncludeCursor
		{
			get
			{
				return this._IncludeCursor;
			}
		}

        internal JobInfo(Settings settings)
        {
            this._ImageDestination = settings.ImageDestinations;
            this._IncludeCursor = settings.IncludeCursor;

            if ((this._ImageDestination & ImageDestinations.File) == ImageDestinations.File)
            {
                this._UseFileOverwrite = settings.UseFileOverwrite;
                this._WorkingDirectory = settings.WorkingDirectory;
                this._FileNameLabel = settings.GetFileNameLabel();
                this._ExternalApp = (settings.UseExternalApp) ? settings.ExternalApp : string.Empty;
                this._FileType = (settings.IsSynchronized) ? settings.FileType : settings.FileType.Clone();
            }
        }

	}
}