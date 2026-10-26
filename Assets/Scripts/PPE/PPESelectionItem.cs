using UnityEngine;
using CBRN.Interactions;

namespace CBRN.PPE
{
    /// <summary>
    /// Interactive 3D object on the PPE selection locker/table.
    /// Implements IGazeInteractable so trainee can inspect and equip the gear via gaze-dwell or tap.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class PPESelectionItem : MonoBehaviour, IGazeInteractable
    {
        [Header("PPE Data")]
        [Tooltip("The ScriptableObject defining this PPE item.")]
        [SerializeField] private PPEItemData ppeData;

        [Header("Visual Elements")]
        [SerializeField] private Renderer itemRenderer;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color hoverColor = Color.yellow;
        [SerializeField] private Color equippedColor = Color.green;

        private Material _instancedMaterial;
        private bool _isEquipped = false;
        private PPESelectionStation _station;

        public PPEItemData ItemData => ppeData;

        public void Initialize(PPEItemData data, PPESelectionStation station)
        {
            ppeData = data;
            _station = station;

            if (itemRenderer == null) itemRenderer = GetComponent<Renderer>();
            if (itemRenderer != null)
            {
                _instancedMaterial = itemRenderer.material;
                normalColor = data != null ? data.rackDisplayColor : Color.white;
                _instancedMaterial.color = normalColor;
            }
        }

        public void SetEquippedState(bool isEquipped)
        {
            _isEquipped = isEquipped;
            if (_instancedMaterial != null)
            {
                _instancedMaterial.color = isEquipped ? equippedColor : normalColor;
            }
        }

        public void OnGazeEnter()
        {
            if (!_isEquipped && _instancedMaterial != null)
            {
                _instancedMaterial.color = hoverColor;
            }
        }

        public void OnGazeStay(float normalizedProgress)
        {
            // Optional dwell feedback
        }

        public void OnGazeExit()
        {
            if (_instancedMaterial != null)
            {
                _instancedMaterial.color = _isEquipped ? equippedColor : normalColor;
            }
        }

        public void OnGazeTrigger()
        {
            if (_station != null && ppeData != null)
            {
                _station.SelectPPE(this);
            }
        }

        private void OnDestroy()
        {
            if (_instancedMaterial != null)
            {
                Destroy(_instancedMaterial);
            }
        }
    }
}
