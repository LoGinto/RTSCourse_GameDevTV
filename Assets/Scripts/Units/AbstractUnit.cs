using GameDevTV.RTS.EventBus;
using GameDevTV.RTS.Events;
using UnityEngine;
using UnityEngine.AI;

namespace GameDevTV.RTS.Units
{
    [RequireComponent(typeof(NavMeshAgent))]

    public abstract class AbstractUnit : MonoBehaviour, ISelectable, IMovable
    {
        [SerializeField] GameObject decalProjector;
        NavMeshAgent agent;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }
        void Start()
        {
            Bus<UnitpSpawnedEvent>.Raise(new UnitpSpawnedEvent(this));//uses abstract unit
        }

        public void Deselect()
        {
           //Debug.Log($"{name} DESELECT, decal active after = {decalProjector.activeSelf}", this);
            decalProjector.SetActive(false);
           Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
        }

        public void MoveTo(Vector3 position)
        {
            agent.SetDestination(position);
        }

        public void Select()
        {
           // Debug.Log($"{name} SELECT", this);
           decalProjector.SetActive(true);
           Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }
    }
}