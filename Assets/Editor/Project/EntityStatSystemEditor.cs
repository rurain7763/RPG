using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EntityStatSystem))]
public class EntityStatSystemEditor : Editor
{
    private void OnEnable()
    {
        EditorApplication.update += Repaint;
    }

    private void OnDisable()
    {
        EditorApplication.update -= Repaint;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        if (Application.isPlaying)
        {
            EntityStatSystem statSystem = (EntityStatSystem)target;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Runtime Stats", EditorStyles.boldLabel);
            EditorGUILayout.LabelField("Max Health: " + statSystem.Health.FinalValue);
            EditorGUILayout.LabelField("Health Regeneration: " + statSystem.HealthRegeneration.FinalValue);
            EditorGUILayout.LabelField("Strength: " + statSystem.Strength.FinalValue);
            EditorGUILayout.LabelField("Agility: " + statSystem.Agility.FinalValue);
            EditorGUILayout.LabelField("Intelligence: " + statSystem.Intelligence.FinalValue);
            EditorGUILayout.LabelField("Vitality: " + statSystem.Vitality.FinalValue);
            EditorGUILayout.LabelField("Damage: " + statSystem.Damage.FinalValue);
            EditorGUILayout.LabelField("Crit Power: " + statSystem.CritPower.FinalValue);
            EditorGUILayout.LabelField("Crit Rate: " + statSystem.CritRate.FinalValue);
            EditorGUILayout.LabelField("Armor Reduction: " + statSystem.ArmorReduction.FinalValue);
            EditorGUILayout.LabelField("Fire Damage: " + statSystem.FireDamage.FinalValue);
            EditorGUILayout.LabelField("Ice Damage: " + statSystem.IceDamage.FinalValue);
            EditorGUILayout.LabelField("Lightning Damage: " + statSystem.LightningDamage.FinalValue);
            EditorGUILayout.LabelField("Armor: " + statSystem.Armor.FinalValue);
            EditorGUILayout.LabelField("Evasion: " + statSystem.Evasion.FinalValue);
            EditorGUILayout.LabelField("Fire Resistance: " + statSystem.FireResistance.FinalValue);
            EditorGUILayout.LabelField("Ice Resistance: " + statSystem.IceResistance.FinalValue);
            EditorGUILayout.LabelField("Lightning Resistance: " + statSystem.LightningResistance.FinalValue);
            EditorGUILayout.LabelField("Move Speed: " + statSystem.MoveSpeed.FinalValue);
            EditorGUILayout.LabelField("Attack Speed: " + statSystem.AttackSpeed.FinalValue);
        }

        if (GUI.changed)
        {
            EditorUtility.SetDirty(target);
        }
    }
}