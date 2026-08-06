using System.Collections.Generic;

[System.Serializable]
public class SpumPackage
{
    public string Name;
    public string Path;
    public string Version;
    public string CreationDate;
    public List<SpumAnimationClip> SpumAnimationData =  new List<SpumAnimationClip>();
    public List<SpumTextureData> SpumTextureData = new List<SpumTextureData>();
}
