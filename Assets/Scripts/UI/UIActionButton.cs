using UnityEngine.UI;
using UnityEngine;

namespace GameDevTV.RTS.UI
{
    public class UIActionButton : MonoBehaviour
    {
        [SerializeField] Image icon;

        public void SetIcon(Sprite icon)
        {
            if(icon == null)
            {
                this.icon.enabled = false;
                return;
            }
            this.icon.enabled = true;
            this.icon.sprite = icon;
        }
    }
}