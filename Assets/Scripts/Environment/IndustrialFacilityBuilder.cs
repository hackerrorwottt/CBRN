using UnityEngine;
using CBRN.Environment;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CBRN.Environment
{
    /// <summary>
    /// Generates a complete, optimized industrial facility using Unity 3D primitives.
    /// Can be triggered in the Unity Inspector via the context menu button.
    /// </summary>
    public class IndustrialFacilityBuilder : MonoBehaviour
    {
        private const string RootName = "[--- INDUSTRIAL FACILITY ---]";

        [ContextMenu("Build Industrial Facility")]
        public void BuildFacility()
        {
            // Clean up existing layout if present
            ClearFacility();

            GameObject root = new GameObject(RootName);
            root.transform.position = Vector3.zero;

            // 1. Create Materials
            Material matConcrete = CreateOrGetMaterial("Mat_Concrete", new Color(0.45f, 0.45f, 0.48f));
            Material matSteel = CreateOrGetMaterial("Mat_IndustrialSteel", new Color(0.25f, 0.30f, 0.35f));
            Material matHazardYellow = CreateOrGetMaterial("Mat_HazardYellow", new Color(0.95f, 0.75f, 0.05f));
            Material matWarningStripe = CreateOrGetMaterial("Mat_WarningStripe", new Color(0.85f, 0.25f, 0.15f));
            Material matStagingBlue = CreateOrGetMaterial("Mat_StagingBlue", new Color(0.20f, 0.45f, 0.75f));
            Material matDeconGreen = CreateOrGetMaterial("Mat_DeconGreen", new Color(0.15f, 0.65f, 0.30f));
            Material matWoodCrate = CreateOrGetMaterial("Mat_WoodCrate", new Color(0.55f, 0.40f, 0.25f));

            // 2. Concrete Ground (40m x 40m footprint)
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground_ConcretePlaza";
            ground.transform.SetParent(root.transform);
            ground.transform.position = new Vector3(0f, -0.25f, 0f);
            ground.transform.localScale = new Vector3(40f, 0.5f, 40f);
            ground.GetComponent<Renderer>().sharedMaterial = matConcrete;
            SetStatic(ground);

            // 3. COLD ZONE: Staging & Command Area (Z: -14 to -8)
            GameObject coldZone = new GameObject("Zone_Cold_StagingArea");
            coldZone.transform.SetParent(root.transform);
            coldZone.transform.position = new Vector3(0f, 0f, -12f);

            // Briefing Board Assembly placed in front-right of trainee spawn (0, 0, -13), angled to face trainee
            GameObject briefingAssembly = new GameObject("Prop_BriefingAssembly");
            briefingAssembly.transform.SetParent(coldZone.transform);
            briefingAssembly.transform.position = new Vector3(2.8f, 0f, -10.5f);
            briefingAssembly.transform.rotation = Quaternion.LookRotation(new Vector3(2.8f, 0f, 2.5f));

            GameObject briefingBoard = GameObject.CreatePrimitive(PrimitiveType.Cube);
            briefingBoard.name = "Prop_BriefingBoard";
            briefingBoard.transform.SetParent(briefingAssembly.transform);
            briefingBoard.transform.localPosition = new Vector3(0f, 1.6f, 0f);
            briefingBoard.transform.localRotation = Quaternion.identity;
            briefingBoard.transform.localScale = new Vector3(3f, 1.8f, 0.1f);
            briefingBoard.GetComponent<Renderer>().sharedMaterial = matStagingBlue;
            SetStatic(briefingBoard);

            // Briefing Board Stand Posts
            CreatePost(briefingAssembly.transform, new Vector3(-1.4f, 0.8f, 0f), matSteel);
            CreatePost(briefingAssembly.transform, new Vector3(1.4f, 0.8f, 0f), matSteel);

            // PPE Selection Station Table (X: -4, Z: -12)
            GameObject ppeTable = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ppeTable.name = "Station_PPELocker";
            ppeTable.transform.SetParent(coldZone.transform);
            ppeTable.transform.position = new Vector3(-4f, 0.5f, -12f);
            ppeTable.transform.localScale = new Vector3(2.5f, 1.0f, 1.0f);
            ppeTable.GetComponent<Renderer>().sharedMaterial = matHazardYellow;
            SetStatic(ppeTable);

            // Cold Zone Trigger Volume
            CreateZoneTrigger(coldZone.transform, new Vector3(0f, 2.5f, 0f), new Vector3(18f, 5f, 10f), ZoneType.ColdZone_Staging);

            // 4. WARM ZONE: Perimeter & Decontamination (Z: -7 to -1)
            GameObject warmZone = new GameObject("Zone_Warm_BufferAndDecon");
            warmZone.transform.SetParent(root.transform);
            warmZone.transform.position = new Vector3(0f, 0f, -4f);

            // Perimeter Security Barriers (Left & Right with central gap)
            CreateBarrier(warmZone.transform, new Vector3(-8f, 0.5f, -6f), new Vector3(8f, 1.0f, 0.3f), matWarningStripe);
            CreateBarrier(warmZone.transform, new Vector3(8f, 0.5f, -6f), new Vector3(8f, 1.0f, 0.3f), matWarningStripe);

            // Triage / Evacuation Drop-Off Station (X: 6, Z: -4)
            GameObject deconTent = GameObject.CreatePrimitive(PrimitiveType.Cube);
            deconTent.name = "Station_TriageEvacuationPoint";
            deconTent.transform.SetParent(warmZone.transform);
            deconTent.transform.position = new Vector3(6f, 1.25f, -4f);
            deconTent.transform.localScale = new Vector3(4f, 2.5f, 4f);
            deconTent.GetComponent<Renderer>().sharedMaterial = matDeconGreen;
            SetStatic(deconTent);

            // Triage Zone Trigger Volume
            CreateZoneTrigger(deconTent.transform, Vector3.zero, new Vector3(5f, 4f, 5f), ZoneType.Evacuation_TriagePoint);

            // Warm Zone Trigger Volume
            CreateZoneTrigger(warmZone.transform, new Vector3(0f, 2.5f, 0f), new Vector3(24f, 5f, 6f), ZoneType.WarmZone_Perimeter);

            // 5. HOT ZONE: Chemical Storage & Ruptured Infrastructure (Z: 0 to 18)
            GameObject hotZone = new GameObject("Zone_Hot_ChemicalHazardArea");
            hotZone.transform.SetParent(root.transform);
            hotZone.transform.position = new Vector3(0f, 0f, 8f);

            // Storage Tank Alpha (Cylinder, X: -5, Z: 8)
            CreateCylinder(hotZone.transform, "Tank_Alpha", new Vector3(-5f, 3.5f, 8f), new Vector3(4f, 3.5f, 4f), matSteel);

            // Storage Tank Beta (Cylinder, X: 5, Z: 8)
            CreateCylinder(hotZone.transform, "Tank_Beta", new Vector3(5f, 3.5f, 8f), new Vector3(4f, 3.5f, 4f), matSteel);

            // Central Chemical Processing Vessel (X: 0, Z: 12)
            CreateCylinder(hotZone.transform, "Vessel_MainReactor", new Vector3(0f, 4f, 12f), new Vector3(5f, 4f, 5f), matSteel);

            // Overhead Connecting Pipes
            CreatePipe(hotZone.transform, "Pipe_AlphaToReactor", new Vector3(-2.5f, 5.5f, 10f), new Vector3(0.4f, 2.8f, 0.4f), new Vector3(0f, 0f, 45f), matHazardYellow);
            CreatePipe(hotZone.transform, "Pipe_BetaToReactor", new Vector3(2.5f, 5.5f, 10f), new Vector3(0.4f, 2.8f, 0.4f), new Vector3(0f, 0f, -45f), matHazardYellow);

            // Rupture Site Pipe & Valve Junction (The Leak Origin at X: -1.5, Z: 5.5)
            GameObject ruptureSource = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            ruptureSource.name = "Source_RupturedPipeJunction";
            ruptureSource.transform.SetParent(hotZone.transform);
            ruptureSource.transform.position = new Vector3(-1.5f, 1.2f, 5.5f);
            ruptureSource.transform.localScale = new Vector3(0.8f, 1.2f, 0.8f);
            ruptureSource.GetComponent<Renderer>().sharedMaterial = matWarningStripe;
            SetStatic(ruptureSource);

            // Spill Retention Basin (Flat cylinder on the ground under the rupture)
            GameObject spillBasin = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            spillBasin.name = "Spill_RetentionBasin";
            spillBasin.transform.SetParent(hotZone.transform);
            spillBasin.transform.position = new Vector3(-1.5f, 0.02f, 5.5f);
            spillBasin.transform.localScale = new Vector3(5.0f, 0.02f, 5.0f);
            spillBasin.GetComponent<Renderer>().sharedMaterial = matHazardYellow;
            SetStatic(spillBasin);

            // Industrial Obstacles (Crates & Barriers creating cover for trapped victims)
            CreateCrate(hotZone.transform, new Vector3(2.5f, 0.75f, 4f), new Vector3(1.5f, 1.5f, 1.5f), matWoodCrate);
            CreateCrate(hotZone.transform, new Vector3(2.5f, 0.5f, 5.8f), new Vector3(1.0f, 1.0f, 1.0f), matWoodCrate);
            CreateCrate(hotZone.transform, new Vector3(-4.5f, 0.6f, 3.5f), new Vector3(1.2f, 1.2f, 1.2f), matWoodCrate);
            CreateCrate(hotZone.transform, new Vector3(0f, 0.6f, 16.5f), new Vector3(2.0f, 1.2f, 1.0f), matWoodCrate);

            // Hot Zone Trigger Volume
            CreateZoneTrigger(hotZone.transform, new Vector3(0f, 3f, 0f), new Vector3(28f, 6f, 20f), ZoneType.HotZone_ChemicalLeak);

            Debug.Log("[IndustrialFacilityBuilder] Complete industrial compound successfully generated!");
        }

        [ContextMenu("Clear Facility")]
        public void ClearFacility()
        {
            GameObject existing = GameObject.Find(RootName);
            if (existing != null)
            {
                DestroyImmediate(existing);
                Debug.Log("[IndustrialFacilityBuilder] Cleared previous facility layout.");
            }
        }

        // --- Helper Methods ---

        private void SetStatic(GameObject go)
        {
#if UNITY_EDITOR
            // Enables Static Batching & NavMesh surface generation
            GameObjectUtility.SetStaticEditorFlags(go, StaticEditorFlags.BatchingStatic | StaticEditorFlags.NavigationStatic);
#endif
        }

        private void CreatePost(Transform parent, Vector3 localPos, Material mat)
        {
            GameObject post = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            post.name = "Post";
            post.transform.SetParent(parent);
            post.transform.localPosition = localPos;
            post.transform.localScale = new Vector3(0.1f, 0.8f, 0.1f);
            post.GetComponent<Renderer>().sharedMaterial = mat;
            SetStatic(post);
        }

        private void CreateBarrier(Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject barrier = GameObject.CreatePrimitive(PrimitiveType.Cube);
            barrier.name = "Barrier_Perimeter";
            barrier.transform.SetParent(parent);
            barrier.transform.position = pos;
            barrier.transform.localScale = scale;
            barrier.GetComponent<Renderer>().sharedMaterial = mat;
            SetStatic(barrier);
        }

        private void CreateCylinder(Transform parent, string name, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject cyl = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            cyl.name = name;
            cyl.transform.SetParent(parent);
            cyl.transform.position = pos;
            cyl.transform.localScale = scale;
            cyl.GetComponent<Renderer>().sharedMaterial = mat;
            SetStatic(cyl);
        }

        private void CreatePipe(Transform parent, string name, Vector3 pos, Vector3 scale, Vector3 rot, Material mat)
        {
            GameObject pipe = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            pipe.name = name;
            pipe.transform.SetParent(parent);
            pipe.transform.position = pos;
            pipe.transform.localScale = scale;
            pipe.transform.localEulerAngles = rot;
            pipe.GetComponent<Renderer>().sharedMaterial = mat;
            SetStatic(pipe);
        }

        private void CreateCrate(Transform parent, Vector3 pos, Vector3 scale, Material mat)
        {
            GameObject crate = GameObject.CreatePrimitive(PrimitiveType.Cube);
            crate.name = "Prop_Crate";
            crate.transform.SetParent(parent);
            crate.transform.position = pos;
            crate.transform.localScale = scale;
            crate.GetComponent<Renderer>().sharedMaterial = mat;
            SetStatic(crate);
        }

        private void CreateZoneTrigger(Transform parent, Vector3 localCenter, Vector3 size, ZoneType type)
        {
            GameObject trigger = new GameObject($"Trigger_{type}");
            trigger.transform.SetParent(parent);
            trigger.transform.localPosition = localCenter;

            BoxCollider box = trigger.AddComponent<BoxCollider>();
            box.isTrigger = true;
            box.size = size;

            ZoneIdentifier identifier = trigger.AddComponent<ZoneIdentifier>();
            // Set private field via reflection in editor script
            var field = typeof(ZoneIdentifier).GetField("zoneType", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            field?.SetValue(identifier, type);
        }

        private Material CreateOrGetMaterial(string matName, Color color)
        {
            Shader standardShader = Shader.Find("Mobile/Diffuse") ?? Shader.Find("Standard");
            Material mat = new Material(standardShader)
            {
                name = matName,
                color = color
            };
            return mat;
        }
    }
}

