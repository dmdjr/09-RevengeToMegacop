using System.Reflection;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

#if UNITY_6000_0_OR_NEWER
using UnityEngine.Rendering.RenderGraphModule;
#endif

namespace FXV
{
    public class ShieldHitsRenderPass : ScriptableRenderPass
    {
        static readonly string RenderTag = "FXVShieldHits";

        CommandBuffer cmdBuffer;

        public ShieldHitsRenderPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;

#if !UNITY_6000_0_OR_NEWER
            cmdBuffer = CommandBufferPool.Get(RenderTag);
            cmdBuffer.Clear();
#endif
        }

        public CommandBuffer GetCommandBuffer()
        {
            return cmdBuffer;
        }

#if UNITY_6000_0_OR_NEWER
        class PassData
        {
            internal RenderTextureDescriptor cameraTextureDescriptor;
            internal TextureHandle cameraColorTarget;
            internal TextureHandle cameraDepthTarget;
        }

        static void ExecutePass(PassData data, UnsafeGraphContext context)
        {
            CommandBuffer cmdBuffer = CommandBufferHelpers.GetNativeCommandBuffer(context.cmd);

            cmdBuffer.SetRenderTarget(data.cameraColorTarget, 0, CubemapFace.Unknown, -1);

            Shield._AddHitsToCommandBuffer(cmdBuffer);

        }

        public override void RecordRenderGraph(RenderGraph renderGraph, ContextContainer frameData)
        {
            UniversalResourceData resourceData = frameData.Get<UniversalResourceData>();

            UniversalCameraData cameraData = frameData.Get<UniversalCameraData>();

            Camera cam = cameraData.camera;
            bool isProperCamera = (cam.cameraType == CameraType.Game);
#if UNITY_EDITOR
            isProperCamera = isProperCamera || (cam.cameraType == CameraType.SceneView);
#endif

            if (!isProperCamera)
            {
                return;
            }

            // The following line ensures that the render pass doesn't blit
            // from the back buffer.
            if (resourceData.isActiveTargetBackBuffer)
                return;

            using (var builder = renderGraph.AddUnsafePass<PassData>(RenderTag, out var passData))
            {
                ConfigureInput(ScriptableRenderPassInput.Depth);

                passData.cameraTextureDescriptor = cameraData.cameraTargetDescriptor;
                passData.cameraColorTarget = resourceData.activeColorTexture;
                builder.UseTexture(passData.cameraColorTarget, AccessFlags.Write);

                passData.cameraDepthTarget = resourceData.activeDepthTexture;
                builder.UseTexture(passData.cameraDepthTarget, AccessFlags.Read);

                builder.SetRenderFunc((PassData data, UnsafeGraphContext context) => ExecutePass(data, context));
            }
        }
#else
        public void Setup(RenderTargetIdentifier currentTarget, RenderTargetIdentifier depthTarget)
        {

        }

        public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
        {

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


            srContext.ExecuteCommandBuffer(cmdBuffer);

            CommandBufferPool.Release(cmdBuffer);
            cmdBuffer = null;
        }

        public override void FrameCleanup(CommandBuffer cmd)
        {

        }
#endif

    }
}