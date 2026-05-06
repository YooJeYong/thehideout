using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RendererUtils;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering.RenderGraphModule;

namespace Hidenet.Rendering
{
    public class MatrixRenderPass : ScriptableRenderPass
    {
        private static readonly List<ShaderTagId> ShaderTagIds = new List<ShaderTagId>
        {
            new ShaderTagId("UniversalForward"),
            new ShaderTagId("UniversalForwardOnly"),
            new ShaderTagId("SRPDefaultUnlit"),
            new ShaderTagId("LightweightForward"),
        };

        private MatrixRenderFeature.Settings settings;
        private FilteringSettings filteringSettings;
        private static readonly ProfilingSampler ProfilingSamplerStatic = new ProfilingSampler("Matrix Override Pass");

        public MatrixRenderPass(MatrixRenderFeature.Settings settings)
        {
            UpdateSettings(settings);
        }

        public void UpdateSettings(MatrixRenderFeature.Settings settings)
        {
            this.settings = settings;
            this.renderPassEvent = settings.renderPassEvent;
            this.filteringSettings = new FilteringSettings(RenderQueueRange.opaque, settings.layerMask);
        }

        // ---------------------------------------------------------------------
        // RenderGraph (Unity 6+) — RecordRenderGraph 정식 경로
        // ---------------------------------------------------------------------

        private class PassData
        {
            public RendererListHandle rendererListHandle;
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            if (settings == null || settings.overrideMaterial == null) return;

            UniversalRenderingData renderingData = frameData.Get<UniversalRenderingData>();
            UniversalCameraData    cameraData    = frameData.Get<UniversalCameraData>();
            UniversalLightData     lightData     = frameData.Get<UniversalLightData>();
            UniversalResourceData  resourceData  = frameData.Get<UniversalResourceData>();

            using (var builder = renderGraph.AddRasterRenderPass<PassData>("Matrix Override Pass", out var passData, ProfilingSamplerStatic))
            {
                var sortingCriteria = SortingCriteria.CommonOpaque;
                var drawingSettings = RenderingUtils.CreateDrawingSettings(
                    ShaderTagIds, renderingData, cameraData, lightData, sortingCriteria);

                drawingSettings.overrideMaterial = settings.overrideMaterial;
                drawingSettings.overrideMaterialPassIndex = 0;

                var rendererListParams = new RendererListParams(renderingData.cullResults, drawingSettings, filteringSettings);
                passData.rendererListHandle = renderGraph.CreateRendererList(rendererListParams);

                builder.UseRendererList(passData.rendererListHandle);

                builder.SetRenderAttachment(resourceData.activeColorTexture, 0);
                builder.SetRenderAttachmentDepth(resourceData.activeDepthTexture);

                builder.AllowPassCulling(false);

                builder.SetRenderFunc((PassData data, RasterGraphContext ctx) =>
                {
                    ctx.cmd.DrawRendererList(data.rendererListHandle);
                });
            }
        }
    }
}
