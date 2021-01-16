// Source Code Attribution/License:
//
// The GltfImporter class in this file is a (heavily) modified version of
// the `UnityGLTF.GLTFEditorImporter` class from Sketchfab's UnityGLTF project,
// published at https://github.com/sketchfab/UnityGLTF with an MIT license.
// The exact version of `UnityGLTF.GLTFEditorImporter` used as the basis
// for this file comes from git commit c54fd454859c9ef8e1244c8d08c3f90089768702
// of https://github.com/sketchfab/UnityGLTF ("Merge pull request #12 from
// sketchfab/feature/updates-repo-url_D3D-4855").
//
// Please refer to the Assets/Piglet/Dependencies/UnityGLTF directory of this
// project for the Sketchfab/UnityGLTF LICENSE file and all other source
// files originating from the Sketchfab/UnityGLTF project.

using GLTF;
using GLTF.Schema;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Text.RegularExpressions;
using ICSharpCode.SharpZipLib.Zip;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Networking;
using UnityGLTF.Extensions;

namespace Piglet
{
	public class GltfImporter
	{
		/// <summary>
		/// URI (local file or remote URL) of the input .gltf/.glb/.zip file.
		/// </summary>
		protected Uri _uri;
		/// <summary>
		/// Raw byte content of the input .gltf/.glb/.zip file.
		/// </summary>
		protected byte[] _data;
		/// <summary>
		/// C# object hierarchy that mirrors JSON of input .gltf/.glb file.
		/// </summary>
		protected GLTFRoot _root;
		/// <summary>
		/// Caches data (e.g. buffers) and Unity assets (e.g. Texture2D)
		/// that are created during a glTF import.
		/// </summary>
		protected GltfImportCache _imported;

		/// <summary>
		/// Prototype for callback(s) that are invoked to report
		/// intermediate progress during a glTF import.
		/// </summary>
		/// <param name="step">
		/// The current step of the glTF import process.  Each step imports
		/// a different type of glTF entity (e.g. textures, materials).
		/// </param>
		/// <param name="completed">
		/// The number of glTF entities (e.g. textures, materials) that have been
		/// successfully imported for the current import step.
		/// </param>
		/// <param name="total">
		/// The total number of glTF entities (e.g. textures, materials) that will
		/// be imported for the current import step.
		/// </param>
		public delegate void ProgressCallback(ImportStep step, int completed, int total);

		/// <summary>
		/// Callback(s) that invoked to report intermediate progress
		/// during a glTF import.
		/// </summary>
		protected ProgressCallback _progressCallback;

		/// <summary>
		/// Constructor
		/// </summary>
		public GltfImporter(Uri uri, byte[] data,
			GltfImportCache imported,
			ProgressCallback progressCallback)
		{
			_uri = uri;
			_data = data;
			_imported = imported;
			_progressCallback = progressCallback;
		}

		/// <summary>
		/// Clear all game objects created by the glTF import from
		/// the Unity scene and from memory.
		/// </summary>
		protected virtual void Clear()
		{
			_imported?.Clear();
		}

		/// <summary>
		/// Read/download the byte content from the input glTF URI (`_uri`)
		/// into `_data`. The input URI may be a local or remote file
		/// (e.g. HTTP URL).
		/// </summary>
		protected IEnumerator ReadUri()
		{
			// Skip download step if input .gltf/.glb/.zip was passed
			// in as raw byte array (i.e. _data != null)

			if (_data != null)
				yield break;

			ImportStep importStep = UriUtil.IsLocalUri(_uri)
				? ImportStep.Read : ImportStep.Download;

			void onProgress(ulong bytesRead, ulong size)
			{
				_progressCallback?.Invoke(importStep, (int)bytesRead, (int)size);
			}

			foreach (var data in UriUtil.ReadAllBytesEnum(_uri, onProgress))
			{
				_data = data;
				yield return null;
			}
		}

		/// <summary>
		/// Return the byte content of the .gltf/.glb file that is
		/// currently being imported.
		/// </summary>
		protected IEnumerable<byte[]> GetGltfBytes()
		{
			if (!ZipUtil.IsZipData(_data))
			{
				yield return _data;
				yield break;
			}

			Regex regex = new Regex("\\.(gltf|glb)$");
			byte[] data = null;

			foreach (var result in ZipUtil.GetEntryBytes(_data, regex))
			{
				data = result;
				yield return null;
			}

			if (data == null)
				throw new Exception("No .gltf/.glb file found in zip archive.");

			yield return data;
		}

		/// <summary>
		/// Parse the JSON content of the input .gltf/.glb file and
		/// create an equivalent hierarchy of C# objects (`_root`).
		/// </summary>
		/// <returns></returns>
		protected IEnumerator ParseFile()
		{
			_progressCallback?.Invoke(ImportStep.Parse, 0, 1);

			byte[] gltf = null;
			foreach (var result in GetGltfBytes())
			{
				gltf = result;
				yield return null;
			}

			FixSpecularGlossinessDefaults();

            // Wrap Json.NET exceptions with our own
            // JsonParseException class, so that applications
            // that use Piglet do not need to compile
            // against the Json.NET DLL.

			try
			{
				_root = GLTFParser.ParseJson(gltf);
			}
			catch (Exception e)
			{
				throw new JsonParseException(
					"Error parsing JSON in glTF file", e);
			}

			_progressCallback?.Invoke(ImportStep.Parse, 1, 1);
		}

		/// <summary>
		/// Use C# reflection to fix incorrect default values for
		/// specular/glossiness values (GLTFSerialization bug).
		///
		/// For the correct default values and other
		/// details about the specular/glossiness glTF extension,
		/// see https://github.com/KhronosGroup/glTF/tree/master/extensions/2.0/Khronos/KHR_materials_pbrSpecularGlossiness
		/// </summary>
		protected void FixSpecularGlossinessDefaults()
		{
			var assembly = Assembly.GetAssembly(typeof(KHR_materials_pbrSpecularGlossinessExtension));
			var specularGlossiness = assembly.GetType("GLTF.Schema.KHR_materials_pbrSpecularGlossinessExtension");

			var specularFactor = specularGlossiness.GetField("SPEC_FACTOR_DEFAULT",
				BindingFlags.Static | BindingFlags.Public);
			specularFactor.SetValue(null, GLTF.Math.Vector3.One);

			var glossinessFactor = specularGlossiness.GetField("GLOSS_FACTOR_DEFAULT",
				BindingFlags.Static | BindingFlags.Public);
			glossinessFactor.SetValue(null, 1.0f);
		}

