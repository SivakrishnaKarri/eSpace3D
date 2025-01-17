using System.IO;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Piglet
{
    public class UnityPathUtil
    {
        public static string NormalizePathSeparators(string path)
        {
            string result = path.Replace("\\\\", "/").Replace("\\", "/");

            // remove trailing slash if present, because this can affect
            // the results of some .NET methods (e.g. `Path.GetDirectoryName`)
            if (result.EndsWith("/"))
                result = result.Substring(0, result.Length - 1);

            return result;
        }

        public static string GetProjectPath(string absolutePath)
        {
            return NormalizePathSeparators(absolutePath.Replace(Application.dataPath, "Assets"));
        }

        public static string GetAbsolutePath(string projectPath)
        {
            return NormalizePathSeparators(projectPath.Replace("Assets", Application.dataPath));
        }

        public static string GetParentDir(string path)
        {
            return NormalizePathSeparators(Path.GetDirectoryName(path));
        }

        public static string GetFileURI(string absolutePath)
        {
            return "file://" + NormalizePathSeparators(absolutePath);
        }

#if UNITY_EDITOR
        public static void RemoveProjectDir(string path)
        {
            if (!Directory.Exists(path))
                return;

            Directory.Delete(path, true);
            AssetDatabase.Refresh();
        }
#endif

    }
}
