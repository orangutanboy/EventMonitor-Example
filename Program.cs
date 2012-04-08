
class Program
{
    static void Main(string[] args)
    {
        var eventMonitor = new EventMonitor();
        var observedObject = new ObservedObjectBuilder(eventMonitor).BuildObservedObject();
        observedObject.RaiseEvent();
        observedObject.RaiseCustomEvent();
        System.Console.WriteLine();
        System.Console.WriteLine("Monitored event count: " + eventMonitor.EventsRaised);
        System.Console.ReadLine();
    }
}

public class EventMonitor
{
    public long EventsRaised { get; private set; }

    private void CountEvent(object sender, System.EventArgs e)
    {
        ++EventsRaised;
    }

    public void RegisterEvents(object eventRaiser)
    {
        //Get the type of the eventraiser object
        var eventRaiserType = eventRaiser.GetType();

        //Extract all events raised by that type
        var events = eventRaiserType.GetEvents();

        foreach (var eventInfo in events)
        {
            //Get the type of delegate required for handling this event
            var eventHandlerType = eventInfo.EventHandlerType;

            var flags = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;

            //Get the method that will be used to handle the event
            var eventHandlerMethod = this.GetType().GetMethod("CountEvent", flags);

            //Create a delegate to the method that will be used to handle the event
            var eventHandler = System.Delegate.CreateDelegate(eventHandlerType, this, eventHandlerMethod);

            //Find the event "add handler" (i.e. the guts of the += operator)
            var addHandler = eventInfo.GetAddMethod();

            //Simulate "event += handler"
            addHandler.Invoke(eventRaiser, new object[] { eventHandler });
        }
    }
}

public class ObservedObject
{
    public class CustomArgs : System.EventArgs
    {
        public bool IsCustom { get { return true; } }
    }

    public event System.EventHandler ObservedEvent;
    public event System.EventHandler<CustomArgs> ObservedCustomEvent;

    public void RaiseEvent()
    {
        if (ObservedEvent != null)
        {
            ObservedEvent(this, System.EventArgs.Empty);
        }
    }

    public void RaiseCustomEvent()
    {
        if (ObservedCustomEvent != null)
        {
            ObservedCustomEvent(this, new CustomArgs());
        }
    }
}

public class ObservedObjectBuilder
{
    private EventMonitor eventMonitor;

    public ObservedObjectBuilder(EventMonitor eventMonitor)
    {
        this.eventMonitor = eventMonitor;
    }

    public ObservedObject BuildObservedObject()
    {
        var observedObject = new ObservedObject();
        eventMonitor.RegisterEvents(observedObject);
        return observedObject;
    }
}

