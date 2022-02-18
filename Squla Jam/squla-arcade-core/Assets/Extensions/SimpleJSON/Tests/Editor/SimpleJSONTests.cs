using UnityEngine;
using UnityEditor;
using SimpleJson;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Squla.TDD
{
	public class SimpleJSONTests
	{
		[Test]
		public void SerializeJSONObject ()
		{
			JsonObject jsonObject = new JsonObject ();
			jsonObject ["name"] = "Matthew";
			jsonObject ["score"] = 30;

			Assert.AreEqual ("Matthew", jsonObject ["name"]);
		}

		[Test]
		public void SerializaJSONArray ()
		{
			JsonArray jsonArray = new JsonArray ();
			jsonArray.Add ("foo");
			jsonArray.Add (10);
			jsonArray.Add (true);

			Assert.Contains ("foo", jsonArray);
		}

		[Test]
		public void DeserializeJSONStringToDictionary ()
		{
			string data = "{\"name\":\"foo\",\"num\":10,\"list1\":[\"foo\",10],\"list2\":[true,null]}";
			var json = (IDictionary<string, object>)SimpleJSON.DeserializeObject (data);

			Assert.AreEqual ("foo", (string)json ["name"]);
			Assert.AreEqual (10, json ["num"]);
		}

	

		[Test]
		/// <summary>
		/// Deserializes the test class.
		/// 
		/// This test is purely for exploring the behaviour of deserializing a class with a JSONObject / IDictionary property
		/// </summary>
		public void DeserializeTestClass ()
		{
			string data = "{\"name\":\"test\", \"actions\": { \"challenge\": { \"href\": \"/v1/game/splash-battle/opponent/756169/challenge\", \"href_type\": \"SplashBattleChallenge\"} } }";
			var dictTest = SimpleJSON.DeserializeObject<DictionaryTestClass> (data);
			var jsonTest = SimpleJSON.DeserializeObject<JsonObjectTestClass> (data);

			Assert.AreEqual (dictTest.name, jsonTest.name);
			Assert.True (dictTest.actions.ContainsKey ("challenge"));
			Assert.False (jsonTest.actions.ContainsKey ("challenge"));
		}

		[Test]
		public void DeserializeClassWithNestedDictionaryField()
		{
			string data = "{\"name\":\"test\", \"active_locales\": {\"de-DE\": {\"APP_ENABLED\": true},\"en-GB\": { \"APP_ENABLED\": true }}}";
			var o = SimpleJSON.DeserializeObject<NestedDictionaryFieldTestClass > (data);

			Assert.IsNotNull (o.active_locales);
			Assert.True (o.active_locales.ContainsKey ("de-DE"));
			Assert.IsTrue (o.active_locales ["de-DE"] is JsonObject);

			var l = SimpleJSON.DeserializeObject<Dictionary<string, bool>>((JsonObject)o.active_locales["de-DE"]);
			Assert.IsNotNull (l);
			Assert.True (l.ContainsKey ("APP_ENABLED"));
			Assert.True (l ["APP_ENABLED"]);
		}

		private class DictionaryTestClass
		{
			public string name = "";
			public Dictionary<object, object> actions = new Dictionary<object, object> ();
		}

		private class JsonObjectTestClass
		{
			public string name = "";
			public JsonObject actions = null;
		}

		private class NestedDictionaryFieldTestClass
		{
			public string name = "";
			public Dictionary<string, object> active_locales = new Dictionary<string, object>();
		}
	}
}

