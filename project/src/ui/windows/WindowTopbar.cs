using Godot;
using System;

using Game;

namespace Game.UI
{
	public class WindowTopbar : Container
	{
		public delegate void DragAction(Vector2 relative);
		public event DragAction OnDrag;
		public event Action OnDragStarted;
		public event Action OnDragEnded;

		private bool _isDragging = false;
		private Vector2 _dragStartPosition;

		public override void _Ready()
		{
			
		}

		/// <summary>
		/// Attached to a <b>gui_input</b> signal
		/// </summary>
		public void OnGuiInput(InputEvent e)
        {
			if (e is InputEventMouseMotion)
            {
				InputEventMouseMotion motionEvent = (InputEventMouseMotion)e;
                if (_isDragging)
                {
					OnDrag.Invoke(motionEvent.Relative);
                }
			}
			else if (e is InputEventMouseButton)
			{
				InputEventMouseButton buttonEvent = (InputEventMouseButton)e;
				if (buttonEvent.ButtonIndex == (int)ButtonList.Left)
                {
                    if (buttonEvent.IsPressed()){
						_dragStartPosition = buttonEvent.GlobalPosition;
						_isDragging = true;
						OnDragStarted?.Invoke();
					}
                    else
                    {
                        if (_isDragging)
                        {
							_isDragging = false;
							OnDragEnded?.Invoke();
                        }
					}
				}
			}
		}

	//  // Called every frame. 'delta' is the elapsed time since the previous frame.
	//  public override void _Process(float delta)
	//  {
	//      
	//  }
	}
}
