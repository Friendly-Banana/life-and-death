﻿using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LoD {
    public class About : MonoBehaviour, IPointerClickHandler {
        public TMP_Text versionText;
        public TMP_Text descText;

        private void Start() {
            versionText.text = $"Version {Application.version}";
        }

        public void OnPointerClick(PointerEventData eventData) {
            var linkIndex =
                TMP_TextUtilities.FindIntersectingLink(descText, eventData.position, eventData.pressEventCamera);
            if (linkIndex == -1)
                return;
            // was a link clicked?
            var linkInfo = descText.textInfo.linkInfo[linkIndex];

            // open the link id as a url, which is the metadata we added in the text field
            Debug.Log($"Open link {linkInfo.GetLinkID()}");
            Application.OpenURL(linkInfo.GetLinkID());
        }
    }
}