		protected IEnumerator LoadBuffers()
		{
			if (_root.Buffers == null || _root.Buffers.Count == 0)
				yield break;

			_progressCallback?.Invoke(ImportStep.Buffer, 0, _root.Buffers.Count);
			for (int i = 0; i < _root.Buffers.Count; ++i)
			{
				GLTF.Schema.Buffer buffer = _root.Buffers[i];

				byte[] data = null;
				foreach (var result in LoadBuffer(buffer, i))
				{
					data = result;
					yield return null;
				}

				_imported.Buffers.Add(data);
				_progressCallback?.Invoke(ImportStep.Buffer, (i + 1), _root.Buffers.Count);

				yield return null;
			}
		}

		/// <summary>
		/// Resolve a relative URI (e.g. path to a PNG file)
		/// by appending it to the URI of the directory containing
		/// the .gltf/.glb file. If the given URI is already an
		/// absolute URI (e.g. an HTTP URL), return the URI unchanged.
		/// If input for the glTF import is a zip archive, append the
		/// URI to the directory path containing the .gltf/.glb file
		/// inside the zip archive.
		/// </summary>
		protected IEnumerable<string> ResolveUri(string uriStr)
		{
			// If the given URI is absolute, we don't need to resolve it.

			Uri uri = new Uri(uriStr, UriKind.RelativeOrAbsolute);
			if (uri.IsAbsoluteUri)
			{
				yield return uriStr;
				yield break;
			}

			// If we are importing from a .zip file, append the given URI
			// to directory path containing the .gltf/.glb file
			// inside the zip.

			if (ZipUtil.IsZipData(_data))
			{
				Regex regex = new Regex("\\.(gltf|glb)$");
				ZipEntry entry = null;
				foreach (var value in ZipUtil.GetEntry(_data, regex))
				{
					entry = value;
					yield return null;
				}

				if (entry == null)
					throw new Exception("error: no .gltf/.glb file found in .zip");

				// Note: The C# Uri class cannot combine two relative
				// URIs, so we must do the work ourselves.

				string resolvedUriStr = entry.Name;

				// If the base URI for the input .gltf/.glb file does not
				// contain a slash, it means that file is located at the root of
				// the .zip archive, and therefore input URI (`uriStr`)
				// does not need to be modified.

				int lastSlashIndex = resolvedUriStr.LastIndexOf('/');
				if (lastSlashIndex < 0)
				{
					yield return uriStr;
					yield break;
				}

				resolvedUriStr = resolvedUriStr.Remove(lastSlashIndex);
				resolvedUriStr += "/";
				resolvedUriStr += uriStr;

				yield return resolvedUriStr;
				yield break;
			}

			if (Application.platform == RuntimePlatform.WebGLPlayer
				&& (_uri == null || !_uri.IsAbsoluteUri))
			{
				throw new UriResolutionException(
					string.Format("Sorry, the Piglet WebGL demo can't load {0} " +
					"because it contains references to other files on " +
					"the local filesystem (e.g. PNG files for textures). " +
					"In general, web browsers are not allowed to read files " +
					"from arbitrary paths on the local filesystem (for " +
					"security reasons).\n" +
					"\n" +
					"Please try a .glb or .zip file instead, as these are " +
					"generally self-contained.",
					_uri != null ? string.Format("\"{0}\"", _uri) : "your glTF file"));
			}

			if (Application.platform == RuntimePlatform.Android
			    && _uri.Scheme == "content")
			{
				throw new UriResolutionException(
					String.Format("Sorry, Piglet can't load \"{0}\" on Android " +
					  "because it contains references to other files (e.g. PNG " +
					  "files for textures) that it isn't allowed to read, for " +
					  "security reasons.\n" +
					  "\n" +
					  "Please try a .glb file instead, as these are " +
					  "generally self-contained.",
						_uri.Segments[_uri.Segments.Length - 1]));
			}

			// Combine the given URI with
			// the URI for the input .gltf/.glb file.
			//
			// Given the other cases handled above, at
			// this point in the code we are certain that:
			//
			// 1. the input file is a .gltf/.glb (not a .zip)
			// 2. the URI for the input .gltf/.glb (`_uri`) is an absolute URI
			// 3. the URI passed to this method (`uriStr`) is a relative URI
			//
			// Note 1: The Uri constructor below
			// will strip the filename segment (if present)
			// from the first Uri before combining
			// it with the second Uri.
			//
			// Note 2: The constructor will throw
			// an exception unless the first Uri is
			// absolute and the second Uri is relative,
			// which is why I can't use the same approach
			// for .zip file paths above.

			var resolvedUri = new Uri(_uri, uriStr);
			yield return resolvedUri.ToString();
		}

		/// <summary>
		/// Extract a glTF buffer that is embedded in the input .glb file.
		/// </summary>
		protected IEnumerable<byte[]> ExtractBinaryChunk(int bufferIndex)
		{
			byte[] gltf = null;
			foreach (var result in GetGltfBytes())
			{
				gltf = result;
				yield return null;
			}

			GLTFParser.ExtractBinaryChunk(gltf, bufferIndex, out var chunk);
			yield return chunk;
		}

