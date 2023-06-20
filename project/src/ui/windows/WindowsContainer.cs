using Godot;
using System;

using Game;
using Godot.Collections;

namespace Game.UI
{
    public class WindowsContainer : Control
    {
        private Window _lastFocusedWindow = null;

        public Array<Window> GetAllWindows()
        {
            Array<Window> windows = new Array<Window>();
            foreach (Godot.Object child in GetChildren())
            {
                if(child is Window)
                {
                    windows.Add((Window)child);
                }
            }
            return windows;
        }

        public void FocusOnWindow(Window window)
        {
            _lastFocusedWindow = window;
            MoveChild(window, GetChildCount() - 1);
            window.OnFocused();
        }

        public T FindWindow<T>()
        {
            foreach (object child in GetChildren())
            {
                if (child is T)
                {
                    return (T)child;
                }
            }
            return default(T);
        }

        public override void _Ready()
        {
            //Global.Instance.GetPlayer().OnObjectToUseChanged += UpdateWindows;
            Global.Instance.GetUIManager().OnOverlayChanged += OnOverlayChanged;
        }

        private void OnOverlayChanged(OverlayType overlay)
        {
            if (overlay == OverlayType.GAME_OVERLAY) OnShown();
        }

        private void OnShown()
        {
            if (_lastFocusedWindow != null) FocusOnWindow(_lastFocusedWindow);
        }

        public void UpdateWindows()
        {
            
        }
    }
}
 