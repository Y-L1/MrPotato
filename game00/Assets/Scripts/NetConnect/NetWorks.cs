using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.SceneManagement;

using UnityEngine.UI;
using System.Net.Sockets;
using System.Net;

public class NetWorks : MonoBehaviour
{
    Socket clientSocket;

    public string userName;
    string passWord;
    string passWordMD5;
    byte[] sendBuff = new byte[1024];
    byte[] readBuff = new byte[1024];
    ulong onePerFrame = 0;

    bool connected = false;

    //建立消息缓冲区
    List<string> msgList = new List<string>();


    string ipAddressPort = "";

    //角色编号
    public string whoAmI;

    public Socket getClientSocket()
    {
        return this.clientSocket;
    }

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }



    //连接服务器
    public void BtnConnectClicked()
    {
        string serverIP = "127.0.0.1";
        int serverPORT = 10001;

        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        clientSocket.Connect(serverIP, serverPORT);

        

        clientSocket.BeginReceive(readBuff, 0, 100, 0, receiveCb, clientSocket);
        connected = true;

        //获取本地Socket的地址
        EndPoint localEndPoint = clientSocket.LocalEndPoint;
        ipAddressPort = ((IPEndPoint)localEndPoint).Address.ToString() + ":" + ((IPEndPoint)localEndPoint).Port.ToString();


    }


    public void Start()
    {
        //BtnConnectClicked();

    }
    private void FixedUpdate()
    {
        onePerFrame++;
        if(onePerFrame % 120 == 0)
        {
            if (connected)
            {
                
                sendBuff = System.Text.Encoding.Default.GetBytes("heartBeat ");
                clientSocket.Send(sendBuff);
                
            }
        }


        if (msgList.Count > 0)
        {
            string handleString = msgList[0];

            msgList.Remove(handleString);
            string[] recvStrs = handleString.Split(' ');
            if (recvStrs[0] != "Position")
            if ("RegistFail" == recvStrs[0])
            {
                Debug.Log("" + handleString);
                GameObject.Find("Canvas/InputUserName").GetComponent<InputField>().text = "用户名已存在";
            }
            if ("RegistSuccess" == recvStrs[0])
            {
                Debug.Log("" + handleString);

                SceneManager.LoadScene(1);
            }

            if ("LoginSuccess" == recvStrs[0])
            {

                SceneManager.LoadScene(1);
            }
            if ("LoginFail" == recvStrs[0])
            {
                if (recvStrs[1] == "1")
                {
                    GameObject.Find("Canvas/InputUserName").GetComponent<InputField>().text = "用户名不存在";
                }
                else if (recvStrs[1] == "2")
                {
                    GameObject.Find("Canvas/InputUserName").GetComponent<InputField>().text = "密码错误";
                }
                else if (recvStrs[1] == "3")
                {
                    GameObject.Find("Canvas/InputUserName").GetComponent<InputField>().text = "用户已经登录";
                }
            }

            //接收从数据库获取的名字
            else if ("GetNameSuccessed" == recvStrs[0])
            {
                bool flag = false;
                foreach (string str in UserNameList.userNameList)
                {
                    string userNameList = str.Split(' ')[0];

                    if (userNameList == recvStrs[1])
                    {
                        flag = true;
                    }
                }

                if (!flag)
                    UserNameList.userNameList.Add(recvStrs[1] + " " + recvStrs[2]);

            }
            else if ("GetNameFail" == recvStrs[0])
            {
                //数据库未找到该用户名

            }

            if (recvStrs[0] == "beginGame")
            {
                Debug.Log("收到S--->C begingame协议");
                whoAmI = recvStrs[1];
                SceneManager.LoadScene(2);
            }

            if (recvStrs[0] == "Flip")
            {
                GameObject.Find("hero2").GetComponent<PlayerControl2>().Flip();
            }
            if (recvStrs[0] == "Fire")
            {
                GameObject.Find("hero2/Gun").GetComponent<Gun2>().Fire();

            }
        }
    }


    public void userNameByMysql(string ipAddressPort)
    {
            //向服务端发送getUserNameByMysql请求
            sendBuff = System.Text.Encoding.UTF8.GetBytes("getUserNameByMysql " + ipAddressPort + " ");
            clientSocket.Send(sendBuff);

    }
    

    public void RegistBtnClicked()
    {
        userName =GameObject.Find("Canvas/InputUserName/Text").GetComponent<Text>().text;
        passWord =GameObject.Find("Canvas/PassWordInput/Text").GetComponent<Text>().text;

        //如果用户名中有空格，则要求重新输入
        var spaceIndex = userName.IndexOf(" ");
        if (spaceIndex != -1)
        {
            GameObject.Find("Canvas/InputUserName").GetComponent<InputField>().text = "用户名不能有空格";
            return;
        }

        name = userName;

        //对密码进行 MD5 散列
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] passWordMD5Byte = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(passWord));
        passWordMD5 = BitConverter.ToString(passWordMD5Byte).Replace("-", ""); //转换为字符串

        //获取本机ip地址和端口号


        //客户端发送申请注册协议
        sendBuff = System.Text.Encoding.UTF8.GetBytes("Regist " + userName + " " + passWordMD5 + " " +ipAddressPort + " ");
        clientSocket.Send(sendBuff);
        

        connected = true;
    }

    public void LoginBtnClicked()
    {
        userName = GameObject.Find("Canvas/InputUserName/Text").GetComponent<Text>().text;
        passWord = GameObject.Find("Canvas/PassWordInput/Text").GetComponent<Text>().text;
        //如果用户名中有空格，则要求重新输入
        var spaceIndex = userName.IndexOf(" ");
        if (spaceIndex != -1)
        {
            GameObject.Find("Canvas/UserNameInput").GetComponent<InputField>().text = "用户名不能有空格";
            return;
        }
        //对密码进行 MD5 散列
        MD5 md5 = new MD5CryptoServiceProvider();
        byte[] passWordMD5Byte = md5.ComputeHash(System.Text.Encoding.Default.GetBytes(passWord));
        passWordMD5 = BitConverter.ToString(passWordMD5Byte).Replace("-", ""); //转换为字符串

        //客户端申请登录
        sendBuff = System.Text.Encoding.UTF8.GetBytes("Login " + userName + " " + passWordMD5 + " " + ipAddressPort + " ");
        clientSocket.Send(sendBuff);
    }

    //数据包接收回调函数
    public void receiveCb(IAsyncResult ar)
    {
        int num = clientSocket.EndReceive(ar);
        string recvStr = System.Text.Encoding.UTF8.GetString(readBuff, 0, num);

        if(recvStr.Split(' ')[0] != "Position")
        msgList.Add(recvStr);
        clientSocket.BeginReceive(readBuff, 0, 1024, 0, receiveCb, clientSocket);
    }
    
}
