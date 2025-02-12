using System.Diagnostics;
using System.IO;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class Convertor : MonoBehaviour
{
    [SerializeField, Range(0,100)] private int denoiseIntensity = 30;
    public void Convert()
    {
        ConvertAudioToText();
    }

    private async UniTask ConvertAudioToText()
    {
        string filePath = STTManager.Instance.FilePath;
        // ���� �����ϴ��� �˻�
        if (!File.Exists(filePath))
        {
            UnityEngine.Debug.LogError("File Not Exist: " + filePath);
            return;
        }

        // ��ȿ�� Ȯ�������� �˻�
        if (!IsValidAudioFormat(filePath))
        {
            UnityEngine.Debug.LogError("Invalid File Extension: " + Path.GetExtension(filePath));
            return;
        }

        // .mp4�����̸� .wav���Ϸ� ��ȯ
        if (Path.GetExtension(filePath) == ".mp4")
        {
            string wavFilePath = await ConvertToWav(filePath);
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

        // ������ ����
        string noiseRemovedFilePath = await RemoveNoise(filePath);
        if (noiseRemovedFilePath != null)
        {
            STTManager.Instance.SetFilePath(noiseRemovedFilePath);
        }
        else
        {
            UnityEngine.Debug.LogError("Fail to Remove Noise");
        }

        // ���̰� ��� ������


        // Processed Audio�� Text�� ��ȯ
        await WhisperManager.Instance.AskWhisper();
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
    private async UniTask<bool> ExecuteFFmpegProcess(string argument, string outputPath)
    {
        // ffmpeg.exe ���� ��ġ
        string ffmpegPath = Path.Combine(Application.dataPath, "Plugins/ffmpeg/bin/ffmpeg.exe");

        // ProcessStartInfo ����
        ProcessStartInfo startInfo = new ProcessStartInfo()
        {
            FileName = ffmpegPath, // ������ ���α׷�
            Arguments = argument, // ������ ����
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

        if (File.Exists(outputPath))
        {
            UnityEngine.Debug.Log($"Execute Success: {outputPath}");
            return true;
        }
        else
        {
            UnityEngine.Debug.LogError("Execute Failed");
            return false;
        }
    }

    private async UniTask<string> ConvertToWav(string filePath)
    {
        UnityEngine.Debug.Log(".wav ���Ϸ� ��ȯ");

        string directoryPath = Path.Combine(Application.dataPath, "AudioProcessings");
        string baseFileName = Path.GetFileNameWithoutExtension(filePath);
        string outputPath = Path.Combine(directoryPath, baseFileName + ".wav");

        int count = 1;
        while (File.Exists(outputPath))
        {
            string newFileName = baseFileName + count.ToString(); 
            outputPath = Path.Combine(directoryPath, newFileName + ".wav");
            count++;  
        }

        // ffmpeg ��ɾ� (mp4 ������ wav�� ��ȯ, whisper���� �����ϴ� ���÷���Ʈ 16000, ���ä�� -ac 1)
        string arguments = $"-i \"{filePath}\" -vn -acodec pcm_s16le -ar 16000 -ac 1 \"{outputPath}\"";

        if (await ExecuteFFmpegProcess(arguments, outputPath))
            return outputPath;
        else
            return null;
    }

    private async UniTask<string> RemoveNoise(string filePath)
    {
        UnityEngine.Debug.Log("������ ����");

        string directoryPath = Path.Combine(Application.dataPath, "AudioProcessings");
        string baseFileName = Path.GetFileNameWithoutExtension(filePath);
        string outputPath = Path.Combine(directoryPath, baseFileName + Path.GetExtension(filePath));

        int count = 1;
        while (File.Exists(outputPath))
        {
            string newFileName = baseFileName + count.ToString();
            outputPath = Path.Combine(directoryPath, newFileName + Path.GetExtension(filePath));
            count++;
        }

        // ffmpeg ��ɾ� (noise ����)
        string arguments = $"-i \"{filePath}\" -vf noise=alls={denoiseIntensity}:allf=t \"{outputPath}\"";

        if (await ExecuteFFmpegProcess(arguments, outputPath))
            return outputPath;
        else
            return null;
    }

    #endregion
}
