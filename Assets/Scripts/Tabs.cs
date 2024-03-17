using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoD {
    internal struct Tab {
        public GameObject content;
        public Button button;
    }

    public class Tabs : MonoBehaviour {
        public Transform figureTabParent;
        public GameObject tabButtonPrefab;
        public GameObject tabContentPrefab;
        private readonly List<Tab> tabs = new();
        private int currentlySelected;

        public void AddTab(string tabName, Action<Transform> SpawnContent) {
            var index = tabs.Count;
            var button = Instantiate(tabButtonPrefab, figureTabParent.GetChild(0)).GetComponent<Button>();
            button.GetComponentInChildren<TMP_Text>().text = tabName;
            button.onClick.AddListener(() => Select(index));
            var content = Instantiate(tabContentPrefab, figureTabParent);
            SpawnContent(content.transform);
            tabs.Add(new Tab { content = content, button = button });
            Select(index);
        }

        public void Select(int index) {
            if (index == currentlySelected) return;
            tabs[currentlySelected].content.SetActive(false);
            tabs[currentlySelected].button.interactable = true;
            currentlySelected = index;
            tabs[currentlySelected].content.SetActive(true);
            tabs[currentlySelected].button.interactable = false;
        }
    }
}