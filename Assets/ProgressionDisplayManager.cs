using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;

public class ProgressionDisplayManager : MonoBehaviour
{
    public ConcurrentDictionary<string, int> jobInfo;

    private Renderer renderer;

    public static ProgressionDisplayManager instance;

    private float nextShaderUpdate;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        renderer = GetComponent<Renderer>();
        Init();
    }

    // Update is called once per frame
    void Update()
    {
        nextShaderUpdate -= Time.deltaTime;
        if (nextShaderUpdate < 0)
        {
            ShaderUpdate();
            nextShaderUpdate = 1;
        }
    }

    public void Init()
    {
        jobInfo = new ConcurrentDictionary<string, int>();
        jobInfo["shouldStop"] = 0;
        jobInfo["scannedFiles"] = 0;
        jobInfo["measuredFiles"] = 0;
        jobInfo["copiedFiles"] = 0;
        jobInfo["failedFiles"] = 0;

        nextShaderUpdate = 0;
    }

    public void ShaderUpdate()
    {
        renderer.material.SetFloat("_FilesToCopy", jobInfo["scannedFiles"]);
        renderer.material.SetFloat("_MeasuredFiles", jobInfo["measuredFiles"]);
        renderer.material.SetFloat("_CopiedFiles", jobInfo["copiedFiles"]);
        renderer.material.SetFloat("_FailedFiles", jobInfo["failedFiles"]);
    }

    public void SetScanProgression(int scanned)
    {
        jobInfo["scannedFiles"] = scanned;
    }

    public void SetMeasureProgression(int measured)
    {
        jobInfo["measuredFiles"] = measured;
    }

    public void SetCopyProgression(int copied, int failed)
    {
        Debug.Log("Copied: " + copied);
        Debug.Log("Failed: " + failed);
        jobInfo["copiedFiles"] = copied;
        jobInfo["failedFiles"] = failed;
    }

    public bool ShouldStop()
    {
        return jobInfo["shouldStop"] != 0;
    }

    public void StopJob()
    {
        jobInfo["shouldStop"] = 1;
    }
}
