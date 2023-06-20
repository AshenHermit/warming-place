using System;
using Godot;

namespace Game.UI
{
    public class ResizeableRect : Control
    {
        private bool _dragging = false;
        private bool _canResize = false;
        private Vector2.Axis _axis = Vector2.Axis.X;
        private bool _resizeX = false;
        private bool _resizeY = false;
        private float _border = 20.0f;

        [Export]
        public NodePath NodeToResizePath;
        public Control NodeToResize{ get { return GetNode<Control>(NodeToResizePath); } }

        public void _OnGuiInput(InputEvent e)
        {
            if (e is InputEventMouseMotion mouseMotion)
            {
                if (_dragging)
                {
                    if (_resizeX) NodeToResize.RectSize += Vector2.Right * mouseMotion.Relative.x;
                    if (_resizeY) NodeToResize.RectSize += Vector2.Down * mouseMotion.Relative.y;
                }
                else
                {
                    UpdateAxisAndAbility(mouseMotion);
                    if (_canResize)
                    {
                        if(_resizeX && _resizeY) MouseDefaultCursorShape = CursorShape.Fdiagsize;
                        else if(_resizeX) MouseDefaultCursorShape = CursorShape.Hsize;
                        else if (_resizeY) MouseDefaultCursorShape = CursorShape.Vsize;
                    }
                    else
                    {
                        MouseDefaultCursorShape = CursorShape.Arrow;
                    }
                }
            }
            if (e is InputEventMouseButton buttonEvent)
            {
                if (_canResize)
                {
                    if(buttonEvent.ButtonIndex == (int)ButtonList.Left)
                    {
                        _dragging = buttonEvent.Pressed;
                    }
                }
            }
        }
        private void UpdateAxisAndAbility(InputEventMouse e)
        {
            _canResize = false;
            _resizeX = false;
            _resizeY = false;
            if (e.Position.x > RectSize.x - _border)
            {
                _resizeX = true;
                _canResize = true;
            }
            if (e.Position.y > RectSize.y - _border)
            {
                _resizeY = true;
                _canResize = true;
            }
        }
    }
}
