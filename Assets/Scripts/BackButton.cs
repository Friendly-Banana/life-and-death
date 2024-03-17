using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace LoD {
    public class BackButton : MonoBehaviour {
        private static readonly Stack<BackButton> active = new();
        private Button button;

        private void Update() {
            if (Input.GetKeyDown(KeyCode.Escape) && active.Peek() == this) button.onClick.Invoke();
        }

        private void OnEnable() {
            active.Push(this);
        }

        private void OnDisable() {
            active.Pop();
        }

        private void OnValidate() {
            button = GetComponent<Button>();
        }
    }
}