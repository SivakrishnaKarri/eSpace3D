using UnityEngine;

namespace Piglet
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// If a MonoBehaviour of type T is attached to this GameObject,
        /// remove it. Otherwise do nothing.
        public static void RemoveComponent<T>(this GameObject gameObject)
            where T : Object
        {
            Object component = gameObject.GetComponent<T>();
            if (component == null)
                return;

            if (Application.isPlaying)
                Object.Destroy(component);
            else
                Object.DestroyImmediate(component);
        }

        /// <summary>
        /// If this GameObject has a MonoBehaviour of type T,
        /// return it. Otherwise, add a new MonoBehaviour of type T
        /// and return that instead.
        /// </summary>
        public static T GetOrAddComponent<T>(this GameObject gameObject)
            where T : Component
        {
            T result = gameObject.GetComponent<T>();
            if (result == null)
                result = gameObject.AddComponent<T>();
            return result;
        }
    }
}