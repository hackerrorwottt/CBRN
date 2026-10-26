using UnityEngine;

namespace CBRN.PPE
{
    /// <summary>
    /// Configurable data container defining a PPE ensemble item.
    /// ScriptableObject ensures zero hardcoded PPE stats or descriptions.
    /// </summary>
    [CreateAssetMenu(fileName = "PPEItem_", menuName = "CBRN/Data/PPE Item", order = 1)]
    public class PPEItemData : ScriptableObject
    {
        [Header("Identification")]
        [Tooltip("Display name of the PPE ensemble.")]
        public string itemName = "Level A Vapor Suit";

        [Tooltip("Educational description of the ensemble components.")]
        [TextArea(2, 4)]
        public string description = "Fully encapsulating chemical-protective suit with positive pressure SCBA. Required for unknown chemical vapor hazards.";

        [Header("Protection Parameters")]
        [Tooltip("The protection level classification.")]
        public PPELevel protectionLevel = PPELevel.LevelA_FullyEncapsulated;

        [Tooltip("Whether this ensemble provides certified protection against airborne chemical vapor/gas.")]
        public bool protectsAgainstVapors = true;

        [Tooltip("Color tint used for visual identification on the selection rack.")]
        public Color rackDisplayColor = new Color(0.95f, 0.75f, 0.1f);

        [Header("Training Weights")]
        [Tooltip("Penalization points if the trainee enters the Hot Zone wearing this item during a vapor leak.")]
        public int incorrectSelectionPenalty = 20;
    }
}

