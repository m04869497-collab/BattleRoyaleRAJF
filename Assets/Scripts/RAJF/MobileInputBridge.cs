using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RAJF
{
    public sealed class MobileInputBridge : MonoBehaviour
    {
        private bool up;
        private bool down;
        private bool left;
        private bool right;
        private bool fireHeld;
        private bool abilityRequested;

        public Vector2 MoveInput
        {
            get
            {
                Vector2 value = new Vector2((right ? 1f : 0f) - (left ? 1f : 0f),
                    (up ? 1f : 0f) - (down ? 1f : 0f));
                return Vector2.ClampMagnitude(value, 1f);
            }
        }
        public bool FireHeld => fireHeld;

        public void BindMoveButton(Button button, int direction)
        {
            if (button == null)
                return;
            AddPointerEvent(button, EventTriggerType.PointerDown, _ => SetDirection(direction, true));
            AddPointerEvent(button, EventTriggerType.PointerUp, _ => SetDirection(direction, false));
            AddPointerEvent(button, EventTriggerType.PointerExit, _ => SetDirection(direction, false));
        }

        public void BindFireButton(Button button)
        {
            if (button == null)
                return;
            AddPointerEvent(button, EventTriggerType.PointerDown, _ => fireHeld = true);
            AddPointerEvent(button, EventTriggerType.PointerUp, _ => fireHeld = false);
            AddPointerEvent(button, EventTriggerType.PointerExit, _ => fireHeld = false);
        }

        public void RequestAbility()
        {
            abilityRequested = true;
        }

        public bool ConsumeAbilityRequest()
        {
            bool requested = abilityRequested;
            abilityRequested = false;
            return requested;
        }

        public void ResetInput()
        {
            up = down = left = right = fireHeld = abilityRequested = false;
        }

        private void SetDirection(int direction, bool pressed)
        {
            switch (direction)
            {
                case 0: up = pressed; break;
                case 1: down = pressed; break;
                case 2: left = pressed; break;
                case 3: right = pressed; break;
            }
        }

        private static void AddPointerEvent(Button button, EventTriggerType eventType,
            UnityAction<BaseEventData> action)
        {
            EventTrigger trigger = button.GetComponent<EventTrigger>();
            if (trigger == null)
                trigger = button.gameObject.AddComponent<EventTrigger>();

            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = eventType };
            entry.callback.AddListener(action);
            trigger.triggers.Add(entry);
        }
    }
}
