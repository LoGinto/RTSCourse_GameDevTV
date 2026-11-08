using GameDevTV.RTS.EventBus;
using GameDevTV.RTS.Units;

namespace GameDevTV.RTS.Events
{
    public struct UnitpSpawnedEvent : IEvent
    {
        public AbstractUnit Unit {get;private set;} 

        public UnitpSpawnedEvent(AbstractUnit unit)
        {
            Unit = unit;
        }
    }
}