using Godot;
using System;
using Game.Utils;

namespace Game
{
    public class DoorXModule : XModule
    {
        [Export]
        public Godot.Collections.Array<NodePath> DoorNodesPaths = new Godot.Collections.Array<NodePath>();

        private Godot.Collections.Array<Door.Unit> _doors = new Godot.Collections.Array<Door.Unit>();

        public override void _Ready()
        {
            base._Ready();
            foreach(NodePath path in DoorNodesPaths)
            {
                Node doorNode = GetNode<Node>(path);
                if (doorNode == null) return;
                _doors.Add(Door.Unit.FromNode(doorNode));
            }
        }

        protected override void PyContextInit()
        {
            base.PyContextInit();
            PyContext?.Connect("_42open_signal", this, "Open");
            PyContext?.Connect("_42close_signal", this, "Close");
        }

        public void Open()
        {
            foreach (Door.Unit door in _doors) door.Open();
        }
        public void Close()
        {
            foreach (Door.Unit door in _doors) door.Close();
        }
    }
}

