using LoD;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NRP : NetworkRoomPlayer
{
    [SyncVar(hook = nameof(ChangeName))]
    public string playerName = "Player";
    [SyncVar(hook = nameof(ChangeColor))]
    public Color color = Color.white;

    public bool isHost => !isClientOnly;//index == 0

    [Command]
    public void CmdSetName(string newName) => playerName = string.IsNullOrWhiteSpace(newName) ? "Player" + index : newName;

    [Command]
    public void CmdSetColor(Color newColor) => color = newColor;

    [Command]
    public void CmdKickPlayer(NRP player)
    {
        // no self-kick
        if (isHost && player.index != index)
            player.connectionToClient.Disconnect();
    }

    GameObject info;
    void ChangeName(string _, string newName) { if (info != null) info.GetComponentsInChildren<TMP_Text>()[0].text = newName; }
    void ChangeColor(Color _, Color newColor) { if (info != null) info.GetComponentsInChildren<TMP_Text>()[0].color = newColor; }
    
    public override void OnClientEnterRoom()
    {
        if (info != null)
        {
            return;
        }
        info = Instantiate(Lobby.singleton.playerPrefab, Lobby.singleton.playerParent);
        TMP_Text[] texts = info.GetComponentsInChildren<TMP_Text>();
        texts[0].text = playerName;
        texts[0].color = color;
        texts[1].text = readyToBegin ? "Ready" : "Not Ready";
        Button button = info.GetComponentInChildren<Button>();
        // no self-kick
        button.interactable = isHost && !isLocalPlayer;
        button.onClick.AddListener(() => CmdKickPlayer(this));
    }

    public override void OnClientExitRoom() => Destroy(info);

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState) { if (info != null) info.GetComponentsInChildren<TMP_Text>()[1].text = newReadyState ? "Ready" : "Not Ready"; }
    
    public override void OnStartLocalPlayer()
    {
        Lobby.singleton.OnLocalPlayerReady();
        if (info != null)
            info.GetComponentInChildren<Button>().interactable = false;
    }
}
