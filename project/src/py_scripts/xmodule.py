from godot import exposed, export
from godot import *
import godot

class Unit():
    def __repr__(self) -> str:
        return "<"+self.__class__.__name__+" object>"

@exposed
class XModule(Object):

    def _init(self):
        self._id = "none"
        self._unit = None
        pass

    def _destroy(self):
        pass

    def _get_python_obj_id(self):
        return id(self)