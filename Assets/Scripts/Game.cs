
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace LoD
{
    public enum BrushMode
    {
        Toggle, On, Off
    }

    public class Game : NetworkBehaviour
    {
        public const string screenshotFolder = "Screenshots";
        const string shareSubject = "Look at my board.";
        const string shareMessage = "Play Life or Death now.";
        const string shareUrl = "https://github.com/Friendly-Banana/DeathOrLife";

        public static Game singleton;
        [SyncVar(hook = nameof(ChangeBoardSize))]
        public Vector2Int boardSize;
        void ChangeBoardSize(Vector2Int oldSize, Vector2Int size) => boardTilemap.transform.parent.position = new Vector3(-boardSize.x / 2f, boardSize.y / 2f, 0) * Zoom.scale;

        public Image brush;
        public Sprite[] brushSprites;
        public BrushMode brushMode = BrushMode.Toggle;
        Vector2Int lastChangedCell;
        public float cooldown = 0.5f;
        float timer;

        public string[] errorMsgs;
        public TMP_Text scoreboard;
        public TMP_Text msgText;
        public TMP_Text cellsLeftText;
        public GameObject gameOverText;
        public GameObject canvas;
        public GameObject inputPanel;
        public GameObject placeFigureButton;
        public GameObject cancelFigureButton;
        public GameObject hoveringFigurePrefab;
        public Transform hoveringFigureParent;

        public Tabs tabs;
        public GameObject figurePrefab;
        public Tilemap boardTilemap;
        public Tilemap canSetTilemap;
        public Tile aliveTile;
        public Tile deadTile;
        public Tile canSetTile;

        HoveringFigure hoveringFigure;
        Player _player;
        Player player
        {
            get
            {
                _player ??= NetworkClient.localPlayer.GetComponent<Player>();
                return _player;
            }
        }
        List<NRP> _roomPlayers;
        List<NRP> roomPlayers
        {
            get
            {
                _roomPlayers ??= FindObjectsOfType<NRP>().Select(x => (NRP)x).OrderBy(x => x.index).ToList();
                return _roomPlayers;
            }
        }

        static Vector3Int ToMapCoords(int x, int y) => new Vector3Int(x, y, 0);

        private void Awake() => singleton = this;

        private void Start()
        {
            msgText.CrossFadeAlpha(0, 0, true);
            inputPanel.SetActive(false);
            FigureCategory[] categories = (FigureCategory[])Enum.GetValues(typeof(FigureCategory));
            for (int i = 0; i < categories.Length; i++)
            {
                FigureCategory c = categories[i];
                tabs.AddTab(c.ToString(), (tab) =>
                {
                    foreach (Figure figure in GameManager.singleton.figures.Where(x => x.category == c))
                    {
                        GameObject go = Instantiate(figurePrefab, tab.GetChild(0).GetChild(0));
                        go.GetComponentInChildren<TMP_Text>().text = figure.name;
                        Tilemap tilemap = go.GetComponentInChildren<Tilemap>();
                        float scale = 100f / Mathf.Max(figure.width, figure.height);
                        tilemap.transform.localScale = Vector3.one * scale;
                        for (int x = 0; x < figure.width; x++)
                            for (int y = 0; y < figure.height; y++)
                                if (figure.cells[x][y])
                                    tilemap.SetTile(ToMapCoords(x, y), aliveTile);
                        Figure f = figure;
                        go.GetComponent<Button>().onClick.AddListener(() =>
                        {
                            hoveringFigure ??= Instantiate(hoveringFigurePrefab, hoveringFigureParent).GetComponent<HoveringFigure>();
                            hoveringFigure.Init(f, player.index, boardTilemap);
                            placeFigureButton.SetActive(true);
                            cancelFigureButton.SetActive(true);
                        });
                    }
                });
                tabs.Select(0);
            }
        }

        private void Update()
        {
            if (player == null || !player.canSet) return;
            if (timer > 0)
                timer -= Time.deltaTime;
#if UNITY_EDITOR || UNITY_STANDALONE
            if (Input.GetMouseButton(0))
            {
                HandleClick(Input.mousePosition, -1);
            }
#else
            foreach (Touch touch in Input.touches)
            {
                if (touch.phase == TouchPhase.Began || touch.phase == TouchPhase.Moved)
                {
                    HandleClick(touch.position, touch.fingerId);
                }
            }
#endif
        }

        private void HandleClick(Vector2 position, int fingerId)
        {
            // mouse on top of buttons
            if (player != null && !EventSystem.current.IsPointerOverGameObject(fingerId) && !Zoom.zooming && (hoveringFigure == null || !hoveringFigure.dragging))
            {
                Vector3 worldPoint = Camera.main.ScreenToWorldPoint(position);
                Vector3Int pos = boardTilemap.WorldToCell(worldPoint);
                Vector2Int pos2 = (Vector2Int)pos;
                if ((pos2 != lastChangedCell || brushMode == BrushMode.Toggle && timer <= 0) && player.canSetAt.Contains(pos2))
                {
                    timer = cooldown;
                    lastChangedCell = pos2;
                    player.CmdSetCell(pos2, brushMode);
                }
            }
        }

        public void TakeScreenshot() => StartCoroutine(Screenshot());
        IEnumerator Screenshot()
        {
            var b = Directory.CreateDirectory(Path.Combine(Application.persistentDataPath, screenshotFolder));
            // TODO
            foreach (var item in b.EnumerateFiles())
            {
            }
            // don't overwrite old screenshots
            int i = 0;
            string filename;
            while (true)
            {
                filename = $"Screenshot {i}.png";
                if (!File.Exists(Path.Combine(Application.persistentDataPath, screenshotFolder, filename)))
                    break;
                i++;
            }
            string fullPath = Path.Combine(Application.persistentDataPath, screenshotFolder, filename);

            canvas.SetActive(false);
#if UNITY_EDITOR || UNITY_STANDALONE
            ScreenCapture.CaptureScreenshot(fullPath);
#elif UNITY_ANDROID || UNITY_IOS
            ScreenCapture.CaptureScreenshot(filename);
#endif
            yield return new WaitForSeconds(0.3f);

            canvas.SetActive(true);
            ShowMsg("Saved as " + filename);
            ShareFile(fullPath);
        }

        public static void ShareFile(string fullPath)
        {
            new NativeShare().AddFile(fullPath).SetSubject(shareSubject).SetText(shareMessage).SetUrl(shareUrl).Share();
        }

        public void SetToggleCooldown(float value)
        {
            cooldown = value;
        }

        public void NextBrush()
        {
            int nextMode = ((int)brushMode + 1) % brushSprites.Length;
            brushMode = (BrushMode)nextMode;
            brush.sprite = brushSprites[nextMode];
        }

        public void PlaceFigure()
        {
            player.CmdPlaceFigure(hoveringFigure.figure, (Vector2Int)boardTilemap.WorldToCell(hoveringFigure.transform.position));
            CancelFigure();
        }

        public void CancelFigure()
        {
            if (hoveringFigure != null)
            {
                Destroy(hoveringFigure.gameObject);
                hoveringFigure = null;
            }
            placeFigureButton.SetActive(false);
            cancelFigureButton.SetActive(false);
        }

        public void CheckInputPanel() => inputPanel.SetActive(player.canSet);

        public void FinishInput()
        {
            inputPanel.SetActive(false);
            CancelFigure();
            canSetTilemap.ClearAllTiles();
            player.canSetAt.Clear();
            player.CmdInputFinished();
        }

        public void Lobby() => GameManager.singleton.ServerChangeScene(GameManager.singleton.RoomScene);

        public void MainMenu()
        {
            GameManager.singleton.StopHost();
            GameManager.singleton.StopClient();
        }

        public void YourTurn()
        {
            ShowMsg("Your turn!");
            inputPanel.SetActive(true);
            placeFigureButton.SetActive(false);
            cancelFigureButton.SetActive(false);
            canSetTilemap.ClearAllTiles();
            foreach (Vector2Int pos in player.canSetAt)
            {
                canSetTilemap.SetTile(ToMapCoords(pos.x, pos.y), canSetTile);
            }
        }

        public void ShowError(Error e) => ShowMsg("<style=Error>" + errorMsgs[(int)e]);

        public void ShowMsg(string msg)
        {
            msgText.CrossFadeAlpha(1, 0, true);
            msgText.text = msg;
            msgText.CrossFadeAlpha(0, 3, false);
        }

        [ClientRpc]
        public void RpcUpdateCell(int x, int y, bool alive, int owner) => UpdateCell(x, y, alive, owner);

        public void UpdateCell(int x, int y, bool alive, int owner)
        {
            Vector3Int pos = ToMapCoords(x, y);
            boardTilemap.SetTile(pos, alive ? aliveTile : deadTile);
            if (alive)
            {
                boardTilemap.SetColor(pos, roomPlayers[owner].color);
            }
        }

        [ClientRpc]
        public void RpcUpdateBoard(string scoreboardText, CellArrayWrapper[] currentBoard)
        {
            scoreboard.text = scoreboardText;
            for (int x = 0; x < currentBoard.Length; x++)
            {
                for (int y = 0; y < currentBoard[x].Length; y++)
                {
                    Cell cell = currentBoard[x][y];
                    UpdateCell(x, y, cell.alive, cell.owner);
                }
            }
        }

        [ClientRpc]
        public void RpcWinner(int index, string name)
        {
            ShowMsg(index == player.index ? "You have won!" : name + " has won!");
            gameOverText.SetActive(true);
        }
    }
}