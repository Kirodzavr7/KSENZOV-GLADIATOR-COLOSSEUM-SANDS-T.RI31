using System.Collections.Generic;
using UnityEngine;

/// <summary>Проигрывает выбранные звуки из аудиопапки исходного архива.</summary>
public class SimpleCombatAudio : MonoBehaviour
{
    [SerializeField] private string resourceFolder = "Gladiators/Audio";
    [SerializeField, Range(0f, 1f)] private float volume = 0.8f;

    private readonly Dictionary<string, List<AudioClip>> _clips = new Dictionary<string, List<AudioClip>>();

    private void Awake()
    {
        foreach (AudioClip clip in Resources.LoadAll<AudioClip>(resourceFolder))
        {
            string group = clip.name;
            int lastUnderscore = group.LastIndexOf('_');
            if (lastUnderscore > 0 && int.TryParse(group.Substring(lastUnderscore + 1), out _))
                group = group.Substring(0, lastUnderscore);

            if (!_clips.TryGetValue(group, out List<AudioClip> list))
            {
                list = new List<AudioClip>();
                _clips.Add(group, list);
            }
            list.Add(clip);
        }

        if (_clips.Count == 0)
            Debug.LogWarning($"SimpleCombatAudio: не найдены AudioClip в Resources/{resourceFolder}", this);
    }

    private void OnEnable()
    {
        CoreCombatEvents.AttackStarted += HandleAttackStarted;
        CoreCombatEvents.Hit += HandleHit;
        CoreCombatEvents.BlockChanged += HandleBlockChanged;
        CoreCombatEvents.Died += HandleDied;
        CoreCombatEvents.StaminaFailed += HandleStaminaFailed;
    }

    private void OnDisable()
    {
        CoreCombatEvents.AttackStarted -= HandleAttackStarted;
        CoreCombatEvents.Hit -= HandleHit;
        CoreCombatEvents.BlockChanged -= HandleBlockChanged;
        CoreCombatEvents.Died -= HandleDied;
        CoreCombatEvents.StaminaFailed -= HandleStaminaFailed;
    }

    private void HandleAttackStarted(SimpleFighter fighter) => Play("swing_light", fighter.transform.position, 0.75f);

    private void HandleBlockChanged(SimpleFighter fighter)
    {
        if (fighter.IsBlocking)
            Play("shield_up", fighter.transform.position, 0.6f);
    }

    private void HandleHit(CoreHitInfo hit)
    {
        switch (hit.result)
        {
            case CoreHitResult.Damaged:
                Play("hit_flesh", hit.point, 0.9f);
                break;
            case CoreHitResult.Killed:
                Play("hit_heavy", hit.point, 1f);
                break;
            case CoreHitResult.Blocked:
                Play("block", hit.point, 0.85f);
                break;
        }
    }

    private void HandleDied(SimpleFighter fighter) => Play("death", fighter.transform.position, 1f);

    private void HandleStaminaFailed(SimpleFighter fighter)
    {
        if (fighter.Team == FighterTeam.Player)
            Play("no_stamina", fighter.transform.position, 0.7f);
    }

    private void Play(string group, Vector3 position, float gain)
    {
        if (!_clips.TryGetValue(group, out List<AudioClip> choices) || choices.Count == 0) return;
        AudioClip clip = choices[Random.Range(0, choices.Count)];
        AudioSource.PlayClipAtPoint(clip, position, volume * gain);
    }
}
