using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class DitherFeature : ScriptableRendererFeature
{
    [System.Serializable]
    public class DitherSettings
    {
        public Material ditherMaterial = null;
        public RenderPassEvent renderPassEvent = RenderPassEvent.AfterRenderingPostProcessing;

        [Range(0.0f, 1.0f)]
        public float spread = 0.5f;

        [Range(2, 16)]
        public int redColorCount = 2;
        [Range(2, 16)]
        public int greenColorCount = 2;
        [Range(2, 16)]
        public int blueColorCount = 2;

        [Range(0, 2)]
        public int bayerLevel = 0;

        [Range(0, 8)]
        public int downSamples = 0;
        public bool pointFilterDown = false;
    }

    public DitherSettings settings = new DitherSettings();

    class DitherPass : ScriptableRenderPass
    {
        private Material ditherMaterial;
        private DitherSettings settings;
        private int tempRTID = Shader.PropertyToID("_TempDitherRT");

        public DitherPass(Material mat, DitherSettings s)
        {
            ditherMaterial = mat;
            settings = s;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (ditherMaterial == null)
                return;

            var cmd = CommandBufferPool.Get("DitherPass");

            var cameraData = renderingData.cameraData;
            RTHandle cameraColor = cameraData.renderer.cameraColorTargetHandle;

            ditherMaterial.SetFloat("_Spread", settings.spread);
            ditherMaterial.SetInt("_RedColorCount", settings.redColorCount);
            ditherMaterial.SetInt("_GreenColorCount", settings.greenColorCount);
            ditherMaterial.SetInt("_BlueColorCount", settings.blueColorCount);
            ditherMaterial.SetInt("_BayerLevel", settings.bayerLevel);

            RenderTextureDescriptor desc = cameraData.cameraTargetDescriptor;
            desc.depthBufferBits = 0;

            // Временный RT
            cmd.GetTemporaryRT(tempRTID, desc, FilterMode.Bilinear);
            cmd.Blit(cameraColor, tempRTID); // копия текущей камеры

            // Downsample, если нужно
            int width = desc.width;
            int height = desc.height;
            for (int i = 0; i < settings.downSamples; i++)
            {
                width = Mathf.Max(width / 2, 2);
                height = Mathf.Max(height / 2, 2);

                cmd.GetTemporaryRT(tempRTID + 1, width, height, 0, settings.pointFilterDown ? FilterMode.Point : FilterMode.Bilinear, desc.colorFormat);
                cmd.Blit(tempRTID, tempRTID + 1, ditherMaterial, settings.pointFilterDown ? 1 : 0);
                cmd.ReleaseTemporaryRT(tempRTID);
                cmd.SetGlobalTexture(tempRTID, tempRTID + 1);
            }

            // Финальный Blit на экран
            cmd.Blit(tempRTID, cameraColor, ditherMaterial, 0);

            cmd.ReleaseTemporaryRT(tempRTID);

            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }
    }

    private DitherPass ditherPass;

    public override void Create()
    {
        if (settings.ditherMaterial != null)
        {
            ditherPass = new DitherPass(settings.ditherMaterial, settings);
            ditherPass.renderPassEvent = settings.renderPassEvent;
        }
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (settings.ditherMaterial != null)
            renderer.EnqueuePass(ditherPass);
    }
}
