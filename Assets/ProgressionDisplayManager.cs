using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressionDisplayManager : MonoBehaviour
{
    private Renderer renderer;

    public int regressionSpeed;

    private float progression;
    private float progressionDelta;

    // Start is called before the first frame update
    void Start()
    {
        renderer = GetComponent<Renderer>();

        progression = 0;
        progressionDelta = 0;
    }

    // Update is called once per frame
    void Update()
    {
        progressionDelta -= regressionSpeed * Time.deltaTime;
    }

    public void SetProgression(float n)
    {
        progressionDelta = n - progression;
        progression = n;
    }
}
