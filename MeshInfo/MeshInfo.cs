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
            get { return "Mesh Info 1.0"; }
        }

        public string Description
        {
            get { return "Hit Ctrl + M to get information about meshes"; }
        }
        #endregion
        private static GUI.UIMainPanel m_mainPanel;

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

           /* m_mainPanel.parent.eventKeyPress += (c, t) =>
                {
                    if(Input.GetKeyDown(KeyCode.LeftAlt) &&
                       Input.GetKeyDown(KeyCode.LeftShift) &&
                       t.keycode == KeyCode.M)
                    {
                        m_mainPanel.isVisible = !m_mainPanel.isVisible;
                    }

                };*/
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

        public static int GetTriangles(PrefabInfo prefab)
        {
            Mesh mesh = null;
            Mesh lodmesh = null;

            if (prefab is BuildingInfo)
                mesh = (prefab as BuildingInfo).m_mesh;
            else if (prefab is PropInfo)
                mesh = (prefab as PropInfo).m_mesh;
            else if (prefab is TreeInfo)
                mesh = (prefab as TreeInfo).m_mesh;
            else if (prefab is VehicleInfo)
                mesh = (prefab as VehicleInfo).m_mesh;

            if (mesh != null && mesh.isReadable)
                return mesh.triangles.Length / 3; // A triangle is 3 points right?

            return 0;
        }
    }
}
