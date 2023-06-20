using System;
using Game.Utils;
using Godot;
using System.Collections.Generic;

namespace Game
{
	public class XModulesServer : Node
	{
		[Export]
		public Godot.Object ServerPythonScriptRes;

		private Dictionary<string, XModule> _modules = new Dictionary<string, XModule>();
		private Godot.Object _serverPythonCtx = null;

		public delegate void StdoutFunc(string data);
		public event StdoutFunc OnStdout;

		public XModulesServer()
		{
			Global.Instance.RegisterXModulesServer(this);
		}

		private void InitPyContext()
        {
			if(ServerPythonScriptRes == null)
            {
				GD.PrintErr("ServerPythonScriptRes is not set");
				GD.PrintErr(Name);
				return;
            }
			_serverPythonCtx = (Godot.Object)ServerPythonScriptRes.Call("new");
			_serverPythonCtx.Connect("_42stdout_signal", this, "_OnStdout");
			_serverPythonCtx.Call("_init");

		}

		public override void _Ready()
        {
			InitPyContext();
		}


		////// briefly comment this function in c#
		// regenerate comments later

		/// <summary>
		/// Adds an XModule object to the dictionary of modules in C# and Python XModulesServer with additional ID processing. <br/>
		/// before calling this method, XModule object python instance must be initiated.
		/// </summary>
		/// <param name="module">The XModule object to add.</param>
		public void AddXModule(XModule module)
        {
			module.Id = module.Id.ToLower().Replace(" ", "_").Replace("-", "_");
			if (_modules.ContainsKey(module.Id))
            {
				string newKey = "";
				for (int i = 2; true; i++)
                {
					newKey = module.Id + i.ToString();
					if (!_modules.ContainsKey(newKey))
                    {
						module.Id = newKey;
						break;
					}
				}
			}
			_serverPythonCtx?.Call("add_xmodule", module.Id, module.PyContext);
			module.PyContext?.Set("_id", module.Id);
			_modules.Add(module.Id, module);
		}

		/// <summary>
		/// Retrieves an XModule object based on its ID.
		/// </summary>
		/// <param name="module">The XModule object with the ID to search for.</param>
		/// <returns>
		/// The XModule object with the matching ID if found; otherwise, null.
		/// </returns>
		public XModule GetXModule(XModule module)
		{
			if (_modules.ContainsKey(module.Id))
			{
				return _modules[module.Id];
			}
			return null;
		}

		/// <summary>
		/// Removes an XModule object from the dictionary of modules based on its ID.
		/// </summary>
		/// <param name="module">The XModule object to remove.</param>
		public void RemoveXModule(XModule module)
        {
			if (_modules.ContainsKey(module.Id))
			{
				_modules.Remove(module.Id);
			}
		}

		public void _OnStdout(string data)
		{
			OnStdout?.Invoke(data);
		}

		public void ExecuteCode(string code)
        {
			_serverPythonCtx?.Call("execute_code", code);
		}
		public void ExecuteInExploiterContext(string code, IEnumerable<string> modulesIds)
		{
			Godot.Collections.Array<string> ids = new Godot.Collections.Array<string>(modulesIds);
			_serverPythonCtx?.Call("execute_code_in_exploiter_context", code, ids);
		}
		public void ExecuteInExploiterContext(string code, IEnumerable<XModule> modulesIds)
		{
			List<string> ids = new List<string>();
			foreach(XModule module in modulesIds)
            {
				ids.Add(module.Id);
			}
			ExecuteInExploiterContext(code, ids);
		}
	}
}
