/*************************************************************************
 *  Copyright © 2019 Mogoson. All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  ContextMenuObjectExample.cs
 *  Description  :  Example of context menu object.
 *------------------------------------------------------------------------
 *  Author       :  Mogoson
 *  Version      :  0.1.0
 *  Date         :  7/26/2019
 *  Description  :  Initial development version.
 *************************************************************************/

using System.Collections.Generic;
using UnityEngine;
using RuntimeGizmos;

namespace MGS.ContextMenu
{
    [AddComponentMenu("MGS/ContextMenu/ContextMenuObjectExample")]
    public class ContextMenuObjectExample : ContextMenuObject
    {
        #region Field and Property
        protected int maxOffset = 3;
        #endregion

        #region Public Method
        /*public IEnumerable<string> CheckDisableMenuItems()
        {
            var disableItems = new List<string>();
            if (transform.localPosition.x >= maxOffset)
            {
                disableItems.Add(ContextMenuItemTags.ADD_POS_X);
            }
            else if (transform.localPosition.x <= -maxOffset)
            {
                disableItems.Add(ContextMenuItemTags.REDUCE_POS_X);
            }

            if (transform.localPosition.y >= maxOffset)
            {
                disableItems.Add(ContextMenuItemTags.ADD_POS_Y);
            }
            else if (transform.localPosition.y <= -maxOffset)
            {
                disableItems.Add(ContextMenuItemTags.REDUCE_POS_Y);
            }
            return disableItems;
        }*/

        private Vector3 defaultPosition;
        private Quaternion defaultRotation;
        private Vector3 defaultScale;


        private void Awake()
        {
            defaultPosition = transform.position;
            defaultRotation = transform.rotation;
            defaultScale = transform.localScale;
        }

        

        public override void OnMenuItemClick(string tag)
        {
            Debug.Log(tag);

            if (tag == ContextMenuItemTags.SELECT && !TransformGizmo.instance.IsAlreadySelected(transform))
            {
                TransformGizmo.instance.Deselect(false);
                TransformGizmo.instance.AddTarget(transform);
            }
            else if (tag == ContextMenuItemTags.DESELECT)
            {
                TransformGizmo.instance.Deselect(true);
            }
            else if (tag == ContextMenuItemTags.MOVE)
            {
                if (!TransformGizmo.instance.IsAlreadySelected(transform))
                {
                    TransformGizmo.instance.Deselect(false);
                    TransformGizmo.instance.AddTarget(transform);
                }
                TransformGizmo.instance.Move();
            }
            else if (tag == ContextMenuItemTags.ROTATE)
            {
                if (!TransformGizmo.instance.IsAlreadySelected(transform))
                {
                    TransformGizmo.instance.Deselect(false);
                    TransformGizmo.instance.AddTarget(transform);
                }
                TransformGizmo.instance.Rotate();
            }
            else if (tag == ContextMenuItemTags.SCALE)
            {
                if (!TransformGizmo.instance.IsAlreadySelected(transform))
                {
                    TransformGizmo.instance.Deselect(false);
                    TransformGizmo.instance.AddTarget(transform);
                }
                TransformGizmo.instance.Scale();
            }
            else if (tag == ContextMenuItemTags.RESET)
            {
                transform.position = defaultPosition;
                transform.rotation = defaultRotation;
                transform.localScale = defaultScale;
            }

            else if (tag == ContextMenuItemTags.FOCUS)
            {
                GILES.pb_SceneCamera.Focus(gameObject);
            }
            else if (tag == ContextMenuItemTags.DELETE)
            {
                if(TransformGizmo.instance.mainTargetRoot == null)
                    TransformGizmo.instance.AddTarget(transform, false);
                TransformGizmo.instance.Delete();
            }
        }
        #endregion
    }
}