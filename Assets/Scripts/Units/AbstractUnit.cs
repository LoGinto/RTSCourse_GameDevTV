using GameDevTV.RTS.EventBus;
using GameDevTV.RTS.Events;
using UnityEngine;
using UnityEngine.AI;

namespace GameDevTV.RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent))]

    public abstract class AbstractUnit : AbstractCommandable,IMovable
    {
        
        NavMeshAgent agent;
        public float AgentRadius => agent.radius;
        

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }
        protected override void Start()
        {
            base.Start();
            Bus<UnitpSpawnedEvent>.Raise(new UnitpSpawnedEvent(this));//uses abstract unit
        }

        

        public void MoveTo(Vector3 position)
        {
            agent.SetDestination(position);
        }
    }
}