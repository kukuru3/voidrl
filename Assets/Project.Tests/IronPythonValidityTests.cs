using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class IronPythonValidityTests
{
    Void.Scripting.ScriptHost scriptHost;

    void LoadTestScript(string script) {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "TestData", $"{script}.py");
        scriptHost.LoadScriptFile(path);
    }

    [OneTimeSetUp]
    public void SetupPython() {
        scriptHost = new Void.Scripting.ScriptHost();
        scriptHost.PrepareScope();
        var assVoid = System.Reflection.Assembly.Load("Void");
        scriptHost.ExposeAssemblyToScripts(assVoid);
        // scriptHost.LoadAllScripts()
    }
    // A Test behaves as an ordinary method
    [Test]
    public void RunUnityCode()
    {
        var str = @"
from UnityEngine import Debug

def log(str):
    Debug.Log(str)

log('if you are seeing this then the test has passed')
        ";
        scriptHost.Execute(str);
    }
    [Test]
    public void CallPythonCodeViaExecuteFunction() {
        LoadTestScript("some_test_methods");
        var resultFoobar = scriptHost.ExecuteFunction("return_plain", "foobar");
        var result21 = scriptHost.ExecuteFunction("multiply", 3, 7);
        Assert.AreEqual(21, result21);
        Assert.AreEqual("foobar", resultFoobar);
    }

    [Test]
    public void PythonRunsAssemblyStaticMethod() {
        LoadTestScript("loopback");
        var result = scriptHost.ExecuteFunction("call_static_method_to_return_foo");

        Assert.AreEqual("foo", result);
    }

    [Test]
    public void PythonInstantiatesCSharpInstanceAndRunsInstanceMethod() {
        LoadTestScript("loopback");
        var result = scriptHost.ExecuteFunction("return_bar_from_constructed_instance");
        Assert.AreEqual("bar", result);
    }

    [Test]
    public void CallInstanceMethodFromPythonViaPassedInstance() {
        LoadTestScript("loopback");
        var instance = new Void.GeneralTester();
        var result = scriptHost.ExecuteFunction("return_bar_from_provided_instance", instance);
        Assert.AreEqual("bar", result);
    }
}
