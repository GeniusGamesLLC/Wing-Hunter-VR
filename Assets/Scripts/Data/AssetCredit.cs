using UnityEngine;

[System.Serializable]
public struct AssetCredit
{
    [Header("Asset Information")]
    public string assetName;
    public string author;
    public string license;
    public string source;
    
    [Header("Attribution")]
    [TextArea(3, 5)]
    public string attributionText;
    
    public AssetCredit(string assetName, string author, string license, string source, string attributionText)
    {
        this.assetName = assetName;
        this.author = author;
        this.license = license;
        this.source = source;
        this.attributionText = attributionText;
    }
}