using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineVariable : MonoBehaviour {
    public Coroutine coroutine { get; private set; }
    public object result;
    private IEnumerator action;
    private MonoBehaviour owner;
    public CoroutineVariable() {
        owner = null;
        action = null;
        result = "start";
        coroutine = null;
    }

    public void RunAction(MonoBehaviour INSPECTING_OWNER, IEnumerator NEW_ACTION) {
        owner = INSPECTING_OWNER;
        action = NEW_ACTION;
        result = "start";
        if (coroutine != default(Coroutine))
            owner.StopCoroutine(coroutine);
        coroutine = owner.StartCoroutine(Run());
    }

    private IEnumerator Run() {
        while ( action.MoveNext() ) {
            result = action.Current;
            yield return result;
        }
    }
}


//private IEnumerator LoadSomeStuff()
//{
//    WWW www = new WWW("http://someurl");
//    yield return www;
//    if (String.IsNullOrEmpty(www.error) {
//        yield return "success";
//    }
//    else
//    {
//        yield return "fail";
//    }
//}

//CoroutineWithData cd = new CoroutineWithData(this, LoadSomeStuff());
//yield return cd.coroutine;
//Debug.Log("result is " + cd.result);  //  'success' or 'fail'