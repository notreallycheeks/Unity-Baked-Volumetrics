using UnityEditor;
using UnityEngine;
using System.IO;

namespace BakedVolumetrics.Editor
{
    /// <summary>
    /// Custom shader GUI for SceneVolumetricFog that handles Light Volumes dependency.
    /// Disables the Light Volumes toggle if the package isn't installed.
    /// </summary>
    public class SceneVolumetricFogShaderGUI : ShaderGUI
    {
        private static bool? _lightVolumesInstalled;

        private static bool IsLightVolumesInstalled()
        {
            if (_lightVolumesInstalled.HasValue)
                return _lightVolumesInstalled.Value;

            // Check if the Light Volumes package is installed by looking for its shader include
            string lightVolumesPath = "Packages/red.sim.lightvolumes/Shaders/LightVolumes.cginc";
            _lightVolumesInstalled = File.Exists(Path.GetFullPath(lightVolumesPath));

            return _lightVolumesInstalled.Value;
        }

        public override void OnGUI(MaterialEditor materialEditor, MaterialProperty[] properties)
        {
            // Find the Light Volumes toggle property
            MaterialProperty useLightVolumes = FindProperty("_UseLightVolumes", properties, false);
            MaterialProperty lightVolumesAdditiveOnly = FindProperty("_LightVolumesAdditiveOnly", properties, false);
            MaterialProperty lightVolumeIntensity = FindProperty("_LightVolumeIntensity", properties, false);

            bool lightVolumesInstalled = IsLightVolumesInstalled();

            // Draw all properties except Light Volumes ones
            foreach (var prop in properties)
            {
                if (prop.name == "_UseLightVolumes" ||
                    prop.name == "_LightVolumesAdditiveOnly" ||
                    prop.name == "_LightVolumeIntensity")
                    continue;

                materialEditor.ShaderProperty(prop, prop.displayName);
            }

            // Draw Light Volumes section with dependency check
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Light Volumes Integration", EditorStyles.boldLabel);

            if (!lightVolumesInstalled)
            {
                EditorGUILayout.HelpBox(
                    "Light Volumes package (red.sim.lightvolumes) is not installed. " +
                    "Install it to enable AudioLink-responsive fog.",
                    MessageType.Info);

                using (new EditorGUI.DisabledScope(true))
                {
                    if (useLightVolumes != null)
                        materialEditor.ShaderProperty(useLightVolumes, "Sample Light Volumes");
                    if (lightVolumesAdditiveOnly != null)
                        materialEditor.ShaderProperty(lightVolumesAdditiveOnly, "Additive Only (Dynamic Lights)");
                    if (lightVolumeIntensity != null)
                        materialEditor.ShaderProperty(lightVolumeIntensity, "Light Volume Intensity");
                }

                // Force disable if somehow enabled
                if (useLightVolumes != null && useLightVolumes.floatValue > 0)
                {
                    useLightVolumes.floatValue = 0;
                    Material mat = materialEditor.target as Material;
                    if (mat != null)
                        mat.DisableKeyword("_USE_LIGHT_VOLUMES");
                }
            }
            else
            {
                if (useLightVolumes != null)
                    materialEditor.ShaderProperty(useLightVolumes, "Sample Light Volumes");
                if (lightVolumesAdditiveOnly != null)
                    materialEditor.ShaderProperty(lightVolumesAdditiveOnly, "Additive Only (Dynamic Lights)");
                if (lightVolumeIntensity != null)
                    materialEditor.ShaderProperty(lightVolumeIntensity, "Light Volume Intensity");
            }
        }
    }
}
