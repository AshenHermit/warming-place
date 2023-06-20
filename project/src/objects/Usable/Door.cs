using Godot;
using System;
using Game.Utils;

namespace Game
{
    public class Door : Usable
    {
        public class Unit : Godot.Object
        {
            public Node DoorNode;

            private AnimationPlayer _animPlayer = null;
            private Godot.Object _body = null;
            private int _defCollisionLayer;

            private string _openAnimationName = "open";
            private string _closeAnimationName = "close";
            public int DisabledCollisionLayer = 0;

            public bool IsOpened
            {
                get
                {
                    if (_body == null || _animPlayer == null) return false;
                    return _animPlayer.AssignedAnimation == _openAnimationName && !_animPlayer.IsPlaying();
                }
            }

            public static Door.Unit FromNode(Node doorNode)
            {
                Door.Unit door = new Door.Unit();
                door.DoorNode = doorNode;

                var animPlayers = door.DoorNode.GetAllChildrenRecursive<AnimationPlayer>();
                if (animPlayers.Count > 0)
                {
                    door._animPlayer = animPlayers[0];
                    door._openAnimationName = door._animPlayer.FindAnimationEndingWith("open");
                    door._closeAnimationName = door._animPlayer.FindAnimationEndingWith("close");
                }
                var bodies = door.DoorNode.GetAllChildrenRecursive<PhysicsBody>();
                if (bodies.Count > 0)
                {
                    door._body = (Godot.Object)bodies[0];
                }
                else
                {
                    door._body = door.DoorNode;
                }
                if (door._body != null) door._defCollisionLayer = (int)door._body.Get("collision_layer");
                GD.Print(door._body);
                return door;
            }
            public void Open()
            {
                if (_animPlayer == null) return;
                if (_animPlayer.IsPlaying()) return;
                if (IsOpened) return;
                _animPlayer.Play(_openAnimationName);

                if (_body == null) return;
                _body.Set("collision_layer", DisabledCollisionLayer);
                _body.Set("collision_mask", DisabledCollisionLayer);
            }
            public void Close()
            {
                if (_animPlayer == null) return;
                if (_animPlayer.IsPlaying()) return;
                if (!IsOpened) return;
                _animPlayer.Play(_closeAnimationName);

                if (_body == null) return;
                _body.Set("collision_layer", _defCollisionLayer);
                _body.Set("collision_mask", _defCollisionLayer);
            }
        }

        private Door.Unit _doorUnit;
        public override void _Ready()
        {
            _doorUnit = Door.Unit.FromNode(this);
            _doorUnit.DisabledCollisionLayer = 8;
        }

        public override void Use(Node invoker)
        {
            if (_doorUnit.IsOpened) _doorUnit.Close();
            else _doorUnit.Open();
        }
    }
}