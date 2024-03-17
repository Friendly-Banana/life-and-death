using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;

namespace LoD {
    public class HoveringFigure : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler,
        IPointerExitHandler {
        public Color normalColor;
        public Color dragColor;
        public Tilemap tilemap;
        public Figure figure;
        public Vector3Int cell;
        public bool dragging;

        public void OnBeginDrag(PointerEventData eventData) {
            dragging = true;
            SetDraggedPosition(eventData);
        }

        public void OnDrag(PointerEventData data) {
            SetDraggedPosition(data);
        }

        public void OnEndDrag(PointerEventData data) {
            dragging = false;
            SetDraggedPosition(data);
        }

        public void OnPointerEnter(PointerEventData eventData) {
            tilemap.color = dragColor;
        }

        public void OnPointerExit(PointerEventData eventData) {
            tilemap.color = normalColor;
        }

        public void Init(Figure fig, int ownerId, Tilemap boardTilemap) {
            tilemap.color = normalColor;
            figure = fig;
            tilemap.ClearAllTiles();
            for (var x = 0; x < figure.cells.Length; x++) {
                for (var y = 0; y < figure.cells[x].Length; y++)
                    if (figure.cells[x][y])
                        tilemap.SetTile(new Vector3Int(x, y), Game.singleton.aliveTile);
            }

            cell = new Vector3Int((Game.singleton.boardSize.x - figure.width) / 2,
                (Game.singleton.boardSize.y - figure.height) / 2, 0);
            transform.position = boardTilemap.CellToWorld(cell);
            GetComponent<RectTransform>().sizeDelta = new Vector2(figure.width, figure.height);
        }

        private void SetDraggedPosition(PointerEventData data) {
            var pos = Camera.main.ScreenToWorldPoint(data.position);
            // snapping
            cell = Game.singleton.boardTilemap.WorldToCell(pos) -
                   new Vector3Int(figure.width / 2, figure.height / 2, 0);
            transform.position = Game.singleton.boardTilemap.CellToWorld(cell);
        }
    }
}