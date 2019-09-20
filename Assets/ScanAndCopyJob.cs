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

    // Publishes the progression of the scanning phase
    public static void PublishScanProgression(int scanned)
    {
        ProgressionDisplayManager.instance.SetScanProgression(scanned);
    }

    // Publishes the progression of the size estimation phase
    public static void PublishMeasureProgression(int measured)
    {
        ProgressionDisplayManager.instance.SetMeasureProgression(measured);
    }

    // Publishes the progression of the copy phase
    public static void PublishCopyProgression(int copied, int failed)
    {
        ProgressionDisplayManager.instance.SetCopyProgression(copied, failed);
    }

    // Checks whether the program is asked to stop or not
    public static bool ShouldStop()
    {
        return ProgressionDisplayManager.instance.ShouldStop();
    }
}

public struct ScanAndCopyJob : IJob
{
    public NativeArray<char> jobSrcPathAttribute;
    public NativeArray<char> jobDstPathAttribute;
    public NativeArray<char> jobExtensionsAttribute;

    public void Execute()
    {
        ProgressionDisplayManager.instance.Init();

        // Manages the progression measure
        ScanAndCopyProgressionManager.stopWatch = new Stopwatch();
        ScanAndCopyProgressionManager.stopWatch.Start();

        // Build a refined form of the inputs
        string srcPath = Utils.CharNativeArrayToString(jobSrcPathAttribute);
        string dstPath = Utils.CharNativeArrayToString(jobDstPathAttribute);
        string[] extensions = Utils.CharNativeArrayToString(jobExtensionsAttribute).Split(' ');
        for (int i = 0; i < extensions.Length; i++)
            extensions[i] = '.' + extensions[i];

        // Builds the list of all the files to copy

        List<string> fileNames = null;
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

        ScanAndCopyProgressionManager.PublishScanProgression(fileNames.Count);
        Debug.Log("Count: " + fileNames.Count.ToString());

        ScanAndCopyProgressionManager.stopWatch.Restart();
        

        // Checks if there is enough free space on the drive

        DirectoryInfo directoryInfo = new DirectoryInfo(dstPath);
        DriveInfo driveInfo = new DriveInfo(directoryInfo.Root.ToString());
        long availableDstSpace = driveInfo.AvailableFreeSpace * 100 / 115;
        Debug.Log("AvailableFreeSpace: " + driveInfo.AvailableFreeSpace);
        Debug.Log("AvailableFreeSpace with security: " + availableDstSpace);

        long neededSpace = 0;

        for (int i = 0; i < fileNames.Count; i++)
        {
            if (ScanAndCopyProgressionManager.stopWatch.ElapsedMilliseconds > 1000)
            {
                ScanAndCopyProgressionManager.PublishMeasureProgression(i);
                ScanAndCopyProgressionManager.stopWatch.Restart();
            }

            FileInfo currentFileInfo = new FileInfo(fileNames[i]);
            neededSpace += currentFileInfo.Length;
        }

        ScanAndCopyProgressionManager.PublishMeasureProgression(fileNames.Count);

        Debug.Log("NeededSpace: " + neededSpace);
        neededSpace = neededSpace * 115 / 100;
        Debug.Log("NeededSpace with security: " + neededSpace);

        if (neededSpace > availableDstSpace)
        {
            Debug.Log("Copy impossible ; not enough space: need " + neededSpace.ToString() + ", have " + availableDstSpace);
            ProgressionDisplayManager.instance.SetErrorCode(4);
            return;
        }

        ScanAndCopyProgressionManager.stopWatch.Restart();

        // Copies all the files
        int failed = 0;
        for (int i = 0; i < fileNames.Count; i++)
        {
            if (ScanAndCopyProgressionManager.stopWatch.ElapsedMilliseconds > 1000)
            {
                if (ScanAndCopyProgressionManager.ShouldStop())
                    return;

                ScanAndCopyProgressionManager.PublishCopyProgression(i - failed, failed);
                ScanAndCopyProgressionManager.stopWatch.Restart();
            }

            try
            {
                FileInfo currentFileInfo = new FileInfo(fileNames[i]);

                string[] splittedFilePath = fileNames[i].Split('\\');
                string newDirectoryName = dstPath + '\\';
                for (int j = 1; j < splittedFilePath.Length - 1; j++)
                    newDirectoryName += '_' + splittedFilePath[j];

                if (!Directory.Exists(newDirectoryName))
                    Directory.CreateDirectory(newDirectoryName);

                File.Copy(fileNames[i], newDirectoryName + "\\" + currentFileInfo.Name);
            }
            catch
            {
                failed++;
                Debug.Log("Could not copy " + fileNames[i]);
            }
        }

        ScanAndCopyProgressionManager.PublishCopyProgression(fileNames.Count - failed, failed);
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
            if (ScanAndCopyProgressionManager.ShouldStop())
                return;

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
