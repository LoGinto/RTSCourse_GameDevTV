using GameDevTV.RTS.EventBus;
using GameDevTV.RTS.Events;
using UnityEngine;
using UnityEngine.AI;

namespace GameDevTV.RTS.Units{
[RequireComponent(typeof(NavMeshAgent))]
public class Worker : MonoBehaviour, ISelectable,IMovable,IEvent
{
    [SerializeField] GameObject decalProjector;
    NavMeshAgent agent;

    void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }

        public void Deselect()
        {
           // Debug.Log($"{name} DESELECT, decal active after = {decalProjector.activeSelf}", this);
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