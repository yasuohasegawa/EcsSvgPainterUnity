using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using SVGPainterUnity;

public class Example : MonoBehaviour {

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void OnPlay() {

        var painter = GetComponent<SVGPainter>();
        painter.Play(5f, PainterEasing.EaseInOutCubic, () => {

            Debug.Log("Complete");

            painter.Rewind(5f, PainterEasing.EaseInOutCubic, () => {
                Debug.Log("Rewind Complete");
            });

        });
    }
}
