/*************************************************************************
 *  Copyright © 2019 Mogoson. All rights reserved.
 *------------------------------------------------------------------------
 *  File         :  ContextMenuItemTags.cs
 *  Description  :  Define tags of context menu items.
 *------------------------------------------------------------------------
 *  Author       :  Mogoson
 *  Version      :  0.1.0
 *  Date         :  7/26/2019
 *  Description  :  Initial development version.
 *************************************************************************/

namespace MGS.ContextMenu
{
    public static class ContextMenuItemTags
    {
        //Selection
        public const string SELECTION = "Selection";
        public const string SELECT = "Select";
        public const string DESELECT = "Deselect";
        public const string FOCUS = "Focus";

        //PRS
        public const string PRS = "PRS";
        public const string MOVE = "Move";
        public const string ROTATE = "Rotate";
        public const string SCALE = "Scale";

        public const string EDITOBJECT = "Edit";
        public const string DELETE = "Delete";
        public const string RESET = "Reset";

    }
}