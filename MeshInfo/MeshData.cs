using ColossalFramework.Globalization;
using ColossalFramework.UI;
using ICities;
using System;
using UnityEngine;

namespace MeshInfo
{
    public class MeshData : IComparable
    {
        public enum Sorting
        {
            Name = 0,
            Triangles,
            LodTriangles,
            Weight,
            LodWeight,
            TextureSize,
            LodTextureSize
        }

        private PrefabInfo m_prefab;

        public PrefabInfo prefab
        {
            get { return m_prefab; }
            set
            {
                if(m_prefab != value)
                {
                    m_prefab = value;

                    name = GetLocalizedName(m_prefab);
                    textureSize = GetTextureSize(m_prefab);
                    lodTextureSize = GetLodTextureSize(m_prefab);

                    GetTriangleInfo(m_prefab, out triangles, out lodTriangles, out weight, out lodWeight);

                    steamID = GetSteamID(m_prefab);
                }
            }
        }

        public string name;
        public int triangles;
        public int lodTriangles;
        public float weight;
        public float lodWeight;
        public Vector2 textureSize;
        public Vector2 lodTextureSize;
        public string steamID;

        public static Sorting sorting = Sorting.Name;
        public static bool ascendingSort = true;

        public MeshData(PrefabInfo prefab)
        {
            this.prefab = prefab;
        }

        public int CompareTo(object obj)
        {
            MeshData a, b;
            if (!ascendingSort)
            {
                a = this;
                b = obj as MeshData;
            }
            else
            {
                b = this;
                a = obj as MeshData;
            }

            if (a == null || b == null) return -1;

            if (sorting == Sorting.Name)
                return b.name.CompareTo(a.name);
            if (sorting == Sorting.Triangles)
                return b.triangles - a.triangles;
            if (sorting == Sorting.LodTriangles)
                return b.lodTriangles - a.lodTriangles;
            if (sorting == Sorting.Weight)
                return Mathf.RoundToInt(b.weight * 100 - a.weight * 100);
            if (sorting == Sorting.LodWeight)
                return Mathf.RoundToInt(b.lodWeight * 100 - a.lodWeight * 100);
            if (sorting == Sorting.TextureSize)
                return (int)(b.textureSize.x * b.textureSize.y - a.textureSize.x * a.textureSize.y);
            if (sorting == Sorting.LodTextureSize)
                return (int)(b.lodTextureSize.x * b.lodTextureSize.y - a.lodTextureSize.x * a.lodTextureSize.y);

            return 0;
        }

        private static string GetLocalizedName(PrefabInfo prefab)
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

        private static string GetSteamID(PrefabInfo prefab)
        {
            string steamID = null;

            if (prefab.name.Contains("."))
            {
                int id;
                steamID = prefab.name.Substring(0, prefab.name.IndexOf("."));
                if (!Int32.TryParse(steamID, out id)) return null;
            }

            return steamID;
        }


        private static void GetTriangleInfo(PrefabInfo prefab, out int triangles, out int lodTriangles, out float weight, out float lodWeight)
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
                if (Mathf.Round(cubicMeters) != 0)
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

        private static Vector2 GetTextureSize(PrefabInfo prefab)
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
                return new Vector2(material.mainTexture.width, material.mainTexture.height);

            return Vector2.zero;
        }

        private static Vector2 GetLodTextureSize(PrefabInfo prefab)
        {
            Material material = null;

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
