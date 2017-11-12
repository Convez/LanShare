using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
        FileUploadResponse
    }
    
    [Serializable]
    public class ConnectionMessage
    {
        public MessageType MessageType { get; set; }
        public bool Next { get; set; }
        public object Message { get; set; }

        public ConnectionMessage(MessageType messageType, bool next, object message)
        {
            MessageType = messageType;
            Next = next;
            Message = message;
        }

        public static byte[] Serialize(ConnectionMessage toSerialize)
        {

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
            IFormatter formatter =
                new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            try
            {
                using (MemoryStream ms = new MemoryStream(toDeserialize))
                {
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
