using Mirror;
using UnityEngine;

namespace LoD
{
    public class Tile3D : NetworkBehaviour
    {
        [SyncVar]
        public Vector2Int pos;
        Material material;

        private void Awake()
        {
            material = GetComponent<Renderer>().material;
        }

        public void SetColor(Color color)
        {
            material.color = color;
        }

        private void OnDestroy()
        {
            Destroy(material);
        }
    }
}
