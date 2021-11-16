using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace TestGameServer
{
    class TCPServer
    {
        short _port;
        int _listener = 50;
        public static long _nowUUID = 10001000; //얘는 User ID이다. (!= Client ID)

        static Queue<byte[]> _sendQueue = new Queue<byte[]>();
        static Queue<PacketClass.stNetworkPacket> _ReceiveQueue = new Queue<PacketClass.stNetworkPacket>();

        public static Queue<byte[]> SendPacketQueue { get { return _sendQueue; } set { _sendQueue = value; } }
        public static Queue<PacketClass.stNetworkPacket> ReceivePacketQueue { get { return _ReceiveQueue; } set { _ReceiveQueue = value; } }

        SocketClass _waitCLI;
        SocketManager _connectedCLI;

        Thread _sendThread;
        Thread _receiveThread;

        ServerLogData _logServer;

        public TCPServer(short port) //소켓, 쓰레드 생성
        {
            _port = port;

            //소켓 관련 클래스
            _waitCLI = new SocketClass();           //서버쪽 wait 소켓 만들고
            _waitCLI.InitSocket(_port, _listener);         //Bind() + Listen()
            _connectedCLI = new SocketManager();    //서버+클라 소켓 모두 제어

            //쓰레드
            _sendThread = new Thread(SendQueueProcess);
            _receiveThread = new Thread(() => ReceiveQueueProcess()); //얘가 람다식?
            _sendThread.Start();
            _receiveThread.Start();

            //로그
            _logServer = new ServerLogData();
        }
        ~TCPServer()  //서버 종료시 해지
        {
            //얘만 있으면 되나?
            //[Comment] waitSocket뿐만 아니라 SocketManager, Server 단에서도 정리할게 뭐 많을 것이다.
            _waitCLI.Disconnect();
        }

        public bool MainProcess()
        {
            //클라~서버 최초 연결
            if (_connectedCLI.AcceptClient(_waitCLI) != null)   //Accept한 소켓이 있다면
            {
                //두 개를 하나로 합치면 더 좋을 듯? 나중에 ㄱㄱ
                SendConnectAlarm(_connectedCLI.AcceptedSocket);     //Connect가 되었을 때에 알람을 클라이언트에게 보낸다.
                SendClientID(_connectedCLI.AcceptedSocketID);       //클라이언트에게 클라이언트ID를 넘긴다.
                Console.WriteLine($"[System/임시] 클라이언트와 연결하였습니다."); //확인용.
            }

            //이미 연결되었다면
            _connectedCLI.PollingReceiveClient();

            //서버 메인 프로세스를 종료한다면
            if (ExitProcess())
            {
                return false; //메인 프로세스 종료.
            }

            return true;
        }

        //Send to Client
        private byte[] MakeNetPacket(byte[] buff, int id)
        {
            int totalSize = Marshal.SizeOf<PacketClass.stNetworkPacket>();
            PacketClass.stNetworkPacket packet = new PacketClass.stNetworkPacket(id, totalSize, buff);
            return PacketClass.SendByteArray(packet);
        }
        public void SendConnectAlarm(Socket client)
        {
            string message = "[System] 서버에 연결하였습니다.";

            PacketClass.stSend_Connect pConnect = new PacketClass.stSend_Connect(message);
            byte[] buff = PacketClass.SendByteArray(pConnect);

            int id = (int)PacketClass.eServer.Send_Connect; //그냥 넣어도 되지만, 함수가 길어져서 그냥 변수 한 번 더 만들었음.
            byte[] sendBuff = MakeNetPacket(buff, id);

            client.Send(sendBuff);
        }
        public void SendClientID(long clientID)
        {
            PacketClass.stSend_ClientID pClientID = new PacketClass.stSend_ClientID(clientID);
            byte[] buff = PacketClass.SendByteArray(pClientID);

            int id = (int)PacketClass.eServer.Send_ClientID;
            byte[] sendBuff = MakeNetPacket(buff, id);

            Socket client = SocketManager.FindClient(clientID);
            client.Send(sendBuff);
        }
        private void SendIDExisted(long clientID, bool existed)
        {
            PacketClass.stSend_ResultRegiUserID pIDResult = new PacketClass.stSend_ResultRegiUserID(clientID, existed);
            byte[] buff = PacketClass.SendByteArray(pIDResult);

            int id = (int)PacketClass.eServer.Send_ResultRegiUserID;
            byte[] sendBuff = MakeNetPacket(buff, id);
            Socket client = SocketManager.FindClient(clientID);

            client.Send(sendBuff);  //하나만 보낼 때는 Send, 여러 상대에게 보낼 때는 SendQueue를 쓰도록 해보자
        }
        private void SendNameExisted(long clientID, bool existed)
        {
            PacketClass.stSend_ResultRegiUserName pNameResult = new PacketClass.stSend_ResultRegiUserName(clientID, existed);
            byte[] buff = PacketClass.SendByteArray(pNameResult);

            int id = (int)PacketClass.eServer.Send_ResultRegiUserName;
            byte[] sendBuff = MakeNetPacket(buff, id);
            Socket client = SocketManager.FindClient(clientID);

            client.Send(sendBuff);  //하나만 보낼 때는 Send, 여러 상대에게 보낼 때는 SendQueue를 쓰도록 해보자
        }
        private void SendRegisterDone(long clientID, bool done)
        {
            PacketClass.stSend_ResultRegisterAccount pNameResult = new PacketClass.stSend_ResultRegisterAccount(clientID, done);
            byte[] buff = PacketClass.SendByteArray(pNameResult);

            int id = (int)PacketClass.eServer.Send_ResultRegisterAccount;
            byte[] sendBuff = MakeNetPacket(buff, id);
            Socket client = SocketManager.FindClient(clientID);

            client.Send(sendBuff);  //하나만 보낼 때는 Send, 여러 상대에게 보낼 때는 SendQueue를 쓰도록 해보자
        }
        private void SendResultLogin(long clientID, bool done, ServerLogData.UserGameInfo info)
        {
            if (done)
            {
                PacketClass.stSend_LoginSuccessUserInfo pLoginSuccess = new PacketClass.stSend_LoginSuccessUserInfo(done, info);
                byte[] buff = PacketClass.SendByteArray(pLoginSuccess);
                int id = (int)PacketClass.eServer.Send_LoginSuccess;
                byte[] sendBuff = MakeNetPacket(buff, id);
                Socket client = SocketManager.FindClient(clientID);
                client.Send(sendBuff);  //하나만 보낼 때는 Send, 여러 상대에게 보낼 때는 SendQueue를 쓰도록 해보자
            }
            else
            {
                //그냥 loginFail을 하나 더 추가해야겠군...
                PacketClass.stSend_LoginFail pLoginFail = new PacketClass.stSend_LoginFail("아이디가 없거나 비밀번호가 잘못되었습니다.");
                byte[] buff = PacketClass.SendByteArray(pLoginFail);
                int id = (int)PacketClass.eServer.Send_LoginFail;
                byte[] sendBuff = MakeNetPacket(buff, id);
                Socket client = SocketManager.FindClient(clientID);
                client.Send(sendBuff);  //하나만 보낼 때는 Send, 여러 상대에게 보낼 때는 SendQueue를 쓰도록 해보자
            }
        }

        //쓰레드
        private void ReceiveQueueProcess()
        {
            while (true)
            {
                if (_ReceiveQueue.Count > 0)
                {
                    //패킷 꺼내고...
                    PacketClass.stNetworkPacket packet = _ReceiveQueue.Dequeue();

                    switch (packet._id)
                    {
                        case (int)PacketClass.eServer.Send_RegiUserID: //등록 > ID 중복 확인
                            {
                                PacketClass.stSend_RegiUserID pRegiIDCheck = (PacketClass.stSend_RegiUserID)PacketClass.ReceiveStructure(packet._data, typeof(PacketClass.stSend_RegiUserID), Marshal.SizeOf<PacketClass.stSend_RegiUserID>());
                                long checkIDCLI = pRegiIDCheck._clientID;
                                string regiCheckID = Encoding.UTF8.GetString(pRegiIDCheck._userID);
                                bool IDExisted = _logServer.CheckIDExisted(regiCheckID); //ID 중복여부 확인 후 bool값 반환 => bool값을 클라에게 보낸다.
                                SendIDExisted(checkIDCLI, IDExisted); //패킷 보낼 떄 클라 id 같이 받아와야 한다;
                                Console.WriteLine($"[{checkIDCLI}] 신규 가입 > ID 중복 여부 확인 요청 건");
                                break;
                            }
                        case (int)PacketClass.eServer.Send_RegiUserName: //등록 > Name 중복 확인
                            {
                                PacketClass.stSend_RegiUserName pRegiName = (PacketClass.stSend_RegiUserName)PacketClass.ReceiveStructure(packet._data, typeof(PacketClass.stSend_RegiUserName), Marshal.SizeOf<PacketClass.stSend_RegiUserName>());
                                long checkNameCLI = pRegiName._clientID;
                                string regiCheckName = Encoding.UTF8.GetString(pRegiName._userName);
                                bool NameExisted = _logServer.CheckNameExisted(regiCheckName);
                                SendNameExisted(checkNameCLI, NameExisted);
                                Console.WriteLine($"[{checkNameCLI}] 신규 가입 > Name 중복 여부 확인 반환 건");
                            }
                            break;
                        case (int)PacketClass.eServer.Send_RegisterAccount:
                            {
                                PacketClass.stSend_RegisterAccount pNewAccount = (PacketClass.stSend_RegisterAccount)PacketClass.ReceiveStructure(packet._data, typeof(PacketClass.stSend_RegisterAccount), Marshal.SizeOf<PacketClass.stSend_RegisterAccount>());
                                long regiCLI = pNewAccount._clientID;
                                string regiID = Encoding.UTF8.GetString(pNewAccount._userID);
                                string regiPW = Encoding.UTF8.GetString(pNewAccount._userPW);
                                string regiName = Encoding.UTF8.GetString(pNewAccount._userName);

                                //신규 가입이니까.. 전체 유저 정보로 추가해야 하는군...
                                ServerLogData.UserGameInfo userGameInfo = new ServerLogData.UserGameInfo(regiID, regiPW, regiName);
                                _logServer.UserData.Add(userGameInfo);

                                //계정 등록이 완료되었다고 클라에게 알린다. //만약 등록과정에서 뻑나면 여기까지 못오겠쥐..? //다른 안전장치가 있을 것 같기도 하고?
                                SendRegisterDone(regiCLI, true);
                                Console.WriteLine($"[{regiCLI}] 신규 가입 > 계정 등록 여부 반환 건");
                            }
                            break;
                        case (int)PacketClass.eServer.Send_LoginTry:
                            PacketClass.stSend_LoginTry pLogin = (PacketClass.stSend_LoginTry)PacketClass.ReceiveStructure(packet._data, typeof(PacketClass.stSend_LoginTry), Marshal.SizeOf<PacketClass.stSend_LoginTry>());
                            long loginCLI = pLogin._clientID;
                            string loginID = Encoding.UTF8.GetString(pLogin._userID);
                            string loginPW = Encoding.UTF8.GetString(pLogin._userPW);

                            if (_logServer.CheckLoginAccount(loginID, loginPW) != -1)
                            {
                                //계정 정보가 있다면 + 저장된 유저 정보를 보낸다.
                                int index = _logServer.CheckLoginAccount(loginID, loginPW);
                                ServerLogData.UserGameInfo info =  _logServer.UserData[index];
                                SendResultLogin(loginCLI, true, info);

                                //현재 접속 유저 리스트에 추가한다.
                                _logServer.NowConnectUSer.Add(loginCLI, info);
                            }
                            else
                            {
                                //계정 정보가 없다면 : 없다면 빈 userinfo만들어서 채운다?
                                ServerLogData.UserGameInfo info = new ServerLogData.UserGameInfo();
                                SendResultLogin(loginCLI, false, info);
                            }
                            //클라가 이걸 받으면 + bool값을 꺼내서 로그인 여부를 판단한다. 판단 가능하겠군.
                            Console.WriteLine($"[{loginCLI}] 로그인 > 계정 정보 확인 요청 건");
                            break;
                    }
                }
            }
        }
        private void SendQueueProcess()
        {
            while (true)
            {
                if (_sendQueue.Count > 0)
                {
                    //Connect 외에는 그냥 Queue에 박는게 좋지 않나?
                    //그런데 특정 클라이언트에게만 보내는건데 흠...
                    //그러면 예전처럼 dic<cliID, socket> 형식이 된다
                    //일단 보류.
                }
            }
        }

        //종료
        private bool ExitProcess()
        {
            //무슨 입력/조작/시점이 필요하려나...?
            return false;
        }
    }
}
