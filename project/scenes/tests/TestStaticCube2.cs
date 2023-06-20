using Godot;
using System;

namespace Game
{
    public class TestStaticCube2 : StaticBody, IUsable
    {
        private bool _usable = true;

        [Export]
        public NodePath CutsceneInstancePath;
        Cutscene CutsceneInstance { get { return GetNode<Cutscene>(CutsceneInstancePath); } }


        public Spatial GetUseInfoPoint()
        {
            return this;
        }

        public string GetUseText()
        {
            return "cutscene test";
        }

        public bool IsUsable()
        {
            return _usable;
        }

        public void SetUsableState(bool usable)
        {
            _usable = usable;
        }

        public void Use(Node invoker)
        {
            if (CutsceneInstance != null)
            {
                Global.Instance.GetPlayerCamera().PlayCutscene(CutsceneInstance, true);
            }
        }

        // Declare member variables here. Examples:
        // private int a = 2;
        // private string b = "text";

        // Called when the node enters the scene tree for the first time.
        public override void _Ready()
        {
        
        }

        //  // Called every frame. 'delta' is the elapsed time since the previous frame.
        //  public override void _Process(float delta)
        //  {
        //      
        //  }
    }
}

