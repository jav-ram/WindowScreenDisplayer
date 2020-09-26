using System;
using System.Collections; 
using System.Collections.Generic; 
using System.Net; 
using System.Net.Sockets;
using System.Text; 
using System.Threading; 
using UnityEngine;  
using System.Globalization;
using System.Text.RegularExpressions;

[System.Serializable]
public class TCPmsg
{
    public string tx;
    public string ty;
    public string tz;

    public string rx;
    public string ry;
    public string rz;

	public string reset;
}

public class TCPtrans {
    public Vector3 t;
    public Vector3 r;
	public bool reset;
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
	
	public Transform center;
	public Boolean projection;

	public Transform _pa;
	public Transform _pb;
	public Transform _pc;
	public Transform _pd;
	public Transform lookTarget;

	public Transform _a;
	public Transform _b;
	public Transform _c;
	public Transform _d;

	public Camera theCam;

	private float lastMessage;

		
	// Use this for initialization
	void Start () { 		
		// Start TcpServer background thread
		tcpListenerThread = new Thread (new ThreadStart(ListenForIncommingRequests));
		tcpListenerThread.IsBackground = true;
		tcpListenerThread.Start();
		Debug.Log(Camera.main.projectionMatrix);
		//ip = LocalIPAddress();
		lastMessage = Time.time;
	}  	
	
	// Update is called once per frame
	void FixedUpdate () {
		//transform.position += Vector3.up * 10.0f;	
		trans = SimulateCamera(msg);
		
		if (msg != null && msg != "" && trans != null) {
			transform.position = trans.t;
			//transform.rotation = Quaternion.Euler(trans.r);
			transform.rotation = Quaternion.LookRotation(-(trans.t) - center.position, Vector3.up);
			//center.rotation = Quaternion.Euler(trans.r*-1);
		}

		if (projection) {
			UpdateProjection();
		} else {
			CustomProjection();
		}
	}

	private void CustomProjection() {

		Vector3 pa, pb, pc, pd;
        pa = _a.position; //Bottom-Left
        pb = _b.position; //Bottom-Right
        pc = _c.position; //Top-Left
        pd = _d.position; //Top-Right
 
        Vector3 pe = theCam.transform.position; // eye position
 
        Vector3 vr = ( pb - pa ).normalized; // right axis of screen
        Vector3 vu = ( pc - pa ).normalized; // up axis of screen
        Vector3 vn = Vector3.Cross( vr, vu ).normalized; // normal vector of screen
 
        Vector3 va = pa - pe; // from pe to pa
        Vector3 vb = pb - pe; // from pe to pb
        Vector3 vc = pc - pe; // from pe to pc
        Vector3 vd = pd - pe; // from pe to pd
 
        float n = -lookTarget.InverseTransformPoint( theCam.transform.position ).z; // distance to the near clip plane (screen)
        float f = theCam.farClipPlane; // distance of far clipping plane
        float d = 1f; // distance from eye to screen

        float l = Vector3.Dot( vr, va ) * n / d; // distance to left screen edge from the 'center'
        float r = Vector3.Dot( vr, vb ) * n / d; // distance to right screen edge from 'center'
        float b = Vector3.Dot( vu, va ) * n / d; // distance to bottom screen edge from 'center'
        float t = Vector3.Dot( vu, vc ) * n / d; // distance to top screen edge from 'center'
 
        Matrix4x4 p = new Matrix4x4(); // Projection matrix
        p[0, 0] = 2.0f * n / ( r - l );
        p[0, 2] = ( r + l ) / ( r - l );
        p[1, 1] = 2.0f * n / ( t - b );
        p[1, 2] = ( t + b ) / ( t - b );
        p[2, 2] = ( f + n ) / ( n - f );
        p[2, 3] = 2.0f * f * n / ( n - f );
        p[3, 2] = -1.0f;
 
        theCam.projectionMatrix = p; // Assign matrix to camera
	}

