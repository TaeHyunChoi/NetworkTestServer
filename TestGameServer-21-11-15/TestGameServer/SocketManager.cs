using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace TestGameServer
{
    class SocketManager
    {
        List<Socket> _ltClientSocket = new List<Socket>();
        static Dictionary<long, Socket> _dicClients = new Dictionary<long, Socket>();

        public Socket AcceptedSocket {get; set;}
        public long AcceptedSocketID { get; set; }
        long _lastClientID = 20000000;

        //Accept
        public Socket AcceptClient(SocketClass waitSocket)    //Accept. TCP Server에서 계속 while 돌린다.
        {
            if (waitSocket != null && waitSocket._socket.Poll(0, SelectMode.SelectRead))
            {
                Socket client = AcceptedSocket = waitSocket._socket.Accept();    //Accept를 하여 소켓이 연결되면 + 알람 메세지를 TCP서버에서 보내겠다고 굳이 AcceptSocket도 만들었다 ㅎㅎ!
                waitSocket.ClientID = AcceptedSocketID = _lastClientID;
                _dicClients.Add(_lastClientID++, client);                  //연결한 클라이언트 목록에 추가
                return client;
            }
            return null;
        }

        //[받기]소켓 정보 받으면 → Receive Queue에 보내기
        public void PollingReceiveClient()
        {
            foreach (Socket client in _dicClients.Values) //이게 안 들어오네..?
            {
                if (client.Poll(0, SelectMode.SelectRead))
                {
                    byte[] buffer = new byte[1024];
                    int buffLength = client.Receive(buffer);

                    if (buffLength > 0)
                    {
                        PacketClass.stNetworkPacket packet = (PacketClass.stNetworkPacket)PacketClass.ReceiveStructure(buffer, typeof(PacketClass.stNetworkPacket), Marshal.SizeOf<PacketClass.stNetworkPacket>());
                        TCPServer.ReceivePacketQueue.Enqueue(packet);
                    }
                }
            }
        }

        //[보내기] 아직... 필요한 단계가 오면 그 때 만들자
        public void SendToClient()
        { 
            
        }

        //찾기
        public static Socket FindClient(long clientID)   //클라이언트 찾기
        {
            //당장 안쓰니까 일단 냅두자~
            return _dicClients[clientID];
        }

        //닫기
        //???
    }
}
