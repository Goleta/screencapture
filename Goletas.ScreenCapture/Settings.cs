//
// Copyright © 2006 - 2007 Maksim Goleta. All rights reserved.
// GOLETAS PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
//

namespace Goletas.ScreenCapture
{
	using System;
    using System.Globalization;
    using Goletas.ScreenCapture.Drawing.Imaging;
    using Goletas.Win32;

	public class Settings : ICloneable
	{
        [Flags]
        private enum FileNamePattern : uint
        {
            None = 0x0,
            Counter = 0x1,
            Year = 0x2,
            Month = 0x4,
            Day = 0x8,
            Hour = 0x10,
            Minute = 0x20,
            Second = 0x40,
            DateTime = Year | Month | Day | Hour | Minute | Second
        }

        private string _FileNameFormat;
        private FileNamePattern _FileNamePattern;
        private int _Counter;
        private FileType _FileType;
        private ImageDestinations _ImageDestinations;
        private string _WorkingDirectory;
        private bool _AlwaysOnTop;
        private bool _AutoStartup;
        private bool _UseExternalApp;
        private string _ExternalApp;
        private bool _UseFileOverwrite;
        private bool _IncludeCursor;

        public virtual bool IsSynchronized
        {
            get
            {
                return false;
            }
        }

        public virtual FileType FileType
        {
            get
            {
                return this._FileType;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                this._FileType = value;
            }
        }

        public virtual ImageDestinations ImageDestinations
        {
            get
            {
                return this._ImageDestinations;
            }
            set
            {
                if ((value & ~ImageDestinations.ClipboardAndFile) != (ImageDestinations)0)
                {
                    throw new ArgumentOutOfRangeException();
                }

                this._ImageDestinations = value;
            }
        }

        public virtual string WorkingDirectory
        {
            get
            {
                return this._WorkingDirectory;
            }
            set
            {
                this._WorkingDirectory = (value == null) ? string.Empty : value;
            }
        }

        public virtual bool AlwaysOnTop
        {
            get
            {
                return this._AlwaysOnTop;
            }
            set
            {
                this._AlwaysOnTop = value;
            }
        }

        public virtual bool AutoStartup
        {
            get
            {
                return this._AutoStartup;
            }
            set
            {
                this._AutoStartup = value;
            }
        }

        public virtual bool UseExternalApp
        {
            get
            {
                return this._UseExternalApp;
            }
            set
            {
                this._UseExternalApp = value;
            }
        }

        public virtual string ExternalApp
        {
            get
            {
                return this._ExternalApp;
            }
            set
            {
                this._ExternalApp = (value == null) ? string.Empty : value;
            }
        }

        public virtual bool UseFileOverwrite
        {
            get
            {
                return this._UseFileOverwrite;
            }
            set
            {
                this._UseFileOverwrite = value;
            }
        }

        public virtual bool IncludeCursor
        {
            get
            {
                return this._IncludeCursor;
            }
            set
            {
                this._IncludeCursor = value;
            }
        }

        public virtual string FileNameFormat
        {
            get
            {
                return this._FileNameFormat;
            }
        }

        object ICloneable.Clone()
        {
            return this.Clone();
        }

        public virtual Settings Clone()
        {
            Settings clone = (Settings)this.MemberwiseClone();
            clone._FileType = this._FileType.Clone();

            return clone;
        }

        public virtual JobInfo GetJobInfo()
        {
            return new JobInfo(this);
        }

        public virtual void FromSettings(Settings source)
        {
            if (source == null)
            {
                throw new ArgumentNullException();
            }

            if (!ReferenceEquals(this, source))
            {
                this._FileType = source._FileType.Clone();
                this._ImageDestinations = source._ImageDestinations;
                this._WorkingDirectory = source._WorkingDirectory;
                this._AlwaysOnTop = source._AlwaysOnTop;
                this._AutoStartup = source._AutoStartup;
                this._UseExternalApp = source._UseExternalApp;
                this._ExternalApp = source._ExternalApp;
                this._UseFileOverwrite = source._UseFileOverwrite;
                this._IncludeCursor = source._IncludeCursor;
                this._FileNameFormat = source._FileNameFormat;
                this._FileNamePattern = source._FileNamePattern;
            }
        }

