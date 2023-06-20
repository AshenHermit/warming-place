using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Text;
using System.IO;
using Game.Utils;
using System.Text.RegularExpressions;
using Newtonsoft.Json;

namespace Terminal
{
    public class Event
    {
        public string type = "";
        public string data = "";

        public Event()
        {
            type = "";
            data = "";
        }
        public Event(string t, string d)
        {
            type = t;
            data = d;
        }

        public List<Event> processEventAndResponse()
        {
            List<Event> events = new List<Event>();
            switch (type)
            {
                case "force_console_content_fetch":
                    events.Add(MakeConsoleContentChangeEvent());
                    break;
                case "run_code":
                    RunCode(data);
                    break;
                default: break;
            }
            return events;
        }

        public static Event MakeConsoleContentChangeEvent()
        {
            return new Event("console_content_change", Game.UI.MacroExploiterWindow.Instance.ConsoleContent);
        }
        public static void RunCode(string code)
        {
            Game.UI.MacroExploiterWindow.Instance.ExecuteCode(code);
        }
    }

    public class EventsPackage
    {
        public List<Event> events = new List<Event>();

        public EventsPackage()
        {
            
        }

        public void ClearEvents()
        {
            events.Clear();
        }

        public List<Event> ProcessEventsAndReply()
        {
            List<Event> revents = new List<Event>();
            foreach (Event e in events)
            {
                if(e != null)
                {
                    var newEvents = e.processEventAndResponse();
                    if (newEvents!=null) revents.AddRange(newEvents);
                }
            }
            return revents;
        }
        public void ProcessEventsAndGatherInSelf(EventsPackage pck)
        {
            List<Event> revents = pck.ProcessEventsAndReply();
            events.AddRange(revents);
        }

        public static EventsPackage FromJSON(string json)
        {
            return JsonConvert.DeserializeObject<EventsPackage>(json);
        }
        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this, Formatting.None);
        }
    }
    public class GameEventsProvider
    {
        public EventsPackage eventsPackage;

        public GameEventsProvider()
        {
            eventsPackage = new EventsPackage();
        }
        
        public void ProcessEventsAndGatherInSelf(EventsPackage pck)
        {
            pck.ProcessEventsAndGatherInSelf(pck);
        }

        public void Initialize()
        {
            Game.UI.MacroExploiterWindow.Instance.OnConsoleContentChange += ConsoleContentChange;
        }

        public void ConsoleContentChange(string content)
        {
            eventsPackage.events.Add(Event.MakeConsoleContentChangeEvent());
        }
    }
}
