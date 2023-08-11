using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Scanner.PostEffects {
    [Serializable]
    [PostProcess(typeof(CRTRenderer), PostProcessEvent.AfterStack, "Scanner/CRT-ize", true)]
    internal class CRT : PostProcessEffectSettings {
        [Range(0f,3f)]
        public FloatParameter chromaticAberration = new() { value = 0.0f };

        [Range(2f, 10f)] public FloatParameter scanlineRepeat = new() { value = 3.0f };
        [Range(2f, 10f)] public FloatParameter dotMatrixRepeat = new() { value = 3.0f };
        [Range(0f, 0.4f)] public FloatParameter greenify = new() { value = 0.05f};
    }

    internal class CRTRenderer : PostProcessEffectRenderer<CRT> {
        public override void Render(PostProcessRenderContext context) {
            var sheet = context.propertySheets.Get(Shader.Find("Scanner/CRT")) ;

            sheet.properties.SetFloat("_ChromAbbDistance",settings.chromaticAberration);
            sheet.properties.SetFloat("_ScanlineRepeat",settings.scanlineRepeat);
            sheet.properties.SetFloat("_DotMatrixRepeat",settings.dotMatrixRepeat);
            sheet.properties.SetFloat("_Greenify",settings.greenify);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
