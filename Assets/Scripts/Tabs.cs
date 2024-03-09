using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoD
{
    struct Tab
    {
        public string name;
        public GameObject content;
        public Button button;
    }

    public class Tabs : MonoBehaviour
    {
        public Transform figureTabParent;
        public GameObject tabButtonPrefab;
        public GameObject tabContentPrefab;
        readonly List<Tab> tabs = new List<Tab>();
        int currentlySelected = 0;

        public void AddTab(string name, Action<Transform> SpawnContent)
        {
            int index = tabs.Count;
            Button button = Instantiate(tabButtonPrefab, figureTabParent.GetChild(0)).GetComponent<Button>();
            button.GetComponentInChildren<TMP_Text>().text = name;
            button.onClick.AddListener(() => Select(index));
            GameObject content = Instantiate(tabContentPrefab, figureTabParent);
            SpawnContent(content.transform);
            tabs.Add(new Tab { name = name, content = content, button = button });
            Select(index);
        }

        public void Select(int index)
        {
            if (index == currentlySelected) return;
            tabs[currentlySelected].content.SetActive(false);
            tabs[currentlySelected].button.interactable = true;
            currentlySelected = index;
            tabs[currentlySelected].content.SetActive(true);
            tabs[currentlySelected].button.interactable = false;
        }
    }
}
