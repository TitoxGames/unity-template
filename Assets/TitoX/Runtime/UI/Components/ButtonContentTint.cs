using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TitoX.UI.Components
{
    /// <summary>Mirrors a Button's color-tint state (pressed, disabled...) onto its child graphics, which Unity only applies to the target graphic.</summary>
    [RequireComponent(typeof(Button))]
    public class ButtonContentTint : MonoBehaviour
    {
        private readonly List<Graphic> contents = new List<Graphic>();
        private Button button;
        private Color appliedTint = Color.white;
        private bool isTextChanged;

        public static void Bind(Component root)
        {
            foreach (Button button in root.GetComponentsInChildren<Button>(true))
            {
                if (button.transition != Selectable.Transition.ColorTint
                    || button.GetComponentInChildren<TMP_Text>(true) == null
                    || button.GetComponent<ButtonContentTint>() != null)
                {
                    continue;
                }

                button.gameObject.AddComponent<ButtonContentTint>();
            }
        }

        private void Awake()
        {
            button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(HandleTextChanged);
        }

        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(HandleTextChanged);
            ApplyTint(Color.white);
        }

        private void HandleTextChanged(Object changed)
        {
            if (changed is Component component && component.transform.IsChildOf(transform))
            {
                isTextChanged = true;
            }
        }

        private void LateUpdate()
        {
            Graphic target = button.targetGraphic;
            if (target != null)
            {
                ApplyTint(target.canvasRenderer.GetColor());
            }
        }

        private void ApplyTint(Color tint)
        {
            if (tint == appliedTint && !isTextChanged)
            {
                return;
            }

            appliedTint = tint;
            isTextChanged = false;
            // Re-collected: TMP adds sub-mesh graphics for fallback-font glyphs whenever the text changes.
            GetComponentsInChildren(true, contents);
            foreach (Graphic graphic in contents)
            {
                // Graphics of nested buttons follow their own button.
                if (graphic != button.targetGraphic && graphic.GetComponentInParent<Button>(true) == button)
                {
                    graphic.canvasRenderer.SetColor(tint);
                }
            }
        }
    }
}
