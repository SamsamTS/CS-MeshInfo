using UnityEngine;
using ColossalFramework.UI;

using System;

namespace MeshInfo.GUI
{
    public class UIMainPanel : UIPanel
    {
        private UITitleBar m_title;
        private UIDropDown m_prefabType;
        private UIDropDown m_sorting;
        private UISprite m_sortDirection;

        private UIFastList m_itemList;

        private MeshData[] m_buildingPrefabs;
        private MeshData[] m_propPrefabs;
        private MeshData[] m_treePrefabs;
        private MeshData[] m_vehiclePrefabs;

        private bool m_showDefault = false;

        private bool m_isSorted = false;
        private const int m_maxIterations = 10;

        public override void Start()
        {
            base.Start();

            backgroundSprite = "UnlockingPanel2";
            isVisible = false;
            canFocus = true;
            isInteractive = true;
            width = 700;
            height = 465;
            relativePosition = new Vector3(Mathf.Floor((GetUIView().fixedWidth - width) / 2), Mathf.Floor((GetUIView().fixedHeight - height) / 2));
            
            SetupControls();

        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void Update()
        {
            base.Update();

            // Super secret key combination
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.M))
            {
                isVisible = true;
                BringToFront();

                m_showDefault = !m_showDefault;
                InitializePreafabLists();
            }
            else if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.M))
            {
                isVisible = !isVisible;

                if (isVisible)
                {
                    InitializePreafabLists();
                    BringToFront();
                }
                else
                {
                    m_showDefault = false;
                }
            }
        }

        private void SetupControls()
        {
            float offset = 40f;

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.iconSprite = "IconAssetBuilding";
            m_title.title = "Mesh Info " + MeshInfo.version;

            // Type DropDown
            UILabel label = AddUIComponent<UILabel>();
            label.textScale = 0.8f;
            label.padding = new RectOffset(0, 0, 8, 0);
            label.relativePosition = new Vector3(15f, offset);
            label.text = "Type :";

            m_prefabType = UIUtils.CreateDropDown(this);
            m_prefabType.width = 110;
            m_prefabType.AddItem("Building");
            m_prefabType.AddItem("Prop");
            m_prefabType.AddItem("Tree");
            m_prefabType.AddItem("Vehicle");
            m_prefabType.selectedIndex = 0;
            m_prefabType.relativePosition = label.relativePosition + new Vector3(60f, 0f);

            m_prefabType.eventSelectedIndexChanged += (c, t) =>
            {
                m_prefabType.enabled = false;
                m_isSorted = false;
                PopulateList();
                m_prefabType.enabled = true;
            };

            // Sorting DropDown
            label = AddUIComponent<UILabel>();
            label.textScale = 0.8f;
            label.padding = new RectOffset(0, 0, 8, 0);
            label.relativePosition = m_prefabType.relativePosition + new Vector3(130f, 0f);
            label.text = "Sort by :";

            m_sorting = UIUtils.CreateDropDown(this);
            m_sorting.width = 125;
            m_sorting.AddItem("Name");
            m_sorting.AddItem("Triangles");
            m_sorting.AddItem("LOD Triangles");
            m_sorting.AddItem("Weight");
            m_sorting.AddItem("LOD Weight");
            m_sorting.AddItem("Texture size");
            m_sorting.selectedIndex = 0;
            m_sorting.relativePosition = label.relativePosition + new Vector3(60f, 0f);

            m_sorting.eventSelectedIndexChanged += (c, t) =>
            {
                m_sorting.enabled = false;
                m_isSorted = false;
                PopulateList();
                m_sorting.enabled = true;
            };

            // Sorting direction
            m_sortDirection = AddUIComponent<UISprite>();
            m_sortDirection.spriteName = "IconUpArrow";
            m_sortDirection.relativePosition = m_sorting.relativePosition + new Vector3(130f, 0f);

            m_sortDirection.eventClick += (c, t) =>
            {
                m_sortDirection.flip = (m_sortDirection.flip == UISpriteFlip.None) ? UISpriteFlip.FlipVertical : UISpriteFlip.None;
                m_isSorted = false;
                PopulateList();
            };

            // Labels
            label = AddUIComponent<UILabel>();
            label.textScale = 0.9f;
            label.text = "Name";
            label.relativePosition = new Vector3(15f, offset + 40f);

            label = AddUIComponent<UILabel>();
            label.textScale = 0.9f;
            label.text = "Texture";
            label.relativePosition = new Vector3(width - 105f, offset + 40f);

            UILabel label2 = AddUIComponent<UILabel>();
            label2.textScale = 0.9f;
            label2.text = "Weight";
            label2.relativePosition = label.relativePosition - new Vector3(90f, 0f);

            label = AddUIComponent<UILabel>();
            label.textScale = 0.9f;
            label.text = "Triangles";
            label.relativePosition = label2.relativePosition - new Vector3(115f, 0f);

            m_itemList = UIFastList.Create<UIPrefabItem>(this);
            m_itemList.rowHeight = 40f;
            m_itemList.backgroundSprite = "UnlockingPanel";
            m_itemList.width = width - 10;
            m_itemList.height = height - offset - 65;
            m_itemList.relativePosition = new Vector3(5, offset + 60);
        }

        private void InitializePreafabLists()
        {
            m_isSorted = false;

            int prefabCount = PrefabCollection<BuildingInfo>.PrefabCount();
            int count = 0;
            int maxCount = prefabCount;

            // Buildings
            m_buildingPrefabs = new MeshData[prefabCount];
            for (uint i = 0; i < prefabCount; i++)
            {
                PrefabInfo prefab = PrefabCollection<BuildingInfo>.GetPrefab(i);
                if (prefab != null && (m_showDefault || prefab.name.Contains(".")))
                {
                    if ((prefab as BuildingInfo).m_mesh == null || !(prefab as BuildingInfo).m_mesh.isReadable) continue;
                    m_buildingPrefabs[count++] = new MeshData(prefab);
                }
            }
            Array.Resize<MeshData>(ref m_buildingPrefabs, count);

            // Props
            prefabCount = PrefabCollection<PropInfo>.PrefabCount();
            count = 0;
            maxCount = Math.Max(maxCount, prefabCount);
            m_propPrefabs = new MeshData[prefabCount];
            for (uint i = 0; i < prefabCount; i++)
            {
                PrefabInfo prefab = PrefabCollection<PropInfo>.GetPrefab(i);
                if (prefab != null && (m_showDefault || prefab.name.Contains(".")))
                {
                    if ((prefab as PropInfo).m_mesh == null || !(prefab as PropInfo).m_mesh.isReadable) continue;
                    m_propPrefabs[count++] = new MeshData(prefab);
                }
            }
            Array.Resize<MeshData>(ref m_propPrefabs, count);

            // Trees
            prefabCount = PrefabCollection<TreeInfo>.PrefabCount();
            count = 0;
            maxCount = Math.Max(maxCount, prefabCount);
            m_treePrefabs = new MeshData[prefabCount];
            for (uint i = 0; i < prefabCount; i++)
            {
                PrefabInfo prefab = PrefabCollection<TreeInfo>.GetPrefab(i);
                if (prefab != null && (m_showDefault || prefab.name.Contains(".")))
                {
                    if ((prefab as TreeInfo).m_mesh == null || !(prefab as TreeInfo).m_mesh.isReadable) continue;
                    m_treePrefabs[count++] = new MeshData(prefab);
                }
            }
            Array.Resize<MeshData>(ref m_treePrefabs, count);

            // Vehicles
            prefabCount = PrefabCollection<VehicleInfo>.PrefabCount();
            count = 0;
            maxCount = Math.Max(maxCount, prefabCount);
            m_vehiclePrefabs = new MeshData[prefabCount];
            for (uint i = 0; i < prefabCount; i++)
            {
                PrefabInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(i);
                if (prefab != null && (m_showDefault || prefab.name.Contains(".")))
                {
                    if ((prefab as VehicleInfo).m_mesh == null || !(prefab as VehicleInfo).m_mesh.isReadable) continue;
                    m_vehiclePrefabs[count++] = new MeshData(prefab);
                }
            }
            Array.Resize<MeshData>(ref m_vehiclePrefabs, count);

            PopulateList();
        }

        private void PopulateList()
        {
            MeshData[] prefabList = null;

            int index = m_prefabType.selectedIndex;
            switch(index)
            {
                case 0:
                    prefabList = m_buildingPrefabs;
                    break;
                case 1:
                    prefabList = m_propPrefabs;
                    break;
                case 2:
                    prefabList = m_treePrefabs;
                    break;
                case 3:
                    prefabList = m_vehiclePrefabs;
                    break;
            }

            if (prefabList == null) return;

            // Sorting
            if (!m_isSorted)
            {
                MeshData.sorting = (MeshData.Sorting)m_sorting.selectedIndex;
                MeshData.ascendingSort = (m_sortDirection.flip == UISpriteFlip.None);
                Array.Sort(prefabList);

                m_isSorted = true;
            }

            // Display
            m_itemList.rowsData.m_buffer = prefabList;
            m_itemList.rowsData.m_size = prefabList.Length;

            m_itemList.DisplayAt(0);
        }
    }
}
