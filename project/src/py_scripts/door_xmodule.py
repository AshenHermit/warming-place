from godot import exposed, export
from godot import *
from .xmodule import XModule

class DoorXModule(XModule):

    _42open_signal = signal()
    _42close_signal = signal()

    def _init(self):
        super()._init()

    def open(self):
        self.call("emit_signal", "_42open_signal")
    def close(self):
        self.call("emit_signal", "_42close_signal")

class door():
    pass