		/// <summary>
		/// Get the byte content of a glTF buffer.
		/// </summary>
		protected IEnumerable<byte[]> LoadBuffer(GLTF.Schema.Buffer buffer, int bufferIndex)
		{
			byte[] data = null;

			// case 1: no URI -> load buffer from .glb segment

			if (buffer.Uri == null)
			{
				foreach (var result in ExtractBinaryChunk(bufferIndex))
				{
					data = result;
					yield return null;
				}

				yield return data;
				yield break;
			}

			// case 2: data URI -> decode data from base64

			if (UriUtil.TryParseDataUri(buffer.Uri, out data))
			{
				yield return data;
				yield break;
			}

			// resolve buffer URI relative to URI
			// for input .gltf/.glb file

			string uri = null;
			foreach (var result in ResolveUri(buffer.Uri))
			{
				uri = result;
				yield return null;
			}

			// case 3: extract buffer file from .zip

			if (ZipUtil.IsZipData(_data))
			{
				foreach (var result in ZipUtil.GetEntryBytes(_data, uri))
				{
					data = result;
					yield return null;
				}

				yield return data;
				yield break;
			}

			// case 4: read/download buffer from URI

			foreach (var result in UriUtil.ReadAllBytesEnum(uri))
				yield return result;
		}

		/// <summary>
		/// Create a texture from in-memory image data (PNG/JPG),
		/// asynchronously.
		/// </summary>
		virtual protected IEnumerable<Texture2D> LoadTexture(byte[] data)
		{
			// Some notes about implementation:
			//
			// I do not use Texture2D.LoadImage() here
			// because it is synchronous and can block the main
			// Unity thread for an unacceptably long time
			// (e.g. 200 ms for a large texture).
			//
			// Instead, I use UnityWebRequestTexture to load
			// the texture asynchronously on a background thread.
			// This works well, but in order to pass the data
			// to UnityWebRequestTexture, we must somehow create
			// a URI for the data. On Windows and Android, I do
			// this by writing the data to a temporary file under
			// Application.temporaryCachePath and using the
			// file path as the URI.  On WebGL, we create a
			// temporary localhost URL for the data
			// on the Javascript side, using `URL.createObjectURL`.

			string uri = null;
			foreach (var result in UriUtil.CreateUri(data))
			{
				uri = result;
				yield return null;
			}

			foreach (var result in UriUtil.ReadTextureEnum(uri))
				yield return result;
		}

		/// <summary>
		/// Create a Unity Texture2D from a glTF image.
		/// </summary>
		protected IEnumerable<Texture2D> LoadImage(Image image, int imageID)
		{
			Texture2D texture = null;
			byte[] data = null;

			// case 1: no URI -> load image data from glTF buffer view

			if (image.Uri == null)
			{
				var bufferView = image.BufferView.Value;
				data = new byte[bufferView.ByteLength];

				var bufferContents = _imported.Buffers[bufferView.Buffer.Id];
				System.Buffer.BlockCopy(bufferContents,
					bufferView.ByteOffset, data, 0, data.Length);

				foreach (var result in LoadTexture(data))
				{
					texture = result;
					yield return null;
				}

				yield return texture;
				yield break;
			}

			// case 2: data URI -> decode data from base64 string

			if (UriUtil.TryParseDataUri(image.Uri, out data))
			{
				foreach (var result in LoadTexture(data))
				{
					texture = result;
					yield return null;
				}

				yield return texture;
				yield break;
			}

			// resolve image URI relative to URI
			// for input .gltf/.glb file

			string uri = null;
			foreach (var result in ResolveUri(image.Uri))
			{
				uri = result;
				yield return null;
			}

			// case 3: extract image bytes from input .zip

			if (ZipUtil.IsZipData(_data))
			{
				foreach (var result in ZipUtil.GetEntryBytes(_data, uri))
				{
					data = result;
					yield return null;
				}

				foreach (var result in LoadTexture(data))
				{
					texture = result;
					yield return null;
				}

				yield return texture;
				yield break;
			}

			// case 4: load texture from an absolute URI
			// (file path or URL)

			foreach (var result in UriUtil.ReadTextureEnum(uri))
				yield return result;
		}

		/// <summary>
		/// Create Unity Texture2D objects from glTF texture descriptions.
		/// </summary>
		protected IEnumerator LoadTextures()
		{
			if (_root.Textures == null || _root.Textures.Count == 0)
				yield break;

			_progressCallback?.Invoke(ImportStep.Texture, 0, _root.Textures.Count);

			// Interleave texture loading tasks, since loading
			// large textures can be slow.

			var tasks = new InterleavedTaskSet<Texture2D>();
			for (var i = 0; i < _root.Textures.Count; ++i)
			{
				// placeholder until Texture2D is loaded
				_imported.Textures.Add(null);
				tasks.Add(LoadTexture(_root.Textures[i], i).GetEnumerator());
			}

			tasks.OnCompleted =
				(textureIndex, texture) =>
				{
					_imported.Textures[textureIndex] = texture;
					_progressCallback?.Invoke(ImportStep.Texture, tasks.NumCompleted, _root.Textures.Count);
				};

			// Pump tasks until all are complete.

			while (tasks.MoveNext())
				yield return null;
		}

		protected IEnumerable<Texture2D> LoadTexture(
			GLTF.Schema.Texture def, int textureIndex)
		{
			var imageId = def.Source.Id;
			var image = _root.Images[imageId];

			Texture2D texture = null;
			foreach (var result in LoadImage(image, imageId))
			{
				texture = result;
				yield return null;
			}

			// Default values
			var desiredFilterMode = FilterMode.Bilinear;
			var desiredWrapMode = UnityEngine.TextureWrapMode.Repeat;

			if (def.Sampler != null)
			{
				var sampler = def.Sampler.Value;
				switch (sampler.MinFilter)
				{
					case MinFilterMode.Nearest:
						desiredFilterMode = FilterMode.Point;
						break;
					case MinFilterMode.Linear:
					default:
						desiredFilterMode = FilterMode.Bilinear;
						break;
				}

				switch (sampler.WrapS)
				{
					case GLTF.Schema.WrapMode.ClampToEdge:
						desiredWrapMode = UnityEngine.TextureWrapMode.Clamp;
						break;
					case GLTF.Schema.WrapMode.Repeat:
					default:
						desiredWrapMode = UnityEngine.TextureWrapMode.Repeat;
						break;
				}
			}

			texture.filterMode = desiredFilterMode;
			texture.wrapMode = desiredWrapMode;

			yield return texture;
		}

