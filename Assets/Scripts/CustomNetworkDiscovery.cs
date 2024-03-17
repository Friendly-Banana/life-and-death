using System;
using System.Net;
using Mirror;
using Mirror.Discovery;
using UnityEngine;
using UnityEngine.Events;

namespace LoD {
    /*
        Discovery Guide: https://mirror-networking.com/docs/Guides/NetworkDiscovery.html
        Documentation: https://mirror-networking.com/docs/Components/NetworkDiscovery.html
        API Reference: https://mirror-networking.com/docs/api/Mirror.Discovery.NetworkDiscovery.html
    */

    public class DiscoveryRequest : NetworkMessage {
        public string version;
    }

    public struct DiscoveryResponse : NetworkMessage {
        // The server that sent this
        // this is a property so that it is not serialized,  but the
        // client fills this up after we receive it
        public IPEndPoint EndPoint { get; set; }

        public Uri uri;

        // Prevent duplicate server appearance when a connection can be made via LAN on multiple NICs
        public long serverId;

        public int totalPlayers;
        public string hostName;
    }

    [Serializable]
    public class ServerFoundUnityEvent : UnityEvent<DiscoveryResponse> {
    }

    [DisallowMultipleComponent]
    [AddComponentMenu("Network/NetworkDiscovery")]
    public class CustomNetworkDiscovery : NetworkDiscoveryBase<DiscoveryRequest, DiscoveryResponse> {
        #region Server

        private long ServerId { get; set; }

        [Tooltip("Transport to be advertised during discovery")]
        public Transport transport;

        [Tooltip("Invoked when a server is found")]
        public ServerFoundUnityEvent OnServerFound;

        public override void Start() {
            ServerId = RandomLong();

            // active transport gets initialized in awake
            // so make sure we set it here in Start()  (after awakes)
            // Or just let the user assign it in the inspector
            if (transport == null)
                transport = Transport.activeTransport;

            base.Start();
        }

        /// <summary>
        ///     Process the request from a client
        /// </summary>
        /// <remarks>
        ///     Override if you wish to provide more information to the clients
        ///     such as the name of the host player
        /// </remarks>
        /// <param name="request">Request comming from client</param>
        /// <param name="endpoint">Address of the client that sent the request</param>
        /// <returns>The message to be sent back to the client or null</returns>
        protected override DiscoveryResponse ProcessRequest(DiscoveryRequest request, IPEndPoint endpoint) {
            // In this case we don't do anything with the request
            // but other discovery implementations might want to use the data
            // in there. This way the client can ask for
            // specific game mode or something

            try {
                // this is an example reply message, return your own
                // to include whatever is relevant for your game
                return new DiscoveryResponse {
                    serverId = ServerId,
                    uri = transport.ServerUri(),
                    totalPlayers = NetworkManager.singleton.numPlayers,
                    hostName = "Server"
                };
            }
            catch (NotImplementedException) {
                Debug.LogError($"Transport {transport} does not support Network Discovery");
                throw;
            }
        }

        #endregion

        #region Client

        /// <summary>
        ///     Create a message that will be broadcasted on the network to discover servers
        /// </summary>
        /// <remarks>
        ///     Override if you wish to include additional data in the discovery message
        ///     such as desired game mode, language, difficulty, etc...
        /// </remarks>
        /// <returns>An instance of DiscoveryRequest with data to be broadcasted</returns>
        protected override DiscoveryRequest GetRequest() {
            return new DiscoveryRequest { version = Application.version };
        }

        /// <summary>
        ///     Process the answer from a server
        /// </summary>
        /// <remarks>
        ///     A client receives a reply from a server, this method processes the
        ///     reply and raises an event
        /// </remarks>
        /// <param name="response">Response that came from the server</param>
        /// <param name="endpoint">Address of the server that replied</param>
        protected override void ProcessResponse(DiscoveryResponse response, IPEndPoint endpoint) {
            // we received a message from the remote endpoint
            response.EndPoint = endpoint;

            // although we got a supposedly valid url, we may not be able to resolve
            // the provided host
            // However we know the real ip address of the server because we just
            // received a packet from it,  so use that as host.
            var realUri = new UriBuilder(response.uri) {
                Host = response.EndPoint.Address.ToString()
            };
            response.uri = realUri.Uri;

            OnServerFound.Invoke(response);
        }

        #endregion
    }
}