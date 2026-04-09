using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Unity.VisualScripting;

#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif

namespace FXV
{
    public class ShieldPostprocessRenderPass : ScriptableRenderPass
    {
        static readonly string RenderTag = "FXVShieldPostprocess";

        static int[] TEMP_TARGET_TEXTURE_ID = null;
        static int SSGI_TARGET_TEXTURE_ID;
        static int SSGI2_TARGET_TEXTURE_ID;

        static int POSITION_FRONT_TARGET_TEXTURE_ID;
        static int POSITION_BACK_TARGET_TEXTURE_ID;

        static ShieldPostprocessRenderPass()
        {
            TEMP_TARGET_TEXTURE_ID = new int[16];
            for (int i = 0; i < TEMP_TARGET_TEXTURE_ID.Length; i++)
            {
                TEMP_TARGET_TEXTURE_ID[i] = Shader.PropertyToID("_FXVTemporaryBuffer_" + i);
            }

            POSITION_FRONT_TARGET_TEXTURE_ID = Shader.PropertyToID("_FXVPositionFrontBuffer");
            POSITION_BACK_TARGET_TEXTURE_ID = Shader.PropertyToID("_FXVPositionBackBuffer");
            SSGI_TARGET_TEXTURE_ID = Shader.PropertyToID("_SSGIBuffer");
            SSGI2_TARGET_TEXTURE_ID = Shader.PropertyToID("_SSGIBuffer2");
        }

        [SerializeField]
        int downSampleSteps = 3;

        ShieldPostprocess.PostprocessContext context;

        RenderTargetIdentifier cameraDepthTarget;

        public ShieldPostprocessRenderPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;
        }

        bool IsDeferred()
        {
            var pipeline = GraphicsSettings.defaultRenderPipeline;
            if (pipeline)
            {
                FieldInfo propertyInfo = pipeline.GetType().GetField("m_RendererDataList", BindingFlags.Instance | BindingFlags.NonPublic);
                ScriptableRendererData scriptableRendererData = ((ScriptableRendererData[])propertyInfo?.GetValue(pipeline))?[0];

                if (scriptableRendererData && (scriptableRendererData.GetType() == typeof(UniversalRendererData)))
                {
                    return ((UniversalRendererData)scriptableRendererData).renderingMode == RenderingMode.Deferred;
                }
            }

            return false;
        }

        public void Setup(RenderTargetIdentifier currentTarget, RenderTargetIdentifier depthTarget)
        {
            cameraDepthTarget = depthTarget;

            context = new ShieldPostprocess.PostprocessContext();

            context.isDeferred = IsDeferred();

            downSampleSteps = ShieldPostprocess.GetMaxDownSampleSteps();

            context.currentTarget = currentTarget;

            context.tempTarget = new RenderTargetIdentifier[downSampleSteps + 1];
            context.tmpTargetSizes = new Vector2[downSampleSteps + 1];
        }


