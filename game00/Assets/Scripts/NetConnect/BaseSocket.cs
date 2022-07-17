using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * 注意这个脚本不是从 MonoBehaviour 类派生而来。
 * 则各场景都可以用 BaseSocket.msgList 的方式使用该消息缓冲区。
 */
public class BaseSocket
{
    static public List<string> msgList = new List<string>();
}
