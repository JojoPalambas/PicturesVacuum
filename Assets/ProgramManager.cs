using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Crosstales.FB;

public class ProgramManager : MonoBehaviour
{
    public InputField srcInput;
    public InputField dstInput;
    public InputField extensionsInput;

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
        Debug.Log("Src folder: " + srcInput.text);
        Debug.Log("Dst folder: " + dstInput.text);
        Debug.Log("Extensions: " + extensionsInput.text);
    }
}
