using UnityEngine;
using ColossalFramework.UI;

using System;

namespace MeshInfo.GUI
{
    public interface IRow<T>
    {
        /// <summary>
        /// Method invoked very often, make sure it is fast
        /// Avoid doing any calculations, the data should be already processed any ready to display.
        /// </summary>
        /// <param name="data">What needs to be displayed</param>
        /// <param name="isRowOdd">Use this to display a different look for your odd rows</param>
        void Display(T data, bool isRowOdd);
    }

    public class UIFastList<K, T> : UIComponent
        where K : UIPanel, IRow<T>
    {
        #region Private members
        private UIPanel m_panel;
        private UIScrollbar m_scrollbar;
        private FastList<K> m_rows;
        private FastList<T> m_rowsData;

        private string m_backgroundSprite;
        private float m_rowHeight = -1;
        private int m_pos = -1;
        #endregion

        #region Public
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

        public FastList<T> rowsData
        {
            get { return m_rowsData; }
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
            if (pos == m_pos) return; // Already at position

            m_pos = pos;

            for (int i = 0; i < m_rows.m_size; i++)
            {
                int dataPos = m_pos + i;
                if (dataPos < m_rowsData.m_size)
                    m_rows[i].Display(m_rowsData[dataPos], (dataPos % 2) == 1);
                else
                    break;
            }

            //TODO scroll position
        }

        public void Clear()
        {
            m_rowsData.Clear();

            for(int i = 0; i < m_rows.m_size; i++)
            {
                m_rows[i].enabled = false;
            }

            //TODO Update scrollbar
        }
        #endregion

        #region Overrides
        public override void Start()
        {
            base.Start();

            if(enabled) SetupControls();
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
                Destroy(m_rows[i]);
            }
        }

        protected override void OnSizeChanged()
        {
            base.OnSizeChanged();

            CheckRows();
        }
        #endregion

        #region Private Methods
        private void CheckRows()
        {
            if (m_panel == null || m_rowHeight <= 0) return;

            int nbRows = Mathf.FloorToInt(height / m_rowHeight);

            m_panel.width = width - m_scrollbar.width;
            m_panel.height = nbRows * m_rowHeight;

            if (m_rows == null)
            {
                m_rows = new FastList<K>();
                m_rows.SetCapacity(nbRows);
            }

            if (m_rows.m_size < nbRows)
            {
                // Adding missing rows
                for (int i = m_rows.m_size; i < nbRows; i++)
                {
                    m_rows.Add(m_panel.AddUIComponent<K>());
                    m_rows[i].height = rowHeight;
                }
            }
            else if (m_rows.m_size > nbRows)
            {
                // Remove excess rows
                for (int i = nbRows; i < m_rows.m_size; i++)
                    Destroy(m_rows[i]);

                m_rows.SetCapacity(nbRows);
            }

            //TODO Update scrollbar
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
            m_scrollbar.width = width - 20f;
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
        }
        #endregion
    }
}
