using System;

using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Globalization;

namespace MeshInfo.GUI
{
    public class UIPrefabItem : UIPanel
    {
        private UILabel m_name;
        private UILabel m_triangles;
        private UILabel m_lodTriangles;
        private UILabel m_weight;
        private UILabel m_lodWeight;
        private UILabel m_textureSize;
        private UITextField m_steamID;
        private UIPanel m_background;

        private PrefabInfo m_prefab;

        public PrefabInfo prefab
        {
            get { return m_prefab; }
            set {
                if (m_prefab == value) return;

                m_prefab = value;
                Refresh();
            }
        }

        public UIPanel background
        {
            get
            {
                if (m_background == null)
                {
                    m_background = AddUIComponent<UIPanel>();
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
            width = 670f;
            height = 40f;

            m_name = AddUIComponent<UILabel>();
            m_name.textScale = 0.9f;
            m_name.width = 300f;
            m_name.height = height;
            m_name.textAlignment = UIHorizontalAlignment.Left;
            m_name.pivot = UIPivotPoint.MiddleLeft;
            m_name.relativePosition = new Vector3(10f, 0f);

            m_textureSize = AddUIComponent<UILabel>();
            m_textureSize.textScale = 0.9f;
            m_textureSize.width = 90f;
            m_textureSize.height = height;
            m_textureSize.textAlignment = UIHorizontalAlignment.Center;
            m_textureSize.pivot = UIPivotPoint.MiddleCenter;
            m_textureSize.padding = new RectOffset(0, 10, 0, 0);
            m_textureSize.AlignTo(this, UIAlignAnchor.TopRight);

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
                if (m_prefab.name.Contains("."))
                    m_steamID.text = m_prefab.name.Substring(0, m_prefab.name.IndexOf("."));
            };

            Refresh();
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

        private void Refresh()
        {
            if (m_name == null) return;

            m_name.text = MeshInfo.GetLocalizedName(m_prefab);

            if (m_prefab.name.Contains("."))
            {
                int id;
                m_steamID.text = m_prefab.name.Substring(0, m_prefab.name.IndexOf("."));
                m_steamID.isVisible = Int32.TryParse(m_steamID.text, out id);
            }
            else
                m_steamID.isVisible = false;

            int triangles;
            int lodTriangles;
            float weight;
            float lodWeight;
            MeshInfo.GetTriangleInfo(prefab, out triangles, out lodTriangles, out weight, out lodWeight);

            m_triangles.text = (triangles > 0) ? triangles.ToString("N0") : "-";
            m_lodTriangles.text = (lodTriangles > 0) ? lodTriangles.ToString("N0") : "-";

            m_weight.text = (weight > 0) ? weight.ToString("N2") : "-";
            if (weight >= 200)
                m_weight.textColor = new Color32(255, 0, 0, 255);
            else if (weight >= 100)
                m_weight.textColor = new Color32(255, 255, 0, 255);
            else if (weight > 0)
                m_weight.textColor = new Color32(0, 255, 0, 255);
            else
                m_weight.textColor = new Color32(255, 255, 255, 255);

            m_lodWeight.text = (lodWeight > 0) ? lodWeight.ToString("N2") : "-";
            if (lodWeight >= 10)
                m_lodWeight.textColor = new Color32(255, 0, 0, 255);
            else if (lodWeight >= 5)
                m_lodWeight.textColor = new Color32(255, 255, 0, 255);
            else if (lodWeight > 0)
                m_lodWeight.textColor = new Color32(0, 255, 0, 255);
            else
                m_lodWeight.textColor = new Color32(255, 255, 255, 255);

            Vector2 textureSize = MeshInfo.GetTextureSize(m_prefab);
            m_textureSize.text = (textureSize != Vector2.zero) ? textureSize.x + "x" + textureSize.y : "-";
        }
    }
}
