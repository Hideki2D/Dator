using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Imaging;

namespace Explorer.Services
{
    public static class ShellIconProvider
    {
        private static readonly ConcurrentDictionary<(string, bool), BitmapSource> Cache = new();

        private const uint SHGFI_ICON = 0x000000100;
        private const uint SHGFI_LARGEICON = 0x000000000;
        private const uint SHGFI_SMALLICON = 0x000000001;
        private const uint SHGFI_USEFILEATTRIBUTES = 0x000000010;

        private const uint FILE_ATTRIBUTE_NORMAL = 0x80;
        private const uint FILE_ATTRIBUTE_DIRECTORY = 0x10;

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct SHFILEINFO
        {
            public IntPtr hIcon;
            public int iIcon;
            public uint dwAttributes;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
            public string szDisplayName;

            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 80)]
            public string szTypeName;
        }

        [DllImport("Shell32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr SHGetFileInfo(
            string pszPath,
            uint dwFileAttributes,
            ref SHFILEINFO psfi,
            uint cbFileInfo,
            uint uFlags);

        [DllImport("user32.dll")]
        private static extern bool DestroyIcon(IntPtr hIcon);

        /// <summary>
        /// Иконка существующего файла, папки или диска.
        /// </summary>
        public static BitmapSource GetIcon(string path, bool small = true)
        {
            return Cache.GetOrAdd((path, small), _ =>
            {
                SHFILEINFO info = new();

                SHGetFileInfo(
                    path,
                    0,
                    ref info,
                    (uint)Marshal.SizeOf<SHFILEINFO>(),
                    SHGFI_ICON | (small ? SHGFI_SMALLICON : SHGFI_LARGEICON));

                if (info.hIcon == IntPtr.Zero)
                    return null!;

                try
                {
                    var image = Imaging.CreateBitmapSourceFromHIcon(
                        info.hIcon,
                        Int32Rect.Empty,
                        BitmapSizeOptions.FromEmptyOptions());

                    image.Freeze();

                    return image;
                }
                finally
                {
                    DestroyIcon(info.hIcon);
                }
            });
        }

        /// <summary>
        /// Иконка по расширению файла (.txt, .png и т.д.)
        /// </summary>
        public static BitmapSource GetFileTypeIcon(string extension, bool small = true)
        {
            extension = extension.StartsWith(".")
                ? extension
                : "." + extension;

            SHFILEINFO info = new();

            SHGetFileInfo(
                extension,
                FILE_ATTRIBUTE_NORMAL,
                ref info,
                (uint)Marshal.SizeOf<SHFILEINFO>(),
                SHGFI_ICON |
                SHGFI_USEFILEATTRIBUTES |
                (small ? SHGFI_SMALLICON : SHGFI_LARGEICON));

            if (info.hIcon == IntPtr.Zero)
                return null!;

            try
            {
                var image = Imaging.CreateBitmapSourceFromHIcon(
                    info.hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                image.Freeze();

                return image;
            }
            finally
            {
                DestroyIcon(info.hIcon);
            }
        }

        /// <summary>
        /// Стандартная иконка папки.
        /// </summary>
        public static BitmapSource GetFolderIcon(bool small = true)
        {
            SHFILEINFO info = new();

            SHGetFileInfo(
                "",
                FILE_ATTRIBUTE_DIRECTORY,
                ref info,
                (uint)Marshal.SizeOf<SHFILEINFO>(),
                SHGFI_ICON |
                SHGFI_USEFILEATTRIBUTES |
                (small ? SHGFI_SMALLICON : SHGFI_LARGEICON));

            if (info.hIcon == IntPtr.Zero)
                return null!;

            try
            {
                var image = Imaging.CreateBitmapSourceFromHIcon(
                    info.hIcon,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                image.Freeze();

                return image;
            }
            finally
            {
                DestroyIcon(info.hIcon);
            }
        }
    }
}
