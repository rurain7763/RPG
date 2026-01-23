using TMPro;
using UnityEngine;

public class EnemyHpDisplayer : MonoBehaviour
{
    [SerializeField, Reference("Hpbar/Text_Name")] private TMP_Text nameText;
    [SerializeField, Reference("Hpbar")] private HpBar hpBar;

    public ICombatable Owner { get; private set; }

    public void Begin(ICombatable owner, string displayName)
    {
        Owner = owner;

        owner.CombatSystem.OnHealthChanged += HandleHpChanged;
        nameText.text = displayName;
        HandleHpChanged();
    }

    private void HandleHpChanged()
    {
        hpBar.SetHp(Owner.CombatSystem.CurrentHealth, Owner.CombatSystem.MaxHealth);
    }

    public void End()
    {
        if (Owner != null)
        {
            Owner.CombatSystem.OnHealthChanged -= HandleHpChanged;
            Owner = null;
        }
    }
}