        internal string GetFileNameLabel()
        {
            if (this._FileNamePattern == FileNamePattern.None)
            {
                return this._FileNameFormat;
            }

            string name = this._FileNameFormat;

            if ((this._FileNamePattern & FileNamePattern.Counter) == FileNamePattern.Counter)
            {
                name = name.Replace("<counter>", (++this._Counter).ToString(CultureInfo.InvariantCulture));
            }

            if ((this._FileNamePattern & FileNamePattern.DateTime) != FileNamePattern.None)
            {
                DateTime time = DateTime.Now;

                if ((this._FileNamePattern & FileNamePattern.Year) == FileNamePattern.Year)
                {
                    name = name.Replace("<yyyy>", time.Year.ToString("0000", CultureInfo.InvariantCulture));
                }

                if ((this._FileNamePattern & FileNamePattern.Month) == FileNamePattern.Month)
                {
                    name = name.Replace("<MM>", time.Month.ToString("00", CultureInfo.InvariantCulture));
                }

                if ((this._FileNamePattern & FileNamePattern.Day) == FileNamePattern.Day)
                {
                    name = name.Replace("<dd>", time.Day.ToString("00", CultureInfo.InvariantCulture));
                }

                if ((this._FileNamePattern & FileNamePattern.Hour) == FileNamePattern.Hour)
                {
                    name = name.Replace("<hh>", time.Hour.ToString("00", CultureInfo.InvariantCulture));
                }

                if ((this._FileNamePattern & FileNamePattern.Minute) == FileNamePattern.Minute)
                {
                    name = name.Replace("<mm>", time.Minute.ToString("00", CultureInfo.InvariantCulture));
                }

                if ((this._FileNamePattern & FileNamePattern.Second) == FileNamePattern.Second)
                {
                    name = name.Replace("<ss>", time.Second.ToString("00", CultureInfo.InvariantCulture));
                }
            }

            if (Settings.IsValidFileName(name))
            {
                return name;
            }

            return string.Empty;
        }

        public static bool IsValidFileNameFormat(string format)
        {
            return UpdateFileNameFormat(format, null);
        }

        private static bool UpdateFileNameFormat(string format, Settings settings)
        {
            if (format != null && format.Length != 0)
            {
                FileNamePattern pattern = FileNamePattern.None;
                string name = format;

                if (name.Contains("<counter>"))
                {
                    pattern |= FileNamePattern.Counter;
                    name = format.Replace("<counter>", "0");
                }

                if (name.Contains("<yyyy>"))
                {
                    pattern |= FileNamePattern.Year;
                    name = name.Replace("<yyyy>", "0000");
                }

                if (name.Contains("<MM>"))
                {
                    pattern |= FileNamePattern.Month;
                    name = name.Replace("<MM>", "00");
                }

                if (name.Contains("<dd>"))
                {
                    pattern |= FileNamePattern.Day;
                    name = name.Replace("<dd>", "00");
                }

                if (name.Contains("<hh>"))
                {
                    pattern |= FileNamePattern.Hour;
                    name = name.Replace("<hh>", "00");
                }

                if (name.Contains("<mm>"))
                {
                    pattern |= FileNamePattern.Minute;
                    name = name.Replace("<mm>", "00");
                }

                if (name.Contains("<ss>"))
                {
                    pattern |= FileNamePattern.Second;
                    name = name.Replace("<ss>", "00");
                }

                if (((pattern != FileNamePattern.None) && Settings.IsValidFileName(name)) || Settings.IsValidFileName(format))
                {
                    if (settings != null)
                    {
                        settings._FileNameFormat = format;
                        settings._FileNamePattern = pattern;
                    }

                    return true;
                }
            }

            return false;
        }

        public virtual bool UpdateFileNameFormat(string format)
        {
            return UpdateFileNameFormat(format, this);
        }

        internal Settings()
        {
            this._IncludeCursor = true;
            this._ImageDestinations = ImageDestinations.Clipboard;
            this._WorkingDirectory = string.Empty; // make "my pictures" default?
            this._ExternalApp = string.Empty;
            this._FileNameFormat = string.Empty;
            this._FileNameFormat = "<yyyy>.<MM>.<dd> <hh>-<mm>-<ss>";
            this._FileNamePattern = FileNamePattern.DateTime;
          //  this._FileType = new PngFileType();
        }


