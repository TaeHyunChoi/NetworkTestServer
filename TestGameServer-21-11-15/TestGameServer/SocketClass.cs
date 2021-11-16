using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;

namespace TestGameServer
{
    class SocketClass
    {
        public Socket _socket;
        long _serverID = 10000000;
        long _clientID;
        public long ClientID { get { return _clientID; } set { _clientID = value; } }

        public SocketClass()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        
        //연결
        public void InitSocket(short port, int _listener)
        {
            try
            {
                Bind(port);            //소켓 주소와 포트를 바인드
                WaitListen(_listener);  //대기상태로 변경
                Console.WriteLine("[System] 서버가 소켓을 생성했습니다. ({0})", _listener);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.ReadLine();
                return; //만약 연결 안되면 그냥 연결 끊어버리자.
            }
        }
        public void Bind(short port)
        {
            _socket.Bind(new IPEndPoint(IPAddress.Any, port));
        }
        public void WaitListen(int capacity)
        {
            _socket.Listen(capacity);
        }

        //주고 받기
        public int Send(byte[] buffer)
        {
            return _socket.Send(buffer);
        }
        public int Receive(byte[] buffer)
        {
            return _socket.Receive(buffer);
        }

        //닫기
        public void Disconnect()
        {
            _socket.Disconnect(true);
        }
    }
}
