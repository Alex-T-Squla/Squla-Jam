using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using SimpleJson;

namespace Squla.TDD
{
	public class SimpleJSONDemo : MonoBehaviour
	{
		public Text jsonStringText;
		public Text deserializedJsonText;
		public Text jsonObjectText;
		public Text objTypeText1;
		public Text jsonArrayText;
		public Text objTypeText2;

		private string jsonString = "{\"name\": \"Test1\", \"lives\": 10, \"health\": 10.0}";

		// Use this for initialization
		void Start ()
		{
			// Deserialized example
			var json = SimpleJSON.DeserializeObject<PlayerInfo> (jsonString);
			Debug.Log ("----- Deserialize Json String Example");
			Debug.Log (json.ToString ());
			Debug.Log ("---------------------------------------");

			jsonStringText.text = "Json Str: " + jsonString;
			deserializedJsonText.text = "Deserialized Json: " + json.ToString ();

			// Json Object example
			JsonObject jsonObject = new JsonObject ();
			jsonObject ["name"] = "Test2";
			jsonObject ["Age"] = 8;
			jsonObject ["Parent"] = false;

			jsonObjectText.text = "JsonObj Str: " + jsonObject.ToString ();
			objTypeText1.text = "Object Type: " + jsonObject.GetType ().ToString ();
			Debug.Log ("----- Json Object Example -------------");
			Debug.Log (jsonObject.ToString ());
			Debug.Log ("---------------------------------------");

			// Json array example
			JsonArray jsonArray = new JsonArray ();
			jsonArray.Add ("foo");
			jsonArray.Add (10);
			jsonArray.Add (true);
			jsonArray.Add (null);

			jsonArrayText.text = "JsonArray Str: " + jsonArray.ToString ();
			objTypeText2.text = "Object Type: " + jsonObject.GetType ().ToString ();
			Debug.Log ("----- Json Array Example --------------");
			Debug.Log (jsonArray.ToString ());
			Debug.Log ("---------------------------------------");
		}

		public class PlayerInfo
		{
			public string name;
			public int lives;
			public float health;

			public override string ToString ()
			{
				return string.Format ("[PlayerInfo]: Name: {0}, Lives: {1}, Health: {2}", name, lives, health);
			}
		}
	}
}