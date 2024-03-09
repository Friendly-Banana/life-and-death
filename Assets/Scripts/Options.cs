using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoD
{
    [System.Serializable]
    public class GameOptions
    {
        public int stopRound = 5;
        // cells players can set per round
        public int cellsToSet = 10;
        public int boardWidth = 9;
        public int boardHeight = 7;
        public bool wrapAround = false;
        public bool randomBoard = false;
        public bool playersCanDie = true;
        public List<int> revive = new List<int>() { 3 };
        public List<int> stayAlive = new List<int>() { 2, 3 };
    }

    public class Options : MonoBehaviour
    {
        public GameOptions data;
        public Toggle wrapAroundToggle;
        public Toggle randomToggle;
        public Toggle playersCanDie;
        public GameObject togglePrefab;
        public Transform reviveParent;
        public Transform aliveParent;
        public List<Slider> sliders;

        private void Awake()
        {
#if UNITY_IOS
			// Forces a different code path in the BinaryFormatter that doesn't rely on run-time code generation (which would break on iOS).
			System.Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
#endif
            data = SaveSystem.LoadSettings();
        }

        private void Start()
        {
            for (int i = 0; i < 9; i++)
            {
                int new_i = i;
                // revive
                GameObject rg = Instantiate(togglePrefab, reviveParent);
                rg.GetComponentInChildren<TMP_Text>().text = i.ToString();
                Toggle rToggle = rg.GetComponent<Toggle>();
                rToggle.isOn = data.revive.Contains(i);
                rToggle.onValueChanged.AddListener((bool b) => SetRevive(new_i, b));
                // alive
                GameObject ag = Instantiate(togglePrefab, aliveParent);
                ag.GetComponentInChildren<TMP_Text>().text = i.ToString();
                Toggle aToggle = ag.GetComponent<Toggle>();
                aToggle.isOn = data.stayAlive.Contains(i);
                aToggle.onValueChanged.AddListener((bool b) => SetStayAlive(new_i, b));
            }
            wrapAroundToggle.isOn = data.wrapAround;
            randomToggle.isOn = data.randomBoard;
            playersCanDie.isOn = data.playersCanDie;
            int[] values = new int[] { data.stopRound, data.cellsToSet, data.boardWidth, data.boardHeight };
            for (int i = 0; i < sliders.Count; i++)
            {
                sliders[i].value = values[i];
                sliders[i].GetComponentInChildren<TMP_Text>().text = values[i].ToString();
                int index = i;
                sliders[i].onValueChanged.AddListener((value) => SetSlider(index, value));
            }
        }

        public void SaveData()
        {
            SaveSystem.SaveSettings(data);
        }

        void SetSlider(int index, Single newValue)
        {
            int value = Mathf.RoundToInt(newValue);
            sliders[index].GetComponentInChildren<TMP_Text>().text = value.ToString();
            switch (index)
            {
                case 0:
                    data.stopRound = value;
                    break;
                case 1:
                    data.cellsToSet = value;
                    break;
                case 2:
                    data.boardWidth = value;
                    break;
                case 3:
                    data.boardHeight = value;
                    break;
            }
        }

        public void WrapAround()
        {
            data.wrapAround = wrapAroundToggle.isOn;
        }

        public void RandomBoard()
        {
            data.randomBoard = randomToggle.isOn;
        }

        public void PlayersCanDie()
        {
            data.playersCanDie = playersCanDie.isOn;
        }

        public void SetRevive(int new_value, bool b)
        {
            if (b)
                data.revive.Add(new_value);
            else
                data.revive.Remove(new_value);
        }

        public void SetStayAlive(int new_value, bool b)
        {
            if (b)
                data.stayAlive.Add(new_value);
            else
                data.stayAlive.Remove(new_value);
        }
    }
}
