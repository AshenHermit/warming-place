using Godot;
using System;

namespace Game
{
    public class PlayerCamera : Camera, IPlayerCamera, IPlayerComponent
    {
        [Export]
        public NodePath PlayerNodePath;
        [Export]
        public float MouseSensivity = 1.0f;

        Vector3 _startPosition;

        // player instance
        Player _player;

        // private fields
        bool _activeState = false;

        Vector2 _mouseMotion;
        public Vector2 GetMouseMotion() => _mouseMotion;
        bool _gotMotion;

        bool _rotationAbility = true;
        public void SetRotationAbility(bool ability) { _rotationAbility = ability; }

        public Spatial StunLookAtObject = null;

        // cutscene
        Spatial _cutsceneCameraContainer;
        Cutscene _cutsceneInstance;
        bool _cutsceneBlending = false;
        float _cutsceneTransition = 0.0f;
        float _cutsceneTransitionSpeed = 2.0f;
        public bool IsCutscenePlaying() => _cutsceneInstance != null;

        float _noiseTimer = 0.0f;
        float _shakeAmplitude = 0.0f;
        float _constantShakingAmount = 0.0f;
        Spatial _shakeContainer;
        FastNoiseLite _noise = new FastNoiseLite();

        Vector3 _headMotion;
        private float _initalFov;

        public float FlexTransformTransition = 0.0f;
        private Transform _flexTransform;
        public Transform FlexTransform {
            get {
                return _flexTransform;
            }
            set
            {
                _flexTransform = value;
            }
        }
        private Transform SecondaryTransform;

        private Tween TweenNode;

        // methods
        /// <summary>
        /// player component setup
        /// </summary>
        /// <param name="player">player instance</param>
        public void Setup(Player player)
        {
            _player = player;
            MakeCurrent();
        }
        /// <summary>
        /// node ready
        /// </summary>
        public override void _Ready()
        {
            _initalFov = Fov;

            TweenNode = new Tween();
            AddChild(TweenNode);

            _flexTransform = Transform;
            SecondaryTransform = Transform;

            Setup(GetNode<Player>(PlayerNodePath));
            Global.Instance.RegisterPlayerCamera(this);

            SetMouseSensivity(100.0f);
            _startPosition = Transform.origin;

            _noise.Seed = Mathf.FloorToInt(GD.Randf() * 1000000.0f);
            _noise.Period = 0.15f;

            _shakeContainer = GetParent<Spatial>();
        }

        public void SetMouseSensivity(float percents)
        {
            MouseSensivity = percents / 100.0f * 0.006f;
        }
        public override void _Process(float delta)
        {
            if (!_player.IsAlive()) return;

            if(!_gotMotion)_mouseMotion = Vector2.Zero;
            _gotMotion = false;

            if (_cutsceneInstance != null)
            {
                UpdateCutscene();
                //FlexTransformTransition = Mathf.Min(FlexTransformTransition + delta * _cutsceneTransitionSpeed, 1.0f);
            }
            else
            {
                UpdateShake(delta);
                UpdateStunnedState(delta);
                AnimateHead();

                if (_activeState)
                {
                    RotateByDelta(_headMotion.y, _headMotion.x);
                }
                _headMotion /= 2.0f;
                //FlexTransformTransition = Mathf.Max(FlexTransformTransition - delta * _cutsceneTransitionSpeed, 0.0f);
            }
            UpdateFlexTransform(delta);
        }
        private void UpdateFlexTransform(float delta)
        {
            Transform = FlexTransform.InterpolateWith(SecondaryTransform, FlexTransformTransition);
        }
        public void Shake(float amount)
        {
            _shakeAmplitude += amount;
        }
        public void StartShakingConstantly(float amount)
        {
            _constantShakingAmount = amount;
        }
        public void StopShakingConstantly()
        {
            _constantShakingAmount = 0.0f;
        }
        void UpdateShake(float delta)
        {
            if (_constantShakingAmount>0.0f) Shake(_constantShakingAmount);
            _noiseTimer += delta;
            _shakeAmplitude /= 1.4f;
            Transform trans = _shakeContainer.Transform;
            trans.origin = new Vector3(_noise.GetNoise2d(-100.0f, _noiseTimer), _noise.GetNoise2d(0.0f, _noiseTimer), _noise.GetNoise2d(100.0f, _noiseTimer))
                *_shakeAmplitude;
            _shakeContainer.Transform = trans;
        }
        void UpdateStunnedState(float delta)
        {
            if (StunLookAtObject != null && !IsCutscenePlaying())
            {
                Transform trans = GlobalTransform;
                trans.basis = new Basis(new Quat(trans.basis).Slerpni(new Quat(trans.LookingAt(StunLookAtObject.GlobalTransform.origin, Vector3.Up).basis), 0.01f));
                GlobalTransform = trans;
            }
        }
        /// <summary>
        /// captures mouse motion
        /// </summary>
        public override void _Input(InputEvent @event)
        {
            if (!_player.IsAlive()) return;
            if (!_activeState) return;
            if (IsCutscenePlaying()) return;

            if (@event is InputEventMouseMotion eventMotion)
            {
                ProcessMouseMotion(eventMotion);
            }
            else if (@event is InputEventMouseButton eventButton)
            {
                ProcessMouseButton(eventButton);
            }
        }
        void ProcessMouseMotion(InputEventMouseMotion eventMotion)
        {
            _mouseMotion = eventMotion.Relative;
            _gotMotion = true;

            if (!_rotationAbility) return;

            _headMotion.y += eventMotion.Relative.y * MouseSensivity * 0.65f;
            _headMotion.x += eventMotion.Relative.x * MouseSensivity * 0.65f;
        }
        void ProcessMouseButton(InputEventMouseButton eventButton)
        {

        }

