using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.FB;
using Unity.Collections;
using Unity.Jobs;
using System.Threading;

public struct ScanAndCopyJob : IJob
{
    public NativeArray<int> jobCommunicationArray;
    public NativeArray<char> jobSrcPathAttribute;
    //public NativeArray<char> jobDstPathAttribute;
    //public NativeArray<char> jobExtentionsAttribute;

    public void Execute()
    {
        foreach (int i in jobCommunicationArray)
        {
            Debug.Log(i);
        }
        foreach (char c in jobSrcPathAttribute)
        {
            Debug.Log(c);
        }
    }
}

public class ProgramManager : MonoBehaviour
{
    // Inputs and outputs

    public InputField srcInput;
    public InputField dstInput;
    public InputField extensionsInput;

    // Job management

    private bool hasJobStarted = false;
    // [0] 1 -> continue the job ; 0 -> stop the job
    // [1] Error code
    // [2] Files scanned
    // [3] Images found
    // [4] Images copied
    // [5] Size to copy
    // [6] Available size
    private JobHandle jobHandle;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void OpenScrFolder()
    {
        string path = FileBrowser.OpenSingleFolder();
        srcInput.text = path;
    }

    public void OpenDstFolder()
    {
        string path = FileBrowser.OpenSingleFolder();
        dstInput.text = path;
    }

    public void ScanAndCopy()
    {
        // Creating the communication array
        NativeArray<int> jobCommunicationArray = new NativeArray<int>(7, Allocator.Persistent);
        jobCommunicationArray[0] = 1;
        jobCommunicationArray[1] = 0;
        jobCommunicationArray[2] = 0;
        jobCommunicationArray[3] = 0;
        jobCommunicationArray[4] = 0;
        jobCommunicationArray[5] = 0;
        jobCommunicationArray[6] = 0;

        // Creating the attributes arrays
        NativeArray<char> jobSrcPathAttribute = new NativeArray<char>(srcInput.text.Length, Allocator.Persistent);
        for (int i = 0; i < srcInput.text.Length; i++)
            jobSrcPathAttribute[i] = srcInput.text[i];

        // Creating the job
        ScanAndCopyJob jobData = new ScanAndCopyJob();
        jobData.jobSrcPathAttribute = jobSrcPathAttribute;
        jobData.jobCommunicationArray = jobCommunicationArray;

        // Scheduling the job
        jobHandle = jobData.Schedule();

        // Handling the job
        jobHandle.Complete();

        // Disposing the communication array
        jobCommunicationArray.Dispose();
    }

    private void SetJobsValues(ScanAndCopyJob job, NativeArray<int> jobCommunicationArray)
    {
    }

    public void StopJob()
    {
    }

    public void OnDestroy()
    {
    }
}
