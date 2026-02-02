public struct OptionSystemInitData
{
    public float BGMVolume;
    public float SFXVolume;
}

public class OptionSystem
{
    private float bgmVolume = 1.0f;
    private float sfxVolume = 1.0f;

    public float BGMVolume
    {
        get => bgmVolume;
        set => bgmVolume = value;
    }

    public float SFXVolume
    {
        get => sfxVolume;
        set => sfxVolume = value;
    }

    public OptionSystem(OptionSystemInitData? initData = null)
    {
        if (initData.HasValue)
        {
            bgmVolume = initData.Value.BGMVolume;
            sfxVolume = initData.Value.SFXVolume;
        }
    }
}