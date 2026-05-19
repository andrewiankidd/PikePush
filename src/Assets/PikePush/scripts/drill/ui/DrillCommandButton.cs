using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PikePush.Drill.UI
{
    public class DrillCommandButton : MonoBehaviour, IPointerClickHandler
    {
        Text labelText;
        Text hintText;
        DrillCommand command;
        Action<DrillCommand> onPressed;

        public static DrillCommandButton Build(Transform parent, DrillCommand cmd, string label, KeyCode hintKey,
            Font font, Action<DrillCommand> onPressed)
        {
            var go = new GameObject($"Button_{cmd}");
            go.transform.SetParent(parent, false);

            var img = go.AddComponent<Image>();
            img.color = new Color(0.18f, 0.18f, 0.22f, 0.95f);
            img.raycastTarget = true;

            var labelGo = new GameObject("Label");
            labelGo.transform.SetParent(go.transform, false);
            var labelRect = labelGo.AddComponent<RectTransform>();
            labelRect.anchorMin = new Vector2(0f, 0.4f);
            labelRect.anchorMax = new Vector2(1f, 1f);
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var labelTxt = labelGo.AddComponent<Text>();
            labelTxt.font = font;
            labelTxt.alignment = TextAnchor.MiddleCenter;
            labelTxt.color = Color.white;
            labelTxt.fontSize = 18;
            labelTxt.text = label;
            labelTxt.raycastTarget = false;

            var hintGo = new GameObject("Hint");
            hintGo.transform.SetParent(go.transform, false);
            var hintRect = hintGo.AddComponent<RectTransform>();
            hintRect.anchorMin = new Vector2(0f, 0f);
            hintRect.anchorMax = new Vector2(1f, 0.4f);
            hintRect.offsetMin = Vector2.zero;
            hintRect.offsetMax = Vector2.zero;
            var hintTxt = hintGo.AddComponent<Text>();
            hintTxt.font = font;
            hintTxt.alignment = TextAnchor.MiddleCenter;
            hintTxt.color = new Color(0.7f, 0.7f, 0.7f);
            hintTxt.fontSize = 14;
            hintTxt.text = ShouldShowKeyHint() ? $"[{hintKey}]" : string.Empty;
            hintTxt.raycastTarget = false;

            var btn = go.AddComponent<DrillCommandButton>();
            btn.labelText = labelTxt;
            btn.hintText = hintTxt;
            btn.command = cmd;
            btn.onPressed = onPressed;
            return btn;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            onPressed?.Invoke(command);
        }

        static bool ShouldShowKeyHint()
        {
            return !Application.isMobilePlatform;
        }
    }
}
