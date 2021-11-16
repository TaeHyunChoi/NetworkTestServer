using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestGameServer
{
    class Program
    {
        static void Main(string[] args)
        {
            TCPServer _server = new TCPServer(80);  //port번호 임의 배정(80)
                                                    //TCP 객체화할 때에 쓰레드까지 다 돌림
            //Main에서 메인 프로세스 가동
            while (true)
            {
                //소켓이 닫혔을 때의 프로세스 : 뭘 해야 할까? 일단 보류

                //소켓이 연결된 다음에
                if (!_server.MainProcess())
                {
                    //메인 프로세스를 진행하지 않을거라면
                    break;
                }
            }
            //서버 해지에 필요한 명령

            Console.Read();
        }
    }
}
