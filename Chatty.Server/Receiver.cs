using Chatty.Shared;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Timers;
using System.Xml.Serialization;

namespace Chatty.Server
{
    public class Receiver
    {
        public event Delegates.MessageReceived MessageReceived;
        public event Delegates.ClientBasicDelegate Disconnected;

        private readonly Thread _recvThread;
        private readonly Thread _sendThread;

        private readonly Guid _guid;

        private Guid _otherClientGuid;

        private readonly TcpServer _server;
        private readonly TcpClient _client;

        private readonly Queue<object> _messages;

        private readonly CState _cState;

        private readonly System.Timers.Timer _timer;
        private long _cTime = 0;

        public Receiver() 
        {
            _cState = new CState();
            _guid = Guid.NewGuid();
            _recvThread = new Thread(ReceiveMethod);
            _sendThread = new Thread(SendMethod);
            _messages = new Queue<object>();

            _timer = new System.Timers.Timer(2000);
            _timer.Elapsed += _timer_Elapsed;
        }

        private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            //If there are no text messages for a period of 60s
            //we consider it as a timeout
            if (Utils.GetUnixTimeSeconds() - _cTime > 60)
            {
                Console.WriteLine("Chat timeout");
                //We empty the _otherClientGuid so other users can request for chat
                var client = GetClientByGuid(_otherClientGuid);
                client?.StopTimer();
                _timer.Stop();
                
                _otherClientGuid = Guid.Empty;
                //Let's send out chat disconnect message to both clients
                ChatDisconnect(client, this);
            }
        }

        public void StopTimer()
        {
            _timer.Stop();
        }

        public Receiver(TcpClient tcpClient, TcpServer server) : this()
        {
            _client = tcpClient;
            _server = server;
            _cState.State = ConnectionStates.Connected;

            _sendThread.Start();
            _recvThread.Start();
        }

        private void ReceiveMethod()
        {
            while(_cState.State == ConnectionStates.Connected)
            {
                try
                {
                    JObject? msg = MessageSerializer.Deserialize(_client.GetStream(),
                        _cState);
                    if (msg == null) break;
                    string type = msg["_type"].ToString();
                    Console.WriteLine($"Message received = {msg["_type"]}");
                    Console.WriteLine($"Message received = {msg}");
                    switch (type)
                    {
                        case "RequestChatMessage":
                            RequestChatMessage rm = JsonConvert.DeserializeObject<RequestChatMessage>(msg.ToString());
                            rm.ClientGuid = GetGuid();
                            Console.WriteLine($"Requester: {GetGuid()}");
                            HandleChatRequest((recv, args) =>
                            {
                                if (args.Accepted)
                                {
                                    recv._otherClientGuid = GetGuid();
                                    _cTime = Utils.GetUnixTimeSeconds();
                                    _timer.Start();
                                }
                                else
                                {
                                    //If the chat request was denied we will assign empty guid
                                    recv._otherClientGuid = Guid.Empty;
                                    _otherClientGuid = Guid.Empty;
                                }
                                Console.WriteLine($"Callback - My ID = {recv.GetGuid()} Other id = {recv._otherClientGuid}");
                                QueueMessage(args);
                            }, rm);
                            Console.WriteLine($"My ID = {GetGuid()} Other id = {_otherClientGuid}");
                            break;
                        case "ResponseChatMessage":
                            ResponseChatMessage responseChatMessage = JsonConvert.DeserializeObject<ResponseChatMessage>(msg.ToString());
                            InvokeMessageCallback(responseChatMessage);
                            break;
                        case "TextMessage":
                            TextMessage textMessage = JsonConvert.DeserializeObject<TextMessage>(msg.ToString());

                            //Check if receiver available
                            if (ClientExists(_otherClientGuid))
                            {
                                _cTime = Utils.GetUnixTimeSeconds();
                                GetClientByGuid(_otherClientGuid)?.QueueMessage(textMessage);
                            }
                            break;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.ToString());   
                }
                Thread.Sleep(50);
            }
            if (_cState.State == ConnectionStates.Disconnected) OnClientDisconnected(this);
        }

