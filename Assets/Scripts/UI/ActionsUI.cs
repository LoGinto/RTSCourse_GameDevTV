using System.Collections.Generic;
using System.Linq;
using GameDevTV.RTS.Commands;
using GameDevTV.RTS.EventBus;
using GameDevTV.RTS.Events;
using GameDevTV.RTS.Units;
using UnityEngine;

namespace GameDevTV.RTS.UI
{
    public class ActionsUI : MonoBehaviour
    {
        [SerializeField] UIActionButton[] actionButtons;
        HashSet<AbstractCommandable> selectedUnits = new(12);

        void Awake()
        {
             Bus<UnitSelectedEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent += HandleUnitDeselected;
            foreach(UIActionButton button in actionButtons)
            {
                button.SetIcon(null);
            }
        }

        private void HandleUnitDeselected(UnitDeselectedEvent args)
        {
            if(args.Unit is AbstractCommandable commandable){
                selectedUnits.Remove(commandable);
                RefreshButtons();
            }
        }

        private void RefreshButtons()
        {
            HashSet<ActionBase> availableCommands = new HashSet<ActionBase>(9);
            foreach(AbstractCommandable commandable in selectedUnits)
            {
                availableCommands.UnionWith(commandable.AvailableCommands);
            }
            for(int i = 0; i < actionButtons.Length; i++)
            {
                ActionBase actionForSlot = availableCommands.Where(action=>action.Slot==i).FirstOrDefault();
                if(actionForSlot != null)
                {
                    actionButtons[i].SetIcon(actionForSlot.Icon);
                }
                else
                {
                    actionButtons[i].SetIcon(null);
                }
            }
        }

        private void HandleUnitSelected(UnitSelectedEvent args)
        {
             if(args.Unit is AbstractCommandable commandable){
                selectedUnits.Add(commandable);
                RefreshButtons();
            }
        }

        void Oestroy()
        {
             Bus<UnitSelectedEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeselectedEvent>.OnEvent -= HandleUnitDeselected;
        }
    }
}