        /// <summary>
        /// Wrapps the supplied <paramref name="settings"/> into the equivalent
        /// class that is safe to share between multiple threads.
        /// </summary>
        /// <param name="settings">
        /// The object to synchronize.
        /// </param>
        /// <returns>
        /// The thread safe wrapper of the <see cref="Settings"/> class.
        /// </returns>
        internal static Settings Synchronized(Settings settings)
        {
            return new __Settings(settings);
        }

        private sealed class __Settings : Settings
        {
            private readonly object _SyncRoot;
            private Settings _Settings;

            public override bool IsSynchronized
            {
                get
                {
                    return true;
                }
            }

            public override bool AlwaysOnTop
            {
                get
                {
                    return this._Settings._AlwaysOnTop;
                }
                set
                {
                    lock (this._SyncRoot)
                    {
                        this._Settings._AlwaysOnTop = value;
                    }
                }
            }

            public override string ExternalApp
            {
                get
                {
                    return this._Settings._ExternalApp;
                }
                set
                {
                    lock (this._SyncRoot)
                    {
                        this._Settings._ExternalApp = (value == null) ? string.Empty : value;
                    }
                }
            }

            public override bool AutoStartup
            {
                get
                {
                    return this._Settings._AutoStartup;
                }
                set
                {
                    lock (this._SyncRoot)
                    {
                        this._Settings._AutoStartup = value;
                    }
                }
            }

            public override string FileNameFormat
            {
                get
                {
                    return this._Settings._FileNameFormat;
                }
            }

            public override FileType FileType
            {
                get
                {
                    return this._Settings._FileType.Clone();
                }
                set
                {
                    if (value == null)
                    {
                        throw new ArgumentNullException();
                    }

                    lock (this._SyncRoot)
                    {
                        this._Settings._FileType = value.Clone();
                    }
                }
            }

            public override ImageDestinations ImageDestinations
            {
                get
                {
                    return this._Settings._ImageDestinations;
                }
                set
                {
                    if ((value & ~ImageDestinations.ClipboardAndFile) != (ImageDestinations)0)
                    {
                        throw new ArgumentOutOfRangeException();
                    }

                    lock (this._SyncRoot)
                    {
                        this._Settings._ImageDestinations = value;
                    }
                }
            }

            public override bool IncludeCursor
            {
                get
                {
                    return this._Settings._IncludeCursor;
                }
                set
                {
                    lock (this._SyncRoot)
                    {
                        this._Settings._IncludeCursor = value;
                    }
                }
            }

            public override bool UseExternalApp
            {
                get
                {
                    return this._Settings._UseExternalApp;
                }
                set
                {
                    lock (this._SyncRoot)
                    {
                        this._Settings._UseExternalApp = value;
                    }
                }
            }

            public override bool UseFileOverwrite
            {
                get
                {
                    return this._Settings._UseFileOverwrite;
                }
                set
                {
                    lock (this._SyncRoot)
                    {
                        this._Settings._UseFileOverwrite = value;
                    }
                }
            }

            public override string WorkingDirectory
            {
                get
                {
                    return this._Settings._WorkingDirectory;
                }
                set
                {
                    lock (this._SyncRoot)
                    {
                        this._Settings._WorkingDirectory = (value == null) ? string.Empty : value;
                    }
                }
            }

            public override Settings Clone()
            {
                lock (this._SyncRoot)
                {
                    return this._Settings.Clone();
                }
            }

            public override JobInfo GetJobInfo()
            {
                lock (this._SyncRoot)
                {
                    return this._Settings.GetJobInfo();
                }
            }

            public override void FromSettings(Settings source)
            {
                if (source == null)
                {
                    throw new ArgumentNullException();
                }

                if (!ReferenceEquals(this, source))
                {
                    lock (this._SyncRoot)
                    {
                        int counter = this._Settings._Counter;
                        this._Settings = source.Clone();
                        this._Settings._Counter = counter;
                    }
                }
            }

            public override bool UpdateFileNameFormat(string format)
            {
                lock (this._SyncRoot)
                {
                    return this._Settings.UpdateFileNameFormat(format);
                }
            }

            internal __Settings(Settings settings)
            {
                this._SyncRoot = new object();
                this._Settings = settings;
            }
        }


