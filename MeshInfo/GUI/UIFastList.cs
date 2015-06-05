using UnityEngine;
using ColossalFramework.UI;

using System;

namespace MeshInfo.GUI
{
    public interface IRowUI
    {
        /// <summary>
        /// Method invoked very often, make sure it is fast
        /// Avoid doing any calculations, the data should be already processed any ready to display.
        /// </summary>
        /// <param name="data">What needs to be displayed</param>
        /// <param name="isRowOdd">Use this to display a different look for your odd rows</param>
        void Display(object data, bool isRowOdd);

        bool enabled { get; set; }
        float height { get; set; }
    }

    public class UIFastList : UIComponent
    {
        #region Private members
        private UIPanel m_panel;
        private UIScrollbar m_scrollbar;
        private FastList<IRowUI> m_rows;
        private FastList<object> m_rowsData;

        private Type m_listType;
        private string m_backgroundSprite;
        private float m_rowHeight = -1;
        private int m_pos = -1;
        #endregion

        #region Public
        /// <summary>
        /// Use this to create the UIFastList.
        /// Do NOT use AddUIComponent.
        /// I had to do that way because MonoBehaviors classes cannot be generic
        /// </summary>
        /// <typeparam name="T">The type of the row UI component</typeparam>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static UIFastList Create<T>(UIComponent parent)
            where T : UIPanel, IRowUI
        {
            UIFastList list = parent.AddUIComponent<UIFastList>();
            list.m_listType = typeof(T);
            return list;
        }

        public string backgroundSprite
        {
            get { return m_backgroundSprite; }
            set
            {
                if (m_backgroundSprite != value)
                {
                    m_backgroundSprite = value;
                    if (m_panel != null)
                        m_panel.backgroundSprite = value;
                }
            }
        }

        public FastList<object> rowsData
        {
            get
            {
                if (m_rowsData == null) m_rowsData = new FastList<object>();
                return m_rowsData;
            }
            set
            {
                if(m_rowsData != value)
                {
                    m_rowsData = value;
                    m_pos = -1;
                    DisplayAt(0);
                }
            }
        }

        public float rowHeight
        {
            get { return m_rowHeight; }
            set
            {
                if(m_rowHeight != value)
                {
                    m_rowHeight = value;
                    CheckRows();
                }
            }
        }

        public void DisplayAt(int pos)
        {
            if(m_rowsData == null) return;

            pos = Mathf.Min(pos, m_rowsData.m_size - m_rows.m_size);
            pos = Mathf.Max(pos, 0);
            //if (pos == m_pos) return; // Already at position

            m_pos = pos;

            for (int i = 0; i < m_rows.m_size; i++)
            {
                int dataPos = m_pos + i;
                if (dataPos < m_rowsData.m_size)
                {
                    m_rows[i].Display(m_rowsData[dataPos], (dataPos % 2) == 1);
                    m_rows[i].enabled = true;
                }
                else
                    m_rows[i].enabled = false;
            }

            UpdateScrollbar();
            //TODO scroll position
        }

        public void Clear()
        {
            m_rowsData.Clear();

            for(int i = 0; i < m_rows.m_size; i++)
            {
                m_rows[i].enabled = false;
            }

            UpdateScrollbar();
        }
        #endregion

        #region Overrides
        public override void Start()
        {
            base.Start();

            if(isVisible) SetupControls();
        }

        public override void OnEnable()
        {
            base.OnEnable();

            SetupControls();
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (m_panel == null) return;

            Destroy(m_panel);
            Destroy(m_scrollbar);

            if (m_rows == null) return;

            for (int i = 0; i < m_rows.m_size; i++)
            {
                Destroy(m_rows[i] as UnityEngine.Object);
            }
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            m_scrollbar.height = height;
            m_scrollbar.trackObject.height = height;
            m_scrollbar.AlignTo(this, UIAlignAnchor.TopRight);

            CheckRows();
        }

        protected override void OnMouseWheel(UIMouseEventParameter p)
        {
            base.OnMouseWheel(p);

            m_pos -= (int)p.wheelDelta;
            DisplayAt(m_pos);
        }
        #endregion

