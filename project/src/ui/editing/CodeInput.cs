using System;
using Godot;

namespace Game.UI
{
    public class CodeInput : LineEdit
    {
        private Godot.Collections.Array<string> _inputHistory;
        private int _inputHistorySelection = 0;
        private bool _focused;
        public bool Focused { get { return _focused; } }

        public delegate void CodeExecuteFunc(string code);
        public event CodeExecuteFunc OnCodeExecute;
        public event Action OnFocused;

        public CodeInput()
        {
            _inputHistory = new Godot.Collections.Array<string>();
        }

        public void _OnFocusEntered()
        {
            _focused = true;
            Global.Instance.GetPlayer().Immobilize();
            OnFocused?.Invoke();
        }
        public void _OnFocusExited()
        {
            _focused = false;
            Global.Instance.GetPlayer().Mobilize();
        }

        public void _OnGuiInput(InputEvent e)
        {
            if (e is InputEventKey keyEvent)
            {
                if (keyEvent.Pressed)
                {
                    switch (keyEvent.Scancode)
                    {
                        case (uint)KeyList.Enter:
                            ReadAndExecuteCode(); break;
                        case (uint)KeyList.Up:
                            ScrollThroughHistory(1); break;
                        case (uint)KeyList.Down:
                            ScrollThroughHistory(-1); break;
                        default:
                            break;
                    }
                }
            }
        }

        public void ReadAndExecuteCode()
        {
            string code = Text;
            ExecuteCode(code);
            Text = "";
            _inputHistorySelection = -1;
            if (_inputHistory.Count > 0)
            {
                if (_inputHistory[0] == code) return;
            }
            _inputHistory.Insert(0, code);
        }

        public void ScrollThroughHistory(int direction)
        {
            if (_inputHistory.Count == 0) return;
            _inputHistorySelection = Math.Max(0, Math.Min(_inputHistorySelection+direction, _inputHistory.Count-1));
            string code = _inputHistory[_inputHistorySelection];
            Text = code;
            CallDeferred("grab_focus");
            CaretPosition = code.Length;
        }

        public void ExecuteCode(string code)
        {
            OnCodeExecute?.Invoke(code);
        }
    }
}
