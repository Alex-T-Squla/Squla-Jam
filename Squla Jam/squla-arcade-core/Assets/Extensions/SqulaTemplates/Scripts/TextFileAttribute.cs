using UnityEngine;

public class TextFileAttribute : PropertyAttribute
{
    public readonly string relativePath;
    public readonly string fileType;

    public TextFileAttribute(string relativePath, string fileType)
    {
        this.relativePath = relativePath;
        this.fileType = fileType;
    }
}
