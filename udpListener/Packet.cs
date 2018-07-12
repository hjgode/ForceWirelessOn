using System;
using System.Collections.Generic;
using System.Text;

namespace udpListener
{
    // ----------------
    // Packet Structure
    // ----------------

    // Description   -> |dataIdentifier|name length|message length|    name   |    message   |
    // Size in bytes -> |       4      |     4     |       4      |name length|message length|

    public enum DataIdentifier
    {
        Message,
        LogIn,
        LogOut,
        Null    
    }

    public class PacketMsg
    {
        private string message;
        public string Message
        {
            get { return message; }
            set { message = value; }
        }
        public PacketMsg()
        {
            this.message = null;
        }
        public PacketMsg(byte[] dataStream)
        {
            // 
            int iLen = 0;
            for (int i = 0; i < dataStream.Length; i++)
            {
                if (dataStream[i] == 0)
                {
                    iLen = i;
                    continue;
                }
            }
            System.Diagnostics.Debug.WriteLine("rcvd: '" + Encoding.UTF8.GetString(dataStream, 0, iLen));
            this.message= Encoding.UTF8.GetString(dataStream,0,iLen);
        }
    }
    public class Packet
    {
        #region Private Members
        private DataIdentifier dataIdentifier; 
        private string name; 
        private string message;
        #endregion

        #region Public Properties
        public DataIdentifier ChatDataIdentifier
        {
            get { return dataIdentifier; }
            set { dataIdentifier = value; }
        }

        public string ChatName
        {
            get { return name; }
            set { name = value; }
        }

        public string ChatMessage
        {
            get { return message; }
            set { message = value; }
        }
        #endregion

        #region Methods

        // Default Constructor
        public Packet()
        {
            this.dataIdentifier = DataIdentifier.Null;
            this.message = null;
            this.name = null;
        }

        public Packet(byte[] dataStream)
        {
            // Read the data identifier from the beginning of the stream (4 bytes)
            this.dataIdentifier = (DataIdentifier)BitConverter.ToInt32(dataStream, 0);

            // Read the length of the name (4 bytes)
            int nameLength = BitConverter.ToInt32(dataStream, 4);

            // Read the length of the message (4 bytes)
            int msgLength = BitConverter.ToInt32(dataStream, 8);

            // Read the name field
            if (nameLength > 0)
                this.name = Encoding.UTF8.GetString(dataStream, 12, nameLength);
            else
                this.name = null;

            // Read the message field
            if (msgLength > 0)
                this.message = Encoding.UTF8.GetString(dataStream, 12 + nameLength, msgLength);
            else
                this.message = null;
        }

        // Converts the packet into a byte array for sending/receiving 
        public byte[] GetDataStream()
        {
            List<byte> dataStream = new List<byte>();

            // Add the dataIdentifier
            dataStream.AddRange(BitConverter.GetBytes((int)this.dataIdentifier));

            // Add the name length
            if (this.name != null)
                dataStream.AddRange(BitConverter.GetBytes(this.name.Length));
            else
                dataStream.AddRange(BitConverter.GetBytes(0));

            // Add the message length
            if (this.message != null)
                dataStream.AddRange(BitConverter.GetBytes(this.message.Length));
            else
                dataStream.AddRange(BitConverter.GetBytes(0));

            // Add the name
            if (this.name != null)
                dataStream.AddRange(Encoding.UTF8.GetBytes(this.name));

            // Add the message
            if (this.message != null)
                dataStream.AddRange(Encoding.UTF8.GetBytes(this.message));

            return dataStream.ToArray();
        }

        #endregion
    }
}
