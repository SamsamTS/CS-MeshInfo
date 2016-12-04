using System;

using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.PlatformServices;
using ColossalFramework.Globalization;

using UIUtils = SamsamTS.UIUtils;

namespace MeshInfo.GUI
{
    public class UIPrefabItem : UIPanel, IUIFastListRow
    {
        private UILabel m_name;
        private UILabel m_triangles;
        private UILabel m_lodTriangles;
        private UILabel m_weight;
        private UILabel m_lodWeight;
        private UILabel m_textureSize;
        private UILabel m_lodTextureSize;
        private UITextField m_steamID;
        private UIPanel m_background;

        private MeshData m_meshData;

        public UIPanel background
        {
            get
            {
                if (m_background == null)
                {
                    m_background = AddUIComponent<UIPanel>();
                    m_background.atlas = UIUtils.GetAtlas("Ingame");
                    m_background.width = width;
                    m_background.height = 40f;
                    m_background.relativePosition = Vector2.zero;

                    m_background.zOrder = 0;
                }

                return m_background;
            }
        }
        
        public override void Start()
        {
            base.Start();

            isVisible = true;
            canFocus = true;
            isInteractive = true;
            width = 740f;
            height = 40f;

            m_name = AddUIComponent<UILabel>();
            m_name.textScale = 0.9f;
            m_name.width = 300f;
            m_name.height = height;
            m_name.textAlignment = UIHorizontalAlignment.Left;
            m_name.pivot = UIPivotPoint.MiddleLeft;
            m_name.relativePosition = new Vector3(10f, 0f);

            m_lodTextureSize = AddUIComponent<UILabel>();
            m_lodTextureSize.textScale = 0.9f;
            m_lodTextureSize.width = 70f;
            m_lodTextureSize.height = height;
            m_lodTextureSize.textAlignment = UIHorizontalAlignment.Center;
            m_lodTextureSize.pivot = UIPivotPoint.MiddleCenter;
            m_lodTextureSize.padding = new RectOffset(0, 10, 0, 0);
            m_lodTextureSize.AlignTo(this, UIAlignAnchor.TopRight);

            m_textureSize = AddUIComponent<UILabel>();
            m_textureSize.textScale = 0.9f;
            m_textureSize.width = 90f;
            m_textureSize.height = height;
            m_textureSize.textAlignment = UIHorizontalAlignment.Center;
            m_textureSize.pivot = UIPivotPoint.MiddleCenter;
            m_textureSize.padding = new RectOffset(0, 10, 0, 0);
            m_textureSize.relativePosition = m_lodTextureSize.relativePosition - new Vector3(90f, 0f);

            m_lodWeight = AddUIComponent<UILabel>();
            m_lodWeight.textScale = 0.9f;
            m_lodWeight.width = 50f;
            m_lodWeight.height = height;
            m_lodWeight.textAlignment = UIHorizontalAlignment.Center;
            m_lodWeight.pivot = UIPivotPoint.MiddleCenter;
            m_lodWeight.relativePosition = m_textureSize.relativePosition - new Vector3(50f, 0f);

            m_weight = AddUIComponent<UILabel>();
            m_weight.textScale = 0.9f;
            m_weight.width = 50f;
            m_weight.height = height;
            m_weight.textAlignment = UIHorizontalAlignment.Center;
            m_weight.pivot = UIPivotPoint.MiddleCenter;
            m_weight.relativePosition = m_lodWeight.relativePosition - new Vector3(50f, 0f);

            m_lodTriangles = AddUIComponent<UILabel>();
            m_lodTriangles.textScale = 0.9f;
            m_lodTriangles.width = 50f;
            m_lodTriangles.height = height;
            m_lodTriangles.textAlignment = UIHorizontalAlignment.Center;
            m_lodTriangles.pivot = UIPivotPoint.MiddleCenter;
            m_lodTriangles.relativePosition = m_weight.relativePosition - new Vector3(50f, 0f);

            m_triangles = AddUIComponent<UILabel>();
            m_triangles.textScale = 0.9f;
            m_triangles.width = 80f;
            m_triangles.height = height;
            m_triangles.textAlignment = UIHorizontalAlignment.Center;
            m_triangles.pivot = UIPivotPoint.MiddleCenter;
            m_triangles.relativePosition = m_lodTriangles.relativePosition - new Vector3(80f, 0f);
            
            m_steamID = UIUtils.CreateTextField(this);
            m_steamID.normalBgSprite = null;
            m_steamID.padding = new RectOffset(5, 5, 14, 14);
            m_steamID.textScale = 0.8f;
            m_steamID.height = 40;
            m_steamID.textColor = new Color32(128, 128, 128, 255);
            m_steamID.selectionBackgroundColor = new Color32(0, 0, 0, 128);
            m_steamID.numericalOnly = true;
            m_steamID.relativePosition = m_triangles.relativePosition - new Vector3(100f, 0f);

            m_steamID.eventTextChanged += (c, t) =>
            {
                if (m_meshData.name.Contains("."))
                    m_steamID.text = m_meshData.name.Substring(0, m_meshData.name.IndexOf("."));
            };

            m_name.eventDoubleClick += (c, t) =>
            {
                if (PlatformService.IsOverlayEnabled() && !String.IsNullOrEmpty(m_steamID.text))
                {
                    PublishedFileId publishedFileId = new PublishedFileId((ulong)Int32.Parse(m_steamID.text));

                    if (publishedFileId != PublishedFileId.invalid)
                        PlatformService.ActivateGameOverlayToWorkshopItem(publishedFileId);
                }
            };
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Destroy(m_name);
            Destroy(m_triangles);
            Destroy(m_lodTriangles);
            Destroy(m_weight);
            Destroy(m_lodWeight);
            Destroy(m_textureSize);
            Destroy(m_steamID);
            Destroy(m_background);
        }

