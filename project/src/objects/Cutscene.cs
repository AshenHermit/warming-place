using Godot;
using System;


namespace Game
{
    public class Cutscene : Spatial
    {
        [Export]
        public AudioStream AudioTrack;
        [Export]
        public bool FreeWhenEnded = true;
        [Export]
        public float TransitionSpeed = 2.0f;

        AnimationPlayer _animationPlayer;
        private bool _playing = false;

        public override void _Ready()
        {
            _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

            if(AudioTrack!=null) Global.Instance.GetAudioManager().PlayNonSpatialSound(AudioTrack);
        }

        public override void _Process(float delta)
        {
            if (_playing && !_animationPlayer.IsPlaying())
            {
                _playing = false;
                if (FreeWhenEnded)
                {
                    QueueFree();
                }
            }
        }

        public void Start()
        {
            _animationPlayer.Play("main");
            _playing = true;
        }

        public float GetAnimationPosition()
        {
            return _animationPlayer.CurrentAnimationPosition;
        }

        public bool IsPlaying()
        {
            return _animationPlayer.IsPlaying();
        }

        public Spatial GetCameraContainer()
        {
            return GetNode<Spatial>("Armature").GetNode<Spatial>("CameraContainer");
        }
    }
}
