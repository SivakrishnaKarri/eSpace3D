using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SFB {
    public interface IStandaloneFileBrowser<T> {
        IEnumerable<T> OpenFilePanel(string title, string directory, ExtensionFilter[] extensions, bool multiselect);
        IEnumerable<T> OpenFolderPanel(string title, string directory, bool multiselect);
        T SaveFilePanel(string title, string directory, string defaultName, ExtensionFilter[] extensions);
        void OpenFilePanelAsync(string title, string directory, ExtensionFilter[] extensions, bool multiselect, Action<IEnumerable<T>> cb);
        void OpenFolderPanelAsync(string title, string directory, bool multiselect, Action<IEnumerable<T>> cb);
        void SaveFilePanelAsync(string title, string directory, string defaultName, ExtensionFilter[] extensions, Action<T> cb);
    }
}
