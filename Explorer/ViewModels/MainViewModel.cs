using Explorer.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;

namespace Explorer.ViewModels
{
    public class MainViewModel
    {
        public ObservableCollection<FileSystemItem> Items { get; }
            = new();

        public void LoadDrives()
        {
            Items.Clear();

            foreach (var drive in DriveInfo.GetDrives())
            {
                Items.Add(new FileSystemItem
                {
                    Name = drive.Name,
                    FullPath = drive.RootDirectory.FullName,
                    Icon = ShellIconProvider.GetIcon(drive.Name),
                    IsDrive = true
                });
            }
        }
        public void LoadFolder(string path)
        {
            Items.Clear();

            foreach (var dir in Directory.GetDirectories(path))
            {
                Items.Add(new FileSystemItem
                {
                    Name = Path.GetFileName(dir),
                    FullPath = dir,
                    Icon = ShellIconProvider.GetIcon(dir),
                    IsDirectory = true
                });
            }

            foreach (var file in Directory.GetFiles(path))
            {
                Items.Add(new FileSystemItem
                {
                    Name = Path.GetFileName(file),
                    FullPath = file,
                    Icon = ShellIconProvider.GetIcon(file)
                });
            }
        }
    }
}
