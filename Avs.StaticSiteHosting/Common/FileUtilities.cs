using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Avs.StaticSiteHosting.Web.Common;

public static class FileUtilities
{
    public static IEnumerable<string> GetFilesFromFolder(string folder)
    {
        if (!Directory.Exists(folder))
        {
            return [];
        }

        var fileList = new List<string>();
        var entries = Directory.EnumerateFileSystemEntries(folder);
        
        foreach (var entry in entries)
        {
            var entryIsDirectory = (File.GetAttributes(entry) & FileAttributes.Directory) == FileAttributes.Directory;
            if (entryIsDirectory)
            {
                var files = GetFilesFromFolder(entry);
                fileList.AddRange(files);
            }
            else
            {
                fileList.Add(entry);
            }
        }
        
        return fileList;
    }
}