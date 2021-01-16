﻿using System.Collections;
using System.IO;

namespace Piglet
{
    public static class FileUtil
    {
        /// <summary>
        /// Delete the file or directory at the given path,
        /// if any such file/directory exists. Directories
        /// are deleted recursively.
        /// </summary>
        static public void DeleteFileOrDirectory(string path)
        {
            if (File.Exists(path))
                File.Delete(path);
            
            if (Directory.Exists(path))
                Directory.Delete(path, true);
        }

        /// <summary>
        /// Write a byte array to a file.
        /// </summary>
        static public IEnumerable WriteAllBytes(string path, byte[] data)
        {
            using (var inputStream = new MemoryStream(data))
            {
                using (var outputStream = new FileStream(
                    path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    var copyTask = StreamUtil.CopyStreamEnum(inputStream, outputStream);
                    while (copyTask.MoveNext())
                        yield return null;
                }
            }
        }
    }
}