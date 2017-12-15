using System;
using System.Collections;
using System.Net;
using UnityEngine;
using System.Text;
using System.IO;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace devm
{
	
	public class SimpleGoogleAnalytics : MonoBehaviour
	{

		const string CollectUrl = "https://www.google-analytics.com/collect";
		const string CollectUrlValidate = "https://www.google-analytics.com/debug/collect";
				
		public int Version = 1;
		public string AppName;
		public string GoogleTrackingID;

		[Tooltip("Does not record, only verifies that the tracking data is correct")]
		public bool OnlyValidate = false;

		public bool AutoTrackScenes = false;

		string clientId;

		string appId;
		string appVersion;

		Hashtable ht = new Hashtable();

		bool sessionStarted = false;

		void Awake()
		{

			clientId = SystemInfo.deviceUniqueIdentifier;
			appId = Application.identifier;
			appVersion = Application.version;

			if (AutoTrackScenes)
			{

				SceneManager.sceneLoaded += (Scene scene, LoadSceneMode loadMode) =>
				{
					if (!sessionStarted)
						StartSession();

					TrackScreen(scene.name);
				};

			}
		}

		Hashtable BaseValues()
		{

			ht.Clear();
			ht.Add("v", Version);
			ht.Add("tid", GoogleTrackingID);
			ht.Add("cid", clientId);
			ht.Add("an", AppName);
			ht.Add("aid", appId);
			ht.Add("av", appVersion);
			ht.Add("ds", Application.platform.ToString());

			return ht;
		}

		public void TrackEvent(string category, string action, string label, long value)
		{
			Hashtable ht = BaseValues();

			ht.Add("t", "event"); 
			ht.Add("ec", category);
			ht.Add("ea", action); 
			if (label != null)
				ht.Add("el", label);
			if (value != 0L)
				ht.Add("ev", value); 

			SendData(ht);
		}




		public void StartSession()
		{
			if(sessionStarted)
				return;

			Hashtable ht = BaseValues();
			ht.Add("sc", "start");
			SendData(ht);
			sessionStarted = true;
		}


		public void TrackPage(string hostname, string page, string title)
		{
			Hashtable ht = BaseValues();

			ht.Add("t", "pageview");
			ht.Add("dh", hostname); 
			ht.Add("dp", page);
			ht.Add("dt", title);

			SendData(ht);
		}


		public void TrackScreen(string screen)
		{
			Hashtable ht = BaseValues();

			ht.Add("t", "screenview");
			ht.Add("cd", screen);   

			SendData(ht);
		}

		#region commerce

		public void EcommerceTransaction(string id, string affiliation, string revenue, string shipping, string tax, string currency)
		{
			Hashtable ht = BaseValues();

			ht.Add("t", "transaction");
			ht.Add("ti", id);
			ht.Add("ta", affiliation);
			ht.Add("tr", revenue);
			ht.Add("ts", shipping);
			ht.Add("tt", tax);
			ht.Add("cu", currency); 

			SendData(ht);
		}

		public void EcommerceItem(string id, string name, string price, string quantity, string code, string category, string currency)
		{
			Hashtable ht = BaseValues();

			ht.Add("t", "item");
			ht.Add("ti", id);
			ht.Add("in", name);
			ht.Add("ip", price);
			ht.Add("iq", quantity);
			ht.Add("ic", code);
			ht.Add("iv", category);
			ht.Add("cu", currency);

			SendData(ht);
		}

		#endregion

		public void TrackSocial(string action, string network, string target)
		{
			Hashtable ht = BaseValues();

			ht.Add("t", "social");
			ht.Add("dh", action);
			ht.Add("dp", network);
			ht.Add("dt", target);

			SendData(ht);
		}

		public void TrackException(string description, bool fatal)
		{
			Hashtable ht = BaseValues();

			ht.Add("t", "exception"); 
			ht.Add("dh", description);
			ht.Add("dp", fatal ? "1" : "0");

			SendData(ht);
		}

		void SendData(Hashtable values)
		{
			
			WWWForm form = new WWWForm();

			foreach (var key in values.Keys)
			{
				form.AddField(key.ToString(), WWW.EscapeURL(values[key].ToString()));
			}

			StartCoroutine(SendDataDo(form));
		
		}


		IEnumerator SendDataDo(WWWForm data)
		{
			string url = OnlyValidate ? CollectUrlValidate : CollectUrl;

			Debug.Log("SimpleGoogleAnalytics sending data");

			var www = new WWW(url, data);

			yield return www;

			if (OnlyValidate)
			{
				if (string.IsNullOrEmpty(www.error))
					Debug.Log(www.text);
				else
					Debug.Log(www.error);
			}

		}

	}
}