        #region Private Methods
        private void CheckRows()
        {
            if (m_panel == null || m_rowHeight <= 0) return;

            int nbRows = Mathf.FloorToInt(height / m_rowHeight);

            m_panel.width = width;
            m_panel.height = nbRows * m_rowHeight;

            if (m_rows == null)
            {
                m_rows = new FastList<IRowUI>();
                m_rows.SetCapacity(nbRows);
            }

            if (m_rows.m_size < nbRows)
            {
                // Adding missing rows
                for (int i = m_rows.m_size; i < nbRows; i++)
                {
                    m_rows.Add(m_panel.AddUIComponent(m_listType) as IRowUI);
                    m_rows[i].height = rowHeight;
                }
            }
            else if (m_rows.m_size > nbRows)
            {
                // Remove excess rows
                for (int i = nbRows; i < m_rows.m_size; i++)
                    Destroy(m_rows[i] as UnityEngine.Object);

                m_rows.SetCapacity(nbRows);
            }

            UpdateScrollbar();
        }

        private void UpdateScrollbar()
        {
            if(m_rowsData != null)
            {
                int steps = Mathf.Max(1, (m_rowsData.m_size - m_rows.m_size));

                m_scrollbar.scrollSize = Mathf.Max(10f, height / steps);
                m_scrollbar.minValue = 0;
                m_scrollbar.maxValue = height - m_scrollbar.scrollSize;
                m_scrollbar.incrementAmount = Mathf.Max(1f, height / steps);
                UpdateScrollPosition();
            }
        }

        private void UpdateScrollPosition()
        {
            int steps = Mathf.Max(1, (m_rowsData.m_size - m_rows.m_size));
            float pos = Mathf.RoundToInt(m_pos * height / steps);
            if (pos != Mathf.RoundToInt(m_scrollbar.value))
                m_scrollbar.value = pos;
        }


        private void SetupControls()
        {
            if (m_panel != null) return;

            // Panel 
            m_panel = AddUIComponent<UIPanel>();
            m_panel.size = size;

            m_panel.autoLayout = true;
            m_panel.autoLayoutDirection = LayoutDirection.Vertical;
            m_panel.autoLayoutStart = LayoutStart.TopLeft;
            m_panel.autoLayoutPadding = new RectOffset(0, 0, 0, 0);
            m_panel.clipChildren = true;
            m_panel.relativePosition = Vector2.zero;

            // Scrollbar
            m_scrollbar = AddUIComponent<UIScrollbar>();
            m_scrollbar.width = 20f;
            m_scrollbar.height = height;
            m_scrollbar.orientation = UIOrientation.Vertical;
            m_scrollbar.pivot = UIPivotPoint.BottomLeft;
            m_scrollbar.AlignTo(this, UIAlignAnchor.TopRight);
            m_scrollbar.minValue = 0;
            m_scrollbar.value = 0;
            m_scrollbar.incrementAmount = 50;

            UISlicedSprite tracSprite = m_scrollbar.AddUIComponent<UISlicedSprite>();
            tracSprite.relativePosition = Vector2.zero;
            tracSprite.autoSize = true;
            tracSprite.size = tracSprite.parent.size;
            tracSprite.fillDirection = UIFillDirection.Vertical;
            tracSprite.spriteName = "ScrollbarTrack";

            m_scrollbar.trackObject = tracSprite;

            UISlicedSprite thumbSprite = tracSprite.AddUIComponent<UISlicedSprite>();
            thumbSprite.relativePosition = Vector2.zero;
            thumbSprite.fillDirection = UIFillDirection.Vertical;
            thumbSprite.autoSize = true;
            thumbSprite.width = thumbSprite.parent.width - 8;
            thumbSprite.spriteName = "ScrollbarThumb";

            m_scrollbar.thumbObject = thumbSprite;

            // Rows
            CheckRows();

            //TODO scroll event handler
            m_scrollbar.eventValueChanged += (c, t) =>
                {
                    int steps = Mathf.Max(0, (m_rowsData.m_size - m_rows.m_size));
                    int pos = Mathf.RoundToInt(m_scrollbar.value / height * steps);
                    if (m_pos != pos) DisplayAt(pos);
                };
        }
        #endregion
    }
}
