using OpenAI;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class Convertor : MonoBehaviour
{
    public void ConvertAudioToText()
    {
        string filePath = STTManager.Instance.FilePath;
        if (!File.Exists(filePath))
        {
            UnityEngine.Debug.LogError("File Not Exist: " + filePath);
            return;
        }

        if (!IsValidAudioFormat(filePath))
        {
            UnityEngine.Debug.LogError("Invalid File Extension: " + Path.GetExtension(filePath));
            return;
        }

        if (Path.GetExtension(filePath) == ".mp4")
        {
            string wavFilePath = ConvertToWav(filePath);
            if (wavFilePath != null)
            {
                STTManager.Instance.SetFilePath(wavFilePath);
            }
            else
            {
                UnityEngine.Debug.LogError("Fail Convert .mp4 File to .wav");
                return;
            }                
        }

        WhisperManager.Instance.AskWhisper();
    }


    #region Audio Verification
    private bool IsValidAudioFormat(string filePath)
    {
        UnityEngine.Debug.Log("����� ���� ���� ����");

        string extension = Path.GetExtension(filePath);
        return ExtensionMethods.whisperExtensions.Contains(extension);
    }
    #endregion

    #region Audio Processing
    private string ConvertToWav(string filePath)
    {
        UnityEngine.Debug.Log(".wav ���Ϸ� ��ȯ");

        string outputPath = Path.Combine(Application.dataPath, "AudioProcessings");
        string outputWavPath = Path.Combine(outputPath, Path.GetFileNameWithoutExtension(filePath) + ".wav");

        // ffmpeg.exe ���� ��ġ
        string ffmpegPath = Path.Combine(Application.dataPath, "Plugins/ffmpeg/bin/ffmpeg.exe");

        // ffmpeg ��ɾ� (mp4 ������ wav�� ��ȯ, whisper���� �����ϴ� ���÷���Ʈ 16000, ���ä�� -ac 1)
        string arguments = $"-i \"{filePath}\" -vn -acodec pcm_s16le -ar 16000 -ac 1 \"{outputWavPath}\"";

        // ProcessStartInfo ����
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = ffmpegPath, // ������ ���α׷�
            Arguments = arguments, // ������ ����
            RedirectStandardOutput = true, // �ܺ� ���μ����� output c#���� ��������
            RedirectStandardError = true, // �ܺ� ���μ����� ���� c#���� ��������
            UseShellExecute = false, // �� ��� ���ϰ� ����
            CreateNoWindow = true // â ���� ����
        };

        // ffmpeg ���μ��� ����
        Process process = new Process
        {
            StartInfo = startInfo
        };

        process.Start();
        process.WaitForExit(); // ��ȯ �Ϸ�� ������ ���

        UnityEngine.Debug.Log(outputWavPath);

        // ��ȯ �Ϸ� �� �޽��� ���
        if (File.Exists(outputWavPath))
        {
            UnityEngine.Debug.Log($"��ȯ �Ϸ�: {outputWavPath}");
            return outputWavPath;
        }
        else
        {
            UnityEngine.Debug.LogError("��ȯ ����");
            return null;
        }
    }
    #endregion
}
