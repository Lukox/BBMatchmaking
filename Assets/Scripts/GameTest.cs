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

public class GameTest : NetworkBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int playerNumber = Matchmaking.playerNumber;   
        string matchID = Matchmaking.matchID;   
        Debug.Log("I am player number "+playerNumber);
        Debug.Log("MatchID "+ matchID);
        if(playerNumber == 0){
            createRelay(matchID);
        }else{
            joinRelay(matchID);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void createRelay(string matchID){
        try{
            Allocation allocation = await RelayService.Instance.CreateAllocationAsync(1);
            string gameCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log(gameCode);
            Debug.Log("REceived from server" + StartClient(0 + " " + matchID + " " +gameCode));

            //CHANGE GAME SCENE WITH GAMECODE

            RelayServerData rsd = new RelayServerData(allocation, "dtls");
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(rsd);
            NetworkManager.Singleton.StartHost();
        } catch(RelayServiceException e){
            Debug.Log(e);
        }   
    }

    public async void joinRelay(string MatchId) {
        try{
            string gameCode = StartClient(1 + " " + MatchId);
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
        //IPAddress ipAddress = IPAddress.Parse("127.0.0.1");
        IPAddress ipAddress = IPAddress.Parse("130.229.191.142"); // replace with server IP address
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
