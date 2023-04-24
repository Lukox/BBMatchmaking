using System.Collections;
using System.Collections.Generic;
//using Unity.Services.Relay;
//using Unity.Services.Relay.Models;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Relay.Models;
using Unity.Services.Relay;
using Unity.Collections;
using Unity.Networking.Transport.Relay;
using Unity.Netcode.Transports.UTP;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

public class ConnectionNetwork : NetworkBehaviour
{

    private static ConnectionNetwork Instance;
    //public static ConnectionNetwork Instance { get { return _instance; } }

    /*private void Awake() {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        } else {
            _instance = this;
        }
    } */

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public async void createRelay(){
        try{
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            string gameCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(gameCode);
            Debug.Log("REceived from server" + StartClient(gameCode));

            //CHANGE GAME SCENE WITH GAMECODE

            RelayServerData rsd = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(rsd);
            NetworkManager.Singleton.StartHost();
        } catch(RelayServiceException e){
            Debug.Log(e);
        }   
    }

    public async void joinRelay() {
        try{
            string gameCode = StartClient("j");
            Debug.Log("SERVER GIVE GAMECODE");
            Debug.Log("Joining Relay with " + gameCode);

            //CHNAGE TO GAME SCENE WITH GAMECODE

            JoinAllocation allocation = await RelayService.Instance.JoinAllocationAsync(gameCode);
            RelayServerData rsd = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(rsd);
            NetworkManager.Singleton.StartClient();
        }catch(RelayServiceException e){
            Debug.Log(e);
        }
    }

    public string StartClient(string gameCode)
    {
        IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        //IPAddress ipAddress = IPAddress.Parse("130.229.191.142"); // replace with server IP address
        IPEndPoint remoteEP = new IPEndPoint(ipAddress, 11000); // replace with server port number

        // Create a TCP/IP socket
        Socket clientSocket = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try {
            // Connect to the remote endpoint
            clientSocket.Connect(remoteEP);

            // Send data to the server
            byte[] sendData = Encoding.ASCII.GetBytes(gameCode);
            clientSocket.Send(sendData);

            // Receive data from the server
            byte[] recvData = new byte[1024];
            int bytesRecv = clientSocket.Receive(recvData);
            string recvStr = Encoding.ASCII.GetString(recvData, 0, bytesRecv);

            return recvStr;
        } catch (Exception e) {
            Debug.Log(e);
            return gameCode;
        }
    }

}
