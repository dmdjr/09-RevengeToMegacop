using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace FXV
{
    public partial class Shield : fxvRenderObject
    {
        static partial void SetupHitsPassURP(Camera camera)
        {
            UnityEngine.Rendering.Universal.UniversalAdditionalCameraData camData;
            if (!camera.TryGetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>(out camData))
            {
                camData = camera.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
            }

            if (!camData)
            {
                return;
            }

            ShieldHitsRenderPass hitsPass = new ShieldHitsRenderPass(UnityEngine.Rendering.Universal.RenderPassEvent.AfterRenderingTransparents);

            hitsCommandBuffer_SRP = hitsPass.GetCommandBuffer();

            camData.scriptableRenderer.EnqueuePass(hitsPass);
        }
    }
}