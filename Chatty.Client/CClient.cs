using Chatty.Shared;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Chatty.Client
{
    public class CClient
    {
        public event Delegates.MessageReceived MessageReceived;
        public event Delegates.TextMessageReceived TextMessageReceived;
        public event Delegates.ChatRequestReceived ChatRequestReceived;
        public event Delegates.ChatResponseReceived ChatResponseReceived;
        public event Delegates.ChatDisconnected ChatDisconnected;

        public event Delegates.Connected Connected;
        public event Delegates.Disconnected Disconnected;

        private readonly Thread _recvThread;
        private readonly Thread _sendThread;

        private TcpClient _client;

        private readonly Queue<object> _messages;

        private readonly CState _cState;

        private List<ResponseCallbackObject> _callBacks;

        public CClient()
        {
            _cState = new CState();
            _cState.State = ConnectionStates.Disconnected;
            _messages = new Queue<object>();
            _client = new TcpClient();
            _sendThread = new Thread(SendMethod);
            _recvThread = new Thread(ReceiveMethod);
            _callBacks = new List<ResponseCallbackObject>();
        }

        public void Connect(string ip)
        {
            if(_cState.State == ConnectionStates.Connected) return;
            new Thread(() =>
            {
                try
                {
                    _client.Connect(ip, 8888);
                    _cState.State = ConnectionStates.Connected;
                    _sendThread.Start();
                    _recvThread.Start();
                    OnConnected();
                }
                catch (Exception ex)
                {
                    OnDisconnected();
                }
            }).Start();
        }

        public void QueueMessage(MessageBase message)
        {
            _messages.Enqueue(message);
        }

        public void RequestChat(Action<CClient, ResponseChatMessage> callback)
        {
            RequestChatMessage requestChatMessage = new RequestChatMessage();

            AddCallback(callback, requestChatMessage);

            QueueMessage(requestChatMessage);
        }

        public void SendTextMessage(string message)
        {
            TextMessage textMessage = new TextMessage();
            textMessage.Message = message;
            QueueMessage(textMessage);
        }

        private void AddCallback(Delegate callback, MessageBase message)
        {
            if(callback != null)
            {
                Guid callbackID = Guid.NewGuid();
                ResponseCallbackObject responseCallback = new ResponseCallbackObject()
                {
                    Id = callbackID,
                    Callback = callback
                };

                message.Id = callbackID;
                _callBacks.Add(responseCallback);
            }
        }

        private void ReceiveMethod()
        {
            while (_cState.State == ConnectionStates.Connected)
            {
                try
                {
                    JObject msg = MessageSerializer.Deserialize(_client.GetStream(),
                        _cState);
                    string type = msg["_type"].ToString();
                    switch (type)
                    {
                        case "RequestChatMessage":
                            RequestChatMessage rcm = JsonConvert.DeserializeObject<RequestChatMessage>(msg.ToString());
                            OnChatRequest((b) =>
                            {
                                ResponseChatMessage responseChatMessage = new ResponseChatMessage();
                                responseChatMessage.Id = rcm.Id;
                                responseChatMessage.Accepted = b;
                                QueueMessage(responseChatMessage);
                            }, rcm.ClientGuid);
                            break;
                        case "ResponseChatMessage":
                            ResponseChatMessage rm = JsonConvert.DeserializeObject<ResponseChatMessage>(msg.ToString());
                            InvokeMessageCallback(rm);
                            OnChatResponseReceived(rm.Accepted, rm.ClientGuid);
                            break;
                        case "TextMessage":
                            TextMessage tm = JsonConvert.DeserializeObject<TextMessage>(msg.ToString());
                            OnTextMessageReceived(tm);
                            break;
                        case "ChatStatusMessage":
                            OnChatDisconnected();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Recv" + ex.Message);
                }
            }
            OnDisconnected();
        }

        private void SendMethod()
        {
            while(_cState.State == ConnectionStates.Connected)
            {
                if (_messages.Count > 0)
                {
                    try
                    {
                        MessageSerializer.Serialize(
                            _client.GetStream(),
                            _messages.Dequeue());
                        Debug.WriteLine($"Remaining msgs - {_messages.Count}");
                    }catch(Exception ex)
                    {
                        Debug.WriteLine(ex.Message);
                    }
                }
                Thread.Sleep(50);
            }
        }

        private void InvokeMessageCallback(MessageBase message)
        {
            var callbackObject = _callBacks.SingleOrDefault(x => x.Id == message.Id);

            if (callbackObject != null)
            {
                callbackObject.Callback.DynamicInvoke(this, message);

                _callBacks.Remove(callbackObject);
            }
        }

        #region Virtuals
        public virtual void OnTextMessageReceived(TextMessage textMessage)
        {
            TextMessageReceived?.Invoke(textMessage);
        }

        public virtual void OnChatResponseReceived(bool status, Guid refGuid)
        {
            ChatResponseReceived?.Invoke(status, refGuid);
        }

        public virtual void OnChatRequest(Action<bool> callback, Guid refGuid)
        {
            ChatRequestReceived?.Invoke(callback, refGuid);
        }

        public virtual void OnMessageReceived(TextMessage messageText)
        {
            MessageReceived?.Invoke(messageText);
        }

        public virtual void OnConnected()
        {
            Connected?.Invoke();
        }

        public virtual void OnDisconnected()
        {
            _cState.State = ConnectionStates.Disconnected;
            Disconnected?.Invoke();
        }

        public virtual void OnChatDisconnected()
        {
            ChatDisconnected?.Invoke();
        }
        #endregion
    }
}
