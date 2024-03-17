using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoD {
    public class Zoom : MonoBehaviour {
        public static float scale = 1;
        public static bool zooming;
        public float min = 0.001f;
        public float max = 10;
        public float scrollSensitivity = 1;
        public List<Transform> toScale;
        public Slider slider;

        private void Update() {
            zooming = false;
            if (Input.GetAxis("Mouse ScrollWheel") != 0f) {
                Scale(scale + Input.GetAxis("Mouse ScrollWheel") * scrollSensitivity);
            }
            else if (Input.touchCount == 2) {
                // get current touch positions
                var tZero = Input.GetTouch(0);
                var tOne = Input.GetTouch(1);
                // get touch position from the previous frame
                var tZeroPrevious = tZero.position - tZero.deltaPosition;
                var tOnePrevious = tOne.position - tOne.deltaPosition;

                var oldTouchDistance = Vector2.Distance(tZeroPrevious, tOnePrevious);
                var currentTouchDistance = Vector2.Distance(tZero.position, tOne.position);

                // get offset value
                var deltaDistance = oldTouchDistance - currentTouchDistance;
                Scale(scale - deltaDistance * scrollSensitivity / 100);
                zooming = true;
            }
        }

        private void OnValidate() {
            slider.minValue = min;
            slider.maxValue = max;
        }

        public void SetSensitivity(float value) {
            scrollSensitivity = value;
        }

        public void Scale(float newScale) {
            newScale = Mathf.Clamp(newScale, min, max);
            slider.value = newScale;
            foreach (var child in toScale) {
                child.position *= newScale / scale;
                child.localScale = Vector3.one * newScale;
            }

            scale = newScale;
        }
    }
}