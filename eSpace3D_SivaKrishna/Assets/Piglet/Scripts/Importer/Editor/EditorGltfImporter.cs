#if UNITY_EDITOR
using Material = UnityEngine.Material;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GLTF;
using GLTF.Schema;
using UnityEngine;
using UnityGLTF.Extensions;
using UnityEditor;

namespace Piglet
{
	/// <summary>
	/// A glTF importer for use inside the Unity Editor.
	/// EditorGltfImporter differs from RuntimeGltfImporter
	/// in the following ways: (1) EditorGltfImporter serializes
	/// the imported assets (e.g. textures, materials, meshes)
	/// to disk as Unity assets during import, whereas
	/// RuntimeGltfImporter only creates assets in memory.
	/// (2) EditorGltfImporter creates a prefab as its
	/// final output, whereas RuntimeGltfImporter creates
	/// an ordinary hierarchy of GameObjects (and returns the
	/// root).
	/// </summary>
	public class EditorGltfImporter : GltfImporter
	{
		// Import paths and options
		/// <summary>
		/// Parent directory of directory where importer will
		/// create Unity prefab and associated files
		/// (e.g. meshes, materials). Must be located inside Unity
		/// project folder.
		/// </summary>
		private string _importPath;

		/// <summary>
		/// Constructor
		/// </summary>
		public EditorGltfImporter(string gltfPath, string importPath,
			ProgressCallback progressCallback = null)
			: base(new Uri(gltfPath), null,
				new EditorGltfImportCache(UnityPathUtil.GetProjectPath(importPath)),
				progressCallback)
		{
			_importPath = importPath;
		}

		/// <summary>
		/// Coroutine-style implementation of GLTF import.
		/// </summary>
		/// <param name="gltfPath">Absolute path to .gltf/.glb file</param>
		/// <param name="importPath">Absolute path of folder where prefab and
		/// associated assets will be created. Must be located under
		/// the "Assets" folder for the current Unity project.
		/// created</param>
		/// <param name="progressCallback">Callback for reporting intermediate
		/// progress during the import.</param>
		public static GltfImportTask GetImportTask(string gltfPath,
			string importPath)
		{
			GltfImportTask importTask = new GltfImportTask();

			EditorGltfImporter importer = new EditorGltfImporter(
				gltfPath, importPath,
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
			importTask.AddTask(importer.CreatePrefabEnum());

			// callbacks to clean up any imported game objects / files
			// when the user aborts the import or an exception
			// occurs
			importTask.OnAborted += importer.Clear;
			importTask.OnException += _ => importer.Clear();

			return importTask;
		}

		/// <summary>
		/// Assign a texture to a material.
		///
		/// Note: The base class implementation inverts the texture using
		/// `SetTextureScale`, because Unity's `Texture.LoadImage`
		/// imports .png/.jpg images upside down.  However, during an
		/// Editor import we flip image with `TextureUtil.FlipTexture`
		/// instead, so that the texture asset is written out to disk
		/// right side up. Hence we need to override (i.e. remove)
		/// the use of `Material.SetTextureScale` in our version
		/// of `SetMaterialTexture`.
		/// </summary>
		override protected void SetMaterialTexture(Material material,
			string shaderProperty, Texture2D texture)
		{
			material.SetTexture(shaderProperty, texture);
		}

		/// <summary>
		/// Create a Unity Texture2D from in-memory PNG/JPG data.
		/// Since blocking the main Unity thread is not a concern
		/// during Editor imports, this method overrides the base class
		/// method to use Texture2D.LoadImage instead of UnityWebRequestTexture,
		/// in order to decrease overall import time.
		/// </summary>
		override protected IEnumerable<Texture2D> LoadTexture(byte[] data)
		{
			Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, true, false);
			texture.LoadImage(data);
			yield return texture;
		}

		override protected UnityEngine.Material LoadMaterial(
			GLTF.Schema.Material def, int materialIndex)
		{
			// Note: In the editor, a texture must be imported with "Texture Type"
			// set to "Normal map" before it can be assigned as the normal map
			// of a material. (I don't know why!)
			//
			// The material import will still work without the fix below, but
			// Unity will show a warning dialog and prompt the user to change
			// the texture type to "Normal map".

			if (def.NormalTexture != null) {
				Texture2D texture = _imported.Textures[def.NormalTexture.Index.Id];
				TextureImporter importer = AssetImporter.GetAtPath(
					AssetDatabase.GetAssetPath(texture)) as TextureImporter;
				importer.textureType = TextureImporterType.NormalMap;
				importer.SaveAndReimport();
			}

			return base.LoadMaterial(def, materialIndex);
		}

		/// <summary>
		/// Create a prefab from the imported hierarchy of game objects.
		/// This is the final output of an Editor glTF import.
		/// </summary>
		protected IEnumerator<GameObject> CreatePrefabEnum()
		{
			string basename = "scene.prefab";
			if (!String.IsNullOrEmpty(_imported.Scene.name))
			{
				basename = String.Format("{0}.prefab",
					GLTFUtils.cleanName(_imported.Scene.name));
			}

			string dir = UnityPathUtil.GetProjectPath(_importPath);
			string path = Path.Combine(dir, basename);

			GameObject prefab =
				PrefabUtility.SaveAsPrefabAsset(_imported.Scene, path);

			// Note: base.Clear() removes imported game objects from
			// the scene and from memory, but does not remove imported
			// asset files from disk.
			base.Clear();

			yield return prefab;
		}

		/// <summary>
		/// Remove any imported game objects from scene and from memory,
		/// and remove any asset files that were generated.
		/// </summary>
		protected override void Clear()
		{
			// remove imported game objects from scene and from memory
			base.Clear();

			// remove Unity asset files that were created during import
			UnityPathUtil.RemoveProjectDir(_importPath);
		}
	}
}
#endif
