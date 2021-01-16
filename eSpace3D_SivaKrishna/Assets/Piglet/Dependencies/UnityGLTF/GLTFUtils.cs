using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
using System.IO;
using System.Text.RegularExpressions;
using System;
using UnityGLTF.Extensions;
using System.Reflection;

public class GLTFUtils
{
	public static Transform[] getSceneTransforms()
	{
		var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
		var gameObjects = scene.GetRootGameObjects();
		return Array.ConvertAll(gameObjects, gameObject => gameObject.transform);
	}

#if UNITY_EDITOR
	public static Transform[] getSelectedTransforms()
	{
		if (Selection.transforms.Length <= 0)
			throw new Exception("No objects selected, cannot export.");

		return Selection.transforms;
	}
#endif

	public static string UnityToSystemPath(string path)
	{
		char unitySeparator = '/';
		char pathSeparator = Path.DirectorySeparatorChar;
		path = path.Replace("Assets", Application.dataPath).Replace(unitySeparator, pathSeparator);
		return path;
	}

	public static string SystemToUnityPath(string path)
	{
		char unitySeparator = '/';
		char pathSeparator = Path.DirectorySeparatorChar;
		path = path.Replace(pathSeparator, unitySeparator).Replace(Application.dataPath, "Assets");
		return path;
	}

	public static string unifyPathSeparator(string path)
	{
		return path.Replace("\\\\", "/").Replace("\\", "/");
	}
	public static string getPathProjectFromAbsolute(string absolutePath)
	{
		return unifyPathSeparator(absolutePath.Replace(Application.dataPath, "Assets"));
	}

	public static string getPathAbsoluteFromProject(string projectPath)
	{
		return unifyPathSeparator(projectPath.Replace("Assets", Application.dataPath));
	}

	public static bool isFolderInProjectDirectory(string path)
	{
		return path.Contains(Application.dataPath);
	}

	public static Regex rgx = new Regex("[^a-zA-Z0-9 -_.]");

	static public string cleanName(string s)
	{
		return rgx.Replace(s, "").Replace("/", " ").Replace("\\", " ").Replace(":", "_").Replace("\"", "");
	}

	static public bool isValidMeshObject(GameObject gameObject)
	{
		return gameObject.GetComponent<MeshFilter>() != null && gameObject.GetComponent<MeshFilter>().sharedMesh != null ||
			   gameObject.GetComponent<SkinnedMeshRenderer>() != null && gameObject.GetComponent<SkinnedMeshRenderer>().sharedMesh != null;
	}

	public static void removeEmptyDirectory(string directoryPath)
	{
		if (!Directory.Exists(directoryPath))
			return;

		DirectoryInfo info = new DirectoryInfo(directoryPath);
		if (info.GetFiles().Length == 0)
			Directory.Delete(directoryPath, true);
	}

	public static void removeFileList(string[] fileList)
	{
		foreach (string file in fileList)
		{
			if (File.Exists(file))
				File.Delete(file);
		}

		foreach (string dir in fileList)
		{
			if (Directory.Exists(dir))
				Directory.Delete(dir);
		}
	}

	public static Matrix4x4 convertMatrixLeftToRightHandedness(Matrix4x4 mat)
	{
		Vector3 position = mat.GetColumn(3);
		convertVector3LeftToRightHandedness(ref position);
		Quaternion rotation = Quaternion.LookRotation(mat.GetColumn(2), mat.GetColumn(1));
		convertQuatLeftToRightHandedness(ref rotation);

		Vector3 scale = new Vector3(mat.GetColumn(0).magnitude, mat.GetColumn(1).magnitude, mat.GetColumn(2).magnitude);
		float epsilon = 0.00001f;

		// Some issues can occurs with non uniform scales
		if (Mathf.Abs(scale.x - scale.y) > epsilon || Mathf.Abs(scale.y - scale.z) > epsilon || Mathf.Abs(scale.x - scale.z) > epsilon)
		{
			Debug.LogWarning("A matrix with non uniform scale is being converted from left to right handed system. This code is not working correctly in this case");
		}

		// Handle negative scale component in matrix decomposition
		if (Matrix4x4.Determinant(mat) < 0)
		{
			Quaternion rot = Quaternion.LookRotation(mat.GetColumn(2), mat.GetColumn(1));
			Matrix4x4 corr = Matrix4x4.TRS(mat.GetColumn(3), rot, Vector3.one).inverse;
			Matrix4x4 extractedScale = corr * mat;
			scale = new Vector3(extractedScale.m00, extractedScale.m11, extractedScale.m22);
		}

		// convert transform values from left handed to right handed
		mat.SetTRS(position, rotation, scale);
		Debug.Log("INVERSIOON");
		return mat;
	}

	public static void convertVector3LeftToRightHandedness(ref Vector3 vect)
	{
		vect.z = -vect.z;
	}

	public static void convertVector4LeftToRightHandedness(ref Vector4 vect)
	{
		vect.z = -vect.z;
		vect.w = -vect.w;
	}

	public static void convertQuatLeftToRightHandedness(ref Quaternion quat)
	{
		quat.w = -quat.w;
		quat.z = -quat.z;
	}

	public static string buildImageName(Texture2D image)
	{
		string extension = GLTFTextureUtils.useJPGTexture(image) ? ".jpg": ".png";
		return image.GetInstanceID().ToString().Replace("-", "") + "_" + image.name + extension;
	}

#if UNITY_EDITOR
	public static bool getPixelsFromTexture(ref Texture2D texture, out Color[] pixels)
	{
		//Make texture readable
		TextureImporter im = AssetImporter.GetAtPath(AssetDatabase.GetAssetPath(texture)) as TextureImporter;
		if (!im)
		{
			pixels = new Color[1];
			return false;
		}

		bool readable = im.isReadable;
		TextureImporterCompression format = im.textureCompression;
		TextureImporterType type = im.textureType;
		bool isConvertedBump = im.convertToNormalmap;
		bool srgb = im.sRGBTexture;
		im.sRGBTexture = false;

		if (!readable)
			im.isReadable = true;
		if (type != TextureImporterType.Default)
			im.textureType = TextureImporterType.Default;

		im.textureCompression = TextureImporterCompression.Uncompressed;
		im.SaveAndReimport();

		pixels = texture.GetPixels();

		if (!readable)
			im.isReadable = false;
		if (type != TextureImporterType.Default)
			im.textureType = type;

		if (isConvertedBump)
			im.convertToNormalmap = true;

		im.sRGBTexture = srgb;
		im.textureCompression = format;
		im.SaveAndReimport();

		return true;
	}
#endif

	public static string buildBlendShapeName(int meshIndex, int targetIndex)
	{
		return "Target_" + meshIndex + "_" + targetIndex;
	}

	public static float[] Vector4ToArray(Vector4 vector)
	{
		float[] arr = new float[4];
		arr[0] = vector.x;
		arr[1] = vector.y;
		arr[2] = vector.z;
		arr[3] = vector.w;

		return arr;
	}

	public static float[] normalizeBoneWeights(Vector4 weights)
	{
		float sum = weights.x + weights.y + weights.z + weights.w;
		if (sum != 1.0f)
		{
			weights = weights / sum;
		}

		return Vector4ToArray(weights);
	}
}