		public GameObject GetSceneObject()
		{
			return _imported.Scene;
		}

		public IEnumerator<GameObject> GetSceneObjectEnum()
		{
			yield return GetSceneObject();
		}

		protected IEnumerator LoadMaterials()
		{
			if (_root.Materials == null || _root.Materials.Count == 0)
				yield break;

			_progressCallback?.Invoke(ImportStep.Material, 0, _root.Materials.Count);
			for(int i = 0; i < _root.Materials.Count; ++i)
			{
				UnityEngine.Material material = LoadMaterial(_root.Materials[i], i);
				_imported.Materials.Add(material);
				_progressCallback?.Invoke(ImportStep.Material, (i + 1), _root.Materials.Count);
				yield return null;
			}
		}

		protected KHR_materials_pbrSpecularGlossinessExtension
			GetSpecularGlossinessExtension(GLTF.Schema.Material def)
		{
			Extension extension;
			if (def.Extensions.TryGetValue(
				"KHR_materials_pbrSpecularGlossiness", out extension))
			{
				return (KHR_materials_pbrSpecularGlossinessExtension)extension;
			}
			return null;
		}

		/// <summary>
		/// Assign a texture to a material.
		/// </summary>
		virtual protected void SetMaterialTexture(UnityEngine.Material material,
			string shaderProperty, Texture2D texture)
		{
			material.SetTexture(shaderProperty, texture);

			// `Texture2D.LoadImage` and `UnityWebRequestTexture`
			// import .png/.jpg images upside down, so we use `SetTextureScale`
			// to flip them right-side-up again.

			material.SetTextureScale(shaderProperty, new Vector2(1, -1));
		}

		virtual protected UnityEngine.Material LoadMaterial(
			GLTF.Schema.Material def, int materialIndex)
		{
			KHR_materials_pbrSpecularGlossinessExtension sg
				= GetSpecularGlossinessExtension(def);

            Shader shader;
			if (sg != null) {
				switch(def.AlphaMode)
				{
				default:
				case AlphaMode.OPAQUE:
					shader = Shader.Find("Piglet/SpecularGlossinessOpaque");
					break;
				case AlphaMode.MASK:
					shader = Shader.Find("Piglet/SpecularGlossinessMask");
					break;
				case AlphaMode.BLEND:
					shader = Shader.Find("Piglet/SpecularGlossinessBlend");
					break;
				}
			} else {
				switch(def.AlphaMode)
				{
				default:
				case AlphaMode.OPAQUE:
					shader = Shader.Find("Piglet/MetallicRoughnessOpaque");
					break;
				case AlphaMode.MASK:
					shader = Shader.Find("Piglet/MetallicRoughnessMask");
					break;
				case AlphaMode.BLEND:
					shader = Shader.Find("Piglet/MetallicRoughnessBlend");
					break;
				}
			}

			var material = new UnityEngine.Material(shader);
			material.name = def.Name;

			// disable automatic deletion of unused material
			material.hideFlags = HideFlags.DontUnloadUnusedAsset;

			if (def.AlphaMode == AlphaMode.MASK)
				material.SetFloat("_alphaCutoff", (float)def.AlphaCutoff);

			if (def.NormalTexture != null)
			{
				SetMaterialTexture(material, "_normalTexture",
					_imported.Textures[def.NormalTexture.Index.Id]);
			}

			if (def.OcclusionTexture != null)
			{
				SetMaterialTexture(material, "_occlusionTexture",
					_imported.Textures[def.OcclusionTexture.Index.Id]);
			}

			material.SetColor("_emissiveFactor",
				def.EmissiveFactor.ToUnityColor());

			if (def.EmissiveTexture != null)
			{
				SetMaterialTexture(material, "_emissiveTexture",
					_imported.Textures[def.EmissiveTexture.Index.Id]);
			}

			var mr = def.PbrMetallicRoughness;
			if (mr != null)
			{
				material.SetColor("_baseColorFactor",
					mr.BaseColorFactor.ToUnityColor());

				if (mr.BaseColorTexture != null)
				{
					SetMaterialTexture(material, "_baseColorTexture",
						_imported.Textures[mr.BaseColorTexture.Index.Id]);
				}

				material.SetFloat("_metallicFactor",
					(float)mr.MetallicFactor);
				material.SetFloat("_roughnessFactor",
					(float)mr.RoughnessFactor);

				if (mr.MetallicRoughnessTexture != null)
				{
					SetMaterialTexture(material, "_metallicRoughnessTexture",
						_imported.Textures[mr.MetallicRoughnessTexture.Index.Id]);
				}
			}

			if (sg != null)
			{
				material.SetColor("_diffuseFactor",
					sg.DiffuseFactor.ToUnityColor());

				if (sg.DiffuseTexture != null)
				{
					SetMaterialTexture(material, "_diffuseTexture",
						_imported.Textures[sg.DiffuseTexture.Index.Id]);
				}

				Vector3 spec3 = sg.SpecularFactor.ToUnityVector3();
				material.SetColor("_specularFactor",
					new Color(spec3.x, spec3.y, spec3.z, 1f));

				material.SetFloat("_glossinessFactor",
					(float)sg.GlossinessFactor);

				if (sg.SpecularGlossinessTexture != null)
				{
					SetMaterialTexture(material, "_specularGlossinessTexture",
						_imported.Textures[sg.SpecularGlossinessTexture.Index.Id]);
				}

			}

			material.hideFlags = HideFlags.None;

			return material;
		}

		protected IEnumerator LoadMeshes()
		{
			if (_root.Meshes == null || _root.Meshes.Count == 0)
				yield break;

			_progressCallback?.Invoke(ImportStep.Mesh, 0, _root.Meshes.Count);
			for(int i = 0; i < _root.Meshes.Count; ++i)
			{
				CreateMeshObject(_root.Meshes[i], i);
				_progressCallback?.Invoke(ImportStep.Mesh, (i + 1), _root.Meshes.Count);
				yield return null;
			}
		}

