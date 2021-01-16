#if UNITY_STANDALONE_LINUX && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace SFB
{

    public class StandaloneFileBrowserLinux : IStandaloneFileBrowser<string>
    {

        private static Action<IEnumerable<string>> _openFileCb;
        private static Action<IEnumerable<string>> _openFolderCb;
        private static Action<string> _saveFileCb;

        [UnmanagedFunctionPointer(CallingConvention.StdCall)]
        public delegate void AsyncCallback(string path);

        [DllImport("StandaloneFileBrowser")]
        private static extern void DialogInit();
        [DllImport("StandaloneFileBrowser")]
        private static extern IntPtr DialogOpenFilePanel(string title, string directory, string extension, bool multiselect);
        [DllImport("StandaloneFileBrowser")]
        private static extern void DialogOpenFilePanelAsync(string title, string directory, string extension, bool multiselect, AsyncCallback callback);
        [DllImport("StandaloneFileBrowser")]
        private static extern IntPtr DialogOpenFolderPanel(string title, string directory, bool multiselect);
        [DllImport("StandaloneFileBrowser")]
        private static extern void DialogOpenFolderPanelAsync(string title, string directory, bool multiselect, AsyncCallback callback);
        [DllImport("StandaloneFileBrowser")]
        private static extern IntPtr DialogSaveFilePanel(string title, string directory, string defaultName, string extension);
        [DllImport("StandaloneFileBrowser")]
        private static extern void DialogSaveFilePanelAsync(string title, string directory, string defaultName, string extension, AsyncCallback callback);

        public StandaloneFileBrowserLinux()
        {
            DialogInit();
        }

        public IEnumerable<string> OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect)
        {
            var paths = Marshal.PtrToStringAnsi(DialogOpenFilePanel(
                title,
                directory,
                GetFilterFromFileExtensionList(extensions),
                multiselect));
            return paths?.Split((char)28);
        }

        public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<IEnumerable<string>> cb)
        {
            _openFileCb = cb;
            DialogOpenFilePanelAsync(
                title,
                directory,
                GetFilterFromFileExtensionList(extensions),
                multiselect,
                result => { _openFileCb(result.Split((char)28)); });
        }

        public IEnumerable<string> OpenFolderPanel(string title, string directory, bool multiselect)
        {
            var paths = Marshal.PtrToStringAnsi(DialogOpenFolderPanel(
                title,
                directory,
                multiselect));
            return paths?.Split((char)28);
        }

        public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<IEnumerable<string>> cb)
        {
            _openFolderCb = cb;
            DialogOpenFolderPanelAsync(
                title,
                directory,
                multiselect,
                result => { _openFolderCb(result.Split((char)28)); });
        }

        public string SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions)
        {
            return Marshal.PtrToStringAnsi(DialogSaveFilePanel(
                title,
                directory,
                defaultName,
                GetFilterFromFileExtensionList(extensions)));
        }

        public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<string> cb)
        {
            _saveFileCb = cb;
            DialogSaveFilePanelAsync(
                title,
                directory,
                defaultName,
                GetFilterFromFileExtensionList(extensions),
                 result => { _saveFileCb(result); });
        }

        private static string GetFilterFromFileExtensionList(ExtensionFilter[] extensions)
        {
            if (extensions == null)
            {
                return "";
            }
            var filterString = "";
            foreach (var filter in extensions)
            {
                filterString += filter.Name + ";";
                foreach (var ext in filter.Extensions)
                {
                    filterString += ext + ",";
                }
                filterString = filterString.Remove(filterString.Length - 1);
                filterString += "|";
            }
            filterString = filterString.Remove(filterString.Length - 1);
            return filterString;
        }
    }
}

#endif