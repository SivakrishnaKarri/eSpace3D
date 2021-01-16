#if UNITY_WSA && !UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using TriLibCore;
using UnityEngine;

namespace SFB
{
    public class StorageItemWithStream : IItemWithStream
    {
        public IStorageItem StorageItem;
        public string Name {
            get {
                return StorageItem.Name;
            }
            set {
            }
        }
        public Stream Stream {get; set;}
    }

    public class StandaloneFileBrowserWinRT : IStandaloneFileBrowser<StorageItemWithStream>
    {
        public IEnumerable<StorageItemWithStream> OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect)
        {
            return null;
        }

        public IEnumerable<StorageItemWithStream> OpenFolderPanel(string title, string directory, bool multiselect)
        {
            return null;
        }

        public StorageItemWithStream SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions)
        {
            return null;
        }

        public void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<IEnumerable<StorageItemWithStream>> cb)
        {
            UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
            {
                var filePicker = new Windows.Storage.Pickers.FileOpenPicker();
                if (extensions != null)
                {
                    foreach (var extension in extensions)
                    {
                        foreach (var filter in extension.Extensions)
                        {
                            filePicker.FileTypeFilter.Add("." + filter);
                        }
                    }
                }
                if (multiselect)
                {
                    var files = await filePicker.PickMultipleFilesAsync();
                    var result = new StorageItemWithStream[files.Count];
                    for (var i = 0; i < files.Count; i++)
                    {
                        result[i] = new StorageItemWithStream()
                        {
                            StorageItem = files[i],
                            Stream = await ReadStorageFile(files[i])
                        };
                    }
                    await Task.Run(() => cb(result));
                }
                else
                {
                    var file = await filePicker.PickSingleFileAsync();
                    var fileWithStream = new StorageItemWithStream()
                    {
                        StorageItem = file,
                        Stream = await ReadStorageFile(file)
                    };
                    await Task.Run(() => cb(new[] { fileWithStream }));
                }
            }, false);
        }

        public void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<IEnumerable<StorageItemWithStream>> cb)
        {
            UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
            {
                var folderPicker = new Windows.Storage.Pickers.FolderPicker();
                var folder = await folderPicker.PickSingleFolderAsync();
                var folderWithStream = new StorageItemWithStream()
                {
                    StorageItem = folder
                };
                await Task.Run(() => cb(new[] { folderWithStream }));
            }, false);
        }

        public void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<StorageItemWithStream> cb)
        {
            UnityEngine.WSA.Application.InvokeOnUIThread(async () =>
            {
                var filePicker = new Windows.Storage.Pickers.FileSavePicker();
                filePicker.SuggestedFileName = defaultName;
                foreach (var extension in extensions)
                {
                    filePicker.FileTypeChoices.Add(extension.Name, extension.Extensions);
                }
                var file = await filePicker.PickSaveFileAsync();
                var fileWithStream = new StorageItemWithStream()
                {
                    StorageItem = file
                };
                await Task.Run(() => cb(fileWithStream));
            }, false);
        }

        private static async Task<Stream> ReadStorageFile(StorageFile storageFile)
        {
            return await storageFile.OpenStreamForReadAsync();
            //using (var stream = await storageFile.OpenStreamForReadAsync())
            //{
            //    var memoryStream = new MemoryStream();
            //    await stream.CopyToAsync(memoryStream);
            //    return memoryStream;
            //}
        }
    }
}
#endif