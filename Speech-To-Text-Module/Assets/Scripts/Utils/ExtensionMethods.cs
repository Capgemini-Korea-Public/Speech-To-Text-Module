using System.Collections.Generic;

public static class ExtensionMethods
{
    // whisper���� �����Ǵ� ����� Ȯ���� ���
    public static readonly HashSet<string> whisperExtensions = new HashSet<string>
    {
        ".mp3", ".mp4", ".mpeg", ".mpga", ".m4a", ".wav", ".webm", "flac",  "oga", "ogg"
    };
}