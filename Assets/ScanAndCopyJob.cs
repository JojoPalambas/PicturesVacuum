using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.FB;
using Unity.Collections;
using Unity.Jobs;
using System.Threading;
using System.IO;
using Stopwatch = System.Diagnostics.Stopwatch;

public abstract class ScanAndCopyProgressionManager
{
    public static Stopwatch stopWatch;

    public static int imagesScanned = 0;

    public static void PublishScanProgression(int progression)
    {
        Debug.Log(progression);
    }

    public static void PublishCopyProgression(int progression)
    {
        Debug.Log(progression);
    }
}

public struct ScanAndCopyJob : IJob
{
    public NativeArray<int> jobCommunicationArray;
    public NativeArray<char> jobSrcPathAttribute;
    public NativeArray<char> jobDstPathAttribute;
    public NativeArray<char> jobExtensionsAttribute;

    public void Execute()
    {
        ScanAndCopyProgressionManager.stopWatch = new Stopwatch();
        ScanAndCopyProgressionManager.stopWatch.Start();

        string srcPath = Utils.CharNativeArrayToString(jobSrcPathAttribute);
        string dstPath = Utils.CharNativeArrayToString(jobDstPathAttribute);
        string[] extensions = Utils.CharNativeArrayToString(jobExtensionsAttribute).Split(' ');
        for (int i = 0; i < extensions.Length; i++)
            extensions[i] = '.' + extensions[i];

        List<string> fileNames = null;
        Debug.Log(srcPath);
        if (srcPath == "")
        {
            DriveInfo[] drives = DriveInfo.GetDrives();
            string[] driveNames = new string[drives.Length];
            for (int i = 0; i < drives.Length; i++)
                driveNames[i] = drives[i].Name;

            fileNames = GetAllFiles(driveNames, extensions);
        }
        else
        {
            string[] srcPathInArray = new string[1];
            srcPathInArray[0] = srcPath;

            fileNames = GetAllFiles(srcPathInArray, extensions);
        }

        ScanAndCopyProgressionManager.imagesScanned = fileNames.Count;
        Debug.Log("Count: " + fileNames.Count.ToString());
    }

    private List<string> GetAllFiles(string[] srcs, string[] extensions)
    {
        Debug.Log("GetAllFiles at " + srcs[0]);

        List<string> accu = new List<string>();

        foreach (string src in srcs)
            GetAllFilesRec(src, extensions, accu);

        return accu;
    }

    private void GetAllFilesRec(string src, string[] extensions, List<string> accu)
    {
        if (ScanAndCopyProgressionManager.stopWatch.ElapsedMilliseconds > 1000)
        {
            ScanAndCopyProgressionManager.PublishScanProgression(accu.Count);
            ScanAndCopyProgressionManager.stopWatch.Restart();
        }
        try
        {
            string[] fileNames = Directory.GetFiles(src, "*", SearchOption.TopDirectoryOnly);
            string[] directoryNames = Directory.GetDirectories(src, "*", SearchOption.TopDirectoryOnly);

            // Traversing the non-directory files
            foreach (string fileName in fileNames)
            {
                // If it's an image, store it and continue
                if (Utils.Contains(extensions, Path.GetExtension(fileName)))
                {
                    accu.Add(fileName);
                    continue;
                }
            }

            // Traversing the directories and making recursive calls
            foreach (string directoryName in directoryNames)
            {
                GetAllFilesRec(directoryName, extensions, accu);
            }
        }
        catch
        {
            Debug.Log("Cannot get some files");
        }
    }
}
