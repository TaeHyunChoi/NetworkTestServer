using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TestGameServer
{
    class PacketClass
    {
        #region Packet Protocol ID
        public enum eServer
        {
            //너무 생각없이 Send로 통일했나.. 흠...
            Send_Connect = 0,
            Send_ClientID,
            Send_RegiUserID,
            Send_ResultRegiUserID,
            Send_RegiUserName,
            Send_ResultRegiUserName,
            Send_RegisterAccount,
            Send_ResultRegisterAccount,
            Send_LoginTry,
            Send_LoginFail,
            Send_LoginSuccess
        }
        #endregion
        #region Packet Struct
        //Network Packet
        [StructLayout(LayoutKind.Sequential)]
        public struct stNetworkPacket
        {
            [MarshalAs(UnmanagedType.U4)]
            public int _id;
            [MarshalAs(UnmanagedType.U4)]
            public int _totalSize;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1016)]
            public byte[] _data;

            //[깨달음} 당연히 얘도 Array.Copy()를 했어야 함...
            public stNetworkPacket(int id, int totalSize, byte[] data)
            {
                _id = id;
                _totalSize = totalSize;
                _data = new byte[1016];
                Array.Copy(data, 0, _data, 0, data.Length);
            }
        }

        //Data Packet
        public struct stSend_Connect
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 200)]
            public byte[] _announce;

            public stSend_Connect(string message)
            {
                _announce = new byte[200];
                byte[] annc = new UTF8Encoding(true, true).GetBytes(message);
                Array.Copy(annc, 0, _announce, 0, annc.Length);
            }
        }
        public struct stSend_ClientID
        {
            [MarshalAs(UnmanagedType.U8)]
            public long _cliID;

            public stSend_ClientID(long id)
            {
                _cliID = id;
            }
        }

        //등록 Register
        public struct stSend_RegiUserID
        {
            [MarshalAs(UnmanagedType.U8)]
            public long _clientID;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] _userID;

            public stSend_RegiUserID(long clientID, string UserID)
            {
                _clientID = clientID;

                _userID = new byte[32];
                byte[] bID = new UTF8Encoding(true, true).GetBytes(UserID);
                Array.Copy(bID, 0, _userID, 0, bID.Length);
            }
        }   //Register > Check ID
        public struct stSend_ResultRegiUserID
        {
            [MarshalAs(UnmanagedType.U8)]
            public long _cliID;

            [MarshalAs(UnmanagedType.Bool)]
            public bool _existed;

            public stSend_ResultRegiUserID(long cliID, bool existed)
            {
                _cliID = cliID;
                _existed = existed;
            }
        }
        public struct stSend_RegiUserName
        {
            [MarshalAs(UnmanagedType.U8)]
            public long _clientID;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] _userName;

            public stSend_RegiUserName(long clientID, string UserID)
            {
                _clientID = clientID;

                _userName = new byte[32];
                byte[] bID = new UTF8Encoding(true, true).GetBytes(UserID);
                Array.Copy(bID, 0, _userName, 0, bID.Length);
            }
        }
        public struct stSend_ResultRegiUserName
        {
            [MarshalAs(UnmanagedType.U8)]
            public long _cliID;

            [MarshalAs(UnmanagedType.Bool)]
            public bool _existed;

            public stSend_ResultRegiUserName(long cliID, bool existed)
            {
                _cliID = cliID;
                _existed = existed;
            }
        }
        public struct stSend_RegisterAccount
        {
            [MarshalAs(UnmanagedType.U8)]
            public long _clientID;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] _userID;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] _userPW;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] _userName;

            //사실 얘는 필요 없을지도..?
            public stSend_RegisterAccount(long clientID, string id, string pw, string name)
            {
                _clientID = clientID;

                _userID = new byte[32];
                _userPW = new byte[32];
                _userName = new byte[32];

                byte[] bID = new UTF8Encoding(true, true).GetBytes(id);
                byte[] bPW = new UTF8Encoding(true, true).GetBytes(pw);
                byte[] bName = new UTF8Encoding(true, true).GetBytes(name);

                Array.Copy(bID, 0, _userID, 0, bID.Length);
                Array.Copy(bPW, 0, _userPW, 0, bPW.Length);
                Array.Copy(bName, 0, _userName, 0, bName.Length);
            }
        }
        public struct stSend_ResultRegisterAccount
        {
            [MarshalAs(UnmanagedType.U8)]
            public long _clientID;

            [MarshalAs(UnmanagedType.Bool)]
            public bool _done;

            public stSend_ResultRegisterAccount(long _cliID, bool done)
            {
                _clientID = _cliID;
                _done = done;
            }
        }

        //로그인 Login
        public struct stSend_LoginTry
        {
            [MarshalAs(UnmanagedType.U8)]
            public long _clientID;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] _userID;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] _userPW;

            public stSend_LoginTry(long clientID, string id, string pw)
            {
                _clientID = clientID;
                _userID = new byte[32];
                _userPW = new byte[32];

                byte[] bID = new UTF8Encoding(true, true).GetBytes(id);
                byte[] bPW = new UTF8Encoding(true, true).GetBytes(pw);

                Array.Copy(bID, 0, _userID, 0, bID.Length);
                Array.Copy(bPW, 0, _userPW, 0, bPW.Length);
            }
        }
        public struct stSend_LoginFail
        {
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1000)]
            public byte[] _announce;

            public stSend_LoginFail(string message)
            {
                _announce = new byte[1000];
                byte[] annc = new UTF8Encoding(true, true).GetBytes(message);
                Array.Copy(annc, 0, _announce, 0, annc.Length);
            }
        }
        public struct stSend_LoginSuccessUserInfo
        {
            //로그인 여부
            [MarshalAs(UnmanagedType.Bool)]
            public bool _loginDone;

            [MarshalAs(UnmanagedType.U8)]
            public long _uuid;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] _id;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 32)]
            public byte[] _name;

            [MarshalAs(UnmanagedType.U4)]
            public int _avatarIndex;

            [MarshalAs(UnmanagedType.U4)]
            public int _clearStageIndex;

            [MarshalAs(UnmanagedType.U4)]
            public int _totalPlayRecord;

            [MarshalAs(UnmanagedType.U4)]
            public int _totalWinCount;

            [MarshalAs(UnmanagedType.U4)]
            public int _totalLoseCount;

            [MarshalAs(UnmanagedType.R4)] //부동 소숫점은 R 이로구나! >>  https://docs.microsoft.com/ko-kr/dotnet/api/system.runtime.interopservices.unmanagedtype?view=net-5.0
            public float _shortestTime;

            //배열로 했으면 반복문 촤라락 돌렸을텐데..
            //Array.Copy를 해봐도 안되네..
            [MarshalAs(UnmanagedType.Struct, SizeConst = 12)]
            public stPut_RecentPlays _recent0;
            public stPut_RecentPlays _recent1;
            public stPut_RecentPlays _recent2;
            public stPut_RecentPlays _recent3;
            public stPut_RecentPlays _recent4;
            public stPut_RecentPlays _recent5;
            public stPut_RecentPlays _recent6;
            public stPut_RecentPlays _recent7;
            public stPut_RecentPlays _recent8;
            public stPut_RecentPlays _recent9;

            public stSend_LoginSuccessUserInfo(bool login, ServerLogData.UserGameInfo info) //ServerLogData.cs에 저장된 유저 정보를 가져오겠다.
            {
                _loginDone = login;

                _uuid = info._uuid;
                _avatarIndex = info._avatarIndex;
                _clearStageIndex = info._clearStageIndex;
                _totalPlayRecord = info._totalPlayRecord;
                _totalWinCount = info._totalWinCount;
                _totalLoseCount = info._totalLoseCount;
                _shortestTime = info._shortestTime;

                _id = new byte[32];
                byte[] bID = new UTF8Encoding(true, true).GetBytes(info._id);
                Array.Copy(bID, 0, _id, 0, bID.Length);

                _name = new byte[32];
                byte[] bName = new UTF8Encoding(true, true).GetBytes(info._name);
                Array.Copy(bName, 0, _name, 0, bName.Length);

                //오늘은 졌지만, 다음엔 꼭 이기리...
                _recent0 = new stPut_RecentPlays(info._recentPlays[0]._result, info._recentPlays[0]._playTime, info._recentPlays[0]._winRate);
                _recent1 = new stPut_RecentPlays(info._recentPlays[1]._result, info._recentPlays[1]._playTime, info._recentPlays[1]._winRate);
                _recent2 = new stPut_RecentPlays(info._recentPlays[2]._result, info._recentPlays[2]._playTime, info._recentPlays[2]._winRate);
                _recent3 = new stPut_RecentPlays(info._recentPlays[3]._result, info._recentPlays[3]._playTime, info._recentPlays[3]._winRate);
                _recent4 = new stPut_RecentPlays(info._recentPlays[4]._result, info._recentPlays[4]._playTime, info._recentPlays[4]._winRate);
                _recent5 = new stPut_RecentPlays(info._recentPlays[5]._result, info._recentPlays[5]._playTime, info._recentPlays[5]._winRate);
                _recent6 = new stPut_RecentPlays(info._recentPlays[6]._result, info._recentPlays[6]._playTime, info._recentPlays[6]._winRate);
                _recent7 = new stPut_RecentPlays(info._recentPlays[7]._result, info._recentPlays[7]._playTime, info._recentPlays[7]._winRate);
                _recent8 = new stPut_RecentPlays(info._recentPlays[8]._result, info._recentPlays[8]._playTime, info._recentPlays[8]._winRate);
                _recent9 = new stPut_RecentPlays(info._recentPlays[9]._result, info._recentPlays[9]._playTime, info._recentPlays[9]._winRate);
            }
        }
        public struct stPut_RecentPlays
        {
            [MarshalAs(UnmanagedType.U4)]
            public int _result;

            [MarshalAs(UnmanagedType.R4)]
            public float _playTime;

            [MarshalAs(UnmanagedType.R4)]
            public float _winRate;

            public stPut_RecentPlays(int result, float time, float rate)
            {
                _result = result;
                _playTime = time;
                _winRate = rate;
            }
        }
        #endregion
        #region Convert Packet Data Type
        public static byte[] SendByteArray(object obj)
        {
            int dataSize = Marshal.SizeOf(obj);
            IntPtr buff = Marshal.AllocHGlobal(dataSize);
            Marshal.StructureToPtr(obj, buff, false);

            byte[] data = new byte[dataSize];
            Marshal.Copy(buff, data, 0, dataSize);
            Marshal.FreeHGlobal(buff);

            return data;
        }
        public static object ReceiveStructure(byte[] data, Type type, int size)
        {
            IntPtr buff = Marshal.AllocHGlobal(data.Length);
            Marshal.Copy(data, 0, buff, data.Length); //복사를 안하면 크기가 빠그라진다~
            object obj = Marshal.PtrToStructure(buff, type);
            Marshal.FreeHGlobal(buff);
            if (Marshal.SizeOf(obj) != size)
            {
                return null;
            }
            return obj;
        }
        #endregion
    }
}

