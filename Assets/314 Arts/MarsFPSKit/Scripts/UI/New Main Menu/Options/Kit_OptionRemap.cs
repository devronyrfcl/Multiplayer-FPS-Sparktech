using MarsFPSKit.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MarsFPSKit
{
    namespace UI
    {
        public class Kit_OptionRemap : MonoBehaviour, IPointerEnterHandler
        {
            public TextMeshProUGUI title;

            public TextMeshProUGUI value;

            public Button btn;

            public UnityEvent onHover = new UnityEvent();

            public Kit_MenuOptions options;

            void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
            {
                onHover.Invoke();
            }
        }
    }
}