		protected virtual void CreateMeshObject(GLTF.Schema.Mesh meshDef, int meshId)
		{
			// Note: In glTF, a mesh is a list of mesh primitives, each of which
			// has its own geometry and material.

			var mesh = new List<KeyValuePair<UnityEngine.Mesh, UnityEngine.Material>>();

			// true if one or more mesh primitives have morph targets
			bool hasMorphTargets = false;

			for (int i = 0; i < meshDef.Primitives.Count; ++i)
			{
				hasMorphTargets |= HasMorphTargets(meshId, i);

				var primitive = meshDef.Primitives[i];

				UnityEngine.Mesh meshPrimitive
					= CreateMeshPrimitive(primitive, meshDef.Name, meshId, i); // Converted to mesh

				UnityEngine.Material material = primitive.Material != null && primitive.Material.Id >= 0
					? _imported.Materials[primitive.Material.Id] : _imported.DefaultMaterial;

				mesh.Add(new KeyValuePair<UnityEngine.Mesh, UnityEngine.Material>(
					meshPrimitive, material));
			}

			_imported.Meshes.Add(mesh);

			// track which meshes have morph target data, so that we
			// can load them in a later step
			if (hasMorphTargets)
				_imported.MeshesWithMorphTargets.Add(meshId);
		}

		virtual protected UnityEngine.Mesh
		CreateMeshPrimitive(MeshPrimitive primitive, string meshName, int meshID, int primitiveIndex)
		{
			var meshAttributes = BuildMeshAttributes(primitive, meshID, primitiveIndex);
			var vertexCount = primitive.Attributes[SemanticProperties.POSITION].Value.Count;

			UnityEngine.Mesh mesh = new UnityEngine.Mesh
			{
				vertices = primitive.Attributes.ContainsKey(SemanticProperties.POSITION)
					? meshAttributes[SemanticProperties.POSITION].AccessorContent.AsVertices.ToUnityVector3()
					: null,
				normals = primitive.Attributes.ContainsKey(SemanticProperties.NORMAL)
					? meshAttributes[SemanticProperties.NORMAL].AccessorContent.AsNormals.ToUnityVector3()
					: null,

				uv = primitive.Attributes.ContainsKey(SemanticProperties.TexCoord(0))
					? meshAttributes[SemanticProperties.TexCoord(0)].AccessorContent.AsTexcoords.ToUnityVector2()
					: null,

				uv2 = primitive.Attributes.ContainsKey(SemanticProperties.TexCoord(1))
					? meshAttributes[SemanticProperties.TexCoord(1)].AccessorContent.AsTexcoords.ToUnityVector2()
					: null,

				uv3 = primitive.Attributes.ContainsKey(SemanticProperties.TexCoord(2))
					? meshAttributes[SemanticProperties.TexCoord(2)].AccessorContent.AsTexcoords.ToUnityVector2()
					: null,

				uv4 = primitive.Attributes.ContainsKey(SemanticProperties.TexCoord(3))
					? meshAttributes[SemanticProperties.TexCoord(3)].AccessorContent.AsTexcoords.ToUnityVector2()
					: null,

				colors = primitive.Attributes.ContainsKey(SemanticProperties.Color(0))
					? meshAttributes[SemanticProperties.Color(0)].AccessorContent.AsColors.ToUnityColor()
					: null,

				triangles = primitive.Indices != null
					? meshAttributes[SemanticProperties.INDICES].AccessorContent.AsTriangles
					: MeshPrimitive.GenerateTriangles(vertexCount),

				tangents = primitive.Attributes.ContainsKey(SemanticProperties.TANGENT)
					? meshAttributes[SemanticProperties.TANGENT].AccessorContent.AsTangents.ToUnityVector4(true)
					: null
			};

			mesh.RecalculateBounds();
			mesh.RecalculateTangents();

			return mesh;
		}

		protected virtual Dictionary<string, AttributeAccessor> BuildMeshAttributes(MeshPrimitive primitive, int meshID, int primitiveIndex)
		{
			Dictionary<string, AttributeAccessor> attributeAccessors = new Dictionary<string, AttributeAccessor>(primitive.Attributes.Count + 1);
			foreach (var attributePair in primitive.Attributes)
			{
				AttributeAccessor AttributeAccessor = new AttributeAccessor()
				{
					AccessorId = attributePair.Value,
					Buffer = _imported.Buffers[attributePair.Value.Value.BufferView.Value.Buffer.Id]
				};

				attributeAccessors[attributePair.Key] = AttributeAccessor;
			}

			if (primitive.Indices != null)
			{
				AttributeAccessor indexBuilder = new AttributeAccessor()
				{
					AccessorId = primitive.Indices,
					Buffer = _imported.Buffers[primitive.Indices.Value.BufferView.Value.Buffer.Id]
				};

				attributeAccessors[SemanticProperties.INDICES] = indexBuilder;
			}

			GLTFHelpers.BuildMeshAttributes(ref attributeAccessors);
			return attributeAccessors;
		}

		/// <summary>
		/// Return true if the given mesh primitive has morph target
		/// data (a.k.a. blend shapes).
		/// </summary>
		protected bool HasMorphTargets(int meshIndex, int primitiveIndex)
		{
			MeshPrimitive primitive
				= _root.Meshes[meshIndex].Primitives[primitiveIndex];

			return primitive.Targets != null
			       && primitive.Targets.Count > 0;
		}

