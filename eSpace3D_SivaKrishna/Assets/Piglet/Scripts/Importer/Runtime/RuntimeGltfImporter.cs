using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Assertions;

namespace Piglet {
    public class RuntimeGltfImporter : GltfImporter
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RuntimeGltfImporter(Uri uri, byte[] data,
            ProgressCallback progressCallback)
            : base(uri, data, new RuntimeGltfImportCache(), progressCallback) {}

        /// <summary>
        /// Returns an asynchronous glTF import task. The import task is
        /// advanced by calling MoveNext(), typically from the
        /// Update() callback of a MonoBehaviour.
        ///
        /// At least one of the "uri" and "data" arguments must be
        /// non-null. If both are non-null, no data is read from
        /// the "uri" argument; the "uri" is only for resolving relative
        /// URIs that appear in the input .gltf/.glb/.zip file provided by
        /// the "data" argument. In the case that "data" argument
        /// is a .zip file, the "uri" argument is completely ignored and
        /// any relative URIs are resolved relative the location of
        /// the .gltf/.glb file within the zip archive.
        /// </summary>
        /// <param name="uri">
        /// The URI of the input .gltf/.glb/.zip file.
        /// </param>
        /// <param name="data">
        /// The raw bytes of the input .gltf/.glb/.zip file.
        /// </param>
        /// <returns>
        /// an asyncronous glTF import task (GltfImportTask)
        /// </returns>
        static public GltfImportTask GetImportTask(Uri uri,
            byte[] data)
        {
            GltfImportTask importTask = new GltfImportTask();

            RuntimeGltfImporter importer
                = new RuntimeGltfImporter(uri, data,
                    (step, completed, total) =>
                        importTask.OnProgress?.Invoke(step, completed, total));

            importTask.AddTask(importer.ReadUri());
            importTask.AddTask(importer.ParseFile());
            importTask.AddTask(importer.LoadBuffers());
            importTask.AddTask(importer.LoadTextures());
            importTask.AddTask(importer.LoadMaterials());
            importTask.AddTask(importer.LoadMeshes());
            importTask.AddTask(importer.LoadScene());
            importTask.AddTask(importer.LoadMorphTargets());
            importTask.AddTask(importer.LoadSkins());

            // note: the final subtask must return the
            // root GameObject for the imported model.
            importTask.AddTask(importer.GetSceneObjectEnum());

            // callbacks to clean up any imported game objects
            // when the user aborts the import or an exception
            // occurs
            importTask.OnAborted += importer.Clear;
            importTask.OnException += _ => importer.Clear();

            return importTask;
        }

        /// <summary>
        /// Returns an asynchronous glTF import task. The import task is
        /// advanced by calling MoveNext(), typically from the
        /// Update() callback of a MonoBehaviour.
        /// </summary>
        /// <param name="uri">
        /// the URI of the input .gltf/.glb/.zip file
        /// </param>
        /// <returns>
        /// an asyncronous glTF import task (GltfImportTask)
        /// </returns>
        public static GltfImportTask GetImportTask(Uri uri)
        {
            return GetImportTask(uri, null);
        }

        /// <summary>
        /// Returns an asynchronous glTF import task. The import task is
        /// advanced by calling MoveNext(), typically from the
        /// Update() callback of a MonoBehaviour.
        /// </summary>
        /// <param name="uri">
        /// absolute URI for the input .gltf/.glb/.zip file (file path
        /// or HTTP URL)
        /// </param>
        /// <returns>
        /// an asyncronous glTF import task (GltfImportTask)
        /// </returns>
        public static GltfImportTask GetImportTask(string uri)
        {
            return GetImportTask(new Uri(uri), null);
        }

        /// <summary>
        /// Returns an asynchronous glTF import task. The import task is
        /// advanced by calling MoveNext(), typically from the
        /// Update() callback of a MonoBehaviour.
        /// </summary>
        /// <param name="data">
        /// the raw bytes of the input .gltf/.glb/.zip file
        /// </param>
        /// <returns>
        /// an asyncronous glTF import task (GltfImportTask)
        /// </returns>
        public static GltfImportTask GetImportTask(byte[] data)
        {
            return GetImportTask(null, data);
        }
    }
}
