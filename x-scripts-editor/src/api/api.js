import { EventHandler } from "event-js";
import { Exportable, FC } from "extripo";
import { Event, EventForceConsoleContentFetch, EventsPackage, TerminalEventsProvider } from "./events";

export class EventsHandlers{
    constructor(){
        this.on_console_content_change = new EventHandler(this);
    }
}



export class Api{
    constructor(){
        this.eventsHandlers = new EventsHandlers()
        this.serverURL = "http://localhost:8080"
        this.listeningInterval = 100
        this.terminalEventsProvider = new TerminalEventsProvider()
    }

    pushEvent(event){
        this.eventsPackage.events.push(event)
    }

    recieveEventsPackage(eventsPackage){
        if(eventsPackage.events.length>0){
            console.log(eventsPackage)
        }
        eventsPackage.events.forEach(e => {
            switch (e.type) {
                case "console_content_change":
                    this.eventsHandlers.on_console_content_change.publish(e.data)
                    break;
                
                default:
                    break;
            }
        });
    }

    // может быть на socket io перейду, хз
    async postMessage(type, data){
        try {
            const response = await fetch(this.serverURL+"/"+type, {
                method: 'POST',
                mode: 'cors',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify(data)
            });
        
            if (!response.ok) {
                throw new Error(`Ошибка при отправке запроса: ${response.status}`);
            }
        
            const rdata = await response.json();
            return rdata
            
        } catch (error) {
            console.error('Ошибка при отправке запроса:', error);
        }
    }
    
    async requestEventSharing() {
        if(this.terminalEventsProvider.eventsPackage.events.length>0){
            console.log(this.terminalEventsProvider.eventsPackage.exportData())
        }
        var data = await this.postMessage("post-event-sharing", this.terminalEventsProvider.eventsPackage.exportData())
        this.terminalEventsProvider.eventsPackage.clear()
        if(!data) return
        var recievedPackage = EventsPackage.create(data)
        this.recieveEventsPackage(recievedPackage)
    }
    
    async eventsListener() {
        while (true) {
            await this.requestEventSharing();
            await new Promise(resolve => setTimeout(resolve, this.listeningInterval));
        }
    }

    startApiWorker(){
        this.eventsListener()
        this.terminalEventsProvider.forceConsoleContentFetch()
    }
}