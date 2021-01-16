/*************************************************************************
 *  Copyright © 2019 Mogoson. All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  ContextMenuHandlerExampleExtend.cs
 *  Description  :  Example of context menu handler.
 *------------------------------------------------------------------------
 *  Author       :  Mogoson
 *  Version      :  0.1.0
 *  Date         :  8/4/2019
 *  Description  :  Initial development version.
 *************************************************************************/

using MGS.UIForm;
using UnityEngine;

namespace MGS.ContextMenu
{
    [AddComponentMenu("MGS/ContextMenu/ContextMenuHandlerExampleExtend")]
    public class ContextMenuHandlerExampleExtend : ContextMenuTriggerHandler
    {
        private const int _height = 25;

        #region Field and Property
        private readonly ContextMenuElementData[] menuElementDatas = new ContextMenuElementData[]
        {
            new ContextMenuItemExtendData(Color.white,Color.black,Color.red,_height,"Select", ContextMenuItemTags.SELECT),
            new ContextMenuItemExtendData(Color.white,Color.black,Color.red,_height,"De-Select", ContextMenuItemTags.DESELECT),
            new ContextMenuItemExtendData(Color.white,Color.black,Color.red,_height,"Focus", ContextMenuItemTags.FOCUS),
            new ContextMenuSeparatorExtendData(Color.gray,1),
            new ContextMenuItemExtendData(Color.white,Color.black,Color.red,_height,"Move", ContextMenuItemTags.MOVE),
            new ContextMenuItemExtendData(Color.white,Color.black,Color.red,_height,"Rotate", ContextMenuItemTags.ROTATE),
            new ContextMenuItemExtendData(Color.white,Color.black,Color.red,_height,"Scale", ContextMenuItemTags.SCALE),
            new ContextMenuSeparatorExtendData(Color.gray,2),
            new ContextMenuItemExtendData(Color.white,Color.black,Color.red,_height,"Edit", ContextMenuItemTags.EDITOBJECT),
            new ContextMenuItemExtendData(Color.white,Color.black,Color.red,_height,"Delete", ContextMenuItemTags.DELETE),
            new ContextMenuItemExtendData(Color.white,Color.black,Color.red,_height,"Reset", ContextMenuItemTags.RESET),
        };

        private IContextMenuForm menuForm;
        #endregion

        #region Private Method
        private void Start()
        {
            //Open menu by UIFormManager to create form instance.
            var menuFormEx = UIFormManager.Instance.OpenForm<ContextMenuFormExtend>();
            menuFormEx.BgColor = Color.black;

            menuForm = menuFormEx;
            menuForm.RefreshElements(menuElementDatas);

            //Close it to hide the form instance.
            menuForm.Close();
        }
        #endregion

        private bool firstContext = true;

        #region Public Method
        public override IContextMenuForm OnMenuTriggerEnter(RaycastHit hitInfo)
        {
            if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.LeftAlt))
                return null;

            var menuObject = hitInfo.transform.GetComponent<ContextMenuObjectExample>();
            if (menuObject == null)
            {
                return null;
            }

            //var disableItems = menuObject.CheckDisableMenuItems();
            //menuForm.DisableItems(disableItems);

            menuForm.Open();
            menuForm.SetPosition(Input.mousePosition);

            //Set the handler of menu form so that we can received the event on menu item click.
            menuForm.Handler = menuObject;

            if (firstContext)
            {
                firstContext = false;
                GameObject g = GameObject.Find("ContextMenu");
               // g.transform.localScale = Vector3.one * 0.85f;
                g.GetComponentInParent<Canvas>().sortingOrder = 71;
            }

            return menuForm;
        }
        #endregion
    }
}