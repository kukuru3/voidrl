using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Scanner.PostEffects {
    [Serializable]
    [PostProcess(typeof(CRTRenderer), PostProcessEvent.AfterStack, "Scanner/CRT-ize", true)]
    internal class CRT : PostProcessEffectSettings {

        [Header("Master")]
        [Range(0f, 1f)] public FloatParameter finalMix = new() { value = 1.0f };

        [Header("Chromatics")]
        [Range(0f,3f)]
        public FloatParameter chromaticAberration = new() { value = 0.0f };
        public ColorParameter shadowColor = new() { value = Color.black };
        [Range(0, 0.05f)] public FloatParameter shadowEnhancer = new() { value = 0.0f };

        [Range(0f, 0.4f)] public FloatParameter greenify = new() { value = 0.05f};

        [Range(0f, 0.2f)] public FloatParameter flickerIntensity = new() { value = 0.02f };

        [Header("Scanlines")]
        [Range(2f, 10f)] public FloatParameter dotMatrixRepeat = new() { value = 3.0f };
        [Range(2f, 10f)] public FloatParameter scanlineRepeat = new() { value = 3.0f };
        [Range(-30f, 30f)] public FloatParameter scanlineSpeed = new() { value = 2f };

        [Header("Colors")]
        [Range(0f, 3f)] public FloatParameter scanlineDark = new() { value = 0.4f };
        [Range(0f, 3f)] public FloatParameter scanlineLight = new() { value = 1f };
        [Range(0f, 1f)] public FloatParameter colorCurve = new() { value = 0.4f };
    }

    internal class CRTRenderer : PostProcessEffectRenderer<CRT> {
        public override void Render(PostProcessRenderContext context) {
            var sheet = context.propertySheets.Get(Shader.Find("Scanner/CRT")) ;

            sheet.properties.SetFloat("_ChromAbbDistance",settings.chromaticAberration);
            sheet.properties.SetFloat("_DotMatrixRepeat",settings.dotMatrixRepeat);
            sheet.properties.SetFloat("_Greenify",settings.greenify);
            sheet.properties.SetFloat("_ColorCurve", settings.colorCurve);
            sheet.properties.SetFloat("_FlickerIntensity", settings.flickerIntensity);
            sheet.properties.SetVector("_ScanlineProps", new Vector4(settings.scanlineRepeat, settings.scanlineSpeed, settings.scanlineLight, settings.scanlineDark));

            sheet.properties.SetColor("_ShadowBaseline", settings.shadowColor);
            sheet.properties.SetFloat("_FinalMix", settings.finalMix);

            Shader.SetGlobalFloat("_AddedSkyboxColor", settings.shadowEnhancer);

            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}
