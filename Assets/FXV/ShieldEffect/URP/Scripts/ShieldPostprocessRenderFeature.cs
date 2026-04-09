using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

namespace FXV
{
    public class ShieldPostprocessRenderFeature : ScriptableRendererFeature
    {
        [System.Serializable]
        public class FeatureSettings
        {
            public bool isEnabled = true;
            public RenderPassEvent whenToInsert = RenderPassEvent.AfterRenderingTransparents;
        }

        public FeatureSettings settings = new FeatureSettings();

        ShieldPostprocessRenderPass renderPass;

        public override void Create()
        {
            renderPass = new ShieldPostprocessRenderPass(settings.whenToInsert);
        }

#if UNITY_6000_0_OR_NEWER
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settings.isEnabled)
            {
                return;
            }

            ShieldPostprocess postprocess = renderingData.cameraData.camera.GetComponent<ShieldPostprocess>();
            if (postprocess == null)
            {
                if (renderingData.cameraData.camera.cameraType != CameraType.SceneView)
                {
                    return;
                }
            }

            renderer.EnqueuePass(renderPass);
        }
#elif UNITY_2022_1_OR_NEWER
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settings.isEnabled)
            {
                return;
            }

            ShieldPostprocess postprocess = renderingData.cameraData.camera.GetComponent<ShieldPostprocess>();
            if (postprocess == null)
            {
                if (renderingData.cameraData.camera.cameraType != CameraType.SceneView)
                {
                    return;
                }
            }

            renderer.EnqueuePass(renderPass);
        }

        public override void SetupRenderPasses(ScriptableRenderer renderer, in RenderingData renderingData)
        {
            if (!settings.isEnabled)
            {
                return;
            }

            ShieldPostprocess postprocess = renderingData.cameraData.camera.GetComponent<ShieldPostprocess>();
            if (postprocess == null)
            {
                if (renderingData.cameraData.camera.cameraType != CameraType.SceneView)
                {
                    return;
                }
            }

            renderPass.Setup(renderer.cameraColorTargetHandle, renderer.cameraDepthTargetHandle);
        }
#else
        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            if (!settings.isEnabled)
            {
                return;
            }

            ShieldPostprocess postprocess = renderingData.cameraData.camera.GetComponent<ShieldPostprocess>();
            if (postprocess == null)
            {
                if (renderingData.cameraData.camera.cameraType != CameraType.SceneView)
                {
                    return;
                }
            }

            var cameraColorTargetIdent = renderer.cameraColorTarget;
            renderPass.Setup(cameraColorTargetIdent, new UnityEngine.Rendering.RenderTargetIdentifier());

            renderer.EnqueuePass(renderPass);
        }
#endif
    }
}
