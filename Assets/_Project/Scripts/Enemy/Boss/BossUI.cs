using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BossUI : MonoBehaviour
{
    [SerializeField] private GameObject container;
    [SerializeField] private GameObject hpBar;
    [SerializeField] private TextMeshProUGUI bossNameText;

    private BossEnemy boss;

    public void Initialize(BossEnemy boss)
    {
        this.boss = boss;
        boss.OnHpChanged += UpdateHpBar;
        boss.OnDeath += OnBossDied;

        if (bossNameText != null)
        {
            bossNameText.text = boss.gameObject.name;
        }

        UpdateHpBar(boss.HpRatio);
        Show();
    }

    public void Show()
    {
        if (container != null) container.SetActive(true);
    }

    public void Hide()
    {
        if (container != null) container.SetActive(false);
    }

    private void UpdateHpBar(float ratio)
    {
        if (hpBar != null)
        {
            hpBar.transform.localScale = new Vector3(ratio, 1, 1);
        }
    }

    private void OnBossDied(Enemy enemy)
    {
        Hide();
    }

    void OnDestroy()
    {
        if (!ReferenceEquals(boss, null))
        {
            boss.OnHpChanged -= UpdateHpBar;
            boss.OnDeath -= OnBossDied;
        }
    }
}
