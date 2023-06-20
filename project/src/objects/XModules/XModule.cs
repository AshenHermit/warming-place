using Godot;
using System;

namespace Game
{
    public class XModule : Spatial
    {
        [Export]
        public string Id;

        [Export]
        public Godot.Object PythonScriptRes;
        public Godot.Object PyContext;

        public XModule()
        {
            
        }
        public override void _Ready()
        {
            PyContextInit();
            PyContext?.Call("_ready");
        }
        protected virtual void PyContextInit()
        {
            if(PythonScriptRes is null) 
            {
                GD.PrintErr(String.Format("PythonScriptRes of object {0} is not set", Name.ToString()));
                return;
            };
            PyContext = (Godot.Object)PythonScriptRes.Call("new");
            PyContext?.Call("_init");
            Global.Instance.GetXModulesServer().AddXModule(this);
        }
        public override void _ExitTree()
        {
            PyContext?.Call("_destroy");
            PyContext?.Free();
            base._EnterTree();
        }
    }
}
