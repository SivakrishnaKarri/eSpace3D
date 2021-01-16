#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Piglet
{
    /// <summary>
    /// An import cache that serializes imported Unity assets to
    /// disk, in addition to holding them in memory.  This import
    /// cache is intended for glTF imports that take place
    /// in the Unity Editor.
    /// </summary>
    public class EditorGltfImportCache : GltfImportCache
    {
        /// <summary>
        /// The base project directory for saving the imported
        /// Unity assets (e.g. "Assets/Imported/MyModel").
        /// </summary>
        protected string _importBaseDir;

        /// <summary>
        /// The project directory for saving imported textures
        /// (e.g. "Assets/Imported/MyModel/Textures").
        /// </summary>
        protected string _importTexturesDir;

        /// <summary>
        /// The project directory for saving imported materials
        /// (e.g. "Assets/Imported/MyModel/Materials").
        /// </summary>
        protected string _importMaterialsDir;

        /// <summary>
        /// The project directory for saving imported meshes
        /// (e.g. "Assets/Imported/MyModel/Meshes").
        /// </summary>
        protected string _importMeshesDir;

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="importBaseDir">
        /// The base project directory for saving the imported
        /// Unity assets (e.g. "Assets/Imported/MyModel").
        /// </param>
        public EditorGltfImportCache(string importBaseDir)
        {
            Textures = new SerializedAssetList<Texture2D>(SerializeTexture);
            Materials = new SerializedAssetList<Material>(SerializeMaterial);
            Meshes = new SerializedAssetList<List<KeyValuePair<Mesh,Material>>>
                (SerializeMesh);

            // create directory structure for imported assets

            _importBaseDir = importBaseDir;
            Directory.CreateDirectory(
                UnityPathUtil.GetAbsolutePath(_importBaseDir));

            _importTexturesDir = Path.Combine(_importBaseDir, "Textures");
            Directory.CreateDirectory(
                UnityPathUtil.GetAbsolutePath(_importTexturesDir));

            _importMaterialsDir = Path.Combine(_importBaseDir, "Materials");
            Directory.CreateDirectory(
                UnityPathUtil.GetAbsolutePath(_importMaterialsDir));

            _importMeshesDir = Path.Combine(_importBaseDir, "Meshes");
            Directory.CreateDirectory(
                UnityPathUtil.GetAbsolutePath(_importMeshesDir));
        }

        /// <summary>
        /// Save the given texture to disk as a Unity asset and
        /// return a new Texture2D. The returned Texture2D
        /// is the same as the original, except that it
        /// knows about the asset file that backs it and will
        /// automatically synchronize in-memory changes to disk.
        /// (For further info, see the Unity documentation for
        /// AssetDatabase.)
        /// </summary>
        /// <param name="texture">
        /// The texture to be serialized to disk.
        /// </param>
        /// <returns>
        /// A new Texture2D that is backed by an asset file.
        /// </returns>
        protected Texture2D SerializeTexture(int index, Texture2D texture)
        {
			// Unity's Texture2D.LoadImage() method imports
			// .png/.jpg images upside down, so flip it
			// right side up again.
			texture = TextureUtil.FlipTexture(texture);

            string basename = String.Format("texture_{0}.png", index);
            string pngPath = Path.Combine(_importTexturesDir, basename);
            byte[] pngData = texture.EncodeToPNG();
            File.WriteAllBytes(UnityPathUtil.GetAbsolutePath(pngPath), pngData);

            AssetDatabase.Refresh();
            texture = (Texture2D) AssetDatabase.LoadAssetAtPath(
                pngPath, typeof(Texture2D));

            return texture;
        }

        /// <summary>
        /// Save the given material to disk as a Unity asset
        /// and return a new Material. The returned Material
        /// is the same as the original, except that it knows
        /// about the .mat file that backs it and will automatically
        /// synchronize in-memory changes to disk. (For further
        /// info, see the Unity documentation for AssetDatabase.)
        /// </summary>
        /// <param name="material">
        /// The material to be serialized to disk
        /// </param>
        /// <returns>
        /// A new Material that is backed by a .mat file
        /// </returns>
        protected Material SerializeMaterial(int index, Material material)
        {
            string basename = String.Format("material_{0}.mat", index);
            string path = Path.Combine(_importMaterialsDir, basename);

            AssetDatabase.CreateAsset(material, path);
            AssetDatabase.Refresh();
            material = (Material) AssetDatabase.LoadAssetAtPath(
                path, typeof(Material));

            return material;
        }

        /// <summary>
        /// Save the input mesh to disk as a set of Unity .asset
        /// files and return a new mesh. The input mesh is a list
        /// mesh primitives, where each primitive is a KeyValuePair
        /// of a Mesh and a Material.  The returned mesh (i.e. list of primitives)
        /// is the same as the input list, except that the Mesh
        /// for each primitive has been replaced by one that is backed
        /// by a Unity .asset file.  These Mesh objects know about
        /// their backing .asset file and will automatically sync
        /// in-memory changes to the Mesh to disk. (For further
        /// info, see the Unity documentation for AssetDatabase.)
        /// </summary>
        /// <param name="mesh">
        /// The mesh (list of mesh primitives) to be serialized to disk.
        /// </param>
        /// <returns>
        /// A new mesh (list of mesh primitives) that is backed by a
        /// set of .asset files (one per mesh primitive).
        /// </returns>
        protected List<KeyValuePair<Mesh, Material>> SerializeMesh(
            int index, List<KeyValuePair<Mesh, Material>> mesh)
        {
            for (int i = 0; i < mesh.Count; ++i)
            {
                Mesh primitiveMesh = mesh[i].Key;
                Material primitiveMaterial = mesh[i].Value;

                string basename = String.Format(
                    "mesh_{0}_{1}.asset", index, i);
                string path = Path.Combine(_importMeshesDir, basename);

                // Serialize the mesh to disk as a Unity asset.
                //
                // Note: The primitiveMaterial does not need
                // to be serialized here, since that has already
                // been done during the earlier material-importing
                // step.

                AssetDatabase.CreateAsset(primitiveMesh, path);
                AssetDatabase.Refresh();
                primitiveMesh = (Mesh) AssetDatabase.LoadAssetAtPath(
                    path, typeof(Mesh));

                mesh[i] = new KeyValuePair<Mesh, Material>(
                    primitiveMesh, primitiveMaterial);
            }

            return mesh;
        }

        /// <summary>
        /// Remove a game object from the scene and from memory.
        /// Note: This method uses Object.DestroyImmediate instead of
        /// Object.Destroy because it is run from inside the Editor.
        /// Object.Destroy relies on the Unity game loop and thus only
        /// works in Play Mode.
        /// </summary>
        override protected void Destroy(GameObject gameObject)
        {
            Object.DestroyImmediate(gameObject);
        }
    }
}
#endif