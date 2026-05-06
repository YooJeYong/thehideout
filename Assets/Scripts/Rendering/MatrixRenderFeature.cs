using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace Hidenet.Rendering
{
    public class MatrixRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class Settings
        {
            [Tooltip("매트릭스 모드일 때 모든 오브젝트에 강제 적용할 머티리얼 (TriplanarGrid 권장)")]
            public Material overrideMaterial;

            [Tooltip("어떤 레이어를 매트릭스로 그릴지 (Hands/UI 레이어 제외 권장)")]
            public LayerMask layerMask = -1;

            [Tooltip("렌더 패스 실행 시점")]
            public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingOpaques;

            [Tooltip("이 Feature 활성 여부 (런타임에 SetActive로도 토글 가능)")]
            public bool isActive = false;
        }

        public Settings settings = new Settings();

        private MatrixRenderPass pass;

        public override void Create()
        {
            pass = new MatrixRenderPass(settings);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settings.isActive) return;
            if (settings.overrideMaterial == null) return;

            pass.UpdateSettings(settings);
            renderer.EnqueuePass(pass);
        }

        public void SetActive(bool active)
        {
            settings.isActive = active;
        }

        public bool IsActive => settings.isActive;
    }
}
