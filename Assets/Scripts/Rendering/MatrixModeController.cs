using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Hidenet.Rendering
{
    public class MatrixModeController : MonoBehaviour
    {
        public static MatrixModeController Instance { get; private set; }

        [Header("Render Feature 참조")]
        [Tooltip("씬 안에서는 못 잡으니 코드로 자동 탐색. 직접 지정도 가능")]
        public MatrixRenderFeature renderFeature;

        [Header("매트릭스 머티리얼")]
        [Tooltip("TriplanarGrid 머티리얼 (M_TriplanarGrid). 페이드/글리치 프로퍼티 동적 제어용")]
        public Material matrixMaterial;

        [Header("Transition")]
        [Tooltip("페이드 지속 시간 (초)")]
        public float fadeDuration = 1.0f;

        [Tooltip("페이드 중 글리치 최대 강도 (0~1)")]
        [Range(0f, 1f)] public float maxGlitch = 0.6f;

        [Header("Debug")]
        [Tooltip("키보드 G 키로 토글 (디버그용, 에디터/PC에서 테스트)")]
        public bool enableKeyboardDebug = true;

        private InputAction matrixToggleAction;
        private bool isMatrixMode = false;
        private Coroutine fadeCoroutine;

        private static readonly int MatrixBlendId = Shader.PropertyToID("_MatrixBlend");
        private static readonly int GlitchAmountId = Shader.PropertyToID("_GlitchAmount");

        void Awake()
        {
            Debug.Log("[Matrix] Awake() called — controller is alive on " + gameObject.name);
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        void OnEnable()
        {
            Debug.Log("[Matrix] OnEnable() — registering InputAction");

            // 오큘러스 A 버튼 (오른쪽 컨트롤러 primaryButton)
            matrixToggleAction = new InputAction(
                name: "MatrixToggle",
                type: InputActionType.Button,
                binding: "<XRController>{RightHand}/primaryButton"
            );
            // 키보드 F 키도 추가 (PC 에디터 디버그용)
            if (enableKeyboardDebug)
            {
                matrixToggleAction.AddBinding("<Keyboard>/f");
            }
            matrixToggleAction.performed += OnTogglePressed;
            matrixToggleAction.Enable();

            Debug.Log($"[Matrix] InputAction enabled. Bindings: {matrixToggleAction.bindings.Count}, Keyboard.current null? {Keyboard.current == null}");

            // 초기 상태: 매트릭스 OFF
            ApplyShaderState(0f, 0f);
            if (renderFeature != null) renderFeature.SetActive(false);
        }

        void Update()
        {
            // 폴백: InputAction이 어떤 이유로 안 먹어도 Keyboard.current로 직접 폴링
            if (!enableKeyboardDebug) return;
            if (Keyboard.current == null) return;
            if (Keyboard.current.fKey.wasPressedThisFrame)
            {
                Debug.Log("[Matrix] Fallback: F key detected via Keyboard.current");
                ToggleMatrixMode();
            }
        }

        void OnDisable()
        {
            if (matrixToggleAction != null)
            {
                matrixToggleAction.performed -= OnTogglePressed;
                matrixToggleAction.Disable();
                matrixToggleAction.Dispose();
                matrixToggleAction = null;
            }
        }

        private void OnTogglePressed(InputAction.CallbackContext ctx)
        {
            Debug.Log($"[Matrix] Toggle pressed (binding: {ctx.control.path})");
            ToggleMatrixMode();
        }

        public void ToggleMatrixMode()
        {
            SetMatrixMode(!isMatrixMode);
        }

        public void SetMatrixMode(bool enabled)
        {
            if (isMatrixMode == enabled) return;
            isMatrixMode = enabled;

            Debug.Log($"[Matrix] SetMatrixMode = {enabled} | renderFeature null? {renderFeature == null} | matrixMaterial null? {matrixMaterial == null}");

            if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
            fadeCoroutine = StartCoroutine(FadeRoutine(enabled));
        }

        private IEnumerator FadeRoutine(bool fadeIn)
        {
            // fadeIn = true → 0→1 (현실 → 매트릭스), Render Feature는 시작 시 ON
            // fadeIn = false → 1→0 (매트릭스 → 현실), Render Feature는 끝날 때 OFF

            if (fadeIn && renderFeature != null) renderFeature.SetActive(true);

            float t = 0f;
            float startBlend = fadeIn ? 0f : 1f;
            float endBlend   = fadeIn ? 1f : 0f;

            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / fadeDuration);
                float blend = Mathf.Lerp(startBlend, endBlend, k);
                // 글리치는 페이드 중간에서 최대, 양 끝에서 0
                float glitch = Mathf.Sin(k * Mathf.PI) * maxGlitch;
                ApplyShaderState(blend, glitch);
                yield return null;
            }

            ApplyShaderState(endBlend, 0f);

            if (!fadeIn && renderFeature != null) renderFeature.SetActive(false);

            fadeCoroutine = null;
        }

        private void ApplyShaderState(float blend, float glitch)
        {
            if (matrixMaterial != null)
            {
                matrixMaterial.SetFloat(MatrixBlendId, blend);
                matrixMaterial.SetFloat(GlitchAmountId, glitch);
            }
            // 글로벌로도 박아서 동일 셰이더 쓰는 다른 머티리얼도 영향 받게
            Shader.SetGlobalFloat(MatrixBlendId, blend);
            Shader.SetGlobalFloat(GlitchAmountId, glitch);
        }

        public bool IsMatrixMode => isMatrixMode;
    }
}
