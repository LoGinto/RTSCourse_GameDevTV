using UnityEngine;
using GameDevTV.RTS.EventBus;
using GameDevTV.RTS.Events;

namespace GameDevTV.RTS.Units{
    public class AbstractCommandable : MonoBehaviour, ISelectable
    {
        [field: SerializeField] public int CurrentHealth { get; private set; }
        [field: SerializeField] public int MaxHealth { get; private set; }
        [SerializeField] private UnitySO UnitSo;
        [SerializeField] GameObject decalProjector;


        protected virtual void Start()
        {
            CurrentHealth = UnitSo.Health;
            MaxHealth = UnitSo.Health;
        }

        public void Deselect()
        {
           //Debug.Log($"{name} DESELECT, decal active after = {decalProjector.activeSelf}", this);
            decalProjector.SetActive(false);
           Bus<UnitDeselectedEvent>.Raise(new UnitDeselectedEvent(this));
        }
         public void Select()
        {
           // Debug.Log($"{name} SELECT", this);
           decalProjector.SetActive(true);
           Bus<UnitSelectedEvent>.Raise(new UnitSelectedEvent(this));
        }
    }
}