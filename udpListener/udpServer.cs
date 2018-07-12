using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections;

namespace udpListener
{
    class udpServer:IDisposable
    {
        //private struct Client
        //{
        //    public EndPoint endPoint;
        //    public string name;
        //}

        int port = 9998;

        // Listing of clients
        private ArrayList clientList;

        // Server socket
        private Socket serverSocket=null;

        const int bufSize=2048;
        // Data stream
        private byte[] dataStream = new byte[bufSize];

        public void Dispose()
        {
            if (serverSocket != null)
            {
                serverSocket.Shutdown(SocketShutdown.Both);
                serverSocket.Close();
                serverSocket = null;
            }
        }
        public udpServer()
        {
            try
            {
                // Initialise the ArrayList of connected clients
                this.clientList = new ArrayList();

                // Initialise the delegate which updates the status
                ///this.updateStatusDelegate = new UpdateStatusDelegate(this.UpdateStatus);

                // Initialise the socket
                serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

                serverSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                serverSocket.Blocking = false;// ExclusiveAddressUse = false; // only if you want to send/receive on same machine.

                // Initialise the IPEndPoint for the server and listen on port
                IPEndPoint server = new IPEndPoint(IPAddress.Any, port);

                // Associate the socket with this IP address and port
                serverSocket.Bind(server);

                // Initialise the IPEndPoint for the clients
                IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);

                // Initialise the EndPoint for the clients
                EndPoint epSender = (EndPoint)clients;

                // Start listening for incoming data
                serverSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epSender, new AsyncCallback(ReceiveData), epSender);

                onUpdateHandler(new MyEventArgs("Listening"));
            }
            catch (Exception ex)
            {
                onUpdateHandler(new MyEventArgs("Load Error: " + ex.Message));
            }
        }

        private void ReceiveData(IAsyncResult asyncResult)
        {
            try
            {
                byte[] data;

                // Initialise a packet object to store the received data
                PacketMsg receivedData = new PacketMsg(this.dataStream);

                //// Initialise a packet object to store the data to be sent
                //Packet sendData = new Packet();

                // Initialise the IPEndPoint for the clients
                IPEndPoint clients = new IPEndPoint(IPAddress.Any, 0);

                // Initialise the EndPoint for the clients
                EndPoint epSender = (EndPoint)clients;

                // Receive all data
                serverSocket.EndReceiveFrom(asyncResult, ref epSender);

                // Start populating the packet to be sent
                //sendData.ChatDataIdentifier = receivedData.ChatDataIdentifier;
                //sendData.ChatName = receivedData.ChatName;

                String recvMsg = receivedData.Message;

                //switch (receivedData.ChatDataIdentifier)
                //{
                //    case DataIdentifier.Message:
                //        sendData.ChatMessage = string.Format("{0}: {1}", receivedData.ChatName, receivedData.ChatMessage);
                //        break;

                //    case DataIdentifier.LogIn:
                //        // Populate client object
                //        Client client = new Client();
                //        client.endPoint = epSender;
                //        client.name = receivedData.ChatName;

                //        // Add client to list
                //        this.clientList.Add(client);

                //        sendData.ChatMessage = string.Format("-- {0} is online --", receivedData.ChatName);
                //        break;

                //    case DataIdentifier.LogOut:
                //        // Remove current client from list
                //        foreach (Client c in this.clientList)
                //        {
                //            if (c.endPoint.Equals(epSender))
                //            {
                //                this.clientList.Remove(c);
                //                break;
                //            }
                //        }

                //        sendData.ChatMessage = string.Format("-- {0} has gone offline --", receivedData.ChatName);
                //        break;
                //}

                //// Get packet as byte array
                //data = sendData.GetDataStream();

                //foreach (Client client in this.clientList)
                //{
                //    if (client.endPoint != epSender || sendData.ChatDataIdentifier != DataIdentifier.LogIn)
                //    {
                //        // Broadcast to all logged on users
                //        serverSocket.BeginSendTo(data, 0, data.Length, SocketFlags.None, client.endPoint, new AsyncCallback(this.SendData), client.endPoint);
                //    }
                //}

                // Listen for more connections again...
                dataStream=new byte[bufSize];   //clear the reveive buffer
                serverSocket.BeginReceiveFrom(this.dataStream, 0, this.dataStream.Length, SocketFlags.None, ref epSender, new AsyncCallback(this.ReceiveData), epSender);

                // Update status through a delegate
                onUpdateHandler(new MyEventArgs( receivedData.Message ));
            }
            catch (Exception ex)
            {
                onUpdateHandler(new MyEventArgs( "ReceiveData Error: " + ex.Message));
            }
        }

        public class MyEventArgs : EventArgs
        {
            //fields
            public string msg { get; set; }
            public MyEventArgs(string s)
            {
                msg = "udp listener: " + s;
            }
        }
        public delegate void updateEventHandler(object sender, MyEventArgs eventArgs);
        public event updateEventHandler updateEvent;
        void onUpdateHandler(MyEventArgs args)
        {
            //anyone listening?
            if (this.updateEvent == null)
                return;
            MyEventArgs a = args;
            this.updateEvent(this, a);
            System.Diagnostics.Debug.Write(args.msg);
        }

    }
}
