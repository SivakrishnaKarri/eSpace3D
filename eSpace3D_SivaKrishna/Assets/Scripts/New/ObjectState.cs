using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct ObjectState 
{
  
    private Transform transform;

    private Vector3 localPosition;
    private Quaternion localRotation;
    private Vector3 localScale;
    private Material objMaterial;

    private bool active;

    public ObjectState(GameObject obj)
    {
        transform = obj.transform;
        localPosition = transform.localPosition;
        localRotation = transform.localRotation;
        localScale = transform.localScale;
        objMaterial = transform.GetComponent<Renderer>().material;

        active = obj.activeSelf;
    }

    public void Apply()
    {
        transform.localPosition = localPosition;
        transform.localRotation = localRotation;
        transform.localScale = localScale;
        transform.gameObject.GetComponent<Renderer>().material = objMaterial; 
        transform.gameObject.SetActive(active);
    }
}