using UnityEngine;
using UnityEngine.UI;

public class AudioButton : MonoBehaviour
{
    public AudioManager audioManager;
    public Image icon;
    public Sprite onSprite;
    public Sprite offSprite;

    public bool isMusicButton = true;

    public void OnClick()
    {
        if (isMusicButton)
        {
            audioManager.ToggleMusic();
            icon.sprite = audioManager.isMusicOn ? onSprite : offSprite;
        }
        else
        {
            audioManager.ToggleSFX();
            icon.sprite = audioManager.isSfxOn ? onSprite : offSprite;
        }
    }
}
