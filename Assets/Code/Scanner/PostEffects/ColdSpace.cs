using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Scanner.PostEffects {
    [Serializable]
    [PostProcess(typeof(ColdSpaceRenderer), PostProcessEvent.AfterStack, "Scanner/Cold Space", true)]
    internal class ColdSpace : PostProcessEffectSettings {
        [Header("Chromatics")]
        public ColorParameter colorLow = new ColorParameter() { value = new Color(0,0,0,1) };
        public ColorParameter colorHigh = new ColorParameter() { value = new Color(0,0,0,1) };

        [Range(0.0f, 1.0f)]public FloatParameter hue = new FloatParameter { value = 0 };
        [Range(-0.3f, 0.3f)]public FloatParameter hueSpan = new FloatParameter { value = 0 };

        public FloatParameter sat0 = new FloatParameter { value = 0 };
        public FloatParameter sat1 = new FloatParameter { value = 0 };
        

    }
    
    internal class ColdSpaceRenderer : PostProcessEffectRenderer<ColdSpace> {
        public override void Render(PostProcessRenderContext context) {
             var sheet = context.propertySheets.Get(Shader.Find("Scanner/Cold Space")) ;

            sheet.properties.SetColor("_ColorA", settings.colorLow);
            sheet.properties.SetColor("_ColorB", settings.colorHigh);

            sheet.properties.SetVector("_Remap", new Vector4(settings.hue - settings.hueSpan / 2, settings.hue + settings.hueSpan / 2, settings.sat0, settings.sat1));

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }

}
