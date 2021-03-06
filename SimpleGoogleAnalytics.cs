﻿using System;
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

		[Header("Anonymize, settings both of these to true makes sure no peronal data is tracked")]
		public bool AnonymizeID = false;
		public bool AnonymizeIP = false;

		string clientId;

		string appId;
		string appVersion;

		Hashtable ht = new Hashtable();

		bool sessionStarted = false;

		[Serializable]
		public class ValidateResponse
		{

			public List<HitParsingResultEntry> hitParsingResult;

			[Serializable]
			public class HitParsingResultEntry
			{
				public bool valid;
			}


			public bool IsValid
			{
				get
				{
					return hitParsingResult[0].valid;
				}
			}

			/* Example of response:
			{
				"hitParsingResult": [ {
					"valid": true,
					"parserMessage": [ ],
					"hit": "/debug/collect?cid=B4BEDE4F-A4C7-54E3-8DDD-6D883A244C0C\"
				} ],
				"parserMessage": [ {
					"messageType": "INFO",
					"description": "Found 1 hit in the request."
				} ]
			}
			*/


		}

		void Awake()
		{
			if (AnonymizeID)
			{
				const string key = "_simplGA_anonID";
				if (PlayerPrefs.HasKey(key))
				{
					clientId = PlayerPrefs.GetString(key);
				}
				else
				{
					//Reasonably unique, since System.Guid is not included in micro mscorlib 
					clientId = UnityEngine.Random.Range(0, int.MaxValue).ToString("x8") + UnityEngine.Random.Range(0, int.MaxValue).ToString("x8");
					PlayerPrefs.SetString(key, clientId);
					PlayerPrefs.Save();
				}
			}
			else
			{
				clientId = SystemInfo.deviceUniqueIdentifier;
			}

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

			if (AnonymizeIP)
				ht.Add("aip", 1);

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
			if (sessionStarted)
				return;

			Hashtable ht = BaseValues();
			ht.Add("sc", "start");
			ht.Add("dp", "start");
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
				{

					var valid = JsonUtility.FromJson<ValidateResponse>(www.text);

					if (valid.IsValid)
						Debug.Log("SimpleGoogleAnalytics hit ok");
					else
					{
						Debug.LogWarning("SimpleGoogleAnalytics, hit failed");
						Debug.Log(www.text);
					}
				}
				else
					Debug.LogWarning(www.error);
			}

		}

	}
}

