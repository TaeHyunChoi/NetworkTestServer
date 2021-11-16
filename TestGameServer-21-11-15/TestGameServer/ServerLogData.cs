using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace TestGameServer
{
    class ServerLogData
    {
        public enum GameResult
        {
            Lose = 0,
            Draw,
            Win
        }
        public struct UserGameInfo //유저 개인별 정보 형식(구조체)
        {
            //유저정보
            public string _id;                  //고유 아이디 (id)
            public string _pw;                  //비밀번호 (pw)
            public long _uuid;                  //고유 인덱스 (uuid)
            public string _name;                //닉네임 (name)
            public int _avatarIndex;            //아바타 인덱스 (avatar image index)

            //게임 정보
            public int _clearStageIndex;        //1인 대전 : 클리어 스테이지 번호
            public float _shortestTime;         //클리어 최단 시간
            public int _totalPlayRecord;        //전체 대전 게임 횟수
            public int _totalWinCount;          //전체 승, 패 전적
            public int _totalLoseCount;         //최근 10인 게임 전적정보 

            public RecentPlays[] _recentPlays;  //승패승승승 : 이런 식으로 최근 10회 게임 전적만 내면 되나? Array라서 탈탈 털리나 흠...

            public UserGameInfo(string id, string pw, string name)
            {
                //최초 생성 시에는 Default값으로 넣는다.
                _uuid = TCPServer._nowUUID++;
                Console.WriteLine("UUID : " + _uuid);
                _avatarIndex = 0;
                _clearStageIndex = _totalPlayRecord = _totalWinCount = _totalLoseCount = 0;
                _shortestTime = 0;

                //최초 생성시 : id, pw, name 입력
                _id = id;
                _pw = pw;
                _name = name;

                //최근 전적도 최초 생성이라면 일단 만들고..
                _recentPlays = new RecentPlays[10];
                for (int n = 0; n < _recentPlays.Length; n++)
                {
                    _recentPlays[n] = new RecentPlays(-1,-1,-1);
                }
            }
        }
        public struct RecentPlays //최근 전적
        {
            public int _result;
            public float _playTime;
            public float _winRate;

            public RecentPlays(int r, int pt, int wr)
            {
                _result = r;
                _playTime = pt;
                _winRate = wr;
            }
        }

        //전체 유저 정보
        List<UserGameInfo> _ltUserData = new List<UserGameInfo>();
        public List<UserGameInfo> UserData { get { return _ltUserData; } set { _ltUserData = value; } }

        //현재 접속 유저 정보
        Dictionary<long, UserGameInfo> _nowConnectUser = new Dictionary<long, UserGameInfo>();
        public Dictionary<long, UserGameInfo> NowConnectUSer { get { return _nowConnectUser; } set { _nowConnectUser = value; } }

        //데이터 검수
        public bool CheckIDExisted(string id)
        {

            for (int n = 0; n < _ltUserData.Count; n++)
            {

                if (_ltUserData[n]._id == id)   //중복된 아이디가 있다!
                {
                    return true;
                }
            }

            //다 돌려봤지만 중복된 값은 없었다.
            return false;
        }
        public bool CheckNameExisted(string name)
        {

            for (int n = 0; n < _ltUserData.Count; n++)
            {
                if (_ltUserData[n]._name == name)   //중복된 아이디가 있다!
                {
                    return true;
                }
            }

            //다 돌려봤지만 중복된 값은 없었다.
            return false;
        }
        public int CheckLoginAccount(string id, string pw)
        {
            //전체 유저 정보를 for문으로 돌려서 찾는게 맞을까..?
            for (int n = 0; n < _ltUserData.Count; n++)
            {
                if (_ltUserData[n]._id == id && _ltUserData[n]._pw == pw)
                {
                    //id, pw가 일치하는 정보가 있습니다~
                    return n;
                }
            }
            return -1;
        }
    }
}
