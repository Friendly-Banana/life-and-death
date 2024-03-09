using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace LoD
{
    public class Player : NetworkBehaviour
    {
        [SyncVar]
        public NRP roomPlayer;
        public int index => roomPlayer.index;
        public string playerName => roomPlayer.playerName;
        public Color color => roomPlayer.color;

        [SyncVar]
        public bool dead = false;
        [SyncVar]
        public bool canSet = false;
        // cells we can set this round
        [SyncVar(hook = nameof(ChangeCellsLeft))]
        public int cellsLeft;
        public List<Vector2Int> canSetAt = new List<Vector2Int>();
        public bool[][] changedCells;

        void ChangeCellsLeft(int old, int cellsLeft)
        {
            if (isLocalPlayer)
                Game.singleton.cellsLeftText.text = $"{cellsLeft} change{(cellsLeft == 1 ? "" : "s")} left";
        }

        [TargetRpc]
        public void TargetTurn(List<Vector2Int> settableArea)
        {
            canSetAt = settableArea;
            Game.singleton.YourTurn();
        }

        [TargetRpc]
        public void Error(Error e) => Game.singleton.ShowError(e);

        [Command]
        public void CmdSetCell(Vector2Int pos, BrushMode brushMode) => GameManager.singleton.SetCell(this, pos, brushMode);

        [Command]
        public void CmdPlaceFigure(Figure figure, Vector2Int pos) => GameManager.singleton.PlaceFigure(this, figure, pos);

        [Command]
        public void CmdInputFinished()
        {
            canSet = false;
            canSetAt.Clear();
            GameManager.singleton.FinishedInput(this);
        }
    }
}