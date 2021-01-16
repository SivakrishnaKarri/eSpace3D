#if UNITY_WEBGL && !UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace SFB
{
    public class BrowserItemWithStream : IItemWithStream
    {
        public string Name { get; set; }
        public Stream Stream { get; set; }
    }

    public class StandloneFileBrowserWebGLHelper : MonoBehaviour
    {
        public Action<IEnumerable<BrowserItemWithStream>> MultipleFilesCallback;
        public Action<BrowserItemWithStream> SingleFileCallback;

        private IEnumerator InvokeCallback(string json)
        {
            var browserFiles = JArray.Parse(json);
            if (browserFiles.Count > 0)
            {
                var browserItemsWithStream = new BrowserItemWithStream[browserFiles.Count];
                for (var i = 0; i < browserFiles.Count; i++)
                {
                    var browserFile = browserFiles[i];
                    var loader = new WWW(browserFile.SelectToken("url").ToString());
                    yield return loader;
                    if (string.IsNullOrWhiteSpace(loader.error))
                    {
                        browserItemsWithStream[i] = new BrowserItemWithStream
                        {
                            Name = browserFile.SelectToken("name").ToString(),
                            Stream = new MemoryStream(loader.bytes)
                        };
                    }
                    else
                    {
                        throw new Exception(loader.error);
                    }
                }
                SingleFileCallback?.Invoke(browserItemsWithStream[0]);
                MultipleFilesCallback?.Invoke(browserItemsWithStream);
            }
            SingleFileCallback = null;
            MultipleFilesCallback = null;
            Destroy(gameObject);
        }
    }

    public class StandaloneFileBrowserWebGL : IStandaloneFileBrowser<BrowserItemWithStream>
    {
        [DllImport("__Internal")]
        private static extern void UploadFile(string gameObjectName, string methodName, string filter, bool multiple);

        [DllImport("__Internal")]
        private static extern void DownloadFile(string gameObjectName, string methodName, string filename, byte[] byteArray, int byteArraySize);

        private bool _processing;

        public byte[] Data;

        public StandaloneFileBrowserWebGL()
        {
        }

        public IEnumerable<BrowserItemWithStream> OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect)
        {
            throw new NotImplementedException("WebGL doesn't support this method.");
        }

        public IEnumerable<BrowserItemWithStream> OpenFolderPanel(string title, string directory, bool multiselect)
        {
            throw new NotImplementedException("WebGL doesn't support this method.");
        }

        public BrowserItemWithStream SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions)
        {
            throw new NotImplementedException("WebGL doesn't support this method.");
        }

        public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<IEnumerable<BrowserItemWithStream>> cb)
        {
            var helper = new GameObject(Guid.NewGuid().ToString()).AddComponent<StandloneFileBrowserWebGLHelper>();
            helper.MultipleFilesCallback = cb;
            UploadFile(helper.name, "InvokeCallback", GetFilterFromFileExtensionList(extensions), false);
        }

        public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<IEnumerable<BrowserItemWithStream>> cb)
        {
            throw new NotImplementedException("WebGL doesn't support this method.");
        }

        public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<BrowserItemWithStream> cb)
        {
            if (Data == null)
            {
                return;
            }
            var helper = new GameObject(Guid.NewGuid().ToString()).AddComponent<StandloneFileBrowserWebGLHelper>();
            helper.SingleFileCallback = cb;
            DownloadFile(helper.name, "InvokeCallback", defaultName, Data, Data.Length);
        }

        private static string GetFilterFromFileExtensionList(ExtensionFilter[] extensions)
        {
            var filterString = "";
            if (extensions != null)
            {
                foreach (var extension in extensions)
                {
                    foreach (var format in extension.Extensions)
                    {
                        if (filterString != "")
                        {
                            filterString += ", ";
                        }
                        filterString += "." + (format[0] == '.' ? format.Substring(1) : format);
                    }
                }
            }
            return filterString;
        }
    }
}
#endif