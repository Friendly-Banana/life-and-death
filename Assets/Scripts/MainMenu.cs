using System.Collections;
using System.Collections.Generic;
using System.IO;
using Mirror;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace LoD
{
    public class MainMenu : MonoBehaviour
    {
        readonly Dictionary<long, DiscoveryResponse> discoveredServers = new Dictionary<long, DiscoveryResponse>();
        public CustomNetworkDiscovery networkDiscovery;
        public Transform serverParent;
        public GameObject serverPrefab;
        public GameObject noServerText;

        public Transform screenshotParent;
        public GameObject screenshotPrefab;
        public GameObject noScreenshotsText;

        int screenshots = 0;

        private void Start()
        {
            Texture2D texture;
            var imageDir = Path.Combine(Application.persistentDataPath, Game.screenshotFolder);
            Directory.CreateDirectory(imageDir);
            foreach (string filePath in Directory.EnumerateFiles(imageDir, "Screenshot *.png"))
            {
                screenshots++;
                texture = new Texture2D(1, 1);
                texture.LoadImage(File.ReadAllBytes(filePath));
                GameObject screenshot = Instantiate(screenshotPrefab, screenshotParent);
                //screenshot.GetComponentInChildren<TMP_Text>().text = Path.GetFileNameWithoutExtension(filePath);
                screenshot.GetComponentInChildren<RawImage>().texture = texture;
                string path = filePath;
                screenshot.GetComponentsInChildren<Button>()[0].onClick.AddListener(() =>
                {
                    File.Delete(path);
                    Destroy(screenshot);
                    screenshots--;
                    noScreenshotsText.SetActive(screenshots == 0);
                });
                screenshot.GetComponentsInChildren<Button>()[1].onClick.AddListener(() => Game.ShareFile(path));
            }
            noScreenshotsText.SetActive(screenshots == 0);
        }

        public void StartServer()
        {
#if UNITY_WEBGL
            NetworkServer.dontListen = true;
            GameManager.singleton.StartHost();
#else
            discoveredServers.Clear();  
            GameManager.singleton.StartHost();
            networkDiscovery.AdvertiseServer();  
#endif
        }

        public void ShowServers()
        {
            discoveredServers.Clear();
            networkDiscovery.StartDiscovery();
            noServerText.SetActive(discoveredServers.Count == 0);
        }

        // found a server, add for later use
        public void AddServer(DiscoveryResponse info)
        {
            discoveredServers[info.serverId] = info;
            noServerText.SetActive(false);
            serverParent.ClearChildren();
            foreach (DiscoveryResponse response in discoveredServers.Values)
            {
                GameObject newServer = Instantiate(serverPrefab, serverParent);
                newServer.GetComponentsInChildren<TMP_Text>()[0].text = response.hostName;
                newServer.GetComponentsInChildren<TMP_Text>()[1].text = $"{response.totalPlayers} / {GameManager.singleton.maxConnections}";
                newServer.GetComponent<Button>().onClick.AddListener(() => JoinServer(response));
            }
        }

        void JoinServer(DiscoveryResponse response)
        {
            networkDiscovery.StopDiscovery();
            GameManager.singleton.StartClient(response.uri);
        }
    }
}
