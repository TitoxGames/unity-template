using System.Collections;
using TMPro;
using UnityEngine;

namespace TitoX.UI
{
    public class CurvedText : MonoBehaviour
    {
        [Header("Curve Settings")]
        [Range(-100f, 100f)]
        public float curveAmount = 20f;

        public bool rotateCharacters = true;
        public bool useCustomCurve = false;

        public AnimationCurve customCurve = new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.5f, 1f),
            new Keyframe(1f, 0f)
        );

        private TMP_Text m_TextComponent;
        private bool m_IsReady = false;
        private float m_LastCurveAmount;
        private bool m_LastRotate;


        IEnumerator Start()
        {
            m_TextComponent = GetComponent<TMP_Text>();

            if (m_TextComponent == null)
            {
                Debug.LogError("CurvedText: No TMP_Text component found on the GameObject.");
                enabled = false;
                yield break;
            }

            yield return null;
            yield return null;

            m_LastCurveAmount = curveAmount;
            m_LastRotate = rotateCharacters;
            m_IsReady = true;

            ApplyCurve();
        }

        void OnEnable()
        {
            if (m_IsReady)
            {
                StartCoroutine(DelayedApply());
            }
        }

        IEnumerator DelayedApply()
        {
            yield return null;
            ApplyCurve();
        }

        public void Refresh()
        {
            if (m_IsReady)
            {
                StartCoroutine(DelayedApply());
            }
        }

        void LateUpdate()
        {
            if (!m_IsReady || m_TextComponent == null) return;

            if (Mathf.Abs(m_LastCurveAmount - curveAmount) > 0.01f ||
                m_LastRotate != rotateCharacters)
            {
                m_LastCurveAmount = curveAmount;
                m_LastRotate = rotateCharacters;
                ApplyCurve();
            }
        }

        void ApplyCurve()
        {
            if (m_TextComponent == null) return;

            try
            {
                m_TextComponent.ForceMeshUpdate();
            }
            catch (System.Exception)
            {
                return;
            }

            TMP_TextInfo textInfo = m_TextComponent.textInfo;
            if (textInfo == null || textInfo.characterCount == 0) return;
            if (textInfo.meshInfo == null || textInfo.meshInfo.Length == 0) return;

            Bounds bounds = m_TextComponent.bounds;
            float boundsMinX = bounds.min.x;
            float boundsMaxX = bounds.max.x;
            float boundsWidth = boundsMaxX - boundsMinX;

            if (boundsWidth <= 0.001f) return;

            for (int i = 0; i < textInfo.characterCount; i++)
            {
                TMP_CharacterInfo charInfo = textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;

                int materialIndex = charInfo.materialReferenceIndex;
                int vertexIndex = charInfo.vertexIndex;

                if (materialIndex >= textInfo.meshInfo.Length) continue;

                Vector3[] vertices = textInfo.meshInfo[materialIndex].vertices;
                if (vertices == null || vertexIndex + 3 >= vertices.Length) continue;

                float charMidX = (vertices[vertexIndex].x + vertices[vertexIndex + 2].x) / 2f;
                float charMidY = (vertices[vertexIndex].y + vertices[vertexIndex + 2].y) / 2f;
                float normalizedX = (charMidX - boundsMinX) / boundsWidth;

                float offsetY;
                if (useCustomCurve)
                {
                    offsetY = customCurve.Evaluate(normalizedX) * curveAmount;
                }
                else
                {
                    float t = (normalizedX - 0.5f) * 2f;
                    offsetY = (1f - t * t) * curveAmount;
                }

                float angle = 0f;
                if (rotateCharacters)
                {
                    if (useCustomCurve)
                    {
                        float delta = 0.001f;
                        float y1 = customCurve.Evaluate(Mathf.Clamp01(normalizedX - delta)) * curveAmount;
                        float y2 = customCurve.Evaluate(Mathf.Clamp01(normalizedX + delta)) * curveAmount;
                        angle = Mathf.Atan2(y2 - y1, delta * 2f * boundsWidth) * Mathf.Rad2Deg;
                    }
                    else
                    {
                        float t = (normalizedX - 0.5f) * 2f;
                        float derivative = -2f * t * curveAmount;
                        angle = Mathf.Atan2(derivative, boundsWidth / 2f) * Mathf.Rad2Deg;
                    }
                }

                Vector3 charCenter = new Vector3(charMidX, charMidY, 0);

                for (int j = 0; j < 4; j++)
                {
                    Vector3 v = vertices[vertexIndex + j];

                    if (rotateCharacters && Mathf.Abs(angle) > 0.01f)
                    {
                        v -= charCenter;
                        float rad = angle * Mathf.Deg2Rad;
                        float cos = Mathf.Cos(rad);
                        float sin = Mathf.Sin(rad);
                        v = new Vector3(
                            v.x * cos - v.y * sin,
                            v.x * sin + v.y * cos,
                            v.z
                        ) + charCenter;
                    }

                    v.y += offsetY;
                    vertices[vertexIndex + j] = v;
                }
            }

            for (int i = 0; i < textInfo.meshInfo.Length; i++)
            {
                if (textInfo.meshInfo[i].mesh == null) continue;
                textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
                m_TextComponent.UpdateGeometry(textInfo.meshInfo[i].mesh, i);
            }
        }
    }
}