		/// <summary>
		/// Assign glTF morph target data to a Unity mesh.
		///
		/// Note: In Unity, morph targets are usually referred to as "blend shapes".
		/// Interpolation between blend shapes is calculated/rendered by
		/// SkinnedMeshRenderer.
		/// </summary>
		protected void LoadMorphTargets(UnityEngine.Mesh mesh, int meshIndex, int primitiveIndex)
		{
			MeshPrimitive primitive
				= _root.Meshes[meshIndex].Primitives[primitiveIndex];

			if (!HasMorphTargets(meshIndex, primitiveIndex))
				return;

			for (int i = 0; i < primitive.Targets.Count; ++i)
			{
				var target = primitive.Targets[i];
				int numVertices = target["POSITION"].Value.Count;

				Vector3[] deltaVertices = new Vector3[numVertices];
				Vector3[] deltaNormals = new Vector3[numVertices];
				Vector3[] deltaTangents = new Vector3[numVertices];

				if(target.ContainsKey("POSITION"))
				{
					NumericArray num = new NumericArray();
					deltaVertices = target["POSITION"].Value
						.AsVector3Array(ref num, _imported.Buffers[0], false)
						.ToUnityVector3(true);
				}
				if (target.ContainsKey("NORMAL"))
				{
					NumericArray num = new NumericArray();
					deltaNormals = target["NORMAL"].Value
						.AsVector3Array(ref num, _imported.Buffers[0], true)
						.ToUnityVector3(true);
				}

				mesh.AddBlendShapeFrame(GLTFUtils.buildBlendShapeName(meshIndex, i),
					1.0f, deltaVertices, deltaNormals, deltaTangents);
			}
		}

		protected virtual void LoadSkinnedMeshAttributes(int meshIndex, int primitiveIndex, ref Vector4[] boneIndexes, ref Vector4[] weights)
		{
			GLTF.Schema.MeshPrimitive prim = _root.Meshes[meshIndex].Primitives[primitiveIndex];
			if (!prim.Attributes.ContainsKey(SemanticProperties.JOINT) || !prim.Attributes.ContainsKey(SemanticProperties.WEIGHT))
				return;

			parseAttribute(ref prim, SemanticProperties.JOINT, ref boneIndexes);
			parseAttribute(ref prim, SemanticProperties.WEIGHT, ref weights);
			foreach(Vector4 wei in weights)
			{
				wei.Normalize();
			}
		}

		private void parseAttribute(ref GLTF.Schema.MeshPrimitive prim, string property, ref Vector4[] values)
		{
			byte[] bufferData = _imported.Buffers[prim.Attributes[property].Value.BufferView.Value.Buffer.Id];
			NumericArray num = new NumericArray();
			GLTF.Math.Vector4[] gltfValues = prim.Attributes[property].Value.AsVector4Array(ref num, bufferData);
			values = new Vector4[gltfValues.Length];

			for (int i = 0; i < gltfValues.Length; ++i)
			{
				values[i] = gltfValues[i].ToUnityVector4();
			}
		}

		/// <summary>
		/// Set up mesh nodes in the scene hierarchy by
		/// attaching MeshFilter/MeshRenderer components
		/// and linking them to the appropriate
		/// meshes/materials.
		///
		/// If a glTF mesh has more than one primitive,
		/// we must create a seperate GameObject for each additional
		/// primitive with its own MeshFilter/MeshRenderer,
		/// which are added as siblings of the GameObject for
		/// mesh primitive 0. See documentation of
		/// GltfImportCache.NodeToMeshPrimitives for further discussion.
		/// </summary>
		protected void SetupMeshNodes()
		{
			foreach (var kvp in _imported.Nodes)
			{
				int nodeIndex = kvp.Key;
				GameObject gameObject = kvp.Value;
				Node node = _root.Nodes[nodeIndex];

				if (node.Mesh == null)
					continue;

				int meshIndex = node.Mesh.Id;

				List<KeyValuePair<UnityEngine.Mesh, UnityEngine.Material>>
					primitives = _imported.Meshes[meshIndex];

				List<GameObject> primitiveNodes = new List<GameObject>();
				for (int i = 0; i < primitives.Count; ++i)
				{
					GameObject primitiveNode;
					if (i == 0)
					{
						primitiveNode = gameObject;
					}
					else
					{
						primitiveNode = createGameObject(
							node.Name ?? "GLTFNode_" + nodeIndex);
						primitiveNode.transform.localPosition
							= gameObject.transform.localPosition;
						primitiveNode.transform.localRotation
							= gameObject.transform.localRotation;
						primitiveNode.transform.localScale
							= gameObject.transform.localScale;
						primitiveNode.transform.SetParent(
							gameObject.transform.parent, false);
					}

					MeshFilter meshFilter
						= primitiveNode.AddComponent<MeshFilter>();
					meshFilter.sharedMesh = primitives[i].Key;

					MeshRenderer meshRenderer
						= primitiveNode.AddComponent<MeshRenderer>();
					meshRenderer.material = primitives[i].Value;

					primitiveNodes.Add(primitiveNode);
				}

				_imported.NodeToMeshPrimitives.Add(
					nodeIndex, primitiveNodes);
			}
		}

		virtual protected GameObject createGameObject(string name)
		{
			return new GameObject(GLTFUtils.cleanName(name));
		}

		protected IEnumerator LoadScene()
		{
			Scene scene = _root.GetDefaultScene();
			if (scene == null)
				throw new Exception("No default scene in glTF file");

			string importName = "model";
			if (_uri != null)
				importName = Path.GetFileNameWithoutExtension(_uri.AbsolutePath);

			_imported.Scene = createGameObject(importName);

			foreach (var node in scene.Nodes)
			{
				var nodeObj = CreateNode(node.Value, node.Id);
				nodeObj.transform.SetParent(_imported.Scene.transform, false);
			}

			SetupMeshNodes();

			yield return null;
		}

