using UnityEngine.EventSystems;
using PikePush.Utls;


namespace PikePush.Controls {

    public class GestureControlsSimple : ControlScheme
    {

        // Method called on pointer down event
        public void OnPointerDown(BaseEventData eventData)
        {
            LogHelper.debug($"[GestureControlsSimple][OnPointerDown]: {eventData.selectedObject.name}");
            this.controls[eventData.selectedObject.name] = true;
        }

        // Method called on pointer up event
        public void OnPointerUp(BaseEventData eventData)
        {
            LogHelper.debug($"[GestureControlsSimple][OnPointerUp]: {eventData.selectedObject.name}");
            this.controls[eventData.selectedObject.name] = false;
        }

        public void SwipeStart(BaseEventData eventData)
        {

        }

        public void SwipeEnd(BaseEventData eventData)
        {

        }
    }
}