using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using SFB;
using Piglet;
using System.Linq;
using System;
using System.Runtime.InteropServices;

public class FileOpsManager : MonoBehaviour
{
	GameObject go;
	int index;
	string goKey;
	string saveDir = "SavedScenes";
	string gdm = "gdm";   //short for gandeevam


	[SerializeField]
	Transform sceneParent;

	[SerializeField]
	RuntimeImportBehaviour importer;



	private void OnEnable()
	{
		MenuStrip.onClickImport += SelectGLTFFileToImport;
		RuntimeImportBehaviour.onModelImportSuccess += SetupNewObject;

		MenuStrip.onClickSave += saveScene;
		MenuStrip.onClickSaveAs += saveSceneAs;
		MenuStrip.onClickOpen += openScene;
		MenuStrip.onClickExit += QuitApp;
		MenuStrip.onClickNew += FreshRoom;
	}

	private void FreshRoom()
	{
		SceneManager.LoadScene(SceneManager.GetActiveScene().name);
	}

	private void Start()
	{
		//Debug.Log(DataManager.Instance.argument);
		if (DataManager.Instance.argument == "open")
		{
			DataManager.Instance.argument = "none";
			Load(DataManager.Instance.filePath);
		}
	}

	private void SelectGLTFFileToImport()
	{
		var extensions = new[] {
			new ExtensionFilter ("3D Files", "gltf", "GLTF")
		};

		var path = StandaloneFileBrowser.OpenFilePanel("Open File", "", extensions, false).ToArray();
		if (path.Length > 0)
		{
			string filePath = path[0];
			importer.Import(filePath);
		}
	}

	private void SetupNewObject(GameObject go)
	{
		List<GameObject> emptyObjects = new List<GameObject>();
		foreach (Transform t in go.GetComponentsInChildren<Transform>())
		{
			if (t.GetComponent<MeshRenderer>() != null)
			{
				t.gameObject.AddComponent<MeshCollider>();
				t.gameObject.AddComponent<MGS.ContextMenu.ContextMenuObjectExample>();
				t.SetParent(sceneParent);
			}
			else
				emptyObjects.Add(t.gameObject);

			t.tag = "Selectable";
			t.gameObject.layer = 9;

			t.name = "runtimeobject";
		}

		//Debug.Log(emptyObjects.Count);

		while (emptyObjects.Count > 0)
		{
			Destroy(emptyObjects[0]);
			emptyObjects.RemoveAt(0);
		}

		//go.transform.SetParent(sceneParent);
	}

	private void OnDisable()
	{
		MenuStrip.onClickImport -= SelectGLTFFileToImport;
		RuntimeImportBehaviour.onModelImportSuccess -= SetupNewObject;
		MenuStrip.onClickSave -= saveScene;
		MenuStrip.onClickSaveAs -= saveSceneAs;
		MenuStrip.onClickOpen -= openScene;
		MenuStrip.onClickExit -= QuitApp;
		MenuStrip.onClickNew -= FreshRoom;
	}

	private void QuitApp()
	{
		Application.Quit();
	}


	public void OnModeChanged(int index)
	{
		switch (index)
		{
			case 1:
				saveScene();
				break;
			case 2:
				openScene();
				break;
			case 3:
				saveSceneAs();
				break;
		}
	}

	private byte[] GetBytesOfTexture(Texture t)
	{
		if (t == null)
			return null;

		Texture mainTexture = t;
		Texture2D texture2D = new Texture2D(mainTexture.width, mainTexture.height, TextureFormat.RGBA32, false);
		RenderTexture currentRT = RenderTexture.active;
		RenderTexture renderTexture = new RenderTexture(mainTexture.width, mainTexture.height, 32);
		Graphics.Blit(mainTexture, renderTexture);
		RenderTexture.active = renderTexture;
		texture2D.ReadPixels(new Rect(0, 0, renderTexture.width, renderTexture.height), 0, 0);
		texture2D.Apply();
		byte[] bytes = texture2D.EncodeToJPG(100);
		Destroy(texture2D);
		return bytes;
	}

