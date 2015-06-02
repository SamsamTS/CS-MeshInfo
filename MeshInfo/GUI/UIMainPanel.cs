using UnityEngine;
using ColossalFramework.UI;
using ColossalFramework.Threading;

using System;
using System.Collections;

namespace MeshInfo.GUI
{
    public class UIMainPanel : UIPanel
    {
        private UITitleBar m_title;
        private UIDropDown m_prefabType;
        private UIDropDown m_sorting;
        private UIScrollablePanel m_scrollablePanel;
        private UIPanel m_panelForScrollPanel;

        private PrefabInfo[] m_buildingPrefabs;
        private PrefabInfo[] m_propPrefabs;
        private PrefabInfo[] m_treePrefabs;
        private PrefabInfo[] m_vehiclePrefabs;

        private UIPrefabItem[] m_itemList = null;

        public override void Start()
        {
            base.Start();

            backgroundSprite = "UnlockingPanel2";
            isVisible = false;
            canFocus = true;
            isInteractive = true;
            width = 700;
            height = 395;
            relativePosition = new Vector3(Mathf.Floor((GetUIView().fixedWidth - width) / 2), Mathf.Floor((GetUIView().fixedHeight - height) / 2));
            
            InitializePreafabLists();
            SetupControls();

            PopulateList();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }

