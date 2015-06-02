using System;

using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Globalization;

namespace MeshInfo.GUI
{
    public class UIPrefabItem : UIPanel, IComparable
    {
        private UILabel m_name;
        private UILabel m_vertices;
        private UILabel m_textureSize;
        private UIPanel m_background;

        private PrefabInfo m_prefab;
        private int m_triangles;

        public PrefabInfo prefab
        {
            get { return m_prefab; }
            set {
                m_prefab = value;

                if (m_name == null) return;
                m_name.text = GetLocalizedName(m_prefab);
                m_vertices.text = GetTriangles(m_prefab);
                m_textureSize.text = GetTextureSize(m_prefab);
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
                    m_background.height = 40;
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
            width = parent.width;
            height = 40;

            m_name = AddUIComponent<UILabel>();
            m_name.textScale = 0.9f;
            m_name.text = GetLocalizedName(m_prefab);
            m_name.relativePosition = new Vector3(10, 13);
            
            m_vertices = AddUIComponent<UILabel>();
            m_vertices.textScale = 0.9f;
            m_vertices.text = GetTriangles(m_prefab);
            m_vertices.width = 90;
            m_vertices.textAlignment = UIHorizontalAlignment.Right;
            m_vertices.relativePosition = new Vector3(480, 13);

            m_textureSize = AddUIComponent<UILabel>();
            m_textureSize.textScale = 0.9f;
            m_textureSize.text = GetTextureSize(m_prefab);
            m_textureSize.width = 90;
            m_vertices.textAlignment = UIHorizontalAlignment.Right;
            m_textureSize.relativePosition = new Vector3(580, 13);

        }

        public int CompareTo(object o)
        {
            if (o == null || !(o is UIPrefabItem)) return 1;

            return (o as UIPrefabItem).m_triangles - m_triangles;
        }

        private string GetLocalizedName(PrefabInfo prefab)
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

        private string GetTriangles(PrefabInfo prefab)
        {
            Mesh mesh = null;
            Mesh lodmesh = null;

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
            }
            else if (prefab is VehicleInfo)
            {
                mesh = (prefab as VehicleInfo).m_mesh;
                lodmesh = (prefab as VehicleInfo).m_lodMesh;
            }

            if (mesh != null)
            {
                if (mesh.isReadable)
                    m_triangles = mesh.triangles.Length / 3; // A triangle is 3 points right?
                else
                    return "N/A";

                if (lodmesh == null)
                    return m_triangles.ToString();
                else if (lodmesh.isReadable)
                    return m_triangles.ToString() + " (" + (lodmesh.triangles.Length / 3).ToString() + ")";
                else
                    return m_triangles.ToString();
            }

            return "N/A";
        }


        private string GetTextureSize(PrefabInfo prefab)
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
                return material.mainTexture.width + "x" + material.mainTexture.height;

            if (prefab is BuildingInfo)
                material = (prefab as BuildingInfo).m_lodMaterial;
            else if (prefab is PropInfo)
                material = (prefab as PropInfo).m_lodMaterial;
            else if (prefab is TreeInfo)
                material = (prefab as TreeInfo).m_lodMaterial;
            else if (prefab is VehicleInfo)
                material = (prefab as VehicleInfo).m_lodMaterial;

            if (material != null && material.mainTexture != null)
                return material.mainTexture.width + "x" + material.mainTexture.height;

            return "N/A";
        }

    }
}
