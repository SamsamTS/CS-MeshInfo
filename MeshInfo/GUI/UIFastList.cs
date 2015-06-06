using UnityEngine;
using ColossalFramework.UI;

using System;

namespace MeshInfo.GUI
{
    public interface IUIFastListRow
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
        private FastList<IUIFastListRow> m_rows;
        private FastList<object> m_rowsData;

        private Type m_listType;
        private string m_backgroundSprite;
        private Color32 m_color;
        private float m_rowHeight = -1;
        private int m_pos = -1;
        private bool m_updateScroll = true;
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
            where T : UIPanel, IUIFastListRow
        {
            UIFastList list = parent.AddUIComponent<UIFastList>();
            list.m_listType = typeof(T);
            return list;
        }

        /// <summary>
        /// Change the sprite of the background
        /// </summary>
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

        /// <summary>
        /// Change the color of the background
        /// </summary>
        public Color32 backgroundColor
        {
            get { return m_color; }
            set
            {
                m_color = value;
                if (m_panel != null)
                    m_panel.color = value;
            }
        }

        /// <summary>
        /// This is the list data that will be send to the IRow.Display method
        /// Changing this list will reset the display position to 0
        /// </summary>
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

        /// <summary>
        /// This MUST be set, it is the height in pixel of each row
        /// </summary>
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

        /// <summary>
        /// Change the position in the list
        /// Display the data at the position in the top row.
        /// This doesn't update the list if the position remind the same
        /// Use DisplayAt for that
        /// </summary>
        public int listPosition
        {
            get { return m_pos; }
            set
            {
                int pos = Mathf.Max(Mathf.Min(value, m_rowsData.m_size - m_rows.m_size), 0);
                if (m_pos != pos)
                    DisplayAt(pos);
            }
        }

        /// <summary>
        /// Display the data at the position in the top row.
        /// This update the list even if the position remind the same
        /// </summary>
        /// <param name="pos"></param>
        public void DisplayAt(int pos)
        {
            if(m_rowsData == null) return;

            pos = Mathf.Max(Mathf.Min(pos, m_rowsData.m_size - m_rows.m_size), 0);

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
        }

        /// <summary>
        /// Clear the list
        /// </summary>
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

            if (m_panel == null) return;

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
                m_rows = new FastList<IUIFastListRow>();
                m_rows.SetCapacity(nbRows);
            }

            if (m_rows.m_size < nbRows)
            {
                // Adding missing rows
                for (int i = m_rows.m_size; i < nbRows; i++)
                {
                    m_rows.Add(m_panel.AddUIComponent(m_listType) as IUIFastListRow);
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

                float scrollSize = Mathf.Max(10f, height * m_rows.m_size / m_rowsData.m_size);
                float amount = Mathf.Max(1f, (height - scrollSize) / steps);

                m_scrollbar.scrollSize = scrollSize;
                m_scrollbar.minValue = 0f;
                m_scrollbar.maxValue = height;
                m_scrollbar.incrementAmount = amount;
                UpdateScrollPosition();
            }
        }

        private void UpdateScrollPosition()
        {
            if (!m_updateScroll) return;

            int steps = Mathf.Max(1, (m_rowsData.m_size - m_rows.m_size));
            float pos = Mathf.RoundToInt(m_pos * (height-m_scrollbar.scrollSize) / steps);
            if (pos != Mathf.RoundToInt(m_scrollbar.value))
                m_scrollbar.value = pos;
        }


        private void SetupControls()
        {
            if (m_panel != null) return;

            // Panel 
            m_panel = AddUIComponent<UIPanel>();
            m_panel.size = size;
            m_panel.backgroundSprite = m_backgroundSprite;
            //m_panel.color = m_color;
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

            m_scrollbar.eventValueChanged += (c, t) =>
                {
                    m_updateScroll = false;
                    int steps = Mathf.Max(0, (m_rowsData.m_size - m_rows.m_size));
                    int pos = Mathf.RoundToInt(m_scrollbar.value / (height - m_scrollbar.scrollSize) * steps);
                    listPosition = pos;
                    m_updateScroll = true;
                };
        }
        #endregion
    }
}
