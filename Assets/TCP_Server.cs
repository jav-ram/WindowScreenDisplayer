using System;
using System.Collections; 
using System.Collections.Generic; 
using System.Net; 
using System.Net.Sockets;
using System.Net.NetworkInformation;
using System.Text; 
using System.Threading; 
using UnityEngine;  
using System.Globalization;
using System.Text.RegularExpressions;

[System.Serializable]
public class TCPmsg
{
    //public string tx;
    //public string ty;
    //public string tz;

    public string rx;
    public string ry;
    public string rz;
    public string rw;
}

public class TCPtrans {
    public Vector3 t;
    public Quaternion r;
}


public class TCP_Server : MonoBehaviour {  	
	#region private members 
	private TcpListener tcpListener; 
	private Thread tcpListenerThread;
	private TcpClient connectedTcpClient; 	
	#endregion
	public string msg;
	public string buffer;
	private TCPtrans trans;

    public string ip;
	public GameObject camera;
	
	public float translationDeadzone;
	public float translationScale;

	public float rotationDeadzone;
		
	// Use this for initialization
	void Start () { 		
		// Start TcpServer background thread 		
		tcpListenerThread = new Thread (new ThreadStart(ListenForIncommingRequests)); 		
		tcpListenerThread.IsBackground = true; 		
		tcpListenerThread.Start();
		//ip = LocalIPAddress();
	}  	
	
	// Update is called once per frame
	void Update () {
		//transform.position += Vector3.up * 10.0f;	
		trans = SimulateCamera(msg);
		if (msg != null && msg != "" && trans != null) {
			transform.position += trans.t * translationScale;
			transform.rotation = trans.r;
		}
	}  	
	
	/// <summary> 	
	/// Runs in background TcpServerThread; Handles incomming TcpClient requests 	
	/// </summary> 	
	private void ListenForIncommingRequests () { 		
		try { 			
			// Create listener on localhost port 8052. 			
			tcpListener = new TcpListener(IPAddress.Parse(ip), 8052); 			
			tcpListener.Start();              
			Debug.Log("Server is listening");              
			Byte[] bytes = new Byte[1024];  			
			while (true) { 				
				using (connectedTcpClient = tcpListener.AcceptTcpClient()) { 					
					// Get a stream object for reading 					
					using (NetworkStream stream = connectedTcpClient.GetStream()) { 						
						int length; 						
						// Read incomming stream into byte arrary. 						
						while ((length = stream.Read(bytes, 0, bytes.Length)) != 0) {
							msg = "";							
							var incommingData = new byte[length]; 							
							Array.Copy(bytes, 0, incommingData, 0, length);  							
							// Convert byte array to string message. 							
							string clientMessage = Encoding.ASCII.GetString(incommingData); 							
							if (!clientMessage.Contains("}")) {
								buffer += clientMessage;
							} else {
								string[] sep = clientMessage.Split('}');
								msg = buffer + sep[0] + "}";
								buffer = sep[1];
							}
						} 					
					} 				
				} 			
			} 		
		} 		
		catch (SocketException socketException) { 			
			Debug.Log("SocketException " + socketException.ToString()); 		
		}     
	}  	
	/// <summary> 	
	/// Send message to client using socket connection. 	
	/// </summary> 	
	private void SendMessage() { 		
		if (connectedTcpClient == null) {             
			return;         
		}  		
		
		try { 			
			// Get a stream object for writing. 			
			NetworkStream stream = connectedTcpClient.GetStream(); 			
			if (stream.CanWrite) {                 
				string serverMessage = "This is a message from your server."; 			
				// Convert string message to byte array.                 
				byte[] serverMessageAsByteArray = Encoding.ASCII.GetBytes(serverMessage); 				
				// Write byte array to socketConnection stream.               
				stream.Write(serverMessageAsByteArray, 0, serverMessageAsByteArray.Length);               
				Debug.Log("Server sent his message - should be received by client");           
			}       
		} 		
		catch (SocketException socketException) {             
			Debug.Log("Socket exception: " + socketException);         
		} 	
	}

	public static string LocalIPAddress() {
		IPHostEntry host;
		string localIP = "0.0.0.0";
		host = Dns.GetHostEntry(Dns.GetHostName());
		foreach (IPAddress ip in host.AddressList)
		{
			if (ip.AddressFamily == AddressFamily.InterNetwork)
			{
				localIP = ip.ToString();
				break;
			}
		}
		return localIP;
	}

	public TCPtrans SimulateCamera(string json) {

		var regex = new Regex(@"\{(.*?)\}");

		TCPmsg msg = new TCPmsg();
		TCPtrans rMsg = new TCPtrans();

		try {
			Match  m = regex.Match(json);
			msg = JsonUtility.FromJson<TCPmsg>(m.Value);
			rMsg.t = new Vector3(
				0f, //float.Parse(msg.tx, CultureInfo.InvariantCulture.NumberFormat),
				0f, //float.Parse(msg.ty, CultureInfo.InvariantCulture.NumberFormat),
				0f // float.Parse(msg.tz, CultureInfo.InvariantCulture.NumberFormat)
			);
			rMsg.r = new Quaternion(
				float.Parse(msg.rx, CultureInfo.InvariantCulture.NumberFormat),
				float.Parse(msg.ry, CultureInfo.InvariantCulture.NumberFormat),
				float.Parse(msg.rz, CultureInfo.InvariantCulture.NumberFormat),
				float.Parse(msg.rw, CultureInfo.InvariantCulture.NumberFormat)
			);
			return rMsg;
		} catch {
			Debug.Log(json);
			return null;
		}

	}
}