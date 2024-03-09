using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace LoD
{
    public class Zoom : MonoBehaviour
    {
        public static float scale = 1;
        public static bool zooming;
        public float min = 0.001f;
        public float max = 10;
        public float scrollSensitivity = 1;
        public List<Transform> toScale;
        public UnityEngine.UI.Slider slider;

        private void OnValidate()
        {
            slider.minValue = min;
            slider.maxValue = max;
        }

        private void Update()
        {
            zooming = false;
            if (Input.GetAxis("Mouse ScrollWheel") != 0f)
            {
                Scale(scale + Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity);
            }
            else if (Input.touchCount == 2)
            {
                // get current touch positions
                Touch tZero = Input.GetTouch(0);
                Touch tOne = Input.GetTouch(1);
                // get touch position from the previous frame
                Vector2 tZeroPrevious = tZero.position - tZero.deltaPosition;
                Vector2 tOnePrevious = tOne.position - tOne.deltaPosition;

                float oldTouchDistance = Vector2.Distance(tZeroPrevious, tOnePrevious);
                float currentTouchDistance = Vector2.Distance(tZero.position, tOne.position);

                // get offset value
                float deltaDistance = oldTouchDistance - currentTouchDistance;
                Scale(scale - deltaDistance * scrollSensitivity / 100);
                zooming = true;
            }
        }

        public void SetSensitivity(float value)
        {
            scrollSensitivity = value;
        }

        public void Scale(float newScale)
        {
            newScale = Mathf.Clamp(newScale, min, max);
            slider.value = newScale;
            foreach (Transform transform in toScale)
            {
                transform.position *= newScale / scale;
                transform.localScale = Vector3.one * newScale;
            }
            scale = newScale;
        }
    }
}
