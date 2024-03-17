using System;
using System.Collections.Generic;
using System.Linq;
using Mirror;
using UnityEngine;
using Random = UnityEngine.Random;

namespace LoD {
    public enum Error {
        NotEnoughCells,
        CantPlaceHere,
        NotYourTurn,
        YouDied
    }

    [Serializable]
    public class CellArrayWrapper {
        public Cell[] array;

        public CellArrayWrapper() {
        }

        public CellArrayWrapper(Cell[] cells) {
            array = cells;
        }

        public int Length => array.Length;

        public Cell this[int index] {
            get => array[index];
            set => array[index] = value;
        }
    }

    public struct Cell {
        public bool alive;
        public int owner;

        public override string ToString() {
            return $"Cell {{Owner Id: {owner}, alive: {alive}}}";
        }

        public Cell Clone() {
            return new Cell { alive = alive, owner = owner };
        }
    }

    internal enum State {
        Updating,
        ShouldRequestInput,
        RequestedInput,
        GameOver
    }

    public class GameManager : NetworkRoomManager {
        public new static GameManager singleton;

        public List<Figure> figures = new();

        public float updateDelay = 0.5f;

        public int currentPlayer;
        public int generation;

        public List<Player> players;

        // Player Id, amount of cells
        private readonly Dictionary<int, int> cellCount = new();

        private Cell[][] board;

        // till next boardUpdate
        private float countdown;
        private GameOptions settings;
        private State state = State.ShouldRequestInput;

        private void Update() {
            if (!NetworkServer.active || !IsSceneActive(GameplayScene) || players.Count == 0)
                return;
            switch (state) {
                case State.Updating:
                    countdown += Time.deltaTime;
                    if (countdown >= updateDelay) {
                        countdown = 0;
                        NextGeneration();
                    }
                    break;
                case State.ShouldRequestInput:
                    state = State.RequestedInput;
                    var player = players[currentPlayer];
                    var array = new bool[board.Length][];
                    for (var x = 0; x < board.Length; x++) array[x] = new bool[board[x].Length];
                    player.changedCells = array;
                    // TODO: increase in waves or smth
                    player.cellsLeft = settings.cellsToSet;
                    player.canSet = true;
                    SetPlaceableArea(player);
                    player.TargetTurn(player.canSetAt);
                    break;
            }
        }

        private bool IsOutOfBoard(int x, int y) {
            return !(0 <= x && x < settings.boardWidth && 0 <= y && y < settings.boardHeight);
        }

        [Server]
        private void RpcUpdateBoard() {
            var scores = string.Join("\n", players.Select(PlayerScore));
            Game.singleton.RpcUpdateBoard($"<b>Generation: {generation}</b>\n" + scores,
                board.Select(x => new CellArrayWrapper(x)).ToArray());

            string PlayerScore(Player x) {
                var player = x.roomPlayer;
                return
                    $"<color=#{ColorUtility.ToHtmlStringRGB(player.color)}>{player.playerName}: {cellCount.GetValueOrDefault(player.index, 0)}";
            }
        }

        private void NextGeneration() {
            cellCount.Clear();
            var newBoard = board.Select(a => a.Select(cell => cell.Clone()).ToArray()).ToArray();
            for (var x = 0; x < board.Length; x++) {
                for (var y = 0; y < board[x].Length; y++) {
                    var cell = NewCellState(x, y);
                    newBoard[x][y] = cell;
                    // increase owner's cell count
                    if (cell.alive) {
                        if (!cellCount.TryAdd(cell.owner, 1))
                            cellCount[cell.owner] += 1;
                    }
                }
            }
            board = newBoard;
            generation++;
            if (generation % settings.stopRound == 0)
                state = State.ShouldRequestInput;
            RpcUpdateBoard();

            if (!settings.playersCanDie)
                return;
            foreach (var player in players) {
                if (cellCount.GetValueOrDefault(player.index) == 0) {
                    player.dead = true;
                    player.Error(Error.YouDied);
                }
            }

            players.RemoveAll(p => p.dead);
            if (players.Count == 1) {
                state = State.GameOver;
                Game.singleton.RpcWinner(players[0].index, players[0].playerName);
            }
            else if (players.Count == 0) {
                state = State.GameOver;
                Game.singleton.RpcWinner(-1, "Nobody");
            }
        }

        // returns alive and owner for next round
        private Cell NewCellState(int x, int y) {
            var neighbourOwners = new Dictionary<int, int>();
            for (var nx = x - 1; nx < x + 2; nx++) {
                for (var ny = y - 1; ny < y + 2; ny++) {
                    int xPos = nx, yPos = ny;
                    // same cell
                    if (nx == x && ny == y)
                        continue;
                    // out of board
                    if (IsOutOfBoard(nx, ny)) {
                        if (!settings.wrapAround)
                            continue;

                        xPos = (xPos % settings.boardWidth + settings.boardWidth) % settings.boardWidth;
                        yPos = (yPos % settings.boardHeight + settings.boardHeight) % settings.boardHeight;
                    }

                    var cell = board[xPos][yPos];
                    // increase owner's cell count
                    if (cell.alive) {
                        if (!neighbourOwners.TryAdd(cell.owner, 1))
                            neighbourOwners[cell.owner] += 1;
                    }
                }
            }

            var aliveNeighbours = neighbourOwners.Values.Sum();
            return new Cell {
                alive = (!board[x][y].alive && settings.revive.Contains(aliveNeighbours)) ||
                        (board[x][y].alive && settings.stayAlive.Contains(aliveNeighbours)),
                owner = neighbourOwners.Count == 0
                    ? Random.Range(0, players.Count)
                    : neighbourOwners.Aggregate((l, r) => l.Value > r.Value ? l : r).Key
            };
        }