	public void SaveRoom(string path)
	{
		List<GameObjectInfo> roomObjects = new List<GameObjectInfo>();
		Transform[] sceneTransforms = sceneParent.GetComponentsInChildren<Transform>(true);

		foreach (Transform t in sceneTransforms)
		{
			GameObject sceneObject = t.gameObject;
			/*goKey = "goKey" + "_" + i;
			print("Saving ... goKey = " + goKey + " gameObject.name = " + sceneObject.name);
			ES3.Save(goKey, sceneObject, filePath);*/

			GameObjectInfo goi = new GameObjectInfo();
			goi.isActive = sceneObject.activeSelf;
			goi.name = sceneObject.name;
			goi.position = sceneObject.transform.position;
			goi.rotation = sceneObject.transform.rotation;
			goi.scale = sceneObject.transform.localScale;
			if (sceneObject.name == "runtimeobject")
			{
				goi.isAddedRuntime = true;
				goi.mesh = new SerializableMesh(sceneObject.GetComponent<MeshFilter>().mesh);

				Material[] mats = sceneObject.GetComponent<MeshRenderer>().materials;
				goi.materials = new SerializableMaterial[mats.Length];
				for (int i=0; i<mats.Length; i++)
				{
					Material mat = mats[i];
					SerializableMaterial sMat = new SerializableMaterial();
					sMat.shaderName = mat.shader.ToString();
					sMat.shaderName = sMat.shaderName.Trim("(UnityEngine.Shader)".ToCharArray());
					sMat.shaderName = sMat.shaderName.Remove(sMat.shaderName.Length - 1, 1);
					string[] textureProperties = mat.GetTexturePropertyNames();
					sMat.propertyTextures = new Dictionary<string, byte[]>();
					sMat.propertyTiling = new Dictionary<string, Vector2>();
					sMat.propertyOffset = new Dictionary<string, Vector2>();
					sMat.propertyValues = new Dictionary<string, float>();


					if (mat.HasProperty("_roughnessFactor"))
						sMat.propertyValues.Add("_roughnessFactor", mat.GetFloat("_roughnessFactor"));
					if (mat.HasProperty("_metallicFactor"))
						sMat.propertyValues.Add("_metallicFactor", mat.GetFloat("_metallicFactor"));

					foreach (string property in textureProperties)
					{
						Texture tex = mat.GetTexture(property);
						if (tex != null)
						{
							sMat.propertyTextures.Add(property, GetBytesOfTexture(tex));
							sMat.propertyTiling.Add(property, mat.GetTextureScale(property));
							sMat.propertyOffset.Add(property, mat.GetTextureOffset(property));
						}
					}
					goi.materials[i] = sMat;
				}
			}

			roomObjects.Add(goi);
		}

		SaveSystem.Save(new LivingRoom(roomObjects.ToArray()), path);

		Debug.Log("successfully saved");
	}

	public void saveScene()
	{
		Scene scene = SceneManager.GetActiveScene();
		string filePath = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + saveDir + Path.DirectorySeparatorChar + scene.name + "." + gdm;
		SaveRoom(filePath);
	}

	public void saveSceneAs()
	{
		print("In saveScene() ...");
		Scene scene = SceneManager.GetActiveScene();
		print("scene name is " + scene.name);
		string savedScenesDir = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + saveDir;
		//var paths = StandaloneFileBrowser.OpenFilePanel("Select a File", savedScenesDir, "gdm", false);
		var filePath = StandaloneFileBrowser.SaveFilePanel("Select a File", savedScenesDir, scene.name, gdm);
		print("filePath = " + filePath);
		SaveRoom(filePath);
	}

	public GameObject FindObject(GameObject parent, string name)
	{
		Transform[] trs = parent.GetComponentsInChildren<Transform>(true);
		foreach (Transform t in trs)
		{
			if (t.name == name)
			{
				return t.gameObject;
			}
		}
		return null;
	}

	public void Load(string path)
	{
		LivingRoom loadedRoom = SaveSystem.Load(path);

		foreach (GameObjectInfo goi in loadedRoom.GetObjects())
		{
			GameObject g = FindObject(sceneParent.gameObject, goi.name);
			Transform obj = null;
			if (g != null)
				obj = g.transform;
			if (goi.name == "runtimeobject")
			{
				GameObject go = new GameObject("runtimeobject");
				go.AddComponent<MeshFilter>().mesh = goi.mesh.GetMesh();
				MeshRenderer meshRenderer = go.AddComponent<MeshRenderer>();
				meshRenderer.materials = new Material[goi.materials.Length];
				for(int j=0; j<goi.materials.Length; j++)
				{
					SerializableMaterial sMat = goi.materials[j];
					Debug.Log(sMat.shaderName);
					Material mat = meshRenderer.materials[j];
					mat.shader = Shader.Find(sMat.shaderName);

					for (int i = 0; i < sMat.propertyTextures.Count; i++)
					{
						byte[] bytes;
						Vector2 tiling, offset;
						string key = sMat.propertyTextures.ElementAt(i).Key;
						sMat.propertyTextures.TryGetValue(key, out bytes);
						sMat.propertyTiling.TryGetValue(key, out tiling);
						sMat.propertyOffset.TryGetValue(key, out offset);

						Texture2D texture2D = new Texture2D(2, 2);
						texture2D.LoadImage(bytes);
						texture2D.Apply();
						mat.SetTexture(key, texture2D);

						mat.SetTextureScale(key, tiling);
						mat.SetTextureOffset(key, offset);
					}

					foreach (var property in sMat.propertyValues)
						mat.SetFloat(property.Key, property.Value);
					
					meshRenderer.materials[j] = mat;
				}

				go.transform.SetParent(sceneParent);
				obj = go.transform;
				go.AddComponent<MeshCollider>();
				go.AddComponent<MGS.ContextMenu.ContextMenuObjectExample>();
				go.tag = "Selectable";
				go.gameObject.layer = 9;

			}
			obj.position = goi.position;
			obj.rotation = goi.rotation;
			obj.localScale = goi.scale;
			obj.gameObject.SetActive(goi.isActive);
		}
	}


	public void openScene()
	{
		print("In openScene() ...");
		Scene scene = SceneManager.GetActiveScene();
		print("scene name is " + scene.name);
		string savedScenesDir = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + saveDir;
		GameObject[] rootGameObjects = scene.GetRootGameObjects();
		index = 0;
		string filename = "";
		string[] paths = (string[])StandaloneFileBrowser.OpenFilePanel("Select a File", savedScenesDir, "gdm", false);
		print(paths.ToString());
		if (paths.Length > 0)
		{
			string filePath = paths[0];
			DataManager.Instance.argument = "open";
			DataManager.Instance.filePath = filePath;
			SceneManager.LoadScene(SceneManager.GetActiveScene().name);
		}

	}

}
