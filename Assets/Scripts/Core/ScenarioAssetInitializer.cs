#if UNITY_EDITOR
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using CBRN.PPE;
using CBRN.Hazards;
using CBRN.Core;
using CBRN.Assessment;
using CBRN.AI;
using CBRN.Rescue;
using CBRN.Environment;
using CBRN.Network;
using CBRN.UI;
using CBRN.Interactions;

namespace CBRN.Core
{
    /// <summary>
    /// Editor automation tool: Generates all required ScriptableObject assets with exact
    /// scenario parameters, and assembles the complete CBRN training scene with all systems wired.
    /// Menu items:
    /// - CBRN -> 1. Generate Scenario ScriptableObjects
    /// - CBRN -> 2. Setup Complete Training Scene
    /// </summary>
    public static class ScenarioAssetInitializer
    {
        private const string SoFolder = "Assets/ScriptableObjects";
        private const string SceneFolder = "Assets/Scenes";
        private const string MainScenePath = "Assets/Scenes/MainTrainingScenario.unity";

        [MenuItem("CBRN/1. Generate Scenario ScriptableObjects")]
        public static void GenerateScriptableObjects()
        {
            if (!AssetDatabase.IsValidFolder(SoFolder))
            {
                AssetDatabase.CreateFolder("Assets", "ScriptableObjects");
            }

            // 1. PPE Items
            CreateOrLoadAsset<PPEItemData>($"{SoFolder}/PPE_LevelD_WorkCoveralls.asset", asset =>
            {
                asset.itemName = "Level D Work Coveralls";
                asset.description = "Standard industrial work uniform. Provides zero chemical or respiratory protection.";
                asset.protectionLevel = PPELevel.LevelD_WorkCoveralls;
                asset.protectsAgainstVapors = false;
                asset.rackDisplayColor = new Color(0.3f, 0.5f, 0.7f);
                asset.incorrectSelectionPenalty = 30;
            });

            CreateOrLoadAsset<PPEItemData>($"{SoFolder}/PPE_LevelC_Respirator.asset", asset =>
            {
                asset.itemName = "Level C Splash Suit & Air-Purifying Respirator";
                asset.description = "Chemical splash coveralls with cartridge mask. Only suitable for known particulates; NOT certified for high vapor concentrations.";
                asset.protectionLevel = PPELevel.LevelC_AirPurifyingResp;
                asset.protectsAgainstVapors = false;
                asset.rackDisplayColor = new Color(0.9f, 0.6f, 0.1f);
                asset.incorrectSelectionPenalty = 20;
            });

            CreateOrLoadAsset<PPEItemData>($"{SoFolder}/PPE_LevelA_VaporSuit.asset", asset =>
            {
                asset.itemName = "Level A Vapor-Protective Encapsulated Ensemble";
                asset.description = "Gas-tight fully encapsulating chemical suit with internal SCBA. Mandatory for unconfined industrial chemical vapor leaks.";
                asset.protectionLevel = PPELevel.LevelA_FullyEncapsulated;
                asset.protectsAgainstVapors = true;
                asset.rackDisplayColor = new Color(0.1f, 0.8f, 0.3f);
                asset.incorrectSelectionPenalty = 0;
            });

            // 2. Hazard Profile
            CreateOrLoadAsset<HazardProfileData>($"{SoFolder}/Hazard_IndustrialChemicalVapor.asset", asset =>
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
            CreateOrLoadAsset<ScoringRubricData>($"{SoFolder}/Rubric_StandardCBRN.asset", asset =>
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

        [MenuItem("CBRN/2. Setup Complete Training Scene")]
        public static void SetupCompleteScene()
        {
            // 1. Ensure ScriptableObjects are generated first
            GenerateScriptableObjects();

            if (!AssetDatabase.IsValidFolder(SceneFolder))
            {
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }

            // 2. Create new blank scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // 3. Directional Light (Optimized for Mobile VR)
            GameObject lightGo = new GameObject("Directional Light");
            Light light = lightGo.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.0f;
            light.shadows = LightShadows.Hard;
            lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

            // 4. Build Industrial Facility
            GameObject builderGo = new GameObject("_FacilityBuilder");
            IndustrialFacilityBuilder builder = builderGo.AddComponent<IndustrialFacilityBuilder>();
            builder.BuildFacility();

            // 5. Setup Player Rig & Cardboard Gaze Camera with Physics Trigger Detection
            GameObject playerRig = new GameObject("PlayerRig");
            playerRig.transform.position = new Vector3(0f, 0f, -13f);

            CapsuleCollider playerCol = playerRig.AddComponent<CapsuleCollider>();
            playerCol.radius = 0.45f;
            playerCol.height = 1.8f;
            playerCol.center = new Vector3(0f, 0.9f, 0f);

            Rigidbody playerRb = playerRig.AddComponent<Rigidbody>();
            playerRb.isKinematic = true;
            playerRb.useGravity = false;

            SafetyZoneMonitor safetyMonitor = playerRig.AddComponent<SafetyZoneMonitor>();

            PlayerPPEState playerPPE = playerRig.AddComponent<PlayerPPEState>();
            PPEItemData defaultPPE = AssetDatabase.LoadAssetAtPath<PPEItemData>($"{SoFolder}/PPE_LevelD_WorkCoveralls.asset");
            var initialPPEField = typeof(PlayerPPEState).GetField("initialPPE", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            initialPPEField?.SetValue(playerPPE, defaultPPE);

            GameObject camGo = new GameObject("Main Camera");
            camGo.transform.SetParent(playerRig.transform);
            camGo.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            camGo.tag = "MainCamera";

            Camera cam = camGo.AddComponent<Camera>();
            cam.nearClipPlane = 0.1f;
            camGo.AddComponent<AudioListener>();

            GazeInteractor interactor = camGo.AddComponent<GazeInteractor>();
            camGo.AddComponent<EditorVRSimulator>();

            // Reticle Visual
            GameObject reticleVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            reticleVisual.name = "ReticleVisual";
            Object.DestroyImmediate(reticleVisual.GetComponent<Collider>());
            reticleVisual.transform.SetParent(camGo.transform);
            reticleVisual.transform.localPosition = new Vector3(0f, 0f, 2.0f);
            reticleVisual.transform.localScale = new Vector3(0.04f, 0.04f, 0.04f);
            GazeReticle reticleComponent = reticleVisual.AddComponent<GazeReticle>();

            var reticleField = typeof(GazeInteractor).GetField("reticle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            reticleField?.SetValue(interactor, reticleComponent);

            // 6. Setup Briefing Board Interaction (Step 1 -> Step 2)
            GameObject briefingBoardObj = GameObject.Find("Prop_BriefingBoard");
            if (briefingBoardObj != null)
            {
                briefingBoardObj.AddComponent<BriefingBoard>();
            }

            // 7. Setup Managers & Rescue System
            GameObject managersGo = new GameObject("[--- MANAGERS ---]");
            MissionManager missionManager = managersGo.AddComponent<MissionManager>();
            AssessmentManager assessmentManager = managersGo.AddComponent<AssessmentManager>();
            HintSystem hintSystem = managersGo.AddComponent<HintSystem>();
            RescueSystem rescueSystem = managersGo.AddComponent<RescueSystem>();
            OfflineQueue offlineQueue = managersGo.AddComponent<OfflineQueue>();
            ApiClient apiClient = managersGo.AddComponent<ApiClient>();
            UIManager uiManager = managersGo.AddComponent<UIManager>();

            // Wire Rubric to AssessmentManager
            ScoringRubricData rubric = AssetDatabase.LoadAssetAtPath<ScoringRubricData>($"{SoFolder}/Rubric_StandardCBRN.asset");
            var rubricField = typeof(AssessmentManager).GetField("rubric", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            rubricField?.SetValue(assessmentManager, rubric);

            // Wire Scenario Steps to MissionManager
            List<ScenarioStepData> stepList = new List<ScenarioStepData>();
            for (int i = 1; i <= 7; i++)
            {
                var files = Directory.GetFiles(SoFolder, $"Step_{i}_*.asset");
                if (files.Length > 0)
                {
                    ScenarioStepData step = AssetDatabase.LoadAssetAtPath<ScenarioStepData>(files[0]);
                    if (step != null) stepList.Add(step);
                }
            }
            var stepsField = typeof(MissionManager).GetField("scenarioSteps", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            stepsField?.SetValue(missionManager, stepList);

            // 8. Setup PPE Selection Station in Cold Zone
            GameObject lockerStation = GameObject.Find("Station_PPELocker");
            if (lockerStation != null)
            {
                PPESelectionStation station = lockerStation.AddComponent<PPESelectionStation>();
                PPEItemData ppeD = AssetDatabase.LoadAssetAtPath<PPEItemData>($"{SoFolder}/PPE_LevelD_WorkCoveralls.asset");
                PPEItemData ppeC = AssetDatabase.LoadAssetAtPath<PPEItemData>($"{SoFolder}/PPE_LevelC_Respirator.asset");
                PPEItemData ppeA = AssetDatabase.LoadAssetAtPath<PPEItemData>($"{SoFolder}/PPE_LevelA_VaporSuit.asset");

                List<PPEItemData> gearList = new List<PPEItemData> { ppeD, ppeC, ppeA };
                var gearField = typeof(PPESelectionStation).GetField("availableGear", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                gearField?.SetValue(station, gearList);

                // Add 3 physical gear boxes seated cleanly on top of the 2.5m locker table
                CreatePPEBox(lockerStation.transform, "PPE_Box_LevelD", new Vector3(-0.32f, 0.7f, 0f), ppeD, station);
                CreatePPEBox(lockerStation.transform, "PPE_Box_LevelC", new Vector3(0.0f, 0.7f, 0f), ppeC, station);
                CreatePPEBox(lockerStation.transform, "PPE_Box_LevelA", new Vector3(0.32f, 0.7f, 0f), ppeA, station);
            }

            // 9. Setup Hazard Placard on Tank Alpha (parented to unscaled Hot Zone root to avoid cylinder scale distortion)
            GameObject tankAlpha = GameObject.Find("Tank_Alpha");
            if (tankAlpha != null)
            {
                GameObject placard = GameObject.CreatePrimitive(PrimitiveType.Cube);
                placard.name = "Placard_UN_HazardDiamond";
                placard.transform.SetParent(tankAlpha.transform.parent);
                placard.transform.position = tankAlpha.transform.position + new Vector3(0f, -1.7f, -2.05f);
                placard.transform.localScale = new Vector3(0.8f, 0.8f, 0.05f);
                placard.transform.rotation = Quaternion.Euler(0f, 0f, 45f); // Diamond orientation

                HazardPlacard placardComp = placard.AddComponent<HazardPlacard>();
                HazardProfileData hazardProfile = AssetDatabase.LoadAssetAtPath<HazardProfileData>($"{SoFolder}/Hazard_IndustrialChemicalVapor.asset");
                var profileField = typeof(HazardPlacard).GetField("hazardProfile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                profileField?.SetValue(placardComp, hazardProfile);
            }

            // 10. Setup Rupture Source Valve & Gas Plume
            GameObject ruptureSource = GameObject.Find("Source_RupturedPipeJunction");
            HazardSystem hazardSystem = null;
            if (ruptureSource != null)
            {
                // Valve Wheel (parented to unscaled Hot Zone root so wheel remains circular)
                GameObject valve = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                valve.name = "Valve_ShutoffWheel";
                valve.transform.SetParent(ruptureSource.transform.parent);
                valve.transform.position = ruptureSource.transform.position + new Vector3(0f, 0.1f, -0.5f);
                valve.transform.localScale = new Vector3(0.6f, 0.1f, 0.6f);
                valve.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

                ValveShutoff valveComp = valve.AddComponent<ValveShutoff>();

                // Gas Plume: unscaled root with SphereCollider + HazardSystem, and separate visual child
                // so scaling the visual does not multiply SphereCollider.radius exponentially.
                GameObject gasPlume = new GameObject("GasPlume_ChemicalVapor");
                gasPlume.transform.position = ruptureSource.transform.position;
                gasPlume.transform.localScale = Vector3.one;

                SphereCollider gasCol = gasPlume.AddComponent<SphereCollider>();
                gasCol.isTrigger = true;
                gasCol.radius = 2.5f;

                GameObject gasVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                gasVisual.name = "GasPlume_Visual";
                Object.DestroyImmediate(gasVisual.GetComponent<Collider>());
                gasVisual.transform.SetParent(gasPlume.transform);
                gasVisual.transform.localPosition = Vector3.zero;
                gasVisual.transform.localScale = new Vector3(5f, 3f, 5f);

                // Semi-transparent material so valve and casualty remain visible inside plume
                Shader shader = Shader.Find("Legacy Shaders/Transparent/Diffuse") ?? Shader.Find("Mobile/Diffuse") ?? Shader.Find("Standard");
                Material gasMat = new Material(shader) { color = new Color(0.85f, 0.80f, 0.2f, 0.35f) };
                gasVisual.GetComponent<Renderer>().sharedMaterial = gasMat;

                hazardSystem = gasPlume.AddComponent<HazardSystem>();
                HazardProfileData hazardProfile = AssetDatabase.LoadAssetAtPath<HazardProfileData>($"{SoFolder}/Hazard_IndustrialChemicalVapor.asset");
                var hazProfileField = typeof(HazardSystem).GetField("profile", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                hazProfileField?.SetValue(hazardSystem, hazardProfile);

                var visualField = typeof(HazardSystem).GetField("gasCloudVisual", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                visualField?.SetValue(hazardSystem, gasVisual.transform);

                var colliderField = typeof(HazardSystem).GetField("gasCollider", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                colliderField?.SetValue(hazardSystem, gasCol);

                var targetHazardField = typeof(ValveShutoff).GetField("targetHazard", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                targetHazardField?.SetValue(valveComp, hazardSystem);
            }

            // 11. Setup Casualty / Victim NPC with Kinematic Rigidbody
            GameObject victim = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            victim.name = "Victim_Casualty_NPC";
            victim.transform.position = new Vector3(2.5f, 1.0f, 7.5f); // Trapped behind crates
            victim.transform.localScale = new Vector3(0.8f, 1.0f, 0.8f);

            Rigidbody victimRb = victim.AddComponent<Rigidbody>();
            victimRb.isKinematic = true;
            victimRb.useGravity = false;

            victim.AddComponent<NavMeshAgent>();
            victim.AddComponent<VictimAI>();

            // 12. Setup Evacuation Point
            GameObject triagePoint = GameObject.Find("Station_TriageEvacuationPoint");
            if (triagePoint != null)
            {
                EvacuationSystem evac = triagePoint.AddComponent<EvacuationSystem>();
            }

            // 13. Setup Teleport Waypoints
            CreateWaypoint("Teleport_ColdZone_Staging", new Vector3(0f, 0.02f, -10f));
            CreateWaypoint("Teleport_WarmZone_Perimeter", new Vector3(0f, 0.02f, -4f));
            CreateWaypoint("Teleport_HotZone_Observation", new Vector3(0f, 0.02f, 3f));
            CreateWaypoint("Teleport_Decon_TriagePoint", new Vector3(6f, 0.02f, -4f));

            // 14. Save Complete Scene
            EditorSceneManager.SaveScene(scene, MainScenePath);

            // Register in Build Settings
            List<EditorBuildSettingsScene> buildScenes = new List<EditorBuildSettingsScene>
            {
                new EditorBuildSettingsScene(MainScenePath, true)
            };
            EditorBuildSettings.scenes = buildScenes.ToArray();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[ScenarioAssetInitializer] === SUCCESS: Complete CBRN Training Scene built and saved to {MainScenePath}! ===");
        }

        private static void CreatePPEBox(Transform parent, string name, Vector3 localPos, PPEItemData data, PPESelectionStation station)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            box.transform.SetParent(parent);
            box.transform.localPosition = localPos;
            // Compensate for parent Station_PPELocker scale (2.5, 1.0, 1.0) so each box is 0.5m x 0.4m x 0.4m
            box.transform.localScale = new Vector3(0.2f, 0.4f, 0.4f);

            PPESelectionItem item = box.AddComponent<PPESelectionItem>();
            item.Initialize(data, station);
        }

        private static void CreateWaypoint(string name, Vector3 pos)
        {
            GameObject node = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            node.name = name;
            node.transform.position = pos;
            node.transform.localScale = new Vector3(1.4f, 0.02f, 1.4f);
            node.AddComponent<TeleportNode>();
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
