using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace utilities
{
    public static class ExtensionMethods
    {
        public static Transform ClearAllChildren(this Transform parentTransform)
        {
            foreach (Transform child in parentTransform)
            {
                GameObject.Destroy(child.gameObject);
            }
            return parentTransform;
        }
    }
}
