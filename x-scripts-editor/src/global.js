import { EventHandler } from "event-js"
import { Api } from "./api/api"



class Global{
    constructor(){
        this.api = new Api()
    }

    runStartProcesses(){
        this.api.startApiWorker()
    }
}

export var global = new Global()
window.global = global