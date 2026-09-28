using System.Collections;
///Credit Martin Nerurkar // www.martin.nerurkar.de // www.sharkbombs.com
///Sourced from - http://www.sharkbombs.com/2015/02/10/tooltips-with-the-new-unity-ui-ugui/
using UnityEngine.EventSystems;
using UnityEngine.Events;

namespace UnityEngine.UI.Extensions
{
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UI/Extensions/Tooltip/Tooltip Trigger")]
    public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [TextAreaAttribute]
        public string text;

        public float Delay;
        public UnityEvent OnShow = new UnityEvent();
        
        public enum TooltipPositioningType {
            mousePosition,
            mousePositionAndFollow,
            transformPosition
        }

        [Tooltip("Defines where the tooltip will be placed and how that placement will occur. Transform position will always be used if this element wasn't selected via mouse")]
        public TooltipPositioningType tooltipPositioningType = TooltipPositioningType.mousePosition;

        /// <summary>
        /// This info is needed to make sure we make the necessary translations if the tooltip and this trigger are children of different space canvases
        /// </summary>
        private bool isChildOfOverlayCanvas = false;

        private bool hovered = false;

        public Vector3 offset;

        protected WaitForSeconds delay;

        protected Coroutine countdown;

        void Start() {
            //attempt to check if our canvas is overlay or not and check our "is overlay" accordingly
            Canvas ourCanvas = GetComponentInParent<Canvas>();
            if (ourCanvas && ourCanvas.renderMode == RenderMode.ScreenSpaceOverlay) {
                isChildOfOverlayCanvas = true;
            }
        }

        /// <summary>
        /// Checks if the tooltip and the transform this trigger is attached to are children of differently-spaced Canvases
        /// </summary>
        public bool WorldToScreenIsRequired
        {
            get
            {
                return (isChildOfOverlayCanvas && ToolTip.Instance.guiMode == RenderMode.ScreenSpaceCamera) ||
                    (!isChildOfOverlayCanvas && ToolTip.Instance.guiMode == RenderMode.ScreenSpaceOverlay);
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            this.countdown = StartCoroutine(StartCountdown(eventData));
        }
        
        protected IEnumerator StartCountdown(PointerEventData eventData) {
            yield return delay;
            this.OnShow?.Invoke();
            switch (tooltipPositioningType) {
                case TooltipPositioningType.mousePosition:
                    StartHover(UIExtensionsInputManager.MousePosition + offset, true);
                    break;
                case TooltipPositioningType.mousePositionAndFollow:
                    StartHover(UIExtensionsInputManager.MousePosition + offset, true);
                    hovered = true;
                    StartCoroutine(HoveredMouseFollowingLoop());
                    break;
                case TooltipPositioningType.transformPosition:
                    StartHover((WorldToScreenIsRequired ? 
                        ToolTip.Instance.GuiCamera.WorldToScreenPoint(transform.position) :
                        transform.position) + offset, true);
                    break;
            }
        }

        IEnumerator HoveredMouseFollowingLoop() {
            while (hovered) {
                StartHover(UIExtensionsInputManager.MousePosition + offset);
                yield return null;
            }
        }

        public void OnSelect(BaseEventData eventData)
        {
            StartHover((WorldToScreenIsRequired ? 
                ToolTip.Instance.GuiCamera.WorldToScreenPoint(transform.position) :
                        transform.position) + offset, true);
        }

        public void OnPointerExit(PointerEventData eventData)
        { 
            if (this.countdown != null) {
                StopCoroutine(this.countdown);
                this.countdown = null;
            }
            
            StopHover();
        }

        public void OnDeselect(BaseEventData eventData)
        {
            StopHover();
        }

        void StartHover(Vector3 position, bool shouldCanvasUpdate = false)
        {
            ToolTip.Instance.SetTooltip(text, position, shouldCanvasUpdate);
        }

        void StopHover()
        {
            hovered = false;
            ToolTip.Instance.HideTooltip();
        }
    }
}
