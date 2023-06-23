using System;
using UnityEngine.Events;
using UnityEngine.EventSystems;

using ScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode;

namespace UnityEngine.UI
{
    [RequireComponent(typeof(Canvas))]
    [ExecuteAlways]
    [AddComponentMenu("Layout/Canvas Scaler with User Scale", 102)]
    [DisallowMultipleComponent]
    /// <summary>
    ///   The Canvas Scaler User Scale extends the existing Canvas Scaler,
    ///   exposing an arbitrary scale factor that should be exposed to the 
    /// </summary>

    public class CanvasScalerUserScale : CanvasScaler
    {
        [SerializeField]
        private float userScaleFactor = 1.0f;
        public float UserScaleFactor
        {
            get => userScaleFactor;
            set
            {
                userScaleFactor = Mathf.Max(0, value);
            }
        }

        private const float logBase = 2;

        private Canvas owningCanvas;

        protected override void OnEnable()
        {
            owningCanvas = GetComponent<Canvas>();
            base.OnEnable();
        }

        /// <summary>
        /// Handles canvas scaling for world canvas.
        /// </summary>
        protected override void HandleWorldCanvas()
        {
            SetScaleFactor(m_DynamicPixelsPerUnit);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit);
        }

        /// <summary>
        /// Handles canvas scaling for a constant pixel size.
        /// </summary>
        protected override void HandleConstantPixelSize()
        {
            SetScaleFactor(m_ScaleFactor * userScaleFactor);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit);
        }

        /// <summary>
        /// Handles canvas scaling that scales with the screen size.
        /// </summary>
        protected override void HandleScaleWithScreenSize()
        {
            Vector2 screenSize = owningCanvas.renderingDisplaySize;

            // Multiple display support only when not the main display. For display 0 the reported
            // resolution is always the desktops resolution since its part of the display API,
            // so we use the standard none multiple display method. (case 741751)
            int displayIndex = owningCanvas.targetDisplay;
            if (displayIndex > 0 && displayIndex < Display.displays.Length)
            {
                Display disp = Display.displays[displayIndex];
                screenSize = new Vector2(disp.renderingWidth, disp.renderingHeight);
            }


            float scaleFactor = 0;
            switch (m_ScreenMatchMode)
            {
                case ScreenMatchMode.MatchWidthOrHeight:
                    {
                        // We take the log of the relative width and height before taking the average.
                        // Then we transform it back in the original space.
                        // the reason to transform in and out of logarithmic space is to have better behavior.
                        // If one axis has twice resolution and the other has half, it should even out if widthOrHeight value is at 0.5.
                        // In normal space the average would be (0.5 + 2) / 2 = 1.25
                        // In logarithmic space the average is (-1 + 1) / 2 = 0
                        float logWidth = Mathf.Log(screenSize.x / m_ReferenceResolution.x, logBase);
                        float logHeight = Mathf.Log(screenSize.y / m_ReferenceResolution.y, logBase);
                        float logWeightedAverage = Mathf.Lerp(logWidth, logHeight, m_MatchWidthOrHeight);
                        scaleFactor = Mathf.Pow(logBase, logWeightedAverage);
                        break;
                    }
                case ScreenMatchMode.Expand:
                    {
                        scaleFactor = Mathf.Min(screenSize.x / m_ReferenceResolution.x, screenSize.y / m_ReferenceResolution.y);
                        break;
                    }
                case ScreenMatchMode.Shrink:
                    {
                        scaleFactor = Mathf.Max(screenSize.x / m_ReferenceResolution.x, screenSize.y / m_ReferenceResolution.y);
                        break;
                    }
            }

            SetScaleFactor(scaleFactor * userScaleFactor);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit);
        }

        ///<summary>
        ///Handles canvas scaling for a constant physical size.
        ///</summary>
        protected override void HandleConstantPhysicalSize()
        {
            float currentDpi = Screen.dpi;
            float dpi = (currentDpi == 0 ? m_FallbackScreenDPI : currentDpi);
            float targetDPI = 1;
            switch (m_PhysicalUnit)
            {
                case Unit.Centimeters: targetDPI = 2.54f; break;
                case Unit.Millimeters: targetDPI = 25.4f; break;
                case Unit.Inches: targetDPI = 1; break;
                case Unit.Points: targetDPI = 72; break;
                case Unit.Picas: targetDPI = 6; break;
            }

            SetScaleFactor((dpi / targetDPI) * userScaleFactor);
            SetReferencePixelsPerUnit(m_ReferencePixelsPerUnit * (targetDPI / m_DefaultSpriteDPI) * userScaleFactor);
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            userScaleFactor = Mathf.Max(0.01f, userScaleFactor);
        }
#endif
    }
}
