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

        private static GameObject m_gameObject;
        private static GUI.UIMainPanel m_mainPanel;

        public static readonly string version = "1.3.1";

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
            m_gameObject = new GameObject("MeshInfo");
            m_gameObject.transform.SetParent(view.transform);

            try
            {
                m_mainPanel = m_gameObject.AddComponent<GUI.UIMainPanel>();
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
            try
            {
                if (m_mainPanel == null) return;
                m_mainPanel.parent.RemoveUIComponent(m_mainPanel);
                GameObject.Destroy(m_mainPanel);
                GameObject.Destroy(m_gameObject);
                m_mainPanel = null;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        public override void OnReleased()
        {
            OnLevelUnloading();
        }
        #endregion
    }
}
