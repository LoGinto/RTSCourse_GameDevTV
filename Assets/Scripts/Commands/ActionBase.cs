using GameDevTV.RTS.Units;
using UnityEngine;

namespace GameDevTV.RTS.Commands
{
    public abstract class ActionBase : ScriptableObject, ICommand
    {
        [field: SerializeField] public Sprite Icon{get;private set;}
        [field:Range(0,8)][field: SerializeField] public int Slot{get; private set;}
        [field:SerializeField] public bool RequiresClickToActivate{get;private set;} = true;
        public abstract bool CanHandle(CommandContext context);

        public abstract void Handle(CommandContext context);
    }
}