        public override void Update()
        {
            base.Update();

            if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftControl)) && Input.GetKeyDown(KeyCode.M))
            {
                isVisible = !isVisible;
            }
        }

        private void SetupControls()
        {
            float offset = 40f;

            // Title Bar
            m_title = AddUIComponent<UITitleBar>();
            m_title.iconSprite = "IconAssetBuilding";
            m_title.title = "Mesh Info 1.0";

            // Prefab Type DropDown
            m_prefabType = UIUtils.CreateDropDown(this);
            m_prefabType.width = 150;
            m_prefabType.AddItem("Building");
            m_prefabType.AddItem("Prop");
            m_prefabType.AddItem("Tree");
            m_prefabType.AddItem("Vehicle");
            m_prefabType.selectedIndex = 0;
            m_prefabType.relativePosition = new Vector3(5, offset);

            m_prefabType.eventSelectedIndexChanged += (c,t) => PopulateList();

            // Prefab Type DropDown
            m_sorting = UIUtils.CreateDropDown(this);
            m_sorting.width = 150;
            m_sorting.AddItem("Name");
            m_sorting.AddItem("Triangles");
            m_sorting.selectedIndex = 0;
            m_sorting.relativePosition = new Vector3(160, offset);

            m_sorting.eventSelectedIndexChanged += (c, t) => PopulateList();

            // Scroll Panel (from Extended Public Transport UI)
            m_panelForScrollPanel = AddUIComponent<UIPanel>();
            m_panelForScrollPanel.gameObject.AddComponent<UICustomControl>();

            m_panelForScrollPanel.backgroundSprite = "UnlockingPanel";
            m_panelForScrollPanel.width = width - 10;
            m_panelForScrollPanel.height = height - offset - 45;
            m_panelForScrollPanel.relativePosition = new Vector3(5, offset + 40);

            m_scrollablePanel = m_panelForScrollPanel.AddUIComponent<UIScrollablePanel>();
            m_scrollablePanel.width = m_scrollablePanel.parent.width - 20f;
            m_scrollablePanel.height = m_scrollablePanel.parent.height;

            m_scrollablePanel.autoLayout = true;
            m_scrollablePanel.autoLayoutDirection = LayoutDirection.Vertical;
            m_scrollablePanel.autoLayoutStart = LayoutStart.TopLeft;
            m_scrollablePanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            m_scrollablePanel.clipChildren = true;

            m_scrollablePanel.pivot = UIPivotPoint.TopLeft;
            m_scrollablePanel.AlignTo(m_scrollablePanel.parent, UIAlignAnchor.TopLeft);

            UIScrollbar scrollbar = m_panelForScrollPanel.AddUIComponent<UIScrollbar>();
            scrollbar.width = scrollbar.parent.width - m_scrollablePanel.width;
            scrollbar.height = scrollbar.parent.height;
            scrollbar.orientation = UIOrientation.Vertical;
            scrollbar.pivot = UIPivotPoint.BottomLeft;
            scrollbar.AlignTo(scrollbar.parent, UIAlignAnchor.TopRight);
            scrollbar.minValue = 0;
            scrollbar.value = 0;
            scrollbar.incrementAmount = 50;

            UISlicedSprite tracSprite = scrollbar.AddUIComponent<UISlicedSprite>();
            tracSprite.relativePosition = Vector2.zero;
            tracSprite.autoSize = true;
            tracSprite.size = tracSprite.parent.size;
            tracSprite.fillDirection = UIFillDirection.Vertical;
            tracSprite.spriteName = "ScrollbarTrack";

            scrollbar.trackObject = tracSprite;

            UISlicedSprite thumbSprite = tracSprite.AddUIComponent<UISlicedSprite>();
            thumbSprite.relativePosition = Vector2.zero;
            thumbSprite.fillDirection = UIFillDirection.Vertical;
            thumbSprite.autoSize = true;
            thumbSprite.width = thumbSprite.parent.width - 8;
            thumbSprite.spriteName = "ScrollbarThumb";

            scrollbar.thumbObject = thumbSprite;

            m_scrollablePanel.verticalScrollbar = scrollbar;
            m_scrollablePanel.eventMouseWheel += (component, param) =>
            {
                var sign = Mathf.Sign(param.wheelDelta);
                m_scrollablePanel.scrollPosition += new Vector2(0, sign * (-1) * 40);
            };
        }

        private void InitializePreafabLists()
        {
            int prefabCount = PrefabCollection<BuildingInfo>.PrefabCount();
            int count = 0;
            int maxCount = prefabCount;

            // Buildings
            m_buildingPrefabs = new PrefabInfo[prefabCount];
            for (uint i = 0; i < prefabCount; i++)
            {
                PrefabInfo prefab = PrefabCollection<BuildingInfo>.GetPrefab(i);
                if (prefab != null && prefab.name.Contains("."))
                    m_buildingPrefabs[count++] = prefab;
            }
            Array.Resize<PrefabInfo>(ref m_buildingPrefabs, count);

            // Props
            prefabCount = PrefabCollection<PropInfo>.PrefabCount();
            count = 0;
            maxCount = Math.Max(maxCount, prefabCount);
            m_propPrefabs = new PrefabInfo[prefabCount];
            for (uint i = 0; i < prefabCount; i++)
            {
                PrefabInfo prefab = PrefabCollection<PropInfo>.GetPrefab(i);
                if (prefab != null && prefab.name.Contains("."))
                    m_propPrefabs[count++] = prefab;
            }
            Array.Resize<PrefabInfo>(ref m_propPrefabs, count);

            // Trees
            prefabCount = PrefabCollection<TreeInfo>.PrefabCount();
            count = 0;
            maxCount = Math.Max(maxCount, prefabCount);
            m_treePrefabs = new PrefabInfo[prefabCount];
            for (uint i = 0; i < prefabCount; i++)
            {
                PrefabInfo prefab = PrefabCollection<TreeInfo>.GetPrefab(i);
                if (prefab != null && prefab.name.Contains("."))
                    m_treePrefabs[count++] = prefab;
            }
            Array.Resize<PrefabInfo>(ref m_treePrefabs, count);

            // Vehicles
            prefabCount = PrefabCollection<VehicleInfo>.PrefabCount();
            count = 0;
            maxCount = Math.Max(maxCount, prefabCount);
            m_vehiclePrefabs = new PrefabInfo[prefabCount];
            for (uint i = 0; i < prefabCount; i++)
            {
                PrefabInfo prefab = PrefabCollection<VehicleInfo>.GetPrefab(i);
                if (prefab != null && prefab.name.Contains("."))
                    m_vehiclePrefabs[count++] = prefab;
            }
            Array.Resize<PrefabInfo>(ref m_vehiclePrefabs, count);

            m_itemList = new UIPrefabItem[maxCount];
        }

        private void PopulateList()
        {
            PrefabInfo[] prefabList = null;

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

            // TODO sorting
            Array.Sort<PrefabInfo>(prefabList, (a, b) =>
                {
                    if(m_sorting.selectedIndex == 0)
                        return MeshInfo.GetLocalizedName(a).CompareTo(MeshInfo.GetLocalizedName(b));
                    else
                        return MeshInfo.GetTriangles(b) - MeshInfo.GetTriangles(a);
                });

            int count = 0;
            for (int i = 0; i < prefabList.Length; i++)
            {
                if (prefabList[i] == null) continue;

                if (m_itemList[count] == null)
                    m_itemList[count] = m_scrollablePanel.AddUIComponent<UIPrefabItem>();

                m_itemList[count].prefab = prefabList[i];
                m_itemList[i].isVisible = true;

                if ((count % 2) == 1)
                {
                    m_itemList[count].background.backgroundSprite = "UnlockingItemBackground";
                    m_itemList[count].background.color = new Color32(0, 0, 0, 128);
                }

                count++;

            }

            for (int i = count; i < m_itemList.Length; i++)
            {
                if (m_itemList[i] == null) continue;
                m_itemList[i].isVisible = false;
            }
        }

    }

}
