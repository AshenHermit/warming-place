import traceback
from godot import exposed, export, signal, Object

import sys
from io import StringIO
import contextlib
import types

import gc

def objects_by_id(id_):
	for obj in gc.get_objects():
		if id(obj) == id_:
			return obj
	raise Exception("No found")

def _42vars(obj):
	v = vars(obj)
	exclude = ["__signals", "__exported", "__tool", "__exposed_python_class"]
	v = {k:v[k] for k in v if not k.startswith("_42") and k not in exclude}
	return v

class ExploiterContext():
	def __init__(self) -> None:
		self._42locals = {}
		self.print_func = print

	def exec(self, code:str, add_locals={}):
		code = str(code)

		self._42locals.update(add_locals)
		self._42locals.update({vars: _42vars})
		# self._42locals["_42locals"] = self._42locals
		v = self._42locals
		# v.update(mcontext.globals)

		old = sys.stdout
		olderr = sys.stderr
		sys.stdout = StringIO()
		sys.stderr = sys.stdout
		done = False
		try:
			print(eval(code, v, v))
			done = True
		except:
			pass
		if not done:
			try:
				# code += "\n\n"+"_42locals.update(locals())"
				exec(code, v, v)
			except Exception as err:
				print(err.args[0])
		
		try:
			self.print_func(sys.stdout.getvalue())
		except:
			sys.stdout = old
			sys.stderr = olderr
			traceback.print_exc(1)
			print(sys.stdout.getvalue())
		sys.stdout = old
		sys.stderr = olderr
		

@exposed
class XModulesServer(Object):
	_42locals = {}
	_42stdout_signal = signal()

	def __init__(self) -> None:
		self.xmodules = {}

	def _init(self):
		self.xmodules = {}
		self.exploiter_context = ExploiterContext()
		self.exploiter_context.print_func = self._42print

	def add_xmodule(self, id, xmodule_py_ctx):
		id = str(id)
		mod_py_id = xmodule_py_ctx.call("_get_python_obj_id")
		xmodule_py_ctx = objects_by_id(mod_py_id)
		ctx = xmodule_py_ctx
		
		if xmodule_py_ctx._unit != None:
			ctx = xmodule_py_ctx._unit
		self.xmodules[id] = ctx

	def _42print(self, *args):
		s = " ".join(list(map(str, args)))
		self.call("emit_signal", "_42stdout_signal", s)
	
	def execute_code(self, code):
		code = str(code)
		
		glob = globals()
		loc = locals()

		done = False
		try:
			print(eval(code, glob, loc))
			done = True
		except:
			pass
		
		if not done:
			try:
				code += "\n\n"+"_42locals.update(locals())"
				exec(code, glob, loc)
			except:
				traceback.print_exc(1)

	def execute_code_in_exploiter_context(self, code, available_xmodules_ids):
		modules = types.SimpleNamespace()
		for id in available_xmodules_ids:
			id = str(id)
			setattr(modules, id, self.xmodules[id])
			
		add_locals = {"modules": modules}
			
		self.exploiter_context.exec(code, add_locals)
		

# class ModuleContext():
#     def __init__(self, module_obj, module_globals, connected_object:Object) -> None:
#         self.instance = module_obj
#         self.globals = module_globals
#         self.connected_object = connected_object

#     def register_method(self, instance_method:str, target_method:str):
#         def bridge(*args):
#             self.connected_object.call(target_method, *args)
#         setattr(self.instance, instance_method, bridge)

#     @staticmethod
#     def make_from_source(code:str, module_class_name:str, connected_object:Object):
#         _42globals = {}
#         v = {"_42globals": _42globals}
#         exec(code+"\n\n"+"_42globals.update(locals())", v, v)
#         instance = None
#         if module_class_name in _42globals:
#             instance = _42globals[module_class_name]()
#         context = ModuleContext(instance, _42globals, connected_object)
#         return context

# class ModuleContextsLibrary():
#     def __init__(self) -> None:
#         self.module_contexts:"dict[str, ModuleContext]" = {}

#     def add(self, module_id:str, module_context:ModuleContext):
#         self.module_contexts[module_id] = module_context

#     def get(self, module_id:str):
#         key = str(module_id)
#         if key in self.module_contexts:
#             return self.module_contexts[key]
#         return None

#     def get_unique_module_id(self, module_id:str):
#         key = str(module_id)
#         i = 0
#         while key in self.module_contexts:
#             i+=1
#             key = str(module_id)+str(i)
#         return key

# @exposed
# class XModulesServer(Object):
#     _42stdout_signal = signal()
#     _42locals = {}

#     _selected_module_uid:str = ""

#     modules_lib:ModuleContextsLibrary = ModuleContextsLibrary()

#     def _42print(self, *args):
#         s = " ".join(list(map(str, args)))
#         self.call("emit_signal", "_42stdout_signal", s)

#     def _42vars(self, obj):
#         v = vars(obj)
#         exclude = ["__signals", "__exported", "__tool", "__exposed_python_class"]
#         v = {k:v[k] for k in v if not k.startswith("_42") and k not in exclude}
#         return v

#     def make_module(self, module_code:str, module_class_name:str, module_id:str):
#         key = self.modules_lib.get_unique_module_id(module_id)

#         context = ModuleContext.make_from_source(module_code, module_class_name)
#         self.modules_lib.add(key, context)
#         self.call_deferred("init_module", key)

#         return key
		
#     def init_module(self, module_id:str):
#         mcontext = self.modules_lib.get(module_id)
#         if mcontext is None: return None
#         mcontext.instance._init()

#     def register_method(self, module_id:str, instance_method:str, target_method:str):
#         mcontext = self.modules_lib.get(module_id)
#         if mcontext is None: return None

#     def exec(self, code:str, module_id:str):
#         code = str(code)
#         mcontext = self.modules_lib.get(module_id)
#         if mcontext is None: return None

#         self._42locals.update({self._selected_module_uid: mcontext.instance, "self": mcontext.instance})
#         self._42locals["_42locals"] = self._42locals
#         v = self._42locals
#         # v.update(mcontext.globals)

#         old = sys.stdout
#         olderr = sys.stderr
#         sys.stdout = StringIO()
#         sys.stderr = sys.stdout
#         done = False
#         try:
#             print(eval(code, v, v))
#             done = True
#         except:
#             pass
#         if not done:
#             try:
#                 code += "\n\n"+"_42locals.update(locals())"
#                 exec(code, v, v)
#             except:
#                 traceback.print_exc(1)
#         try:
#             self._42print(sys.stdout.getvalue())
#         except:
#             sys.stdout = old
#             sys.stderr = olderr
