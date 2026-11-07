using UnityEngine;
using UnityEngine.AI;

namespace GameDevTV.RTS.Units{
[RequireComponent(typeof(NavMeshAgent))]
public class Worker : MonoBehaviour, ISelectable
{
    [SerializeField] Transform target; 
    [SerializeField] GameObject decalProjector;
    NavMeshAgent agent;

        public void Deselect()
        {
            Debug.Log($"{name} DESELECT, decal active after = {decalProjector.activeSelf}", this);
            decalProjector.SetActive(false);
        }

        public void Select()
        {
            Debug.Log($"{name} SELECT", this);
           decalProjector.SetActive(true);
        }

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
        }
        void Update()
        { 
            if(target != null)
            {
                agent.SetDestination(target.position);
            }
        }
    }
}