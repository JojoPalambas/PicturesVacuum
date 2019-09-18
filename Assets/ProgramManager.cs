using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.FB;
using Unity.Collections;
using Unity.Jobs;
using System.Threading;
using System.IO;

// TODO Error log
// TODO Animations

public struct ScanAndCopyJob : IJob
{
    public NativeArray<int> jobCommunicationArray;
    public NativeArray<char> jobSrcPathAttribute;
    public NativeArray<char> jobDstPathAttribute;
    public NativeArray<char> jobExtensionsAttribute;

    public void Execute()
    {
        string srcPath = Utils.CharNativeArrayToString(jobSrcPathAttribute);
        string dstPath = Utils.CharNativeArrayToString(jobDstPathAttribute);
        string[] extensions = Utils.CharNativeArrayToString(jobExtensionsAttribute).Split(' ');

        string[] fileNames = new string[0];
        if (srcPath == "")
        {
            DriveInfo[] drives = DriveInfo.GetDrives();

            for (int i = 0; i < drives.Length; i++)
            {
                fileNames = Directory.GetFiles(drives[i].Name, "*.png", SearchOption.AllDirectories);
                Debug.Log("Drive " + drives[i].Name + " done");
            }
        }
        else
        {
            fileNames = Directory.GetFiles(srcPath, "*.png", SearchOption.AllDirectories);
            foreach (string fileName in fileNames)
            {
                Debug.Log(fileName);
            }
        }

        jobCommunicationArray[0] = 0;
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
    NativeArray<int> jobCommunicationArray;
    NativeArray<char> jobSrcPathAttribute;
    NativeArray<char> jobDstPathAttribute;
    NativeArray<char> jobExtensionsAttribute;
    private JobHandle jobHandle;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (jobHandle.IsCompleted)
        {
            Debug.Log("Stop");
            StopJob();
        }
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
        jobCommunicationArray = new NativeArray<int>(7, Allocator.Persistent);
        jobCommunicationArray[0] = 1;
        jobCommunicationArray[1] = 0;
        jobCommunicationArray[2] = 0;
        jobCommunicationArray[3] = 0;
        jobCommunicationArray[4] = 0;
        jobCommunicationArray[5] = 0;
        jobCommunicationArray[6] = 0;

        // Creating the attributes arrays
        jobSrcPathAttribute = new NativeArray<char>(srcInput.text.Length, Allocator.Persistent);
        for (int i = 0; i < srcInput.text.Length; i++)
            jobSrcPathAttribute[i] = srcInput.text[i];
        jobDstPathAttribute = new NativeArray<char>(dstInput.text.Length, Allocator.Persistent);
        for (int i = 0; i < dstInput.text.Length; i++)
            jobDstPathAttribute[i] = dstInput.text[i];
        jobExtensionsAttribute = new NativeArray<char>(extensionsInput.text.Length, Allocator.Persistent);
        for (int i = 0; i < extensionsInput.text.Length; i++)
            jobExtensionsAttribute[i] = extensionsInput.text[i];

        // Creating the job
        ScanAndCopyJob jobData = new ScanAndCopyJob();
        jobData.jobSrcPathAttribute = jobSrcPathAttribute;
        jobData.jobDstPathAttribute = jobDstPathAttribute;
        jobData.jobExtensionsAttribute = jobExtensionsAttribute;
        jobData.jobCommunicationArray = jobCommunicationArray;

        // Scheduling the job
        jobHandle = jobData.Schedule();
    }

    private void SetJobsValues(ScanAndCopyJob job, NativeArray<int> jobCommunicationArray)
    {
    }

    public void StopJob()
    {
        // Handling the job
        if (!jobHandle.IsCompleted)
            jobHandle.Complete();

        // Disposing the communication array
        if (jobCommunicationArray.Length > 0)
            jobCommunicationArray.Dispose();
        if (jobSrcPathAttribute.Length > 0)
            jobSrcPathAttribute.Dispose();
        if (jobDstPathAttribute.Length > 0)
            jobDstPathAttribute.Dispose();
        if (jobExtensionsAttribute.Length > 0)
            jobExtensionsAttribute.Dispose();
    }

    public void OnDestroy()
    {
        StopJob();
    }
}