        //
        // Windows
        //     http://msdn2.microsoft.com/en-us/library/Aa365247.aspx
        //     http://msdn2.microsoft.com/en-us/library/ms810456.aspx
        //
        // Illigal computer name characters:    ~ ! @ # $ ^ & ( ) = + [ ] { } ; ' ,    range 0 through 31    and any illigal chars for file name
        // Illigal file/folder name characters:    \ / : * ? " < > |    range 0 through 31
        //

        // TODO: add CLOCK$ to reserved names?


        internal static bool IsValidFileName(string e)
        {
            int i;

            if (((e == null) || (e.Length == 0) || (e.Length > (NativeMethods.MAX_PATH - 4)) || (e[0] == '.') || (e[0] == ' ') || (e[i = e.Length - 1] == '.') || (e[i] == ' '))
                ||
                (
                    ((e.Length == 4) || ((e.Length > 4) && (e[4] == '.')))
                    &&
                    (
                        (
                            ((e[0] == 'c' || e[0] == 'C') && (e[1] == 'o' || e[1] == 'O') && (e[2] == 'm' || e[2] == 'M')) // COM
                            ||
                            ((e[0] == 'l' || e[0] == 'L') && (e[1] == 'p' || e[1] == 'P') && (e[2] == 't' || e[2] == 'T')) // LPT
                        )
                        &&
                        (e[3] >= '0' && e[3] <= '9') // digit
                    )
                )
                ||
                (
                    ((e.Length == 3) || ((e.Length > 3) && (e[3] == '.')))
                    &&
                    (
                        ((e[0] == 'a' || e[0] == 'A') && (e[1] == 'u' || e[1] == 'U') && (e[2] == 'x' || e[2] == 'X')) // AUX
                        ||
                        ((e[0] == 'c' || e[0] == 'C') && (e[1] == 'o' || e[1] == 'O') && (e[2] == 'n' || e[2] == 'N')) // CON
                        ||
                        ((e[0] == 'n' || e[0] == 'N') && (e[1] == 'u' || e[1] == 'U') && (e[2] == 'l' || e[2] == 'L')) // NUL
                        ||
                        ((e[0] == 'p' || e[0] == 'P') && (e[1] == 'r' || e[1] == 'R') && (e[2] == 'n' || e[2] == 'N')) // PRN
                    )
                ))
            {
                return false;
            }

            for (i = 0; i < e.Length; i++)
            {
                char c = e[i];

                if ((c <= '\x001f') || (c == '\\') || (c == '/') || (c == ':') || (c == '*') || (c == '?') || (c == '\"') || (c == '<') || (c == '>') || (c == '|'))
                {
                    return false;
                }
            }

            return true;
        }

        internal static bool IsValidPathToExecutable(string e)
        {
            if ((e.Length > 7) && IsValidPath(e))
            {
                int i = e.Length - 1;

                if (e[i] == 'e' || e[i] == 'E')
                {
                    i--;

                    if (e[i] == 'x' || e[i] == 'X')
                    {
                        i--;

                        if (e[i] == 'e' || e[i] == 'E')
                        {
                            i--;

                            return (e[i] == '.');
                        }
                    }
                }
            }

            return false;
        }


        //     \\localhost\test
        //     \\localhost\test\
        //     D:\My Documents\My Programming
        //     D:\My Documents\My Programming\

