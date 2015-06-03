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
        private UIScrollablePanel m_scrollablePanel;
        private UIPanel m_panelForScrollPanel;
        private UISprite m_sortDirection;

        private PrefabInfo[] m_buildingPrefabs;
        private PrefabInfo[] m_propPrefabs;
        private PrefabInfo[] m_treePrefabs;
        private PrefabInfo[] m_vehiclePrefabs;

        private bool m_showDefault = false;

        private int m_itemIndex = -1;
        private bool m_isSorted = false;
        private const int m_maxIterations = 10;

        private UIPrefabItem[] m_itemList = null;

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

                ClearItemList();
                m_showDefault = !m_showDefault;
                InitializePreafabLists();
            }
            else if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) && Input.GetKeyDown(KeyCode.M))
            {
                isVisible = !isVisible;

                if (isVisible)
                    InitializePreafabLists();
                else
                {
                    ClearItemList();
                    m_showDefault = false;
                }
            }

            if(m_itemIndex > -1) PopulateList();
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
                m_itemIndex = 0;
                m_isSorted = false;
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
                m_itemIndex = 0;
                m_isSorted = false;
                m_sorting.enabled = true;
            };

            // Sorting direction
            m_sortDirection = AddUIComponent<UISprite>();
            m_sortDirection.spriteName = "IconUpArrow";
            m_sortDirection.relativePosition = m_sorting.relativePosition + new Vector3(130f, 0f);

            m_sortDirection.eventClick += (c, t) =>
            {
                m_sortDirection.flip = (m_sortDirection.flip == UISpriteFlip.None) ? UISpriteFlip.FlipVertical : UISpriteFlip.None;
                m_itemIndex = 0;
                m_isSorted = false;
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

            // Scroll Panel (from Extended Public Transport UI)
            m_panelForScrollPanel = AddUIComponent<UIPanel>();
            m_panelForScrollPanel.gameObject.AddComponent<UICustomControl>();

            m_panelForScrollPanel.backgroundSprite = "UnlockingPanel";
            m_panelForScrollPanel.width = width - 10;
            m_panelForScrollPanel.height = height - offset - 65;
            m_panelForScrollPanel.clipChildren = true;
            m_panelForScrollPanel.relativePosition = new Vector3(5, offset + 60);

            m_scrollablePanel = m_panelForScrollPanel.AddUIComponent<UIScrollablePanel>();
            m_scrollablePanel.width = m_panelForScrollPanel.width - 20f;
            m_scrollablePanel.height = m_panelForScrollPanel.height;

            m_scrollablePanel.autoLayout = true;
            m_scrollablePanel.autoLayoutDirection = LayoutDirection.Vertical;
            m_scrollablePanel.autoLayoutStart = LayoutStart.TopLeft;
            m_scrollablePanel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            m_scrollablePanel.clipChildren = true;

            m_scrollablePanel.pivot = UIPivotPoint.TopLeft;
            m_scrollablePanel.AlignTo(m_panelForScrollPanel, UIAlignAnchor.TopLeft);

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
            m_itemIndex = 0;
            m_isSorted = false;

            int prefabCount = PrefabCollection<BuildingInfo>.PrefabCount();
            int count = 0;
            int maxCount = prefabCount;

            // Buildings
            m_buildingPrefabs = new PrefabInfo[prefabCount];
            for (uint i = 0; i < prefabCount; i++)
            {
                PrefabInfo prefab = PrefabCollection<BuildingInfo>.GetPrefab(i);
                if (prefab != null && (m_showDefault || prefab.name.Contains(".")))
                {
                    if ((prefab as BuildingInfo).m_mesh == null || !(prefab as BuildingInfo).m_mesh.isReadable) continue;
                    m_buildingPrefabs[count++] = prefab;
                }
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
                if (prefab != null && (m_showDefault || prefab.name.Contains(".")))
                {
                    if ((prefab as PropInfo).m_mesh == null || !(prefab as PropInfo).m_mesh.isReadable) continue;
                    m_propPrefabs[count++] = prefab;
                }
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
                if (prefab != null && (m_showDefault || prefab.name.Contains(".")))
                {
                    if ((prefab as TreeInfo).m_mesh == null || !(prefab as TreeInfo).m_mesh.isReadable) continue;
                    m_treePrefabs[count++] = prefab;
                }
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
                if (prefab != null && (m_showDefault || prefab.name.Contains(".")))
                {
                    if ((prefab as VehicleInfo).m_mesh == null || !(prefab as VehicleInfo).m_mesh.isReadable) continue;
                    m_vehiclePrefabs[count++] = prefab;
                }
            }
            Array.Resize<PrefabInfo>(ref m_vehiclePrefabs, count);

            m_itemList = new UIPrefabItem[maxCount];
        }

        private void ClearItemList()
        {
            if(m_itemList == null) return;

            m_itemIndex = -1;
            for(int i = 0; i< m_itemList.Length; i++)
            {
                if(m_itemList[i] != null)
                {
                    //m_scrollablePanel.RemoveUIComponent(m_itemList[i]);
                    Destroy(m_itemList[i]);
                }
            }
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

            if (prefabList == null) return;

            // Sorting
            if (!m_isSorted) SortList(prefabList);

            for (int start = m_itemIndex; m_itemIndex < prefabList.Length && m_itemIndex < start + m_maxIterations; m_itemIndex++)
            {
                if (m_itemList[m_itemIndex] == null)
                    m_itemList[m_itemIndex] = m_scrollablePanel.AddUIComponent<UIPrefabItem>();

                m_itemList[m_itemIndex].prefab = prefabList[m_itemIndex];
                m_itemList[m_itemIndex].isVisible = true;

                if ((m_itemIndex % 2) == 1)
                {
                    m_itemList[m_itemIndex].background.backgroundSprite = "UnlockingItemBackground";
                    m_itemList[m_itemIndex].background.color = new Color32(0, 0, 0, 128);
                }
            }

            if (m_itemIndex == prefabList.Length) m_itemIndex = -1;

            for (int i = prefabList.Length; i < m_itemList.Length; i++)
            {
                if (m_itemList[i] == null) continue;
                m_itemList[i].isVisible = false;
            }
        }

        private void SortList(PrefabInfo[] prefabList)
        {
            Array.Sort<PrefabInfo>(prefabList, (a, b) =>
            {
                int trianglesA, trianglesB;
                int lodTrianglesA, lodTrianglesB;
                float weightA, weightB;
                float lodWeightA, lodWeightB;

                if (m_sortDirection.flip == UISpriteFlip.FlipVertical)
                {
                    if (m_sorting.selectedIndex == 0)
                        return MeshInfo.GetLocalizedName(b).CompareTo(MeshInfo.GetLocalizedName(a));

                    if (m_sorting.selectedIndex == 5)
                    {
                        Vector2 sizeA = MeshInfo.GetTextureSize(a);
                        Vector2 sizeB = MeshInfo.GetTextureSize(b);
                        return (int)(sizeB.x * sizeB.y - sizeA.x * sizeA.y);
                    }

                    MeshInfo.GetTriangleInfo(a, out trianglesA, out lodTrianglesA, out weightA, out lodWeightA);
                    MeshInfo.GetTriangleInfo(b, out trianglesB, out lodTrianglesB, out weightB, out lodWeightB);

                    if (m_sorting.selectedIndex == 1)
                        return trianglesB - trianglesA;
                    if (m_sorting.selectedIndex == 2)
                        return lodTrianglesB - lodTrianglesA;
                    if (m_sorting.selectedIndex == 3)
                        return Mathf.RoundToInt(weightB * 100 - weightA * 100);
                    if (m_sorting.selectedIndex == 4)
                        return Mathf.RoundToInt(lodWeightB * 100 - lodWeightA * 100);
                }
                else
                {
                    if (m_sorting.selectedIndex == 0)
                        return MeshInfo.GetLocalizedName(a).CompareTo(MeshInfo.GetLocalizedName(b));

                    if (m_sorting.selectedIndex == 5)
                    {
                        Vector2 sizeA = MeshInfo.GetTextureSize(a);
                        Vector2 sizeB = MeshInfo.GetTextureSize(b);
                        return (int)(sizeA.x * sizeA.y - sizeB.x * sizeB.y);
                    }

                    MeshInfo.GetTriangleInfo(a, out trianglesA, out lodTrianglesA, out weightA, out lodWeightA);
                    MeshInfo.GetTriangleInfo(b, out trianglesB, out lodTrianglesB, out weightB, out lodWeightB);

                    if (m_sorting.selectedIndex == 1)
                        return trianglesA - trianglesB;
                    if (m_sorting.selectedIndex == 2)
                        return lodTrianglesA - lodTrianglesB;
                    if (m_sorting.selectedIndex == 3)
                        return Mathf.RoundToInt(weightA * 100 - weightB * 100);
                    if (m_sorting.selectedIndex == 4)
                        return Mathf.RoundToInt(lodWeightA * 100 - lodWeightB * 100);
                }

                return 0;
            });

            m_isSorted = true;
        }
    }
}
