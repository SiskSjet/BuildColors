using System;
using System.Collections.Generic;
using Sandbox.ModAPI;
using Sisk.BuildColors.Net.Delegates;
using Sisk.BuildColors.Net.Messages;
using Sisk.BuildColors.Net.Wrapper;

// ReSharper disable ExplicitCallerInfoArgument
// ReSharper disable TryCastAlwaysSucceeds
// ReSharper disable MergeCastWithTypeCheck

namespace Sisk.BuildColors.Net {
    // todo: finalize Network and move it to Mod_Utils as library.
    /// <summary>
    ///     A class that handles networking.
    /// </summary>
    public class Network {
        private readonly Dictionary<long, HashSet<MessageHandlerWrapper>> _entityMessageHandler = new Dictionary<long, HashSet<MessageHandlerWrapper>>();
        private readonly ushort _id;
        private readonly Dictionary<string, HashSet<MessageHandlerWrapper>> _messageHandler = new Dictionary<string, HashSet<MessageHandlerWrapper>>();
        private readonly MessageHandlerWrapperComparer _wrapperComparer = new MessageHandlerWrapperComparer();

        /// <summary>
        ///     Create a new instance of <see cref="Network" />.
        /// </summary>
        /// <param name="id">
        ///     A unique id which is used to send messages and register message handler. No other mod should use the
        ///     same id.
        /// </param>
        public Network(ushort id) {
            _id = id;

            Register<EntityMessage>(OnEntityMessageReceived);
            MyAPIGateway.Multiplayer.RegisterMessageHandler(_id, OnMessageReceived);
        }

        /// <summary>
        ///     Indicates if networking is active.
        /// </summary>
        private bool Active => MyAPIGateway.Multiplayer.MultiplayerActive;

        /// <summary>
        ///     Indicates if this is a client.
        /// </summary>
        public bool IsClient => !(IsServer && IsDedicated);

        /// <summary>
        ///     Indicates if this is a deticated server.
        /// </summary>
        public bool IsDedicated => MyAPIGateway.Utilities.IsDedicated;

        /// <summary>
        ///     Indicates if this is a server.
        /// </summary>
        public bool IsServer => MyAPIGateway.Multiplayer.IsServer;

        /// <summary>
        ///     Id for this network handler.
        /// </summary>
        public ulong MyId => MyAPIGateway.Multiplayer.MyId;

        /// <summary>
        ///     Close all network connections and close gracefully.
        /// </summary>
        public void Close() {
            MyAPIGateway.Multiplayer.UnregisterMessageHandler(_id, OnMessageReceived);
            Unregister<EntityMessage>(OnEntityMessageReceived);
        }

        /// <summary>
        ///     Register a message handler for given type.
        /// </summary>
        /// <typeparam name="TMessageType">The type of the message.</typeparam>
        /// <param name="handler">The handler executed on received message of given type.</param>
        public void Register<TMessageType>(MessageHandler<TMessageType> handler) where TMessageType : IMessage {
            var type = typeof(TMessageType).FullName;

            if (type == null) {
                throw new Exception($"Unable to get type of {typeof(TMessageType)}");
            }

            if (!_messageHandler.ContainsKey(type)) {
                _messageHandler.Add(type, new HashSet<MessageHandlerWrapper>(_wrapperComparer));
            }

            _messageHandler[type].Add(MessageHandlerWrapper.Create(handler));
        }

        /// <summary>
        ///     Register a entity message handler for given entity id.
        /// </summary>
        /// <typeparam name="TMessageType">The type of the message.</typeparam>
        /// <param name="entityId">The entity id.</param>
        /// <param name="handler">The handler executed on received message of given entity id.</param>
        public void Register<TMessageType>(long entityId, EntityMessageHandler<TMessageType> handler) where TMessageType : IEntityMessage {
            if (!_entityMessageHandler.ContainsKey(entityId)) {
                _entityMessageHandler.Add(entityId, new HashSet<MessageHandlerWrapper>(_wrapperComparer));
            }

            _entityMessageHandler[entityId].Add(MessageHandlerWrapper.Create(handler));
        }

        /// <summary>
        ///     Sends a message to given recipient.
        /// </summary>
        /// <param name="message">The message send to recipient.</param>
        /// <param name="recipient">The recipient who should receive this message.</param>
        public void Send(IMessage message, ulong recipient) {
            if (!Active) {
                return;
            }

            var bytes = WrapAndSerialize(message);
            MyAPIGateway.Multiplayer.SendMessageTo(_id, bytes, recipient);
        }

        /// <summary>
        ///     Sends an entity message to given recipient.
        /// </summary>
        /// <param name="message">The entity message send to recipient.</param>
        /// <param name="recipient">The recipient who should receive this message.</param>
        public void Send(IEntityMessage message, ulong recipient) {
            if (!Active) {
                return;
            }

            var bytes = WrapAndSerialize(message);
            MyAPIGateway.Multiplayer.SendMessageTo(_id, bytes, recipient);
        }

