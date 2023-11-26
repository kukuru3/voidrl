using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Scanner.PostEffects {
    [Serializable]
    [PostProcess(typeof(SpaceBlurRenderer), PostProcessEvent.BeforeStack, "Scanner/Blur", true)]
    internal class SpaceBlur : PostProcessEffectSettings {
        [Range(0f, 1f)] public FloatParameter factor = new FloatParameter { value = 1.0f };
        [Range(0f, 1f)]public FloatParameter  diagonalFactor= new FloatParameter { value = 1.0f };

    }

    internal class SpaceBlurRenderer : PostProcessEffectRenderer<SpaceBlur> {
        public override void Render(PostProcessRenderContext context) {
            var sheet = context.propertySheets.Get(Shader.Find("Scanner/Space Blur"));
            sheet.properties.SetFloat("_Factor", settings.factor);
            sheet.properties.SetFloat("_DiagonalFactor", settings.diagonalFactor);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}