	private void UpdateProjection() {
        Vector3 pa, pb, pc, pd;
        pa = _pa.position; //Bottom-Left
        pb = _pb.position; //Bottom-Right
        pc = _pc.position; //Top-Left
        pd = _pd.position; //Top-Right
 
        Vector3 pe = theCam.transform.position; // eye position
 
        Vector3 vr = ( pb - pa ).normalized; // right axis of screen
        Vector3 vu = ( pc - pa ).normalized; // up axis of screen
        Vector3 vn = Vector3.Cross( vr, vu ).normalized; // normal vector of screen
 
        Vector3 va = pa - pe; // from pe to pa
        Vector3 vb = pb - pe; // from pe to pb
        Vector3 vc = pc - pe; // from pe to pc
        Vector3 vd = pd - pe; // from pe to pd
 
        float n = -lookTarget.InverseTransformPoint( theCam.transform.position ).z; // distance to the near clip plane (screen)
        float f = theCam.farClipPlane; // distance of far clipping plane
        float d = 1f; // distance from eye to screen

        float l = Vector3.Dot( vr, va ) * n / d; // distance to left screen edge from the 'center'
        float r = Vector3.Dot( vr, vb ) * n / d; // distance to right screen edge from 'center'
        float b = Vector3.Dot( vu, va ) * n / d; // distance to bottom screen edge from 'center'
        float t = Vector3.Dot( vu, vc ) * n / d; // distance to top screen edge from 'center'
 
        Matrix4x4 p = new Matrix4x4(); // Projection matrix
        p[0, 0] = 2.0f * n / ( r - l );
        p[0, 2] = ( r + l ) / ( r - l );
        p[1, 1] = 2.0f * n / ( t - b );
        p[1, 2] = ( t + b ) / ( t - b );
        p[2, 2] = ( f + n ) / ( n - f );
        p[2, 3] = 2.0f * f * n / ( n - f );
        p[3, 2] = -1.0f;
 
        theCam.projectionMatrix = p; // Assign matrix to camera
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
			byte[] bytes = new byte[4096];
			while (true) {
				connectedTcpClient = tcpListener.AcceptTcpClient();

				string dataFromClient = null;

				NetworkStream stream = connectedTcpClient.GetStream();
				int length;
				while ((length = stream.Read(bytes, 0, bytes.Length)) != 0)  {
					var incommingData = new byte[length];
					Array.Copy(bytes, 0, incommingData, 0, length);
					// Convert byte array to string message.
					string clientMessage = Encoding.ASCII.GetString(incommingData);

					msg = "";
					
					if (clientMessage.Contains("{")) {
						string[] sep = clientMessage.Split('{');
						buffer = "{" + sep[1];
					}

					if (clientMessage.Contains("}")) {
						string[] sep = clientMessage.Split('}');
						msg = buffer + sep[0] + "}";
						buffer = sep[1];
					}
				}
			} 		
		} 		
		catch (SocketException socketException) { 			
			Debug.Log("SocketException " + socketException.ToString()); 		
		}
	}

	public TCPtrans SimulateCamera(string json) {

		var regex = new Regex(@"\{(.*?)\}");

		TCPmsg msg = new TCPmsg();
		TCPtrans rMsg = new TCPtrans();

		try {
			Match m = regex.Match(json);
			msg = JsonUtility.FromJson<TCPmsg>(m.Value);
			
			rMsg.t = new Vector3(
				float.Parse(msg.tx, CultureInfo.InvariantCulture.NumberFormat),
				float.Parse(msg.ty, CultureInfo.InvariantCulture.NumberFormat),
				float.Parse(msg.tz, CultureInfo.InvariantCulture.NumberFormat)
			);
			rMsg.t += new Vector3(0f, 0f, -1f);

			rMsg.r = new Vector3(
				float.Parse(msg.rx, CultureInfo.InvariantCulture.NumberFormat),
				float.Parse(msg.ry, CultureInfo.InvariantCulture.NumberFormat),
				float.Parse(msg.rz, CultureInfo.InvariantCulture.NumberFormat)
			);

			//rMsg.r = Vector3.Lerp(transform.eulerAngles, rMsg.r, 0.5f);
			rMsg.reset = msg.reset == "true" ? true : false;
			//calculate
			float curr = Time.time;
			if (curr - lastMessage > 0f)
				Debug.Log(curr - lastMessage);
			lastMessage = curr;
			return rMsg;
		} catch {
			return null;
		}

	}
}