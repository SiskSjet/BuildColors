using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRage.Utils;

namespace Sisk.BuildColors.Services {

    /// <summary>
    /// Carries a share from one player to another. Everything goes through the server, which stamps
    /// the sender, so a client cannot put someone else's name on a share.
    /// </summary>
    public static class ShareNetwork {
        /// <summary>
        /// Everyone online rather than one player.
        /// </summary>
        public const ulong EVERYONE = 0UL;

        private const ushort HANDLER_ID = 47311;

        /// <summary>
        /// A share larger than this is dropped, so one client cannot flood the others.
        /// </summary>
        private const int MAX_PACKET_BYTES = 64 * 1024;

        private static readonly List<IMyPlayer> _playerBuffer = new List<IMyPlayer>();

        private static bool _registered;

        /// <summary>
        /// Raised on the receiving client once a share arrives.
        /// </summary>
        public static event Action<SharePacket> Received;

        /// <summary>
        /// Sharing needs someone to share with, which means a multiplayer session.
        /// </summary>
        public static bool IsAvailable {
            get { return MyAPIGateway.Multiplayer != null && MyAPIGateway.Multiplayer.MultiplayerActive; }
        }

        public static void Register() {
            if (_registered || MyAPIGateway.Multiplayer == null) {
                return;
            }

            MyAPIGateway.Multiplayer.RegisterSecureMessageHandler(HANDLER_ID, OnMessage);
            _registered = true;
        }

        public static void Unregister() {
            if (!_registered) {
                return;
            }

            MyAPIGateway.Multiplayer.UnregisterSecureMessageHandler(HANDLER_ID, OnMessage);
            _registered = false;
        }

        /// <summary>
        /// The players a share can be sent to right now, the local player left out.
        /// </summary>
        public static List<SharePlayer> GetRecipients() {
            var recipients = new List<SharePlayer>();

            if (MyAPIGateway.Players == null) {
                return recipients;
            }

            var self = MyAPIGateway.Multiplayer != null ? MyAPIGateway.Multiplayer.MyId : 0UL;

            _playerBuffer.Clear();
            MyAPIGateway.Players.GetPlayers(_playerBuffer);

            foreach (var player in _playerBuffer) {
                if (player == null || player.IsBot || player.SteamUserId == 0UL || player.SteamUserId == self) {
                    continue;
                }

                recipients.Add(new SharePlayer(player.SteamUserId, player.DisplayName));
            }

            _playerBuffer.Clear();
            recipients.Sort((left, right) => string.Compare(left.Name, right.Name, StringComparison.InvariantCultureIgnoreCase));

            return recipients;
        }

        /// <summary>
        /// Hands a share to the server for delivery. False when it could not be packed or sent.
        /// </summary>
        public static bool Send(SharePacket packet) {
            if (packet == null || !IsAvailable) {
                return false;
            }

            byte[] bytes;
            if (!TrySerialize(packet, out bytes)) {
                return false;
            }

            if (MyAPIGateway.Multiplayer.IsServer) {
                Relay(packet, MyAPIGateway.Multiplayer.MyId);
                return true;
            }

            MyAPIGateway.Multiplayer.SendMessageToServer(HANDLER_ID, bytes);

            return true;
        }

        private static void OnMessage(ushort id, byte[] bytes, ulong sender, bool fromServer) {
            if (bytes == null || bytes.Length > MAX_PACKET_BYTES) {
                return;
            }

            SharePacket packet;
            if (!TryDeserialize(bytes, out packet)) {
                return;
            }

            if (MyAPIGateway.Multiplayer.IsServer && !fromServer) {
                Relay(packet, sender);
                return;
            }

            if (fromServer) {
                Deliver(packet);
            }
        }

        /// <summary>
        /// Runs on the server: names the sender and passes the share on to whoever it is for.
        /// </summary>
        private static void Relay(SharePacket packet, ulong senderId) {
            packet.SenderId = senderId;
            packet.SenderName = GetPlayerName(senderId);

            byte[] bytes;
            if (!TrySerialize(packet, out bytes)) {
                return;
            }

            var self = MyAPIGateway.Multiplayer.MyId;

            if (packet.Recipient != EVERYONE) {
                if (packet.Recipient == senderId) {
                    return;
                }

                if (packet.Recipient == self) {
                    Deliver(packet);
                } else {
                    MyAPIGateway.Multiplayer.SendMessageTo(HANDLER_ID, bytes, packet.Recipient);
                }

                return;
            }

            _playerBuffer.Clear();
            MyAPIGateway.Players.GetPlayers(_playerBuffer);

            foreach (var player in _playerBuffer) {
                if (player == null || player.IsBot || player.SteamUserId == 0UL || player.SteamUserId == senderId) {
                    continue;
                }

                if (player.SteamUserId == self) {
                    Deliver(packet);
                } else {
                    MyAPIGateway.Multiplayer.SendMessageTo(HANDLER_ID, bytes, player.SteamUserId);
                }
            }

            _playerBuffer.Clear();
        }

        private static void Deliver(SharePacket packet) {
            if (MyAPIGateway.Utilities.IsDedicated) {
                return;
            }

            var handler = Received;
            if (handler != null) {
                handler(packet);
            }
        }

        private static string GetPlayerName(ulong steamId) {
            if (MyAPIGateway.Players == null) {
                return null;
            }

            _playerBuffer.Clear();
            MyAPIGateway.Players.GetPlayers(_playerBuffer);

            string name = null;

            foreach (var player in _playerBuffer) {
                if (player != null && player.SteamUserId == steamId) {
                    name = player.DisplayName;
                    break;
                }
            }

            _playerBuffer.Clear();

            return name;
        }

        private static bool TrySerialize(SharePacket packet, out byte[] bytes) {
            bytes = null;

            try {
                bytes = MyAPIGateway.Utilities.SerializeToBinary(packet);
            } catch (Exception exception) {
                MyLog.Default.Error($"BuildColors could not pack a share: {exception.Message}");
                return false;
            }

            return bytes != null && bytes.Length <= MAX_PACKET_BYTES;
        }

        private static bool TryDeserialize(byte[] bytes, out SharePacket packet) {
            packet = null;

            try {
                packet = MyAPIGateway.Utilities.SerializeFromBinary<SharePacket>(bytes);
            } catch (Exception) {
                return false;
            }

            return packet != null;
        }
    }

    /// <summary>
    /// A player a share can be sent to.
    /// </summary>
    public struct SharePlayer {
        public SharePlayer(ulong id, string name) {
            Id = id;
            Name = name ?? string.Empty;
        }

        public ulong Id { get; private set; }

        public string Name { get; private set; }
    }
}
