using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 피격 시 화면 가장자리에 빨간 플래시를 표시하는 컴포넌트.
/// flashImage에 스프라이트가 없으면 비네트 텍스처를 런타임에 자동 생성한다.
/// DamageFlashListener가 Flash()를 호출하여 효과를 시작한다.
/// </summary>
public class DamageFlash : MonoBehaviour
{
    [SerializeField] private Image flashImage;
    [SerializeField] private float maximumIntensity = 1f;

    private float flashIntensity;
    private float flashDuration;
    private float flashTimeRemaining;

    void Start()
    {
        if (flashImage != null && flashImage.sprite == null)
        {
            flashImage.sprite = GenerateVignetteSprite();
        }

        if (flashImage != null)
        {
            flashImage.color = new Color(1f, 0f, 0f, 0f);
        }
    }

    void Update()
    {
        if (flashImage == null) return;

        if (flashTimeRemaining <= 0f)
        {
            flashImage.color = new Color(1f, 0f, 0f, 0f);
            return;
        }

        flashTimeRemaining -= Time.unscaledDeltaTime;

        float normalizedTime = Mathf.Clamp01(flashTimeRemaining / flashDuration);
        float decay = normalizedTime * normalizedTime;
        float alpha = flashIntensity * decay;

        flashImage.color = new Color(1f, 0f, 0f, alpha);
    }

    /// <summary>
    /// 피격 플래시를 시작한다. 현재 남은 플래시보다 강한 경우에만 덮어쓴다.
    /// </summary>
    /// <param name="intensity">플래시 강도 (maximumIntensity로 클램프)</param>
    /// <param name="duration">지속 시간(초)</param>
    public void Flash(float intensity, float duration)
    {
        if (duration <= 0f) return;
        intensity = Mathf.Min(intensity, maximumIntensity);

        if (flashTimeRemaining > 0f)
        {
            float remainingStrength = flashIntensity * (flashTimeRemaining / flashDuration);
            if (intensity <= remainingStrength) return;
        }

        flashIntensity = intensity;
        flashDuration = duration;
        flashTimeRemaining = duration;
    }

    private Sprite GenerateVignetteSprite()
    {
        int size = 512;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[size * size];
        float center = size * 0.5f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float normalizedX = (x - center) / center;
                float normalizedY = (y - center) / center;
                float distance = Mathf.Sqrt(normalizedX * normalizedX + normalizedY * normalizedY);
                float alpha = Mathf.Clamp01((distance - 0.5f) / 0.5f);
                alpha = alpha * alpha;
                pixels[y * size + x] = new Color(1f, 0f, 0f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }
}
