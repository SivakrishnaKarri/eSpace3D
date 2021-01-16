using System.Collections.Generic;
using UnityEngine;

namespace Piglet
{
    /// <summary>
    /// Caches Unity assets that have been created during a glTF import.
    ///
    /// Caching Unity assets facilitates their reuse in later
    /// import phases. For example, imported Unity textures
    /// need to be accessed/reused when importing materials.
    /// </summary>
    abstract public class GltfImportCache
    {
        /// <summary>
        /// Default material used for a mesh, if none
        /// is specified in the GLTF file.
        /// </summary>
        public Material DefaultMaterial;

        /// <summary>
        /// Binary data buffers loaded from GLTF file.
        /// </summary>
        public List<byte[]> Buffers;

        /// <summary>
        /// Textures loaded from GLTF file. In GLTF, textures
        /// are images with additional parameters applied
        /// (e.g. scaling, filtering).
        /// </summary>
        public IList<Texture2D> Textures;

        /// <summary>
        /// Materials imported from GLTF file.
        /// </summary>
        public IList<Material> Materials;

        /// <summary>
        /// Meshes imported from GLTF file. In GLTF, meshes
        /// consist of one or more submeshes called "primitives",
        /// where each primitive can have a different material.
        /// Here the outer list are the top-level meshes and the inner
        /// lists are the primitives that make up each mesh.
        /// </summary>
        public IList<List<KeyValuePair<Mesh,Material>>> Meshes;

        /// <summary>
        /// The nodes of the GLTF scene hierarchy, which have
        /// a one-to-one correspondence to Unity GameObjects.
        /// The integer keys of the dictionary correspond to
        /// indices in the GLTF nodes array.
        ///
        /// I use a dictionary to hold the nodes because they
        /// are created while traversing the scene hierarchy,
        /// and thus are not necessarily loaded in array order.
        /// </summary>
        public Dictionary<int, GameObject> Nodes;

        /// <summary>
        /// The GameObject corresponding to the root of the
        /// imported glTF scene.
        /// </summary>
        public GameObject Scene;

        /// <summary>
        /// Maps mesh index -> node indices. The same mesh
        /// may be attached to multiple nodes, causing it
        /// to be instantiated multiple times in a scene
        /// (e.g. blades of grass).
        /// </summary>
        public Dictionary<int, List<int>> MeshToNodes;

        /// <summary>
        /// Maps node index -> game objects for mesh primitives.
        /// 
        /// A glTF mesh is composed of one or more "mesh primitives",
        /// where each primitive has its own geometry data and
        /// material. Each Unity mesh that we create during a glTF
        /// import corresponds to a single glTF mesh primitive
        /// (not to an entire glTF mesh!).
        /// 
        /// Moreover, when we are importing the glTF scene hierarchy
        /// into Unity, we must create a separate game object
        /// for each mesh primitive, since Unity allows only one
        /// mesh/material combo per game object. The
        /// game objects for mesh primitives belonging to the
        /// same glTF mesh are created as siblings in the Unity
        /// scene hierarchy. Only the game object
        /// for the first mesh primitive (i.e. primitive 0)
        /// is added to the `Nodes` dictionary, whereas the
        /// full list of sibling game objects (including primitive 0)
        /// is recorded in `NodeToMeshPrimitives`.
        /// </summary>
        public Dictionary<int, List<GameObject>> NodeToMeshPrimitives;

        /// <summary>
        /// Indices of meshes that have one or more mesh primitives
        /// containing morph targets.
        /// </summary>
        public List<int> MeshesWithMorphTargets;

        /// <summary>
        /// Maps skin index -> node indices
        /// </summary>
        public Dictionary<int, List<int>> SkinToNodes;
    
        public GltfImportCache()
        {
            DefaultMaterial = new UnityEngine.Material(
                Shader.Find("Piglet/MetallicRoughnessOpaque"));
        
            Buffers = new List<byte[]>();
            Nodes = new Dictionary<int, GameObject>();
            Scene = null;
        
            MeshToNodes = new Dictionary<int, List<int>>();
            NodeToMeshPrimitives = new Dictionary<int, List<GameObject>>();
            MeshesWithMorphTargets = new List<int>();
            SkinToNodes = new Dictionary<int, List<int>>();
        }

        /// <summary>
        /// Remove a game object from the Unity scene and from memory.
        /// </summary>
        virtual protected void Destroy(GameObject gameObject)
        {
            Object.Destroy(gameObject);
        }
    
        /// <summary>
        /// Destroy all game objects created by the glTF import.
        /// </summary>
        public void Clear()
        {
            foreach (var gameObject in Nodes.Values)
                Destroy(gameObject);

            foreach (var gameObjects in NodeToMeshPrimitives.Values)
            {
                for (int i = 0; i < gameObjects.Count; ++i)
                {
                    // Skip destroying the game object for mesh primitive 0,
                    // since that game object also belongs to Nodes
                    // and has already been destroyed in the loop above.
                    // See comment for NodeToMeshPrimitives for
                    // further info.
                    if (i == 0)
                        continue;
                
                    Destroy(gameObjects[i]);
                }
            }
        
            if (Scene != null)
                Destroy(Scene);
        
            // tell Unity to unload any game objects that are not referenced
            // in the scene (e.g. the game objects we destroyed above)
            Resources.UnloadUnusedAssets();
        }
    }
}