        internal static void ConfigureContext(ShieldPostprocess.PostprocessContext context, CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ShieldPostprocess postprocess = null;
            if (postprocess == null)
            {
                postprocess = ShieldPostprocess.GetMainInstance();
                if (postprocess == null)
                {
                    return;
                }
            }

            postprocess.forceDisableGI = (context.isDeferred == false);

            cmd.GetTemporaryRT(TEMP_TARGET_TEXTURE_ID[0], cameraTextureDescriptor);

            context.tempTarget[0] = new RenderTargetIdentifier(TEMP_TARGET_TEXTURE_ID[0]);

            int ww = cameraTextureDescriptor.width;
            int hh = cameraTextureDescriptor.height;
            float downsample = (1.0f - postprocess.downSampleRate);

            context.tmpTargetSizes[0] = new Vector2(ww, hh);

            for (int i = 1; i < context.tempTarget.Length; i++)
            {
                ww = (int)(ww * downsample);
                hh = (int)(hh * downsample);

                if (cameraTextureDescriptor.vrUsage != VRTextureUsage.TwoEyes)
                {
                    cmd.GetTemporaryRT(TEMP_TARGET_TEXTURE_ID[i], ww, hh, 0, FilterMode.Bilinear, cameraTextureDescriptor.colorFormat);
                }
                else
                {
                    context.isTwoEyeVR = true;
                    postprocess.forceDisableGI = true;
                    cmd.GetTemporaryRTArray(TEMP_TARGET_TEXTURE_ID[i], ww, hh, 2, 0, FilterMode.Bilinear, cameraTextureDescriptor.colorFormat);
                }
                context.tempTarget[i] = new RenderTargetIdentifier(TEMP_TARGET_TEXTURE_ID[i]);

                context.tmpTargetSizes[i] = new Vector2(ww, hh);
            }

            if (postprocess.IsGloballIlluminationEnabled())
            {
                int giW = (int)context.tmpTargetSizes[postprocess.giAtIteration].x;
                int giH = (int)context.tmpTargetSizes[postprocess.giAtIteration].y;

                cmd.GetTemporaryRT(SSGI_TARGET_TEXTURE_ID, new RenderTextureDescriptor(giW, giH, cameraTextureDescriptor.colorFormat, 0));
                context.ssgiTarget = new RenderTargetIdentifier(SSGI_TARGET_TEXTURE_ID);

                cmd.GetTemporaryRT(SSGI2_TARGET_TEXTURE_ID, new RenderTextureDescriptor(giW, giH, cameraTextureDescriptor.colorFormat, 0));
                context.ssgiTarget2 = new RenderTargetIdentifier(SSGI2_TARGET_TEXTURE_ID);

                RenderTextureDescriptor posBufferDesc = cameraTextureDescriptor;
                posBufferDesc.colorFormat = RenderTextureFormat.ARGBFloat;

                cmd.GetTemporaryRT(POSITION_FRONT_TARGET_TEXTURE_ID, posBufferDesc);
                context.positionFrontTarget = new RenderTargetIdentifier(POSITION_FRONT_TARGET_TEXTURE_ID);

                cmd.GetTemporaryRT(POSITION_BACK_TARGET_TEXTURE_ID, posBufferDesc);
                context.positionBackTarget = new RenderTargetIdentifier(POSITION_BACK_TARGET_TEXTURE_ID);
            }
        }
        internal static void CleanupContext(ShieldPostprocess.PostprocessContext context, CommandBuffer cmd)
        {
            if (context != null && context.tempTarget != null)
            {
                for (int i = 0; i < context.tempTarget.Length; i++)
                {
                    cmd.ReleaseTemporaryRT(TEMP_TARGET_TEXTURE_ID[i]);
                }

                cmd.ReleaseTemporaryRT(POSITION_FRONT_TARGET_TEXTURE_ID);
                cmd.ReleaseTemporaryRT(POSITION_BACK_TARGET_TEXTURE_ID);
                cmd.ReleaseTemporaryRT(SSGI_TARGET_TEXTURE_ID);
                cmd.ReleaseTemporaryRT(SSGI2_TARGET_TEXTURE_ID);
            }
        }


#if UNITY_6000_0_OR_NEWER
        class PassData
        {
            internal ShieldPostprocess.PostprocessContext context;
            internal ShieldPostprocess postprocess;

            internal RenderTextureDescriptor cameraTextureDescriptor;
            internal TextureHandle cameraColorTarget;
            internal TextureHandle cameraDepthTarget;
            internal TextureHandle cameraNormalsTarget;
        }

        static void ExecutePass(PassData data, UnsafeGraphContext context)
        {
            CommandBuffer cmdBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);

            data.context.currentTarget = data.cameraColorTarget;

            ConfigureContext(data.context, cmdBuffer, data.cameraTextureDescriptor);

            if (data.postprocess.IsGloballIlluminationEnabled())
            {
                cmdBuffer.SetRenderTarget(new RenderTargetIdentifier[] {
                                                data.context.tempTarget[0],
                                                data.context.positionFrontTarget,
                                                data.context.positionBackTarget,
                                            }, data.cameraDepthTarget, 0, CubemapFace.Unknown, -1);

                cmdBuffer.SetGlobalTexture("_GBuffer2", data.cameraNormalsTarget);
            }
            else
            {
                cmdBuffer.SetRenderTarget(data.context.tempTarget[0], 0, CubemapFace.Unknown, -1);
            }

            data.postprocess._PushRenderObjectsToCommandBuffer(cmdBuffer);

            data.postprocess._AddPostprocessToCommandBuffer(cmdBuffer, data.context);

            CleanupContext(data.context, cmdBuffer);
        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            // The following line ensures that the render pass doesn't blit
            // from the back buffer.
            if (resourceData.isActiveTargetBackBuffer)
                return;

            Camera cam = cameraData.camera;
            bool isProperCamera = (cam.cameraType == CameraType.Game);
#if UNITY_EDITOR
            isProperCamera = isProperCamera || (cam.cameraType == CameraType.SceneView);
#endif

            if (!isProperCamera)
            {
                return;
            }

            ShieldPostprocess postprocess = cameraData.camera.GetComponent<ShieldPostprocess>();
            if (postprocess == null)
            {
                postprocess = ShieldPostprocess.GetMainInstance();
                if (postprocess == null)
                {
                    return;
                }
            }

