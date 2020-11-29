using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net;

public class Network : MonoBehaviour
{
    //network
    public string ip = "hinorid.duckdns.org";
    public const int port = 23456;

    IPAddress ipAddr;
    Socket socket;
    IPEndPoint endPoint;

    Thread servercheck_thread;

    byte[] readBuffer = new byte[1500];

    Queue<string> netBuffer = new Queue<string>();

    object buffer_lock = new object();



    //


    Vector3 mousePos;
    Vector3 prevPos;
    Vector3 expectedPos;
    Transform pos;
    Camera cam;

    LineRenderer lineRenderer;

    const float drawTime = 0.5f;

    double elapsedTime = 0;
    int updateCount = 0;
    float acculmatedSpeed = 0;

    public float speed = 0;
    public Vector2 speedVec = Vector2.zero;

    public TMPro.TMP_Text name;
    public TMPro.TMP_Text status;



    void Start()
    {
        pos = GetComponent<Transform>();
        cam = GameObject.Find("Camera").GetComponent<Camera>();
        lineRenderer = GameObject.FindObjectOfType<LineRenderer>();
        name.text = "hello";


        serverOn();
        StartCoroutine(buffer_update());
    }

    IEnumerator buffer_update()
    {
        while (true)
        {
            yield return null; // 코루틴에서 반복문을 쓸수있게 해준다. 없으면 유니티 멈춤
            BufferSystem();
        }
    }

    void serverOn()
    {
        socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);

        IPHostEntry hostEntry = Dns.GetHostEntry(ip);        

        ipAddr = IPAddress.Parse(hostEntry.HostName);
        Debug.Log(ipAddr.ToString());
        endPoint = new IPEndPoint(ipAddr, port);
        socket.Connect(endPoint);

        servercheck_thread = new Thread(onServerReceive);
    }

    void onServerReceive()
    {
        while(true)
        {
            try
            {
                socket.Receive(readBuffer, 0, readBuffer.Length, SocketFlags.None);

                System.Array.Clear(readBuffer, 0, readBuffer.Length);       //어레이 초기화
            }
            catch
            {

            }
        }
    }

    void BufferSystem()
    {
        while (netBuffer.Count != 0) // 큐의 크기가 0이 아니면 작동,   만약 while를 안하면 프레임마다 버퍼를 처리 하는데 많은 패킷을 처리할때는 처리 되는 량보다 쌓이는 량이 많아져 작동이 제대로 이루어지지않음
        {
            string b = null;
            lock (buffer_lock)
            {
                b = netBuffer.Dequeue();// 큐에 담겨있는 버퍼를 스트링에 넣고 사용하기
            }
            Debug.Log("server ->" + b); //버퍼를 사용한다.
        }
    }


    public void ServerSend(string str)
    {
        Encoding euckr = Encoding.GetEncoding(51949);
        byte[] sbuff = euckr.GetBytes(str);         //string to byte로 변환
        socket.Send(sbuff, 0, sbuff.Length, SocketFlags.None);
    }


    // Update is called once per frame
    void Update()
    {
        mousePos = Input.mousePosition;
        Vector3 objPos = cam.ScreenToWorldPoint(mousePos);
        objPos.z = -1;

        prevPos = pos.position;
        pos.position = objPos;

        Vector3 cursorVec = (objPos - prevPos);
        Vector3 deltaSpeedVec = cursorVec / Time.deltaTime;
        deltaSpeedVec.z = 0;
        speedVec = new Vector2(deltaSpeedVec.x, deltaSpeedVec.y);

        speed = deltaSpeedVec.magnitude;

        lineRenderer.SetPosition(0, objPos);
        lineRenderer.SetPosition(1, objPos + (cursorVec * 10));

        elapsedTime += Time.deltaTime;
        acculmatedSpeed += speed;
        updateCount += 1;

        //N초마다 갱신
        if (elapsedTime > drawTime)
        {
            elapsedTime -= drawTime;

            //N초당 평균 속력 계산
            float printSpeed = acculmatedSpeed / (float)updateCount;
            acculmatedSpeed = 0;
            updateCount = 0;
            speed = Mathf.Round(printSpeed * 100) * 0.01f;

            status.text = speed.ToString();
            Debug.Log(mousePos.ToString());
        }


        if(Input.anyKeyDown)
        {
            ServerSend("test string");
        }

    }
}
