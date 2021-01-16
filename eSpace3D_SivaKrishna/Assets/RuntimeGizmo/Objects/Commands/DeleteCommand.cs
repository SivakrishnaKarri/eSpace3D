using System;
using CommandUndoRedo;
using UnityEngine;
using System.Collections.Generic;

namespace RuntimeGizmos
{
	public class DeleteCommand : ICommand
	{
		protected GameObject[] targets;
		protected TransformGizmo transformGizmo;

		public DeleteCommand(TransformGizmo transformGizmo, GameObject[] targets)
		{
			this.transformGizmo = transformGizmo;
			this.targets = targets;
		}

		public void Execute()
		{
			foreach(GameObject g in targets)
				g.SetActive(false);
		}

		public void UnExecute()
		{
			foreach (GameObject g in targets)
				g.SetActive(true);
		}
	}

}
