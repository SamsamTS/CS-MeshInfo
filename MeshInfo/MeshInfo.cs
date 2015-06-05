using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using System;
using UnityEngine;

namespace MeshInfo
{
    public class MeshInfo : LoadingExtensionBase, IUserMod
    {
        #region IUserMod implementation
        public string Name
        {
            get { return "Mesh Info " + version; }
        }

        public string Description
        {
            get { return "Load a save then hit Ctrl + M to get information about asset meshes"; }
        }
        #endregion
        private static GUI.UIMainPanel m_mainPanel;

        public static readonly string version = "1.1.1";

        public static bool stopLoading = false;
                
        #region LoadingExtensionBase overrides
        /// <summary>
        /// Called when the level (game, map editor, asset editor) is loaded
        /// </summary>
        public override void OnLevelLoaded(LoadMode mode)
        {
            // Is it an actual game ?
            if (mode != LoadMode.LoadGame && mode != LoadMode.NewGame)
                return;

            // Creating GUI
            UIView view = UIView.GetAView();

            try
            {
                m_mainPanel = (GUI.UIMainPanel)view.AddUIComponent(typeof(GUI.UIMainPanel));
            }
            catch(Exception e)
            {
                stopLoading = true;

                DebugUtils.Warning("Couldn't create the UI. Please relaunch the game.");
                Debug.LogException(e);
            }
        }

        /// <summary>
        /// Called when the level is unloaded
        /// </summary>
        public override void OnLevelUnloading()
        {
            base.OnLevelUnloading();

            if (m_mainPanel == null) return;
            m_mainPanel.parent.RemoveUIComponent(m_mainPanel);
            GameObject.Destroy(m_mainPanel);
        }

        public override void OnReleased()
        {
            base.OnReleased();

            if (m_mainPanel == null) return;
            m_mainPanel.parent.RemoveUIComponent(m_mainPanel);
            GameObject.Destroy(m_mainPanel);
        }
        #endregion

        public static string GetLocalizedName(PrefabInfo prefab)
        {
            string localizedName = Locale.GetUnchecked("VEHICLE_TITLE", prefab.name);

            if (localizedName.StartsWith("VEHICLE_TITLE"))
            {
                localizedName = prefab.name;
                // Removes the steam ID and trailing _Data from the name
                localizedName = localizedName.Substring(localizedName.IndexOf('.') + 1).Replace("_Data", "");
            }

            return localizedName;
        }


        public static void GetTriangleInfo(PrefabInfo prefab, out int triangles, out int lodTriangles, out float weight, out float lodWeight)
        {
            Mesh mesh = null;
            Mesh lodmesh = null;

            triangles = 0;
            lodTriangles = 0;
            weight = 0;
            lodWeight = 0;

            if (prefab is BuildingInfo)
            {
                mesh = (prefab as BuildingInfo).m_mesh;
                lodmesh = (prefab as BuildingInfo).m_lodMesh;
            }
            else if (prefab is PropInfo)
            {
                mesh = (prefab as PropInfo).m_mesh;
                lodmesh = (prefab as PropInfo).m_lodMesh;
            }
            else if (prefab is TreeInfo)
            {
                mesh = (prefab as TreeInfo).m_mesh;
                lodmesh = (prefab as TreeInfo).m_lodMesh16;
            }
            else if (prefab is VehicleInfo)
            {
                mesh = (prefab as VehicleInfo).m_mesh;
                lodmesh = (prefab as VehicleInfo).m_lodMesh;
            }

            if (mesh != null && mesh.isReadable)
                triangles = mesh.triangles.Length / 3; // A triangle is 3 points right?

            if (lodmesh != null && lodmesh.isReadable)
                lodTriangles = lodmesh.triangles.Length / 3;
            
            if (triangles != 0 && mesh.bounds != null)
            {
                Vector3 boundsSize = mesh.bounds.size;
                float cubicMeters = boundsSize.x + boundsSize.y + boundsSize.z;
                if(Mathf.Round(cubicMeters) != 0)
                    weight = triangles / cubicMeters;
            }

            if (lodTriangles != 0 && lodmesh.bounds != null)
            {
                Vector3 boundsSize = lodmesh.bounds.size;
                float cubicMeters = boundsSize.x + boundsSize.y + boundsSize.z;
                if (Mathf.Round(cubicMeters) != 0)
                    lodWeight = lodTriangles / cubicMeters;
            }
        }

        public static Vector2 GetTextureSize(PrefabInfo prefab)
        {
            Material material = null;
            if (prefab is BuildingInfo)
                material = (prefab as BuildingInfo).m_material;
            else if (prefab is PropInfo)
                material = (prefab as PropInfo).m_material;
            else if (prefab is TreeInfo)
                material = (prefab as TreeInfo).m_material;
            else if (prefab is VehicleInfo)
                material = (prefab as VehicleInfo).m_material;

            if (material != null && material.mainTexture != null)
                return new Vector2(material.mainTexture.width, material.mainTexture.height) ;

            if (prefab is BuildingInfo)
                material = (prefab as BuildingInfo).m_lodMaterial;
            else if (prefab is PropInfo)
                material = (prefab as PropInfo).m_lodMaterial;
            else if (prefab is TreeInfo)
                material = (prefab as TreeInfo).m_lodMaterial;
            else if (prefab is VehicleInfo)
                material = (prefab as VehicleInfo).m_lodMaterial;

            if (material != null && material.mainTexture != null)
                return new Vector2(material.mainTexture.width, material.mainTexture.height);

            return Vector2.zero;
        }
    }
}
