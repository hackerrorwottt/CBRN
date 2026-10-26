#if UNITY_EDITOR
using System.IO;
using UnityEngine;
using UnityEditor;
using CBRN.PPE;
using CBRN.Hazards;
using CBRN.Core;
using CBRN.Assessment;
using CBRN.AI;
using CBRN.Rescue;
using CBRN.Environment;
using CBRN.Network;
using CBRN.UI;

namespace CBRN.Core
{
    /// <summary>
    /// Editor automation tool: Generates all required ScriptableObject assets with exact
    /// scenario parameters, and assembles the complete CBRN training scene with all systems wired.
    /// Menu item: CBRN -> Setup Complete Training Scenario.
    /// </summary>
    public static class ScenarioAssetInitializer
    {
        private const string SoFolder = "Assets/ScriptableObjects";

        [MenuItem("CBRN/1. Generate Scenario ScriptableObjects")]
        public static void GenerateScriptableObjects()
        {
            if (!AssetDatabase.IsValidFolder(SoFolder))
            {
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            }

            // 1. PPE Items
            PPEItemData ppeD = CreateOrLoadAsset<PPEItemData>($"{SoFolder}/PPE_LevelD_WorkCoveralls.asset", asset =>
            {
                asset.itemName = "Level D Work Coveralls";
                asset.description = "Standard industrial work uniform. Provides zero chemical or respiratory protection.";
                asset.protectionLevel = PPELevel.LevelD_WorkCoveralls;
                asset.protectsAgainstVapors = false;
                asset.rackDisplayColor = new Color(0.3f, 0.5f, 0.7f);
                asset.incorrectSelectionPenalty = 30;
            });

            PPEItemData ppeC = CreateOrLoadAsset<PPEItemData>($"{SoFolder}/PPE_LevelC_Respirator.asset", asset =>
            {
                asset.itemName = "Level C Splash Suit & Air-Purifying Respirator";
                asset.description = "Chemical splash coveralls with cartridge mask. Only suitable for known particulates; NOT certified for high vapor concentrations.";
                asset.protectionLevel = PPELevel.LevelC_AirPurifyingResp;
                asset.protectsAgainstVapors = false;
                asset.rackDisplayColor = new Color(0.9f, 0.6f, 0.1f);
                asset.incorrectSelectionPenalty = 20;
            });

            PPEItemData ppeA = CreateOrLoadAsset<PPEItemData>($"{SoFolder}/PPE_LevelA_VaporSuit.asset", asset =>
            {
                asset.itemName = "Level A Vapor-Protective Encapsulated Ensemble";
                asset.description = "Gas-tight fully encapsulating chemical suit with internal SCBA. Mandatory for unconfined industrial chemical vapor leaks.";
                asset.protectionLevel = PPELevel.LevelA_FullyEncapsulated;
                asset.protectsAgainstVapors = true;
                asset.rackDisplayColor = new Color(0.1f, 0.8f, 0.3f);
                asset.incorrectSelectionPenalty = 0;
            });

            // 2. Hazard Profile
            HazardProfileData hazard = CreateOrLoadAsset<HazardProfileData>($"{SoFolder}/Hazard_IndustrialChemicalVapor.asset", asset =>
            {
                asset.hazardName = "Toxic Industrial Vapor Plume (Analogue)";
                asset.placardCode = "UN 1005 (Ammonia Analogue) / Class 2.3";
                asset.hazardDescription = "Rapidly expanding corrosive and toxic gas cloud. Inhalation & skin absorption hazard.";
                asset.minimumRequiredPPE = PPELevel.LevelA_FullyEncapsulated;
                asset.initialRadius = 2.5f;
                asset.baseSpreadSpeed = 0.35f;
                asset.maxSpreadRadius = 16.0f;
                asset.dissipationSpeed = 1.2f;
                asset.toxicityRatePerSecond = 15.0f;
                asset.incorrectEntryPenalty = 25;
            });

            // 3. Scoring Rubric
            ScoringRubricData rubric = CreateOrLoadAsset<ScoringRubricData>($"{SoFolder}/Rubric_StandardCBRN.asset", asset =>
            {
                asset.maxPossibleScore = 100;
                asset.passingScore = 70;
                asset.wrongPPEPenalty = 30;
                asset.prematureRescuePenalty = 20;
                asset.zoneViolationPenalty = 10;
                asset.timeOverrunPenaltyPerBlock = 5;
                asset.hintPenalty = 3;
                asset.maxAllowedCriticalMistakes = 3;
            });

            // 4. Scenario Steps (1 through 7)
            string[] titles = {
                "Briefing & Incident Acknowledgment",
                "Hazard Identification (UN Placard)",
                "PPE Selection (Don Level A Suit)",
                "Safety Zone Perimeter Assessment",
                "Casualty Triage & Victim Rescue",
                "Evacuation to Decon Triage Point",
                "Debrief & Performance Evaluation"
            };

            string[] objectives = {
                "Examine incident parameters at the Briefing Board in the Cold Zone.",
                "Inspect the UN hazard diamond placard mounted on the chemical tanks.",
                "Select and equip the certified Level A vapor-protective ensemble at the PPE rack.",
                "Cross the yellow buffer perimeter and assess the safe staging boundary.",
                "Locate the trapped technician in the Hot Zone and calm them via gaze triage.",
                "Escort the casualty out of the danger area into the Green Triage Zone.",
                "Review the simulation performance scorecard and mistake log."
            };

            string[] hints = {
                "Look at the large blue Briefing Board in front of your starting location.",
                "Look for the diamond hazard placard located on Tank Alpha.",
                "Equip the Green Level A suit. Do not use Level D or C for vapor plumes.",
                "Ensure your Level A suit is sealed before passing the yellow perimeter barriers.",
                "Approach the panicked technician and dwell/tap to initiate casualty extraction.",
                "Walk towards the Green Decontamination Tent in the Warm Zone.",
                "Review your score and areas for procedural improvement."
            };

            for (int i = 1; i <= 7; i++)
            {
                int index = i;
                CreateOrLoadAsset<ScenarioStepData>($"{SoFolder}/Step_{index}_{titles[index - 1].Replace(" ", "_")}.asset", asset =>
                {
                    asset.stepIndex = index;
                    asset.stepTitle = $"Step {index}: {titles[index - 1]}";
                    asset.objectiveText = objectives[index - 1];
                    asset.standardHint = hints[index - 1];
                    asset.basePoints = 15;
                    asset.targetTimeSeconds = index == 5 ? 120f : 60f;
                });
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ScenarioAssetInitializer] All 12 ScriptableObjects generated successfully!");
        }

        private static T CreateOrLoadAsset<T>(string path, System.Action<T> configure) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset == null)
            {
                asset = ScriptableObject.CreateInstance<T>();
                configure(asset);
                AssetDatabase.CreateAsset(asset, path);
            }
            else
            {
                configure(asset);
                EditorUtility.SetDirty(asset);
            }
            return asset;
        }
    }
}
#endif
