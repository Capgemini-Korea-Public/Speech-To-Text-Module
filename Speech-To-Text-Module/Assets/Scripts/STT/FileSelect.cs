using UnityEngine;
using UnityEditor;

public class FileSelect : MonoBehaviour
{
    // Directory���� ���� ����
    public void Select()
    {
        // Title, Directory ,File Type ������ ����
        string filePath = EditorUtility.OpenFilePanel("Select Audio File", "", "");

        // TODO :: ���� �������� üũ
        if(!string.IsNullOrEmpty(filePath))
            WhisperManager.Instance.SetFilePath(filePath);
        else
            Debug.LogError("File not Exist");
    }
}
