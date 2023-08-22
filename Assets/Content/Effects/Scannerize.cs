using System;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.Scripting;

namespace Scanner { 
    [Serializable]
    [PostProcess(typeof(ScannerizeRenderer), PostProcessEvent.AfterStack, "Scanner/Scannerize")]
    public class Scannerize : PostProcessEffectSettings {
        [Range(0f,1f)]
        public FloatParameter fadeAmount = new() { value = 0.0f };
    }
    
    [Preserve]
    public class ScannerizeRenderer : PostProcessEffectRenderer<Scannerize> {
        public override void Render(PostProcessRenderContext context) {
            // if (context.camera.targetTexture == null) return;
            var sheet = context.propertySheets.Get(Shader.Find("Scanner/Postprocessing/Scannerize"));  
            // sheet.properties.SetFloat("_fadeAmount", settings.fadeAmount);
            context.command.BlitFullscreenTriangle(context.source, context.destination, sheet, 0);
        }
    }
}