            if (!postprocess.enabled)
            {
                return;
            }

            using (var builder = renderGraph.AddUnsafePass<PassData>(RenderTag, out var passData))
            {
                passData.postprocess = postprocess;

                passData.context = new ShieldPostprocess.PostprocessContext();
                passData.context.isDeferred = IsDeferred();

                downSampleSteps = ShieldPostprocess.GetMaxDownSampleSteps();
                passData.context.tempTarget = new RenderTargetIdentifier[downSampleSteps + 1];
                passData.context.tmpTargetSizes = new Vector2[downSampleSteps + 1];

                passData.context.downSampleSteps = postprocess.numberOfIterations;
                passData.context.downSampleRate = postprocess.downSampleRate;
                passData.context.targetWidth = cameraData.camera.scaledPixelWidth;
                passData.context.targetHeight = cameraData.camera.scaledPixelHeight;
                passData.context.viewProjectInverse = (cameraData.camera.projectionMatrix * cameraData.camera.worldToCameraMatrix).inverse;
                passData.context.fullScreenMesh = RenderingUtils.fullscreenMesh;

                ConfigureInput(ScriptableRenderPassInput.Depth);

                if (postprocess.IsGloballIlluminationEnabled())
                {
                    ConfigureInput(ScriptableRenderPassInput.Normal);
                }

                passData.cameraTextureDescriptor = cameraData.cameraTargetDescriptor;
                passData.cameraColorTarget = resourceData.activeColorTexture;
                builder.UseTexture(passData.cameraColorTarget, AccessFlags.Write);

                passData.cameraDepthTarget = resourceData.activeDepthTexture;
                builder.UseTexture(passData.cameraDepthTarget, AccessFlags.Read);

                if (postprocess.IsGloballIlluminationEnabled())
                {
                    passData.cameraNormalsTarget = resourceData.cameraNormalsTexture;
                    builder.UseTexture(passData.cameraNormalsTarget, AccessFlags.Read);
                }

                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => ExecutePass(data, context));
            }
        }
#else
        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {
            ConfigureContext(context, cmd, cameraTextureDescriptor);
        }

        public override void Execute(ScriptableRenderContext srContext, ref RenderingData renderingData)
        {
            Camera cam = renderingData.cameraData.camera;
            bool isProperCamera = (cam.cameraType == CameraType.Game);
#if UNITY_EDITOR
            isProperCamera = isProperCamera || (cam.cameraType == CameraType.SceneView);
#endif

            if (!isProperCamera)
            {
                return;
            }

            ShieldPostprocess postprocess = renderingData.cameraData.camera.GetComponent<ShieldPostprocess>();
            if (postprocess == null)
            {
                postprocess = ShieldPostprocess.GetMainInstance();
                if (postprocess == null)
                {
                    return;
                }
            }

            if (!postprocess.enabled)
            {
                return;
            }

#if !UNITY_2022_1_OR_NEWER
            context.currentTarget = renderingData.cameraData.renderer.cameraColorTarget;
            cameraDepthTarget = renderingData.cameraData.renderer.cameraDepthTarget;
#endif
            context.downSampleSteps = postprocess.numberOfIterations;
            context.downSampleRate = postprocess.downSampleRate;
            context.targetWidth = renderingData.cameraData.camera.scaledPixelWidth;
            context.targetHeight = renderingData.cameraData.camera.scaledPixelHeight;
            context.viewProjectInverse = (renderingData.cameraData.camera.projectionMatrix * renderingData.cameraData.camera.worldToCameraMatrix).inverse;
            context.fullScreenMesh = RenderingUtils.fullscreenMesh;

            CommandBuffer cmdBuffer = CommandBufferPool.Get(RenderTag);

            cmdBuffer.Clear();

            if (postprocess.IsGloballIlluminationEnabled())
            {
                cmdBuffer.SetRenderTarget(new RenderTargetIdentifier[] {
                                                context.tempTarget[0],
                                                context.positionFrontTarget,
                                                context.positionBackTarget,
                                            }, cameraDepthTarget, 0, CubemapFace.Unknown, -1);
            }
            else
            {
                cmdBuffer.SetRenderTarget(context.tempTarget[0], 0, CubemapFace.Unknown, -1);
            }

            postprocess._PushRenderObjectsToCommandBuffer(cmdBuffer);

            postprocess._AddPostprocessToCommandBuffer(cmdBuffer, context);

            srContext.ExecuteCommandBuffer(cmdBuffer);

            CommandBufferPool.Release(cmdBuffer);
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {
            CleanupContext(context, cmd);
        }
#endif

    }
}