using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text;
using System.Linq;
using HoloToolkit.Unity;
using System.Collections.Generic;

#if !UNITY_EDITOR
using Windows.Networking.Sockets;
using Windows.Networking.Connectivity;
using Windows.Networking;
#endif

namespace ManipulateWalls
{
    public class UDPManager : Singleton<UDPManager>
    {
        public string UDP_PORT;
        public string externalIP;
        public string externalPort;

        private bool NEW_SEQUENCE = false;
        private volatile SequenceData sd = new SequenceData();

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

                if (NEW_SEQUENCE && sd != null)
                {
                    ExperimentManager.Instance.SetTrialList(sd);
                    NEW_SEQUENCE = false;
                    sd = null;
                }
            }
        }

        public void DisposeSocket()
        {
#if !UNITY_EDITOR
            if (socket != null)
            {
                socket.Dispose();
            }
#endif
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
                    sd = JsonUtility.FromJson<SequenceData>(message);
                    NEW_SEQUENCE = true;
                }            
                catch (FormatException e)
                {
                    Debug.Log(e.Message);
                }
            });
        }
    }
#endif
    }

    [Serializable]
    public class SequenceData
    {
        [Serializable]
        public class TrialString
        {
            public string[] start;
            public string[] target;
            public string z_type;
            public string xy_type;
        }

        public string id;
        public string mode;
        public string method;
        public TrialString[] trials;
    }
}
