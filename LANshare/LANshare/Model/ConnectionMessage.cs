using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Diagnostics;

namespace LANshare.Model
{
    [Serializable]
    public enum MessageType
    {
        UserAdvertisement,
        UserDisconnectingNotification,
        IpcBaseFolder,
        IpcElement,
        ProfileImageRequest,
        ProfileImageResponse,
        FileUploadRequest,
        FileUploadResponse,
        NewDirectory,
        EndDirectory,
        NewFile,
        FileChunk,
        TotalUploadSize,
        OperationCanceled
    }
    
    [Serializable]
    public class ConnectionMessage
    {
        [JsonProperty(PropertyName = "PropertyName")]
        public MessageType MessageType { get; set; }
        [JsonProperty(PropertyName ="Next")]
        public bool Next { get; set; }
        [JsonProperty(PropertyName ="Message")]
        public object Message { get; set; }

        public ConnectionMessage() { }
        public ConnectionMessage(MessageType messageType, bool next, object message)
        {
            MessageType = messageType;
            Next = next;
            Message = message;
        }

        public static byte[] Serialize(ConnectionMessage toSerialize)
        {
            return Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(toSerialize));
            IFormatter formatter =
                new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();

            using (MemoryStream ms = new MemoryStream())
            {
                formatter.Serialize(ms, toSerialize);
                return ms.ToArray();
            }
        }

        public static ConnectionMessage Deserialize(byte[] toDeserialize)
        {
            string s = Encoding.UTF8.GetString(toDeserialize);
            Debug.WriteLine(s);
            return JsonConvert.DeserializeObject<ConnectionMessage>(s);
            IFormatter formatter =
                new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            try
            {
                using (MemoryStream ms = new MemoryStream(toDeserialize))
                {
                    ms.Seek(0, SeekOrigin.Begin);
                    ms.Position = 0;
                    return formatter.Deserialize(ms) as ConnectionMessage;
                }
            }
            catch (ArgumentNullException e)
            {
                return null;
            }
        }
    }
}
