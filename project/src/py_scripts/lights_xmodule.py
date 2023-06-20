from godot import exposed, export
from godot import *
import godot
from .xmodule import XModule, Unit

class lights_unit(Unit):
    def __init__(self) -> None:
        self.x = 32
    
    def print_keke(self):
        print("no i dont wanna print this")
        
    def __repr__(self) -> str:
        return "<"+self.__class__.__name__+" object>"

@exposed
class lights_xmodule(XModule):
    a = 3
    b = "keke lol"

    def _init(self):
        super()._init()
        self._unit = lights_unit()
        print("py script initialized")
        print(f"{self._id}: {self.b} -- {self.a}")
        pass

    def _ready(self):
        print("py script ready")
        pass
