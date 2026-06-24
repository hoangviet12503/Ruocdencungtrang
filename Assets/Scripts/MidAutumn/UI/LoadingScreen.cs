using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using MidAutumn.Gameplay;

namespace MidAutumn.UI
{
    /// <summary>
    /// Simple loading screen shown when game starts. Hides once GameManager finishes loading.
    /// </summary>
    public class LoadingScreen : MonoBehaviour
    {
        private CanvasGroup _canvasGroup;
        private float _fadeDuration = 0.5f;

        private void Awake()
        {
            SetupUI();
        }

        private void Start()
        {
            _canvasGroup.alpha = 1f;
            StartCoroutine(WaitForGameAndHide());
        }

        private void SetupUI()
        {
            var rt = GetComponent<RectTransform>();
            if (rt == null)
                gameObject.AddComponent<RectTransform>();

            // Setup Canvas
            var canvas = GetComponent<Canvas>();
            if (canvas == null)
            {
                canvas = gameObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 100;
            }

            if (GetComponent<CanvasScaler>() == null)
            {
                var scaler = gameObject.AddComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1080, 1920);
            }

            if (GetComponent<GraphicRaycaster>() == null)
                gameObject.AddComponent<GraphicRaycaster>();

            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null)
                _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            // Setup Background panel
            Transform bgTransform = transform.Find("Background");
            if (bgTransform == null)
            {
                var bgGO = new GameObject("Background");
                bgTransform = bgGO.transform;
                bgTransform.SetParent(transform, false);
            }

            var bgRect = bgTransform.GetComponent<RectTransform>();
            if (bgRect == null)
                bgRect = bgTransform.gameObject.AddComponent<RectTransform>();

            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;

            var bgImage = bgTransform.GetComponent<Image>();
            if (bgImage == null)
                bgImage = bgTransform.gameObject.AddComponent<Image>();

            bgImage.color = new Color(0.13f, 0.71f, 0.87f, 1f); // Cyan from design

            // Setup Loading text
            Transform textTransform = transform.Find("LoadingText");
            if (textTransform == null)
            {
                var textGO = new GameObject("LoadingText");
                textTransform = textGO.transform;
                textTransform.SetParent(transform, false);
            }

            var textRect = textTransform.GetComponent<RectTransform>();
            if (textRect == null)
                textRect = textTransform.gameObject.AddComponent<RectTransform>();

            textRect.anchoredPosition = Vector2.zero;
            textRect.sizeDelta = new Vector2(600, 300);

            var textComponent = textTransform.GetComponent<Text>();
            if (textComponent == null)
                textComponent = textTransform.gameObject.AddComponent<Text>();

            textComponent.text = "Đang tải...";
            textComponent.font = Resources.Load<Font>("DINPro-Medium");
            textComponent.fontSize = 48;
            textComponent.fontStyle = FontStyle.Bold;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.color = Color.white;
        }

        private IEnumerator WaitForGameAndHide()
        {
            // Wait until GameManager is ready
            while (GameManager.Instance == null || GameManager.Instance.Slots == null)
                yield return null;

            // Extra time for UI to settle
            yield return new WaitForSeconds(0.3f);

            // Fade out
            float elapsed = 0f;
            while (elapsed < _fadeDuration)
            {
                elapsed += Time.deltaTime;
                _canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / _fadeDuration);
                yield return null;
            }

            _canvasGroup.alpha = 0f;
            gameObject.SetActive(false);
        }
    }
}
