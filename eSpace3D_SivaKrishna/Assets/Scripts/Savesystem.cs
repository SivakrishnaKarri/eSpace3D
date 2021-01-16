using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OdinSerializer;
using System.IO;
using System;

public static class SaveSystem
{
    public static void Save(LivingRoom data, string filePath)
    {
        List<UnityEngine.Object> unityObjectReferences = new List<UnityEngine.Object>();
        unityObjectReferences.Add(new Mesh());
        unityObjectReferences.Add(new Material(Shader.Find("Standard")));

        byte[] bytes = SerializationUtility.SerializeValue(data, DataFormat.Binary, out unityObjectReferences);
        File.WriteAllBytes(filePath, bytes);
    }

    public static LivingRoom Load(string filePath)
    {
        byte[] bytes = File.ReadAllBytes(filePath);
        return SerializationUtility.DeserializeValue<LivingRoom>(bytes, DataFormat.Binary);
    }
}

[Serializable]
public class LivingRoom
{
    [OdinSerialize, NonSerialized]
    GameObjectInfo[] objects;
    public LivingRoom(GameObjectInfo[] os)
    {
        this.objects = os;
    }

    public GameObjectInfo[] GetObjects()
    {
        return objects;
    }
}

[Serializable]
public class GameObjectInfo
{
    public bool isActive = true;
    public string name;
    public bool isAddedRuntime;
    public SerializableMesh mesh;
    public SerializableMaterial[] materials;
    public byte[] textureData;
    public Vector3 position, scale;
    public Quaternion rotation;
}


[Serializable]
public class SerializableMaterial
{
    public string shaderName;
    public Dictionary<string, byte[]> propertyTextures;
    public Dictionary<string, Vector2> propertyTiling;
    public Dictionary<string, Vector2> propertyOffset;
    public Dictionary<string, float> propertyValues;

}

[Serializable]
public class SerializableMesh
{
    [SerializeField]
    public Vector3[] vertices;
    [SerializeField]
    public int[] triangles;
    [SerializeField]
    public Vector2[] uv;
    [SerializeField]
    public Vector2[] uv2;
    [SerializeField]
    public Vector3[] normals;
    [SerializeField]
    public Color[] colors;

    public SerializableMesh(Mesh m) // Constructor: takes a mesh and fills out SerializableMeshInfo data structure which basically mirrors Mesh object's parts.
    {

        vertices = new Vector3[m.vertices.Length];
        vertices = m.vertices;
        triangles = new int[m.triangles.Length];
        triangles = m.triangles;
        uv = new Vector2[m.uv.Length];
        uv = m.uv;
        uv2 = new Vector2[m.uv2.Length];
        uv2 = m.uv2;
        normals = new Vector3[m.normals.Length];
        normals = m.normals;
        colors = new Color[m.colors.Length];
        colors = m.colors;

    }

    // GetMesh gets a Mesh object from currently set data in this SerializableMeshInfo object.
    // Sequential values are deserialized to Mesh original data types like Vector3 for vertices.
    public Mesh GetMesh()
    {
        Mesh m = new Mesh();
        List<Vector3> verticesList = new List<Vector3>();
        for (int i = 0; i < vertices.Length; i++)
        {
            verticesList.Add(vertices[i]);
        }
        m.SetVertices(verticesList);
        m.triangles = triangles;
        List<Vector2> uvList = new List<Vector2>();
        for (int i = 0; i < uv.Length; i++)
        {
            uvList.Add(uv[i]);
        }
        m.SetUVs(0, uvList);
        List<Vector2> uv2List = new List<Vector2>();
        for (int i = 0; i < uv2.Length; i++)
        {
            uv2List.Add(uv2[i]);
        }
        m.SetUVs(1, uv2List);
        List<Vector3> normalsList = new List<Vector3>();
        for (int i = 0; i < normals.Length; i++)
        {
            normalsList.Add(normals[i]);
        }
        m.SetNormals(normalsList);
        m.colors = colors;

        return m;
    }
}
