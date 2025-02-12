using UnityEngine;
using UnityEditor;

public class FileSelector : MonoBehaviour
{
    // Directory���� ���� ����
    public void FileSelect()
    {
        // Title, Directory, File Type ����
        string filePath = EditorUtility.OpenFilePanel("Select Audio File", "", "");

        if(!string.IsNullOrEmpty(filePath))
            STTManager.Instance.SetFilePath(filePath);
        else
            Debug.LogError("Invalid File");
    }
}
