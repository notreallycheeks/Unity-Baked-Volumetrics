using UdonSharp;
using UnityEngine;
using VRC.SDKBase;

namespace BakedVolumetrics
{
    /// <summary>
    /// Manages fog exclusion volumes and updates shader parameters.
    /// Place one instance in your scene and assign exclusion volumes to it.
    /// </summary>
    [UdonBehaviourSyncMode(BehaviourSyncMode.None)]
    [AddComponentMenu("Baked Volumetrics/Fog Exclusion Manager")]
    public class FogExclusionManager : UdonSharpBehaviour
    {
        private const int MAX_EXCLUSION_BOXES = 8;

        [Header("Exclusion Volumes")]
        [Tooltip("Assign all FogExclusionVolume objects in your scene here (max 8)")]
        public Transform[] exclusionVolumes = new Transform[0];

        [Header("Update Settings")]
        [Tooltip("Update shader every frame (enable if volumes move at runtime)")]
        public bool continuousUpdate = false;

        [Tooltip("Update interval in seconds (0 = every frame). Only used if continuousUpdate is true.")]
        public float updateInterval = 0f;

        // Arrays for shader data
        private Vector4[] _positions;
        private Vector4[] _sizes;
        private Matrix4x4[] _worldToLocalMatrices;
        private float[] _useRotation;

        // Cached shader property IDs
        private int _propPositions;
        private int _propSizes;
        private int _propWorldToLocal;
        private int _propCount;
        private int _propUseRotation;

        private float _lastUpdateTime;
        private bool _initialized;

        private void Start()
        {
            // Cache property IDs using VRCShader (whitelisted in Udon)
            // Property names must start with _Udon for VRChat
            _propPositions = VRCShader.PropertyToID("_UdonExclusionBoxPositions");
            _propSizes = VRCShader.PropertyToID("_UdonExclusionBoxSizes");
            _propWorldToLocal = VRCShader.PropertyToID("_UdonExclusionBoxWorldToLocal");
            _propCount = VRCShader.PropertyToID("_UdonExclusionBoxCount");
            _propUseRotation = VRCShader.PropertyToID("_UdonExclusionBoxUseRotation");

            // Initialize arrays
            _positions = new Vector4[MAX_EXCLUSION_BOXES];
            _sizes = new Vector4[MAX_EXCLUSION_BOXES];
            _worldToLocalMatrices = new Matrix4x4[MAX_EXCLUSION_BOXES];
            _useRotation = new float[MAX_EXCLUSION_BOXES];

            // Initialize with identity matrices
            for (int i = 0; i < MAX_EXCLUSION_BOXES; i++)
            {
                _worldToLocalMatrices[i] = Matrix4x4.identity;
            }

            _initialized = true;

            // Initial update
            UpdateShaderProperties();
        }

        private void Update()
        {
            if (!_initialized) return;
            if (!continuousUpdate) return;

            if (updateInterval > 0f)
            {
                if (Time.time - _lastUpdateTime >= updateInterval)
                {
                    UpdateShaderProperties();
                    _lastUpdateTime = Time.time;
                }
            }
            else
            {
                UpdateShaderProperties();
            }
        }

        /// <summary>
        /// Manually trigger shader property update
        /// </summary>
        public void UpdateShaderProperties()
        {
            if (!_initialized) return;

            int count = exclusionVolumes.Length;
            if (count > MAX_EXCLUSION_BOXES) count = MAX_EXCLUSION_BOXES;

            for (int i = 0; i < MAX_EXCLUSION_BOXES; i++)
            {
                if (i < count && exclusionVolumes[i] != null)
                {
                    Transform t = exclusionVolumes[i];

                    _positions[i] = new Vector4(t.position.x, t.position.y, t.position.z, 0f);

                    Vector3 halfScale = t.lossyScale * 0.5f;
                    _sizes[i] = new Vector4(halfScale.x, halfScale.y, halfScale.z, 0f);

                    _worldToLocalMatrices[i] = t.worldToLocalMatrix;
                    _useRotation[i] = (t.rotation != Quaternion.identity) ? 1f : 0f;
                }
                else
                {
                    _positions[i] = Vector4.zero;
                    _sizes[i] = Vector4.zero;
                    _worldToLocalMatrices[i] = Matrix4x4.identity;
                    _useRotation[i] = 0f;
                }
            }

            // Use VRCShader for global shader properties (VRChat whitelisted API)
            VRCShader.SetGlobalVectorArray(_propPositions, _positions);
            VRCShader.SetGlobalVectorArray(_propSizes, _sizes);
            VRCShader.SetGlobalMatrixArray(_propWorldToLocal, _worldToLocalMatrices);
            VRCShader.SetGlobalInteger(_propCount, count);
            VRCShader.SetGlobalFloatArray(_propUseRotation, _useRotation);
        }
    }
}
