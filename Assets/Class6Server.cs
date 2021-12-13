using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.IO;
using UnityEngine.UI;

public class Class6Server : MonoBehaviour
{
   int maxConnections = 1000;
    int reliableChannelID;
    int unreliableChannelID;
    int hostID;
    int socketPort = 4498;

    private LinkedList<PlayerAccount1> playerAccount1;
    // Start is called before the first frame update
    void Start()
    {
        NetworkTransport.Init();
        ConnectionConfig config = new ConnectionConfig();
        reliableChannelID = config.AddChannel(QosType.Reliable);
        unreliableChannelID = config.AddChannel(QosType.Unreliable);
        HostTopology topology = new HostTopology(config, maxConnections);
        hostID = NetworkTransport.AddHost(topology, socketPort, null);

        playerAccount1 = new LinkedList<PlayerAccount1>();
    }

    // Update is called once per frame
    void Update()
    {

        int recHostID;
        int recConnectionID;
        int recChannelID;
        byte[] recBuffer = new byte[1024];
        int bufferSize = 1024;
        int dataSize;
        byte error = 0;

        NetworkEventType recNetworkEvent = NetworkTransport.Receive(out recHostID, out recConnectionID, out recChannelID, recBuffer, bufferSize, out dataSize, out error);

        switch (recNetworkEvent)
        {
            case NetworkEventType.Nothing:
                break;
            case NetworkEventType.ConnectEvent:
                Debug.Log("Connection, " + recConnectionID);
                break;
            case NetworkEventType.DataEvent:
                string msg = Encoding.Unicode.GetString(recBuffer, 0, dataSize);
                ProcessRecievedMsg(msg, recConnectionID);
                break;
            case NetworkEventType.DisconnectEvent:
                Debug.Log("Disconnection, " + recConnectionID);
                break;
        }

    }
  
    public void SendMessageToClient(string msg, int id)
    {
        byte error = 0;
        byte[] buffer = Encoding.Unicode.GetBytes(msg);
        NetworkTransport.Send(hostID, id, reliableChannelID, buffer, msg.Length * sizeof(char), out error);
    }
    
    private void ProcessRecievedMsg(string msg, int id)
    {
        Debug.Log("msg recieved = " + msg + ".  connection id = " + id);

        string[] csv = msg.Split(',');

        int signifier = int.Parse(csv[0]);

        if (signifier == ClientToServerSignifiers1.CreateAccountAttempt)
        {
            string n = csv[1];
            string p = csv[2];

            bool userNameAlreadyIsUse = false;
            
            foreach (PlayerAccount1 pa in playerAccount1)
            {
                if (pa.name == n)
                {
                    userNameAlreadyIsUse = true;
                    break;
                }
            }

            if (!userNameAlreadyIsUse)
            {
                PlayerAccount1 pa = new PlayerAccount1(n, p);
                playerAccount1.AddLast(pa); 
                SendMessageToClient(ServerToClientSignifiers1.CreateAccountSuccess + "",id);
            }
            else
            {
                SendMessageToClient(ServerToClientSignifiers1.CreateAccountFailure + "",id);

            }
           
        }
       else if (signifier == ClientToServerSignifiers1.LoginAttempt)
        {
            string n = csv[1];
            string p = csv[2];

            bool userNameFound = false;
            
            foreach (PlayerAccount1 pa in playerAccount1)
            {
                if (pa.name == n)
                {
                    userNameFound = true;
                    if (pa.password == p)
                    {
                        SendMessageToClient(ServerToClientSignifiers1.LoginSuccess + "",id);
                    }
                    else
                    {
                        SendMessageToClient(ServerToClientSignifiers1.LoginFailure + "",id);

                    }
                }
            }

            if (!userNameFound)
            {
                SendMessageToClient(ServerToClientSignifiers1.LoginFailure + "",id);
            }
        }

    }
}

public class PlayerAccount1
{
    public string name, password;

    public PlayerAccount1(string Name, string Password)
    {
        name = Name;
        password = Password;
    }
}

public static class ServerToClientSignifiers1
{
    public const int CreateAccountSuccess = 1;
    public const int LoginSuccess = 2;

    public const int CreateAccountFailure = 3;
    
    public const int LoginFailure = 4;

   
}

public static class ClientToServerSignifiers1
{
    public const int LoginAttempt = 1;
    public const int CreateAccountAttempt = 2;
}
    






