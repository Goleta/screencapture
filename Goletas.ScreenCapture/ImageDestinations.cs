//
// Copyright © 2005 - 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.ScreenCapture
{
	using System;

    /// <summary>
    /// Specifies captured area destination.
    /// </summary>
	[Flags]
	public enum ImageDestinations : int
	{
        /// <summary>
        /// Clipboard.
        /// </summary>
		Clipboard			= 1,

        /// <summary>
        /// File.
        /// </summary>
		File				= 2,

		/// <summary>
		/// Clipboard and file.
		/// </summary>
		ClipboardAndFile	= Clipboard | File,

	/*	/// <summary>
		/// Printer. This feature is not supported.
		/// </summary>
		Printer				= 4 */
	}
}