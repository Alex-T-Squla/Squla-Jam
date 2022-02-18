using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Squla.TDD
{
    [CreateAssetMenu(fileName = "TestSuiteDataSet", menuName = "TestData/TestSuiteDataSet", order = 105)]
    public class TestSuiteDataSet: ScriptableObject
    {
        public string title;
        public List<Test> tests = new List<Test>();
        
	    [Serializable]
	    public class Test
	    {
		    public bool enabled = true;
		    public GameObject test;
	    }
    }
}