        internal static bool IsValidPath(string e)
        {
            if ((e != null) && (e.Length < (NativeMethods.MAX_PATH)))
            {
                int i;

                if ((e.Length > 4) && (e[0] == '\\') && (e[1] == '\\')) // network path
                {
                    i = 2;

                    while (i < e.Length) // check for illigal computer name chars
                    {
                        char c = e[i];
                        i++;

                        if (c == '\\')
                        {
                            break;
                        }

                        if ((c <= '\x001f') || (c == '/') || (c == ':') || (c == '*') || (c == '?') || (c == '\"') || (c == '<') || (c == '>') || (c == '|') || (c == '~') || (c == '!') || (c == '@') || (c == '#') || (c == '$') || (c == '^') || (c == '&') || (c == '(') || (c == ')') || (c == '=') || (c == '+') || (c == '[') || (c == ']') || (c == '{') || (c == '}') || (c == ';') || (c == '\'') || (c == ',') || (c == ' ') || (c == '.'))
                        {
                            return false;
                        }
                    }

                    if ((e.Length - i) == 0) // check for computer name length
                    {
                        return false;
                    }
                }
                else if ((e.Length > 2) && (((e[0] >= 'a') && (e[0] <= 'z')) || ((e[0] >= 'A') && (e[0] <= 'Z'))) && (e[1] == ':') && (e[2] == '\\')) // local path
                {
                    i = 3;
                }
                else
                {
                    return false;
                }

                while (i < e.Length)
                {
                    int j = i;
                    char c;

                    do
                    {
                        c = e[j];

                        if (c == '\\')
                        {
                            break;
                        }

                        if ((c <= '\x001f') || (c == '/') || (c == ':') || (c == '*') || (c == '?') || (c == '\"') || (c == '<') || (c == '>') || (c == '|'))
                        {
                            return false;
                        }

                        j++;
                    }
                    while (j < e.Length);


                    int s = j - i;

                    if ((s == 0) || (c == '.') || (c == ' ') || (e[i] == '.') || (e[i] == ' ')) // check for the subpath name length, last and first chars in the name
                    {
                        return false;
                    }


                    if ((((s == 4) || ((s > 4) && (e[i + 4] == '.')))
                        &&
                            (
                                (
                                    ((e[i] == 'c' || e[i] == 'C') && (e[i + 1] == 'o' || e[i + 1] == 'O') && (e[i + 2] == 'm' || e[i + 2] == 'M')) // COM
                                    ||
                                    ((e[i] == 'l' || e[i] == 'L') && (e[i + 1] == 'p' || e[i + 1] == 'P') && (e[i + 2] == 't' || e[i + 2] == 'T')) // LPT
                                )
                                &&
                                (e[i + 3] >= '0' && e[i + 3] <= '9') // digit
                            )
                        )
                        ||
                        (
                            ((s == 3) || ((s > 3) && (e[i + 3] == '.')))
                            &&
                            (
                                ((e[i] == 'a' || e[i] == 'A') && (e[i + 1] == 'u' || e[i + 1] == 'U') && (e[i + 2] == 'x' || e[i + 2] == 'X')) // AUX
                                ||
                                ((e[i] == 'c' || e[i] == 'C') && (e[i + 1] == 'o' || e[i + 1] == 'O') && (e[i + 2] == 'n' || e[i + 2] == 'N')) // CON
                                ||
                                ((e[i] == 'n' || e[i] == 'N') && (e[i + 1] == 'u' || e[i + 1] == 'U') && (e[i + 2] == 'l' || e[i + 2] == 'L')) // NUL
                                ||
                                ((e[i] == 'p' || e[i] == 'P') && (e[i + 1] == 'r' || e[i + 1] == 'R') && (e[i + 2] == 'n' || e[i + 2] == 'N')) // PRN
                            )
                        ))
                    {
                        return false;
                    }

                    i = j + 1;
                }

                return true;
            }

            return false;
        }

        private static bool IsHexRange(ref string e, int start, int end)
        {
            do
            {
                char c = e[start];

                if (!(((c >= '0') && (c <= '9')) || ((c >= 'a') && (c <= 'f')) || ((c >= 'A') && (c <= 'F'))))
                {
                    return false;
                }

                start++;
            }
            while (start <= end);

            return true;
        }

        internal static bool IsGuid(string e) // {2cd20838-55a6-4193-ad7d-e4e883903505}
        {
            return ((e != null) && (e.Length == 38) && (e[0] == '{') && IsHexRange(ref e, 1, 8) && (e[9] == '-') && IsHexRange(ref e, 10, 13) && (e[14] == '-') && IsHexRange(ref e, 15, 18) && (e[19] == '-') && IsHexRange(ref e, 20, 23) && (e[24] == '-') && IsHexRange(ref e, 25, 36) && (e[37] == '}'));
        }

        internal static bool Int32ToBoolTrueOnFail(object e)
        {
            if (e is int)
            {
                return (int)e != 0;
            }

            return true;
        }

        internal static bool Int32ToBoolFalseOnFail(object e)
        {
            if (e is int)
            {
                return (int)e != 0;
            }

            return false;
        }

        internal static ImageDestinations ToImageDestinations(object e)
        {
            if (e is int)
            {
                ImageDestinations Destinations = (ImageDestinations)e;

                if ((Destinations & ~ImageDestinations.ClipboardAndFile) == (ImageDestinations)0)
                {
                    return Destinations;
                }
            }

            return ImageDestinations.Clipboard;
        }

    }
}