        public void AnimateHead()
        {
            if (_player.Steps > 0.0f)
            {
                Transform trans = FlexTransform;
                trans.origin = Vector3.Zero;
                trans.origin = _startPosition + new Vector3(
                    Mathf.Cos(_player.Steps/2.0f) * 0.1f, 
                    Mathf.Sin(_player.Steps) * 0.15f, 
                    0.0f);
                FlexTransform = trans;
            }
        }

        public void PlayCutscene(string cutsceneName, bool blend=false)
        {
            PackedScene cutscene = GD.Load<PackedScene>(Global.cutscenesDirectory + cutsceneName + ".tscn");
            _cutsceneInstance = cutscene.Instance<Cutscene>();
            GetParent().AddChild(_cutsceneInstance);
            _cutsceneInstance.Transform = Transform.Identity;
            PlayCutscene(_cutsceneInstance, blend);
        }
        public void PlayCutscene(Cutscene cutsceneInstance, bool blend=false)
        {
            _cutsceneInstance = cutsceneInstance;
            _cutsceneBlending = blend;
            _cutsceneInstance.Start();
            _cutsceneCameraContainer = _cutsceneInstance.GetCameraContainer();

            _player.Disable();
            _player.SetWeapon(null, null);
            _cutsceneTransition = 0.0f;

            TweenNode.InterpolateProperty(this, 
                "FlexTransformTransition", 
                FlexTransformTransition, 1.0f, 1.0f / _cutsceneInstance.TransitionSpeed,
                Tween.TransitionType.Linear, Tween.EaseType.InOut);
            TweenNode.Start();
        }
        public void UpdateCutscene()
        {
            if (_cutsceneInstance == null) return;

            SecondaryTransform = (GetParent<Spatial>().GlobalTransform.AffineInverse() * _cutsceneCameraContainer.GlobalTransform);

            if (!_cutsceneInstance.IsPlaying())
            {
                TweenNode.InterpolateProperty(this,
                    "FlexTransformTransition",
                    FlexTransformTransition, 0.0f, 1.0f / _cutsceneInstance.TransitionSpeed,
                    Tween.TransitionType.Quad, Tween.EaseType.InOut);
                TweenNode.Start();

                _cutsceneInstance = null;
                _player.Enable();
            }
        }

        /// <summary>
        /// rotateCamera with angles in radians
        /// </summary>
        /// <param name="deltaX">X axis angle in radians</param>
        /// <param name="deltaY">Y axis angle in radians</param>
        public void RotateByDelta(float deltaX, float deltaY)
        {   
            Transform trans = FlexTransform;
            Vector3 rotation = FlexTransform.basis.Quat().GetEuler();
            rotation.x = Mathf.Clamp(rotation.x - deltaX, -Mathf.Pi / 2.0f + Mathf.Pi / 32.0f, Mathf.Pi / 2.0f - Mathf.Pi / 32.0f);
            trans.basis = new Basis(rotation);
            FlexTransform = trans;
            //GDE.Print(rotation.x);
            _player.RotateY(-deltaY);
        }
        /// <summary>
        /// activate or deactivate camera, sets mouse mode
        /// </summary>
        /// <param name="activeState">new active state</param>
        public void SetActiveState(bool activeState)
        {
            if (activeState && _player.Disabled) {
                Input.SetMouseMode(Input.MouseMode.Captured);
                return;
            };

            _activeState = activeState;
            Input.SetMouseMode(_activeState ? Input.MouseMode.Captured : Input.MouseMode.Visible);
            if(_activeState) MakeCurrent();
        }
        /// <summary>
        /// check if active
        /// </summary>
        public bool IsActive() { return _activeState; }
        /// <summary>
        /// Intersects a ray in a given space. The returned object is a dictionary with the
        ///     following fields:  <br/>
        ///     collider: The colliding object.  <br/>
        ///     collider_id: The colliding object's ID.  <br/>
        ///     normal: The object's surface normal at the intersection point.  <br/>
        ///     position: The intersection point.   <br/>
        ///     rid: The intersecting object's Godot.RID.   <br/>
        ///     shape: The shape index of the colliding shape.  <br/>
        ///     If the ray did not intersect anything, then an empty dictionary is returned instead.  <br/>
        ///     Additionally, the method can take an exclude array of objects or Godot.RIDs that  <br/>
        ///     are to be excluded from collisions, a collision_mask bitmask representing the  <br/>
        ///     physics layers to check in, or booleans to determine if the ray should collide  <br/>
        ///     with Godot.PhysicsBodys or Godot.Areas, respectively.  <br/>
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="collisionMask"></param>
        /// <returns></returns>
        public Godot.Collections.Dictionary CastRay(float distance, uint collisionMask=1)
        {
            return GetWorld().DirectSpaceState.IntersectRay(
                GlobalTransform.origin,
                GlobalTransform.origin - GlobalTransform.basis.z * distance,
                new Godot.Collections.Array { _player }, collisionMask);
        }
    }
}