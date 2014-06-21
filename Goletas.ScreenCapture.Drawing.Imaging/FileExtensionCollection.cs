//
// Copyright © 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.ScreenCapture.Drawing.Imaging
{
    using System;

    public sealed class FileExtensionCollection
    {
        private string[] _Items;

        public int Count
        {
            get
            {
                return this._Items.Length;
            }
        }

        public string this[int index]
        {
            get
            {
                return this._Items[index];
            }
        }

        public FileExtensionCollection(params string[] items)
        {
            this._Items = items;
        }

    }
}