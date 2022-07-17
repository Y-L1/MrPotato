using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;

using UnityEngine.SceneManagement;

public class RoomsNetWork : MonoBehaviour
{
    GameObject netObject;
    Socket tempSocket;
    ulong onePerFrame = 0;

    string localAddressStr;
    byte[] readBuff = new byte[1024];
    byte[] sendBuff = new byte[1024];

    //string recvStr = "roomList 3 127.0.0.1:10007 127.0.0.1:20006 127.0.0.1:30012";
    string recvStr = "";

    GameObject[] roomAddressTexts;
    GameObject[] enterRoomBtns;
    string[] strRooms;



    void Start()
    {
        netObject = GameObject.FindGameObjectWithTag("netSocket");
        tempSocket = netObject.GetComponent<NetWorks>().getClientSocket();

        tempSocket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCb, tempSocket);

        //获取本地Socket的地址
        EndPoint localEndPoint = tempSocket.LocalEndPoint;
        localAddressStr = ((IPEndPoint)localEndPoint).Address.ToString() + ":" + ((IPEndPoint)localEndPoint).Port.ToString();


    }

    public void ReceiveCb(IAsyncResult ar)
    {
        int num = tempSocket.EndReceive(ar);
        string receiveStr = System.Text.Encoding.UTF8.GetString(readBuff, 0, num);

        //Debug.Log(receiveStr);

        BaseSocket.msgList.Add(receiveStr);
        tempSocket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCb, tempSocket);
    }

    private void FixedUpdate()
    {
        if (BaseSocket.msgList.Count > 0)
        {
            string str = BaseSocket.msgList[0];

            //Debug.Log(str);

            BaseSocket.msgList.Remove(str);
            string[] recvStrs = str.Split(' ');

            //Debug.Log(recvStrs[0]);

            if (recvStrs[0] == "roomList")
            {
                recvStr = str;

                //Debug.Log(recvStr);
            }
            if (recvStrs[0] == "beginGame")
            {
                Debug.Log("收到S--->C begingame协议");
                netObject.GetComponent<NetWorks>().whoAmI = recvStrs[1];
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




    void Update()
    {

        onePerFrame++;
        if(onePerFrame % 120 == 0)
        {
            //tempSocket.Send(System.Text.Encoding.Default.GetBytes("heartBeat"));
            
            try
            {
                refreshRooms(recvStr);
            }
            catch (IndexOutOfRangeException e)
            {
                //Debug.Log("recvstr:" + recvStr);
            }
        }
    }


    

    //编写刷新房间显示的函数，根据 recvStr 的值动态生成文本框和按钮，并为按钮添加单击事件的处理委托
    public void refreshRooms(string recvStr)
    {
        //Debug.Log(recvStr);
        if(roomAddressTexts != null)
        {
            for (int i = 0; i < roomAddressTexts.Length; i++)
                Destroy(roomAddressTexts[i]);
        }
        if(enterRoomBtns != null)
        {
            for (int i = 0; i < enterRoomBtns.Length; i++)
                Destroy(enterRoomBtns[i]);
        }

        strRooms = recvStr.Split(' ');
        int roomCount = int.Parse(strRooms[1]);
        roomAddressTexts = new GameObject[roomCount];
        enterRoomBtns = new GameObject[roomCount];



        //将数据库中的用户信息存入数据表中
        if(UserNameList.userNameList.Count < roomCount)
        {
            for (int i = 0; i < roomCount; i++)
            {
                netObject.GetComponent<NetWorks>().userNameByMysql(strRooms[i + 2]);
                
            }
        }
        

        for(int i = 0; i < roomCount; i++)
        {
            roomAddressTexts[i] = GameObject.Instantiate(Resources.Load("Text1", typeof(GameObject))) as GameObject;
            roomAddressTexts[i].transform.SetParent(GameObject.Find("Canvas/Image/GameObject").transform, false);


            //roomAddressTexts[i].GetComponent<Text>().text = strRooms[i + 2];


            roomAddressTexts[i].GetComponent<Text>().text = GetNameOnList(strRooms[i + 2]);

            enterRoomBtns[i] = GameObject.Instantiate(Resources.Load("Button1", typeof(GameObject))) as GameObject;
            enterRoomBtns[i].transform.SetParent(GameObject.Find("Canvas/Image/GameObject").transform, false);
            if (localAddressStr == strRooms[i + 2])
            {
                enterRoomBtns[i].gameObject.transform.Find("Text").GetComponent<Text>().text = "自己";
                enterRoomBtns[i].GetComponent<Button>().enabled = false;
            }
            else
            {
                enterRoomBtns[i].gameObject.transform.Find("Text").GetComponent<Text>().text = "enter";
            }
                

            enterRoomBtns[i].GetComponent<EnterRoomBtnNum>().setNum(i);

            GameObject tempObj = enterRoomBtns[i];

            enterRoomBtns[i].GetComponent<Button>().onClick.AddListener(
                delegate ()
                {
                    
                    this.OnClick(tempObj);
                }
                );
        }
    }

    //通过Ip和port获取list中的用户名
    string GetNameOnList(string ipAddressPort)
    {
        foreach(string str in UserNameList.userNameList)
        {
            if(ipAddressPort == str.Split(' ')[1])
            {
                return str.Split(' ')[0];
            }
        }
        return ipAddressPort;
    }

    //进入房间

    public void OnClick(GameObject sender)
    {
        int btnIndex = sender.GetComponent<EnterRoomBtnNum>().getNum();
        string sendStr = "enterRoom " + strRooms[btnIndex + 2] + " ";
        sendBuff = System.Text.Encoding.Default.GetBytes(sendStr);
        tempSocket.Send(sendBuff);

    }

    public void BtnCreateRoomClicked()
    {
        sendBuff = System.Text.Encoding.Default.GetBytes("createRoom ");
        tempSocket.Send(sendBuff);
        GameObject.Find("Canvas/Button").GetComponent<Button>().enabled = false;
    }
}