        public void Display(object prefab, bool isRowOdd)
        {
            m_meshData = prefab as MeshData;

            if (m_meshData == null || m_name == null) return;

            m_name.text = m_meshData.name;

            m_steamID.text = (m_meshData.steamID == null) ? "" : m_meshData.steamID;
            m_steamID.isVisible = (m_meshData.steamID != null);

            m_triangles.text = (m_meshData.triangles > 0) ? m_meshData.triangles.ToString("N0") : "-";
            m_lodTriangles.text = (m_meshData.lodTriangles > 0) ? m_meshData.lodTriangles.ToString("N0") : "-";

            m_weight.text = (m_meshData.weight > 0) ? m_meshData.weight.ToString("N2") : "-";
            if (m_meshData.weight >= 200)
                m_weight.textColor = new Color32(255, 0, 0, 255);
            else if (m_meshData.weight >= 100)
                m_weight.textColor = new Color32(255, 255, 0, 255);
            else if (m_meshData.weight > 0)
                m_weight.textColor = new Color32(0, 255, 0, 255);
            else
                m_weight.textColor = new Color32(255, 255, 255, 255);

            m_lodWeight.text = (m_meshData.lodWeight > 0) ? m_meshData.lodWeight.ToString("N2") : "-";
            if (m_meshData.lodWeight >= 10)
                m_lodWeight.textColor = new Color32(255, 0, 0, 255);
            else if (m_meshData.lodWeight >= 5)
                m_lodWeight.textColor = new Color32(255, 255, 0, 255);
            else if (m_meshData.lodWeight > 0)
                m_lodWeight.textColor = new Color32(0, 255, 0, 255);
            else
                m_lodWeight.textColor = new Color32(255, 255, 255, 255);

            m_textureSize.text = (m_meshData.textureSize != Vector2.zero) ? m_meshData.textureSize.x + "x" + m_meshData.textureSize.y : "-";
            m_lodTextureSize.text = (m_meshData.lodTextureSize != Vector2.zero) ? m_meshData.lodTextureSize.x + "x" + m_meshData.lodTextureSize.y : "-";

            if (isRowOdd)
            {
                background.backgroundSprite = "UnlockingItemBackground";
                background.color = new Color32(0, 0, 0, 128);
            }
            else
                background.backgroundSprite = null;
        }

        public void Select(bool isRowOdd) { }
        public void Deselect(bool isRowOdd) { }
    }
}