		protected virtual GameObject CreateNode(Node node, int index)
		{
			var nodeObj = createGameObject(node.Name != null && node.Name.Length > 0 ? node.Name : "GLTFNode_" + index);

			Vector3 position;
			Quaternion rotation;
			Vector3 scale;
			node.GetUnityTRSProperties(out position, out rotation, out scale);
			nodeObj.transform.localPosition = position;
			nodeObj.transform.localRotation = rotation;
			nodeObj.transform.localScale = scale;

			// record mesh -> node mappings, for later use in loading morph target data
			if (node.Mesh != null)
			{
				if (!_imported.MeshToNodes.TryGetValue(node.Mesh.Id, out var nodes))
				{
					nodes = new List<int>();
					_imported.MeshToNodes.Add(node.Mesh.Id, nodes);
				}

				nodes.Add(index);
			}

			// record skin -> node mappings, for later use in loading skin data
			if (node.Skin != null)
			{
				if (!_imported.SkinToNodes.TryGetValue(node.Skin.Id, out var nodes))
				{
					nodes = new List<int>();
					_imported.SkinToNodes.Add(node.Skin.Id, nodes);
				}

				nodes.Add(index);
			}

			_imported.Nodes.Add(index, nodeObj);
			_progressCallback?.Invoke(ImportStep.Node, _imported.Nodes.Count, _root.Nodes.Count);

			if (node.Children != null)
			{
				foreach (var child in node.Children)
				{
					var childObj = CreateNode(child.Value, child.Id);
					childObj.transform.SetParent(nodeObj.transform, false);
				}
			}

			return nodeObj;
		}

		private bool isValidSkin(int skinIndex)
		{
			if (skinIndex >= _root.Skins.Count)
				return false;

			Skin glTFSkin = _root.Skins[skinIndex];

			return glTFSkin.Joints.Count > 0 && glTFSkin.Joints.Count == glTFSkin.InverseBindMatrices.Value.Count;
		}

		/// <summary>
		/// Load morph target data (a.k.a. blend shapes) for the given
		/// mesh primitive.
		/// </summary>
		protected void LoadMorphTargets(int meshIndex)
		{
			// load morph target data for each mesh primitive

			int numPrimitives = _imported.Meshes[meshIndex].Count;
			for (int i = 0; i < numPrimitives; ++i)
			{
				var primitive = _imported.Meshes[meshIndex][i];
				var mesh = primitive.Key;

				LoadMorphTargets(mesh, meshIndex, i);
			}

			// Add/configure SkinnedMeshRenderer on game objects
			// corresponding to mesh primitives.

			// if mesh isn't referenced by any nodes in the scene hierarchy
			if (!_imported.MeshToNodes.TryGetValue(meshIndex, out var nodeIndices))
				return;

			// for each scene node that has the mesh attached
			foreach (int nodeIndex in nodeIndices)
			{
				// for each game object corresponding to a mesh primitive
				var gameObjects = _imported.NodeToMeshPrimitives[nodeIndex];
				for (int i = 0; i < gameObjects.Count; ++i)
				{
					if (!HasMorphTargets(meshIndex, i))
						return;

					var gameObject = gameObjects[i];
					var primitive = _imported.Meshes[meshIndex][i];
					var mesh = primitive.Key;
					var material = primitive.Value;

					// By default, GameObjects for mesh primitives
					// get a MeshRenderer/MeshFilter attached to them
					// in SetupMeshNodes().
					//
					// However, for primitives with morph targets,
					// we need to replace these two components with
					// a SkinnedMeshRenderer.

					gameObject.RemoveComponent<MeshRenderer>();
					gameObject.RemoveComponent<MeshFilter>();

					SkinnedMeshRenderer renderer
						= gameObject.GetOrAddComponent<SkinnedMeshRenderer>();

					renderer.sharedMesh = mesh;
					renderer.sharedMaterial = material;
				}
			}

		}

		/// <summary>
		/// Load morph targets (a.k.a. blend shapes).
		/// </summary>
		protected IEnumerator LoadMorphTargets()
		{
			if (_imported.MeshesWithMorphTargets.Count == 0)
				yield break;

			_progressCallback?.Invoke(ImportStep.MorphTarget, 0,
				_imported.MeshesWithMorphTargets.Count);

			for (int i = 0; i < _imported.MeshesWithMorphTargets.Count; ++i)
			{
				int meshIndex = _imported.MeshesWithMorphTargets[i];
				LoadMorphTargets(meshIndex);

				_progressCallback?.Invoke(ImportStep.MorphTarget, i + 1,
					_imported.MeshesWithMorphTargets.Count);
				yield return null;
			}
		}

		/// <summary>
		/// Load skinning data for a single skin and apply it to
		/// the relevant meshes.
		/// </summary>
		/// <param name="skinIndex"></param>
		protected void LoadSkin(int skinIndex)
		{
			if (!isValidSkin(skinIndex))
			{
				Debug.LogErrorFormat(
					"Piglet: skipped loading skin {0}: skin data is empty/invalid",
					skinIndex);
				return;
			}

			// load skinning data

			Skin skin = _root.Skins[skinIndex];

			Matrix4x4[] bindposes = GetBindPoses(skin);
			Transform[] bones = GetBones(skin);

			Transform rootBone = null;
			if(skin.Skeleton != null)
				rootBone = _imported.Nodes[skin.Skeleton.Id].transform;

			// apply skinning data to each node/mesh that uses the skin

			foreach (var nodeIndex in _imported.SkinToNodes[skinIndex])
			{
				Node node = _root.Nodes[nodeIndex];
				if (node.Mesh == null)
					continue;

				// attach/configure a SkinnedMeshRenderer for each
				// mesh primitive
				for (int i = 0; i < _imported.Meshes[node.Mesh.Id].Count; ++i)
					SetupSkinnedMeshPrimitive(nodeIndex, i, bindposes,bones, rootBone);
			}
		}

		/// <summary>
		/// Load skinning data for meshes.
		/// </summary>
		protected IEnumerator LoadSkins()
		{
			if (_root.Skins == null || _root.Skins.Count == 0)
				yield break;

			_progressCallback?.Invoke(ImportStep.Skin, 0, _root.Skins.Count);

			for (int i = 0; i < _root.Skins.Count; ++i)
			{
				LoadSkin(i);

				_progressCallback?.Invoke(ImportStep.Skin, i + 1, _root.Skins.Count);
				yield return null;
			}
		}

