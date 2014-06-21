//
// Copyright © 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.ScreenCapture.Drawing.Imaging
{
    using System;
    using System.Text;
    using System.Globalization;
    using Microsoft.Win32;

    public sealed class FileTypeCollection
    {
        private FileType[] _Items;

        public int Count
        {
            get
            {
                return this._Items.Length;
            }
        }

        public FileType this[int index]
        {
            get
            {
                return this._Items[index].Clone();
            }
        }

        public bool Replace(FileType item)
        {
            if (item == null)
            {
                throw new ArgumentNullException();
            }

            for (int i = 0; i < this._Items.Length; i++)
            {
                if (this._Items[i].FormatId.Equals(item.FormatId))
                {
                    this._Items[i] = item.Clone();

                    return true;
                }
            }

            return false;
        }

        public int IndexOf(Guid formatId)
        {
            for (int i = 0; i < this._Items.Length; i++)
            {
                if (this._Items[i].FormatId.Equals(formatId))
                {
                    return i;
                }
            }

            return -1;
        }

        public string[] GetDisplayNames()
        {
            string[] info = new string[this._Items.Length];

            for (int i = 0; i < this._Items.Length; i++)
            {
                info[i] = this._Items[i].ToString();
            }

            return info;
        }


        public string GetTypeFilter()
        {
            StringBuilder filter = new StringBuilder(256);

            for (int i = 0; i < this._Items.Length; i++)
            {
                filter.Append(this._Items[i].Name);
                filter.Append(" (*.");
                filter.Append(this._Items[i].Extensions[0]);
                filter.Append(")|*.");
                filter.Append(this._Items[i].Extensions[0]);
                filter.Append("|");
            }

            return filter.ToString(0, filter.Length - 1);
        }

        internal FileTypeCollection(FileType[] items)
        {
            this._Items = items;
        }

    }
}