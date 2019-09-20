using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using UnityEngine;
using TMPro;

public class ProgressionDisplayManager : MonoBehaviour
{
    public ConcurrentDictionary<string, int> jobInfo;

    private Renderer renderer;

    public static ProgressionDisplayManager instance;

    public TextMeshProUGUI scannedDisplayer;
    public TextMeshProUGUI measuredDisplayer;
    public TextMeshProUGUI copiedDisplayer;
    public TextMeshProUGUI failedDiplayer;
    public TextMeshProUGUI errorLog;

    private float nextShaderUpdate;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        renderer = GetComponent<Renderer>();

        renderer.material.SetFloat("_FilesToCopy", 0);
        renderer.material.SetFloat("_MeasuredFiles", 0);
        renderer.material.SetFloat("_CopiedFiles", 0);
        renderer.material.SetFloat("_FailedFiles", 0);

        Init();
    }

    // Update is called once per frame
    void Update()
    {
        nextShaderUpdate -= Time.deltaTime;
        if (nextShaderUpdate < 0)
        {
            ShaderUpdate();
            TextDisplayUpdate();
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
        jobInfo["errorLog"] = 0;

        nextShaderUpdate = 0;
    }

    public void ShaderUpdate()
    {
        renderer.material.SetFloat("_FilesToCopy", jobInfo["scannedFiles"]);
        renderer.material.SetFloat("_MeasuredFiles", jobInfo["measuredFiles"]);
        renderer.material.SetFloat("_CopiedFiles", jobInfo["copiedFiles"]);
        renderer.material.SetFloat("_FailedFiles", jobInfo["failedFiles"]);
    }

    public void TextDisplayUpdate()
    {
        scannedDisplayer.text = "Scanned files: " + jobInfo["scannedFiles"].ToString();
        measuredDisplayer.text = "Measured files: " + jobInfo["measuredFiles"].ToString();
        copiedDisplayer.text = "Copied files: " + jobInfo["copiedFiles"].ToString();
        failedDiplayer.text = "Failed files: " + jobInfo["failedFiles"].ToString();

        switch (jobInfo["errorLog"])
        {
            case 0:
                errorLog.text = "";
                break;
            case 1:
                errorLog.text = "Error: Invalid source";
                break;
            case 2:
                errorLog.text = "Error: Invalid destination";
                break;
            case 3:
                errorLog.text = "Error: Invalid extensions";
                break;
            case 4:
                errorLog.text = "Error: Not enough space on destination disk";
                break;
            case 5:
                errorLog.text = "Error: Manual stop";
                break;
            default:
                errorLog.text = "Error: Unknown error";
                break;
        }
    }

    public void SetErrorCode(int errorCode)
    {
        jobInfo["errorLog"] = errorCode;
        TextDisplayUpdate();
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

    public void StopJob(int errorCode)
    {
        jobInfo["shouldStop"] = 1;
        jobInfo["errorLog"] = errorCode;
    }
}
