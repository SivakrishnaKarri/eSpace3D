using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Piglet
{
    public static class BoundsUtil
    {
        /// <summary>
        /// Calculate the world space axis-aligned bounding box
        /// for a hierarchy of game objects containing zero or more meshes.
        /// If the hierarchy does not contain any meshes, then
        /// return value will be null (i.e. Bounds.HasValue == false).
        /// </summary>
        public static Bounds? GetRendererBoundsForHierarchy(GameObject o)
        {
            Bounds? bounds = null;

            // Note: Renderer.bounds returns the bounding box
            // in world space, whereas Mesh.bounds return the
            // bounding box in local space.

            Renderer renderer = o.GetComponent<Renderer>();
            if (renderer != null) {
                bounds = renderer.bounds;
            }

            foreach (Transform child in o.transform) {
                Bounds? childBounds = GetRendererBoundsForHierarchy(child.gameObject);
                if (childBounds.HasValue) {
                    if (!bounds.HasValue) {
                        bounds = childBounds;
                    } else {
                        Bounds tmp = bounds.Value;
                        tmp.Encapsulate(childBounds.Value);
                        bounds = tmp;
                    }
                }
            }

            return bounds;
        }
    }
}