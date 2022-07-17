using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SocketTest : MonoBehaviour
{
    Socket clientSocket;
    bool connected = false;
    byte[] readBuff = new byte[1024];
    
    

    ulong add1PerFrame = 0;
    byte[] sendBuff = new byte[1024];

    public void BtnConnectClicked()
    {
        clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        clientSocket.Connect("127.0.0.1", 10001);
        
        clientSocket.BeginReceive(readBuff, 0, 100, 0, receiveCb, clientSocket);
        connected = true;
    }
    
    void FixedUpdate()
    {
        add1PerFrame++;
        if (add1PerFrame % 120 == 0)
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
            GameObject.Find("Canvas/outputText").GetComponent<Text>().text = handleString;
        }

    }
    
    List<string> msgList = new List<string>();
    public void receiveCb(IAsyncResult ar)
    {
        int num = clientSocket.EndReceive(ar);
        string recvStr = System.Text.Encoding.UTF8.GetString(readBuff, 0, num);
        msgList.Add(recvStr);
        clientSocket.BeginReceive(readBuff, 0, 1024, 0, receiveCb, clientSocket);
    }


    public void LoadNextScece()
    {
        SceneManager.LoadScene(2);
    }
}