        private void SendMethod()
        {
            while (_cState.State == ConnectionStates.Connected)
            {
                try
                {
                    if (_messages.Count > 0)
                    {
                        MessageSerializer.Serialize(
                            _client.GetStream(), 
                            _messages.Dequeue());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
        }

        public void QueueMessage(MessageBase message)
        {
            _messages.Enqueue(message);
        }

        public Guid GetGuid()
        {
            return _guid;
        }

        /// <summary>
        /// Returns the Receiver by Guid
        /// </summary>
        /// <param name="refGuid"></param>
        /// <returns></returns>
        private Receiver GetClientByGuid(Guid refGuid)
        {
            try
            {
                return _server.receivers.Where(x => x.GetGuid() == refGuid).First();
            }catch(Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Check availability of receiver by Guid
        /// </summary>
        /// <param name="refGuid"></param>
        /// <returns></returns>
        private bool ClientExists(Guid refGuid)
        {
            return _server.receivers.Find(x => x.GetGuid() == refGuid) != null;
        }

        /// <summary>
        /// This function select a random client and send the request to it.
        /// We need to first check if a certain receiver contains _otherClientGuid and we will ignore them.
        /// 
        /// </summary>
        /// <param name="callback"></param>
        /// <param name="requestChatMessage"></param>
        private void HandleChatRequest(Action<Receiver, ResponseChatMessage> callback, RequestChatMessage requestChatMessage)
        {
            //Get the number of available clients
            var allRecvCount = _server.receivers.Count;
            Console.WriteLine($"Recv allRecvCount = {allRecvCount}");

            //First check and filter the receivers that already
            //selected. Simply, we'll select _otherClientGuid zero
            var lot = _server.receivers.Where(x => x._otherClientGuid == Guid.Empty);

            if (allRecvCount > 1)
            {
                //Get this receivers index
                var cIdx = _server.receivers.FindIndex(x => x.GetGuid() == GetGuid());
                //Get receivers without this receiver
                var withoutMe = _server.receivers.Where(x => x.GetGuid() != GetGuid());
                //Get receivers with empty Guid which means they are not engaged in chat
                var allChattys = withoutMe.Where(x => x._otherClientGuid == Guid.Empty);
                var chattyCount = allChattys.Count();   
                if (chattyCount == 0) return;

                var range = Enumerable.Range(0, chattyCount).Where(i => i != cIdx);

                //Generate a random number
                Random rnd = new Random();
                var rIdx = rnd.Next(0, chattyCount);
                //Get random client
                var randomChatty = allChattys.ElementAt(rIdx);
                _otherClientGuid = randomChatty.GetGuid();
                Console.WriteLine($"OtherClient = {_otherClientGuid}");
                //Add callback to handle when we receive from the random client
                AddCallback(callback, requestChatMessage);
                //Send requestchatmessage to the random client
                randomChatty.QueueMessage(requestChatMessage);
            }
            else
            {
                Console.WriteLine($"Sorry, there's only {allRecvCount} client(s) available");
                ResponseChatMessage responseChatMessage = new ResponseChatMessage();
                responseChatMessage.Id = requestChatMessage.Id;
                responseChatMessage.Accepted = false;
                responseChatMessage.NumOfClients = allRecvCount;

                QueueMessage(responseChatMessage);
            }
        }

        private void ChatDisconnect(Receiver? r1, Receiver? r2)
        {
            r1?.QueueMessage(new ChatStatusMessage());
            r2?.QueueMessage(new ChatStatusMessage());
        }

        private void AddCallback(Delegate callback, MessageBase message)
        {
            if (callback != null)
            {
                Guid callbackID = Guid.NewGuid();
                ResponseCallbackObject responseCallback = new ResponseCallbackObject()
                {
                    Id = callbackID,
                    Callback = callback
                };

                message.Id = callbackID;
                _server._callBacks.Add(responseCallback);
            }
        }

        private void InvokeMessageCallback(MessageBase message)
        {
            var callbackObject = _server._callBacks.SingleOrDefault(x => x.Id == message.Id);

            if (callbackObject != null)
            {
                callbackObject.Callback.DynamicInvoke(this, message);

                _server._callBacks.Remove(callbackObject);
            }
        }

        public virtual void OnMessageReceived(object message)
        {
            MessageReceived?.Invoke(message);
        }

        public virtual void OnClientDisconnected(Receiver receiver) 
        {
            _server.receivers.Remove(receiver);
            Disconnected?.Invoke(receiver);
        }
    }
}
