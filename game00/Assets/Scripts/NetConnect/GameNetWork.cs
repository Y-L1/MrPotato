using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameNetWork : MonoBehaviour
{
    public GameObject WF;
    GameObject netObj;
    GameObject hero1;
    GameObject hero2;
    string woAmI;
    ulong add1PerFrame = 0;
    byte[] readBuff = new byte[1024];
    byte[] sendBuff = new byte[1024];
    Vector2 newPostion;
    Socket tempSocket;



    // Start is called before the first frame update
    void Start()
    {
        

        netObj = GameObject.FindGameObjectWithTag("netSocket");
        tempSocket = netObj.GetComponent<NetWorks>().getClientSocket();
        tempSocket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCb, tempSocket);

        short woAmI = short.Parse(netObj.GetComponent<NetWorks>().whoAmI);
        setPosition(woAmI);
    }

    public void ReceiveCb(IAsyncResult ar)
    {
        

        int num = tempSocket.EndReceive(ar);
        string receiveStr = System.Text.Encoding.Default.GetString(readBuff, 0, num);
        if(receiveStr != " ")
        BaseSocket.msgList.Add(receiveStr);
        tempSocket.BeginReceive(readBuff, 0, 1024, 0, ReceiveCb, tempSocket);
    }

    void setPosition(short woAmI)
    {
        hero1 = GameObject.Find("hero");
        hero2 = GameObject.Find("hero2");

        float x1, x2, y1, y2;
        y1 = y2 = hero1.transform.position.y;

        if(woAmI == 0)
        {
            x1 = 17;
            x2 = -17;
        }
        else
        {
            x1 = -17;
            x2 = 17;
        }
        Vector2 position1 = new Vector2(x1, y1);
        Vector2 position2 = new Vector2(x2, y2);
        hero1.transform.position = position1;
        hero2.transform.position = position2;
    }
    

    

    private void Update()
    {
        if (BaseSocket.msgList.Count > 0)
        {
            string str = BaseSocket.msgList[0];
            BaseSocket.msgList.Remove(str);
            string[] recvStrs = str.Split(' ');
            if (recvStrs[0] == "Position")
            {
                float x = float.Parse(recvStrs[1]);
                float y = float.Parse(recvStrs[2]);
                newPostion = new Vector2(x, y);
                GameObject.Find("hero2").transform.position = newPostion;
                float h = float.Parse(recvStrs[3]);
                GameObject.Find("hero2").GetComponent<PlayerHealth2>().health = h;
                GameObject.Find("hero2").GetComponent<PlayerHealth2>().UpdateHealthBar();
                if (h <= 0)
                {
                    GameObject.Find("hero2").GetComponent<Animator>().SetTrigger("Die");

                    string sendStr = "Win" + " ";
                    sendBuff = System.Text.Encoding.Default.GetBytes(sendStr);
                    tempSocket.Send(sendBuff);
                }
            }
            if (recvStrs[0] == "beginGame")
            {
                Debug.Log("收到S--->C begingame协议");
                netObj.GetComponent<NetWorks>().whoAmI = recvStrs[1];
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
            if (recvStrs[0] == "Win")
            {
                WF.SetActive(true);
                WF.GetComponentsInChildren<Text>()[0].text = "Win";
            }
            else if(recvStrs[0] == "Fail")
            {
                WF.SetActive(true);
                WF.GetComponentsInChildren<Text>()[0].text = "Fail";
            }

        }


        /*************发送Position协议************/
        add1PerFrame++;
        if (add1PerFrame % 5 == 0)
        {
            string sendStr = "Position ";
            sendStr += hero1.transform.position.x.ToString() + ' '; //!!!!!!!!!
            sendStr += hero1.transform.position.y.ToString() + ' ';
            float h = hero1.GetComponent<PlayerHealth>().health;
            sendStr += h.ToString() + ' ';
            sendBuff = System.Text.Encoding.Default.GetBytes(sendStr); //!!!!!!!!!!!!!
            tempSocket.Send(sendBuff);
        }
    }
    public void Exit()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif

        //向服务端发送游戏结束退出协议
        sendBuff = System.Text.Encoding.Default.GetBytes("Exit " + GameObject.FindGameObjectWithTag("netSocket").GetComponent<NetWorks>().userName + " ");
        tempSocket.Send(sendBuff);
    }

}
