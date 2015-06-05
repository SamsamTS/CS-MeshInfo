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

        public static readonly string version = "1.2";

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
    }
}
