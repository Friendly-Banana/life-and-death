using UnityEngine;

namespace LoD {
    public static class Extensions {
        public static void ClearChildren(this Transform parent) {
            foreach (Transform child in parent) Object.Destroy(child.gameObject);
        }
    }
}