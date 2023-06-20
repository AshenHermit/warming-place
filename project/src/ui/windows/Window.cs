using Godot;
using System;

using Game;

namespace Game.UI
{
	public class Window : Container
	{
		public WindowTopbar Topbar { get{ return GetChild<WindowTopbar>(0); } }
		public Control ParentControl { get { return GetParent<Control>(); } }

		public override void _Ready()
		{
			Topbar.OnDrag += OnDrag;
			Topbar.OnDragStarted += OnDragStarted;
			ParentControl.Connect("resized", this, "OnParentResized");
		}

		private void OnDrag(Vector2 relative)
		{
			RectPosition = RectPosition + relative;
			BoundOutOfAllColliding();
		}
		private void OnDragStarted()
		{
			GrabWindowFocus();
		}
		protected void GrabWindowFocus()
        {
			Global.Instance.GetUIManager().WindowsContainer.FocusOnWindow(this);
		}
		
		public virtual void OnFocused()
		{
			
		}

		/// <summary>
		/// Attached to a <b>gui_input</b> signal of background PanelContainer
		/// </summary>
		public void OnGameOverlayGUIEvent(InputEvent e)
		{
			if (e is InputEventMouseButton)
			{
				InputEventMouseButton mbuttonEvent = (InputEventMouseButton)e;
				if (mbuttonEvent.ButtonIndex == (int)ButtonList.Left)
				{
					
				}
			}
		}

		public void OnParentResized()
		{
			BoundOutOfAllColliding();
		}

		private void BoundOutOfAllColliding()
		{
			BoundOutOfParentBorders();
			// changed my mind
			// BoundOutOfSiblingControls();
		}

		private void BoundOutOfParentBorders() 
		{
			Control parent = ParentControl;
			Vector2 pos = RectPosition;
			if (pos.x < 0.0f) pos.x = 0.0f;
			if (pos.y < 0.0f) pos.y = 0.0f;
			if (pos.x + RectSize.x > parent.RectSize.x) pos.x = parent.RectSize.x - RectSize.x;
			if (pos.y + RectSize.y > parent.RectSize.y) pos.y = parent.RectSize.y - RectSize.y;
			RectPosition = pos;
		}
		private void BoundOutOfControl(Control c)
		{
			Vector2 pos = RectPosition;
			Rect2 intersection = c.GetRect().Clip(GetRect());
			// unfinished
			RectPosition = pos;
		}
		private void BoundOutOfSiblingControls()
		{
			Godot.Collections.Array<Control> siblings = ParentControl.GetAllChildrenRecursive<Control>();
			foreach (Control sibling in siblings)
			{
				if (sibling == this) continue;
				BoundOutOfControl(sibling);
			}
		}
	}
}
