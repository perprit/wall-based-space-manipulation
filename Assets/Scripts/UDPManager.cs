using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;
using System.Linq;
using HoloToolkit.Unity;
using System.Collections.Generic;
using ManipulateWalls;

#if !UNITY_EDITOR
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Networking;
#endif

public class UDPManager : Singleton<UDPManager> {
    public string UDP_PORT;
    public string externalIP;
    public string externalPort;

    private volatile MouseEvent MouseDelta = new MouseEvent();

    public readonly static Queue<Action> ExecuteOnMainThread = new Queue<Action>();

#if !UNITY_EDITOR
    DatagramSocket socket;
#endif

    // Use this for initialization
#if !UNITY_EDITOR
    async void Start()
    {
        Debug.Log("Waiting for a connection...");

        socket = new DatagramSocket();
        socket.MessageReceived += Socket_MessageReceived;
        HostName IP = null;
        try
        {
            var icp = NetworkInformation.GetInternetConnectionProfile();
            IP = Windows.Networking.Connectivity.NetworkInformation.GetHostNames()
                .SingleOrDefault(hn =>
                hn.IPInformation?.NetworkAdapter != null && hn.IPInformation.NetworkAdapter.NetworkAdapterId
                == icp.NetworkAdapter.NetworkAdapterId);
            await socket.BindEndpointAsync(IP, UDP_PORT);
            //await socket.BindServiceNameAsync(UDP_PORT);
        }
        catch (Exception e)
        {
            Debug.Log(e.ToString());
            Debug.Log(SocketError.GetStatus(e.HResult).ToString());
            return;
        }

        Debug.Log("listening from " + IP + " : " + UDP_PORT);
        Debug.Log("exit start");
    }

    private async System.Threading.Tasks.Task SendMessage(string message)
    {
        using (var stream = await socket.GetOutputStreamAsync(new Windows.Networking.HostName(externalIP), externalPort))
        {
            using (var writer = new Windows.Storage.Streams.DataWriter(stream))
            {
                var data = Encoding.UTF8.GetBytes(message);

                writer.WriteBytes(data);
                await writer.StoreAsync();
                Debug.Log("Sent: " + message);
            }
        }
    }
#else
    void Start()
    {

    }
#endif

    void Update()
    {
        while (ExecuteOnMainThread.Count > 0)
        {
            ExecuteOnMainThread.Dequeue().Invoke();
        }
    }

#if !UNITY_EDITOR
    private async void Socket_MessageReceived(Windows.Networking.Sockets.DatagramSocket sender,
        Windows.Networking.Sockets.DatagramSocketMessageReceivedEventArgs args)
    {
        Debug.Log("GOT MESSAGE: ");
        //Read the message that was received from the UDP echo client.
        Stream streamIn = args.GetDataStream().AsStreamForRead();
        StreamReader reader = new StreamReader(streamIn);
        string message = await reader.ReadLineAsync();

        Debug.Log("MESSAGE: " + message);

        if (ExecuteOnMainThread.Count == 0)
        {
            ExecuteOnMainThread.Enqueue(() =>
            {
                Debug.Log("Enqueue MESSAGE: " + message);
                try
                {
                    //MouseEvent me = JsonUtility.FromJson<MouseEvent>(message);
                    //MouseDelta.add(me);                
                }            
                catch (FormatException e)
                {
                    Debug.Log(e.Message);
                }
            });
        }
    }
#endif

    [Serializable]
    public class KeyboardEvent
    {

    }
    [Serializable]
    public class MouseEvent
    {
        public int l;   // left button
        public int m;   // middle button
        public int r;   // right button
        public int s;   // scroll wheel
        public int x;   // x mouse movement
        public int y;   // y mouse movement

        public void clear()
        {
            l = 0;
            m = 0;
            r = 0;
            s = 0;
            x = 0;
            y = 0;
        }

        public void add(MouseEvent me)
        {
            this.l += me.l;
            this.m += me.m;
            this.r += me.r;
            this.s += me.s;
            this.x += me.x;
            this.y += me.y;
        }

        public bool isZero()
        {
            return this.x == 0 && this.y == 0 && this.l == 0 && this.m == 0 && this.r == 0 && this.s == 0;
        }

        public bool isScroll()
        {
            return this.s != 0;
        }
    }
}
