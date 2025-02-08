using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Icy.Base;
using Cysharp.Threading.Tasks;

public class ExampleRoot : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
		
	}

    // Update is called once per frame
    void Update()
    {
#if UNITY_EDITOR
		TestPlayground.Update();
#endif
	}
}
