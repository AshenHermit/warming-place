import { Exportable, FC } from "extripo";

export class Event extends Exportable{
    constructor(){
        super()
        this.type = ""
        this.data = ""
    }
    static new(){
        return new this()
    }
}

export class EventForceConsoleContentFetch extends Event{
    constructor(){
        super()
        this.type = "force_console_content_fetch"
    }
}
export class EventRunCode extends Event{
    constructor(code=""){
        super()
        this.type = "run_code"
        this.data = code
    }
}

export class EventsPackage extends Exportable{
    constructor(){
        super()
        this.configFields({
            events: FC.arrayOf(Event),
        })
        this.events = []
    }
    clear(){
        this.events.splice(0, this.events.length)
    }
    addEvent(e){
        this.events.push(e)
    }
}

export class TerminalEventsProvider{
    constructor(){
        this.eventsPackage = new EventsPackage()
    }
    forceConsoleContentFetch(){
        this.eventsPackage.addEvent(EventForceConsoleContentFetch.new())
    }
    runCode(code){
        this.eventsPackage.addEvent(new EventRunCode(code))
    }
}