        [Server]
        public void SetCell(Player player, Vector2Int pos, BrushMode brushMode) {
            if (!player.canSetAt.Contains(pos)) {
                player.Error(Error.CantPlaceHere);
                return;
            }

            int x = pos.x, y = pos.y;
            if (IsOutOfBoard(x, y)) {
                Debug.LogError("IndexOutOfRangeException");
                return;
            }

            var targetValue = false;
            switch (brushMode) {
                case BrushMode.Toggle:
                    targetValue = !board[x][y].alive;
                    break;
                case BrushMode.On:
                    targetValue = true;
                    break;
                case BrushMode.Off:
                    break;
            }

            if (board[x][y].alive == targetValue)
                return;

            if (player.changedCells[x][y]) {
                player.changedCells[x][y] = false;
                player.cellsLeft++;
            }
            else if (player.cellsLeft > 0) {
                player.changedCells[x][y] = true;
                player.cellsLeft--;
            }
            else {
                player.Error(Error.NotEnoughCells);
                return;
            }

            board[x][y].alive = targetValue;
            if (targetValue)
                board[x][y].owner = player.index;
            Game.singleton.RpcUpdateCell(x, y, targetValue, player.index);
        }

        [Server]
        public void FinishedInput(Player player) {
            if (player.index != currentPlayer) {
                player.Error(Error.NotYourTurn);
                return;
            }

            currentPlayer++;
            state = State.ShouldRequestInput;
            // last Player, input round finished
            if (currentPlayer >= players.Count) {
                currentPlayer = 0;
                state = State.Updating;
            }
        }

        [Server]
        // returns the area in which the player can change cells
        private void SetPlaceableArea(Player player) {
            player.canSetAt.Clear();
            for (var x = 0; x < board.Length; x++)
            for (var y = 0; y < board[x].Length; y++)
                if (!board[x][y].alive || board[x][y].owner == player.index)
                    player.canSetAt.Add(new Vector2Int(x, y));
        }

        private int FigureCost(Player player, Figure figure, Vector2Int pos) {
            var cellsChanged = 0;
            for (var x = 0; x < figure.cells.Length; x++)
            for (var y = 0; y < figure.cells[x].Length; y++) {
                if (IsOutOfBoard(x + pos.x, y + pos.y))
                    continue;
                if (figure.cells[x][y] && !player.canSetAt.Contains(new Vector2Int(x + pos.x, y + pos.y)))
                    return int.MaxValue;
                if (figure.cells[x][y] == board[x + pos.x][y + pos.y].alive)
                    continue;
                if (player.changedCells[x + pos.x][y + pos.y])
                    cellsChanged--;
                else
                    cellsChanged++;
            }

            return cellsChanged;
        }

        [Server]
        public void PlaceFigure(Player player, Figure figure, Vector2Int pos) {
            var cost = FigureCost(player, figure, pos);
            if (cost == int.MaxValue) {
                player.Error(Error.CantPlaceHere);
                return;
            }

            if (cost > player.cellsLeft) {
                player.Error(Error.NotEnoughCells);
                return;
            }

            for (var x = 0; x < figure.cells.Length; x++) {
                for (var y = 0; y < figure.cells[x].Length; y++) {
                    if (IsOutOfBoard(x + pos.x, y + pos.y))
                        continue;
                    player.changedCells[x + pos.x][y + pos.y] = !player.changedCells[x + pos.x][y + pos.y];
                    if (figure.cells[x][y])
                        board[x + pos.x][y + pos.y] = new Cell { alive = true, owner = player.index };
                }
            }

            player.cellsLeft -= cost;
            RpcUpdateBoard();
        }

        #region NetworkManager

        public override void Awake() {
            base.Awake();
            singleton = this;
        }

        /// <summary>
        ///     This is called on the server when a networked scene finishes loading.
        /// </summary>
        /// <param name="sceneName">Name of the new scene.</param>
        public override void OnRoomServerSceneChanged(string sceneName) {
            if (sceneName == GameplayScene) {
                settings = SaveSystem.LoadSettings();
                updateDelay = settings.stopRound switch {
                    < 15 => 0.5f,
                    < 50 => 0.4f,
                    _ => 0.25f
                };
                Game.singleton.boardSize = new Vector2Int(settings.boardWidth, settings.boardHeight);
                board = new Cell[settings.boardWidth][];
                for (var x = 0; x < settings.boardWidth; x++) {
                    board[x] = new Cell[settings.boardHeight];
                    for (var y = 0; y < board[x].Length; y++) {
                        board[x][y] = new Cell();
                        if (settings.randomBoard && Random.value <= 0.5f) {
                            board[x][y].alive = true;
                            board[x][y].owner = Random.Range(0, players.Count);
                        }
                    }
                }
            }
            else {
                players.Clear();
            }
        }

        /// <summary>
        ///     This is called on the server when it is told that a client has finished switching from the room scene to a game
        ///     player scene.
        ///     <para>
        ///         When switching from the room, the room-player is replaced with a game-player object. This callback function
        ///         gives an opportunity to apply state from the room-player to the game-player object.
        ///     </para>
        /// </summary>
        /// <param name="conn">The connection of the player</param>
        /// <param name="roomPlayer">The room player object.</param>
        /// <param name="gamePlayer">The game player object.</param>
        /// <returns>False to not allow this player to replace the room player.</returns>
        public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer,
            GameObject gamePlayer) {
            players.Add(gamePlayer.GetComponent<Player>());
            gamePlayer.GetComponent<Player>().roomPlayer = roomPlayer.GetComponent<NRP>();
            RpcUpdateBoard();
            return base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);
        }

        #endregion
    }
}