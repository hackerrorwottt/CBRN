using System;
using System.Collections.Generic;
using UnityEngine;

namespace CBRN.PPE
{
    /// <summary>
    /// Manages the PPE donning locker in the Cold Zone.
    /// Exposes available PPE ensembles as interactive 3D items,
    /// equips chosen gear to the PlayerPPEState, and triggers events for the MissionManager.
    /// </summary>
    public class PPESelectionStation : MonoBehaviour
    {
        [Header("Available Gear Configurations")]
        [Tooltip("The list of PPE items presented to the trainee (e.g. Level D, Level C, Level A).")]
        [SerializeField] private List<PPEItemData> availableGear = new List<PPEItemData>();

        [Header("Target Trainee State")]
        [Tooltip("Reference to the player's PPE state component.")]
        [SerializeField] private PlayerPPEState playerPPEState;

        [Header("Item Placement")]
        [Tooltip("Child transforms where PPE items should be spawned/aligned.")]
        [SerializeField] private Transform[] rackSlots;

        // Events
        public event Action<PPEItemData> OnPPEChanged;

        private List<PPESelectionItem> _spawnedItems = new List<PPESelectionItem>();

        private void Start()
        {
            if (playerPPEState == null)
            {
                playerPPEState = FindObjectOfType<PlayerPPEState>();
            }

            SetupRackItems();
        }

        private void SetupRackItems()
        {
            // If physical slot children exist, populate them
            PPESelectionItem[] existing = GetComponentsInChildren<PPESelectionItem>();
            if (existing.Length > 0)
            {
                for (int i = 0; i < existing.Length; i++)
                {
                    if (i < availableGear.Count)
                    {
                        existing[i].Initialize(availableGear[i], this);
                        _spawnedItems.Add(existing[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Called when trainee selects an item on the rack via gaze/tap.
        /// </summary>
        public void SelectPPE(PPESelectionItem selectedItem)
        {
            if (selectedItem == null || selectedItem.ItemData == null) return;

            // Update visuals on all rack items
            foreach (var item in _spawnedItems)
            {
                item.SetEquippedState(item == selectedItem);
            }

            // Equip on player
            if (playerPPEState != null)
            {
                playerPPEState.EquipPPE(selectedItem.ItemData);
            }

            OnPPEChanged?.Invoke(selectedItem.ItemData);
        }
    }
}