        /// <summary>
        ///     Send a message to the server.
        /// </summary>
        /// <param name="message">The message sent to the server.</param>
        public void SendToServer(IMessage message) {
            if (!Active) {
                return;
            }

            var bytes = WrapAndSerialize(message);
            MyAPIGateway.Multiplayer.SendMessageToServer(_id, bytes);
        }

        /// <summary>
        ///     Send an entity message to the server.
        /// </summary>
        /// <param name="message">The entity message sent to the server.</param>
        public void SendToServer(IEntityMessage message) {
            if (!Active) {
                return;
            }

            var bytes = WrapAndSerialize(message);
            MyAPIGateway.Multiplayer.SendMessageToServer(_id, bytes);
        }

        /// <summary>
        ///     Send a message to other network handler.
        /// </summary>
        /// <param name="message">The message sent to others.</param>
        public void Sync(IMessage message) {
            if (!Active) {
                return;
            }

            var bytes = WrapAndSerialize(message);
            MyAPIGateway.Multiplayer.SendMessageToOthers(_id, bytes);
        }

        /// <summary>
        ///     Send an entity message to other network handler.
        /// </summary>
        /// <param name="message">The message sent to others.</param>
        public void Sync(IEntityMessage message) {
            if (!Active) {
                return;
            }

            var bytes = WrapAndSerialize(message);
            MyAPIGateway.Multiplayer.SendMessageToOthers(_id, bytes);
        }

        /// <summary>
        ///     Unregister a message handler for given type.
        /// </summary>
        /// <typeparam name="TMessageType">The type of the message.</typeparam>
        /// <param name="handler">The handler to unregister.</param>
        public void Unregister<TMessageType>(MessageHandler<TMessageType> handler) where TMessageType : IMessage {
            var type = typeof(TMessageType).FullName;

            if (type == null) {
                throw new Exception($"Unable to get type of {typeof(TMessageType)}");
            }

            if (_messageHandler.ContainsKey(type)) {
                var wrapper = MessageHandlerWrapper.Create(handler);
                if (_messageHandler[type].Contains(wrapper)) {
                    _messageHandler[type].Remove(wrapper);
                }
            }
        }

        /// <summary>
        ///     Unregister a message handler for given entity id.
        /// </summary>
        /// <typeparam name="TMessageType">The type of the message.</typeparam>
        /// <param name="entityId">The entity id.</param>
        /// <param name="handler">The handler to unregister.</param>
        public void Unregister<TMessageType>(long entityId, EntityMessageHandler<TMessageType> handler) where TMessageType : IEntityMessage {
            if (_entityMessageHandler.ContainsKey(entityId)) {
                var wrapper = MessageHandlerWrapper.Create(handler);
                if (_entityMessageHandler[entityId].Contains(wrapper)) {
                    _entityMessageHandler[entityId].Remove(wrapper);
                }
            }
        }

        private void OnEntityMessageReceived(ulong sender, EntityMessage entityMessage) {
            var wrapper = entityMessage.Wrapper;

            if (_entityMessageHandler.ContainsKey(wrapper.EntityId)) {
                var handlers = _entityMessageHandler[wrapper.EntityId];
                foreach (var handler in handlers) {
                    var message = handler.Deserialize(wrapper);
                    handler.Invoke(wrapper.Sender, message);
                }
            }
        }

        private void OnMessageReceived(byte[] bytes) {
            var wrapper = MyAPIGateway.Utilities.SerializeFromBinary<MessageWrapper>(bytes);

            if (_messageHandler.ContainsKey(wrapper.Type)) {
                var handlers = _messageHandler[wrapper.Type];
                foreach (var handler in handlers) {
                    var message = handler.Deserialize(wrapper);
                    handler.Invoke(wrapper.Sender, message);
                }
            }
        }

        /// <summary>
        ///     Wrap a message and serialize the wrapped message.
        /// </summary>
        /// <param name="message">The message that get wrapped.</param>
        /// <returns>Returns the serialized wrapper as a byte[].</returns>
        private byte[] WrapAndSerialize(IMessage message) {
            var wrapper = new MessageWrapper(message.GetType().FullName) {
                Sender = MyId,
                Content = message.Serialze()
            };

            return MyAPIGateway.Utilities.SerializeToBinary(wrapper);
        }

        /// <summary>
        ///     Wrap a message and serialize the wrapped message.
        /// </summary>
        /// <param name="message">The message that get wrapped.</param>
        /// <returns>Returns the serialized wrapper as a byte[].</returns>
        private byte[] WrapAndSerialize(IEntityMessage message) {
            var wrapper = new EntityMessageWrapper {
                EntityId = message.EntityId,
                Sender = MyId,
                Content = message.Serialze()
            };

            return WrapAndSerialize(new EntityMessage { Wrapper = wrapper });
        }
    }
}