		/// <summary>
		/// Add/configure a SkinnedMeshRenderer for a mesh primitive.
		/// </summary>
		/// <param name="nodeIndex">The glTF node index of the parent mesh instance</param>
		/// <param name="primitiveIndex">The mesh primitive index</param>
		/// <param name="bindposes">Matrices that hold inverse transforms for the bones</param>
		/// <param name="bones">Transforms of the bones</param>
		/// <param name="rootBone">Root bone for the skin (typically null)</param>
		protected void SetupSkinnedMeshPrimitive(int nodeIndex, int primitiveIndex,
			Matrix4x4[] bindposes, Transform[] bones, Transform rootBone)
		{
			int meshIndex = _root.Nodes[nodeIndex].Mesh.Id;
			var primitive = _imported.Meshes[meshIndex][primitiveIndex];
			UnityEngine.Mesh mesh = primitive.Key;
			UnityEngine.Material material = primitive.Value;

			// All GameObjects that represent a mesh primitive
			// get a MeshRenderer/MeshFilter attached to them
			// by default in SetupMeshNodes().
			//
			// For skinned meshes, we need to replace these
			// two components with a SkinnedMeshRenderer.
			// Since a SkinnedMeshRenderer is also used for
			// interpolating/rendering morph targets
			// (a.k.a. blend shapes), we may have already
			// replaced the MeshRenderer/MeshFilter
			// with a SkinnedMeshRenderer during the
			// morph target importing step.

			GameObject primitiveNode
				= _imported.NodeToMeshPrimitives[nodeIndex][primitiveIndex];

			primitiveNode.RemoveComponent<MeshRenderer>();
			primitiveNode.RemoveComponent<MeshFilter>();

			SkinnedMeshRenderer renderer
				= primitiveNode.GetOrAddComponent<SkinnedMeshRenderer>();

			renderer.sharedMesh = mesh;
			renderer.sharedMaterial = material;
			renderer.bones = bones;
			renderer.rootBone = rootBone;

			mesh.boneWeights = GetBoneWeights(meshIndex, primitiveIndex);
			mesh.bindposes = bindposes;
		}

		/// <summary>
		/// Get bindpose matrices for a skinned mesh, in Unity's native format.
		/// The bindpose matrices are inverse transforms of the bones
		/// in their default pose. In glTF, these matrices are provided
		/// by the 'inverseBindMatrices' property of a skin.
		///
		/// See https://docs.unity3d.com/ScriptReference/Mesh-bindposes.html
		/// for a minimal example of how to set up a skinned mesh in
		/// Unity including bone weights, bindposes, etc.
		/// </summary>
		protected Matrix4x4[] GetBindPoses(Skin skin)
		{
			byte[] bufferData = _imported.Buffers[
				skin.InverseBindMatrices.Value.BufferView.Value.Buffer.Id];

			NumericArray content = new NumericArray();
			GLTF.Math.Matrix4x4[] inverseBindMatrices
				= skin.InverseBindMatrices.Value.AsMatrixArray(ref content, bufferData);

			List<Matrix4x4> bindposes = new List<Matrix4x4>();
			foreach (GLTF.Math.Matrix4x4 mat in inverseBindMatrices)
				bindposes.Add(mat.ToUnityMatrix().switchHandedness());

			return bindposes.ToArray();
		}

		/// <summary>
		/// Get bone weights for a skinned mesh, in Unity's native format.
		///
		/// See https://docs.unity3d.com/ScriptReference/Mesh-bindposes.html
		/// for a minimal example of how to set up a skinned mesh in
		/// Unity including bone weights, bindposes, etc.
		/// </summary>
		protected BoneWeight[] GetBoneWeights(int meshIndex, int primitiveIndex)
		{
			MeshPrimitive primitive
				= _root.Meshes[meshIndex].Primitives[primitiveIndex];

			UnityEngine.Mesh mesh
				= _imported.Meshes[meshIndex][primitiveIndex].Key;

			if (!primitive.Attributes.ContainsKey(SemanticProperties.JOINT)
			    || !primitive.Attributes.ContainsKey(SemanticProperties.WEIGHT))
				return null;

			Vector4[] bones = new Vector4[1];
			Vector4[] weights = new Vector4[1];

			LoadSkinnedMeshAttributes(meshIndex, primitiveIndex, ref bones, ref weights);
			if(bones.Length != mesh.vertices.Length || weights.Length != mesh.vertices.Length)
			{
				Debug.LogErrorFormat("Not enough skinning data "
					 + "(bones: {0}, weights: {1}, verts: {2})",
				      bones.Length, weights.Length, mesh.vertices.Length);
				return null;
			}

			BoneWeight[] boneWeights = new BoneWeight[mesh.vertices.Length];
			int maxBonesIndex = 0;
			for (int i = 0; i < boneWeights.Length; ++i)
			{
				// Unity seems expects the the sum of weights to be 1.
				float[] normalizedWeights = GLTFUtils.normalizeBoneWeights(weights[i]);

				boneWeights[i].boneIndex0 = (int)bones[i].x;
				boneWeights[i].weight0 = normalizedWeights[0];

				boneWeights[i].boneIndex1 = (int)bones[i].y;
				boneWeights[i].weight1 = normalizedWeights[1];

				boneWeights[i].boneIndex2 = (int)bones[i].z;
				boneWeights[i].weight2 = normalizedWeights[2];

				boneWeights[i].boneIndex3 = (int)bones[i].w;
				boneWeights[i].weight3 = normalizedWeights[3];

				maxBonesIndex = (int)Mathf.Max(maxBonesIndex,
					bones[i].x, bones[i].y, bones[i].z, bones[i].w);
			}

			return boneWeights;
		}

		/// <summary>
		/// Get the bone transforms for a skin, in Unity's native format.
		///
		/// See https://docs.unity3d.com/ScriptReference/Mesh-bindposes.html
		/// for a minimal example of how to set up a skinned mesh in
		/// Unity including bone weights, bindposes, etc.
		/// </summary>
		protected Transform[] GetBones(Skin skin)
		{
			Transform[] bones = new Transform[skin.Joints.Count];
			for (int i = 0; i < skin.Joints.Count; ++i)
				bones[i] = _imported.Nodes[skin.Joints[i].Id].transform;
			return bones;
		}
	}
}
