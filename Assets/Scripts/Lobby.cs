using System.Collections;
using System.Collections.Generic;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoD
{
    public class Lobby : NetworkBehaviour
    {
        const string NAME_KEY = "Name";
        const string COLOR_KEY = "Color";
        public static Lobby singleton;
        public List<Color> colors;
        public GameObject colorPrefab;
        public Transform colorParent;
        public GameObject playerPrefab;
        public Transform playerParent;
        public TMP_Text startText;
        public TMP_InputField nameInput;
        public GameObject optionsButton;
        int selected;

        NRP _player;
        NRP localPlayer
        {
            get
            {
                _player ??= NetworkClient.localPlayer.GetComponent<NRP>();
                return _player;
            }
        }

        private void Awake()
        {
            singleton = this;
            optionsButton.SetActive(false);
        }

        public void OnLocalPlayerReady()
        {
            optionsButton.SetActive(localPlayer.isHost);
            nameInput.text = PlayerPrefs.GetString(NAME_KEY);
            localPlayer.CmdSetName(nameInput.text);
            for (int i = 0; i < colors.Count; i++)
            {
                GameObject go = Instantiate(colorPrefab, colorParent);
                go.GetComponent<Image>().color = colors[i];
                int index = i;
                go.GetComponent<Button>().onClick.AddListener(() => SelectColor(index));
            }
            SelectColor(PlayerPrefs.GetInt(COLOR_KEY, 0));
        }

        void SelectColor(int i)
        {
            PlayerPrefs.SetInt(COLOR_KEY, i);
            colorParent.GetChild(selected).GetComponent<Button>().interactable = true;
            colorParent.GetChild(i).GetComponent<Button>().interactable = false;
            localPlayer.CmdSetColor(colors[i]);
            selected = i;
        }

        public void SetNewName()
        {
            PlayerPrefs.SetString(NAME_KEY, nameInput.text);
            localPlayer.CmdSetName(nameInput.text);
        }

        public void StartGame()
        {
            startText.text = !localPlayer.readyToBegin ? "Cancel" : "Ready";
            localPlayer.CmdChangeReadyState(!localPlayer.readyToBegin);
        }

        public void LeaveLobby()
        {
            GameManager.singleton.StopClient();
            GameManager.singleton.StopHost();
        }
    }
}