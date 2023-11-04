using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Scanner.PostEffects {
    [Serializable]
    [PostProcess(typeof(SciFiOverlayRenderer), PostProcessEvent.AfterStack, "Scanner/Sci-Fi Overlay", true)]
    internal class SciFiOverlay : PostProcessEffectSettings {
        [Range(0,1)]public FloatParameter scanlineStrength = new FloatParameter { value = 0.2f };
        [Range(0,30)]public FloatParameter hexRadius = new FloatParameter { value = 10f};
    }

    internal class SciFiOverlayRenderer : PostProcessEffectRenderer<SciFiOverlay> {
        public override void Render(PostProcessRenderContext context) {
            var sheet = context.propertySheets.Get(Shader.Find("Scanner/Sci-Fi Overlay")) ;
            sheet.properties.SetFloat("_Scanline", settings.scanlineStrength);
            sheet.properties.SetFloat("_HexRadius", settings.hexRadius);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }

}