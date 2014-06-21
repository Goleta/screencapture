//
// Copyright © 2005 - 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.Win32
{
    using System;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Sequential)]
    internal struct RECT
	{
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;

		public bool Contains(POINT point)
		{
			if ((point.X >= this.Left) && (point.X <= this.Right))
			{
				return ((point.Y >= this.Top) && (point.Y <= this.Bottom));
			}

			return false;
		}

        public bool Intersect(ref RECT other, out RECT intersection)
        {
            RECT e;

            if (((e.Right = (this.Right < other.Right) ? this.Right : other.Right) >= (e.Left = (this.Left > other.Left) ? this.Left : other.Left)) && ((e.Bottom = (this.Bottom < other.Bottom) ? this.Bottom : other.Bottom) >= (e.Top = (this.Top > other.Top) ? this.Top : other.Top)))
            {
                intersection = e;
                return true;
            }

            intersection = new RECT();
            return false;
        }

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }

	}
}