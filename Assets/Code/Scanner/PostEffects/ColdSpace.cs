using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Scanner.PostEffects {

    [Serializable]
    [PostProcess(typeof(ColdSpaceRenderer), PostProcessEvent.AfterStack, "Scanner/Cold Space", true)]
    internal class ColdSpace : PostProcessEffectSettings {

        [Header("Color remap")]
        [Range(0.0f, 1.0f)]public FloatParameter hue = new FloatParameter { value = 0 };
        [Range(-0.3f, 0.3f)]public FloatParameter hueSpan = new FloatParameter { value = 0 };

        public FloatParameter targetSaturationLow = new FloatParameter { value = 0 };
        public FloatParameter targetSaturationHigh = new FloatParameter { value = 0 };
        
        public FloatParameter aberration = new FloatParameter { value = 0 };

        [Range(0,1)]public FloatParameter preserveSaturationMin = new FloatParameter {value = 0.7f};
        [Range(0,1)]public FloatParameter preserveSaturationMax = new FloatParameter {value = 1.0f};

        [Range(0,1)]public FloatParameter desaturateAnyway = new FloatParameter {value = 0.3f};

    }
    
    internal class ColdSpaceRenderer : PostProcessEffectRenderer<ColdSpace> {
        public override void Render(PostProcessRenderContext context) {
             var sheet = context.propertySheets.Get(Shader.Find("Scanner/Cold Space")) ;

            sheet.properties.SetVector("_Remap", new Vector4(settings.hue - settings.hueSpan / 2, settings.hue + settings.hueSpan / 2, settings.targetSaturationLow, settings.targetSaturationHigh));
            sheet.properties.SetVector("_CorrectionRamp", new Vector4(
                settings.preserveSaturationMin, settings.preserveSaturationMax, 
                settings.desaturateAnyway, 1.0f
            ));

            sheet.properties.SetFloat("_Aberration", settings.aberration);


            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }

}