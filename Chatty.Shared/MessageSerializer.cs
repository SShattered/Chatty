using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Chatty.Shared
{
    public static class MessageSerializer
    {
        public static void Serialize(Stream stream, object message)
        {
            var ms = new MemoryStream();
            var sw = new StreamWriter(ms);
            var jw = new JsonTextWriter(sw);

            JsonSerializer jsonSerializer = new();
            jsonSerializer.Serialize(jw, message);
            jw.Flush();
            sw.Flush();
            ms.Seek(0, SeekOrigin.Begin);

            Debug.WriteLine($"Size={ms.Length}");
            byte[] size = {
                        (byte)(ms.Length & 0xFF),
                        (byte)((ms.Length >> 8) & 0xFF)
                    };
            stream.Write(size);
            stream.Write(ms.ToArray()); 

            /*using (MemoryStream ms = new())
            {
                using(StreamWriter sw = new(ms))
                {
                    jsonSerializer.Serialize(sw, message);
                    //Send data size first
                    Debug.WriteLine($"Size={ms.Length}");
                    byte[] size = {
                        (byte)(ms.Length & 0xFF),
                        (byte)((ms.Length >> 8) & 0xFF)
                    };
                    stream.Write(
                        size
                        );
                    //Send data
                    stream.Write(ms.ToArray());
                }
            }*/
        }

        public static JObject Deserialize(Stream stream, CState cState)
        {
            //First read 2bytes for message length
            byte[] size = new byte[2];
            byte counter = 0;
            //Timeout is set to 1sec
            //We don't want to block all the time
            //We need to cancel this whole process if cState changes
            stream.ReadTimeout = 1000;
            while (cState.State == ConnectionStates.Connected)
            {
                try
                {
                    int b = stream.ReadByte(); //Read a single byte
                    if (b == -1) { cState.State = ConnectionStates.Disconnected; return default; }
                    size[counter++] = (byte)b;
                    if (counter == 2) break;
                }
                catch (IOException e) { }
                catch (SocketException e) { }
                catch (Exception e)
                {
                    //Debug.WriteLine(e.Message);
                }
            }
            //Do bit-shifting
            //Message length
            int length = (size[1] << 8) & 0xFF00 |
                            (size[0] & 0x00FF);
            counter = 0;
            byte[] dataBytes = new byte[length];
            while (cState.State == ConnectionStates.Connected)
            {
                try
                {
                    int b = stream.ReadByte(); //Read a single byte
                    if (b == -1) { cState.State = ConnectionStates.Disconnected; return default; }
                    dataBytes[counter++] = (byte)b;
                    if (counter == length) break;
                }
                catch (IOException e) { }
                catch (SocketException e) { }
                catch (Exception e)
                {
                    //Log ex
                }
            }
            var jObj = JObject.Parse(ASCIIEncoding.UTF8.GetString(dataBytes));
            //Console.WriteLine(jObj);
            //Console.WriteLine($"Type = {jObj["_type"]}");
            return jObj;
        }
    }
}
