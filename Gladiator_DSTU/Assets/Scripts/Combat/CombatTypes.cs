using System;
using UnityEngine;

public enum CoreFighterState
{
    Free,
    Attacking,
    Blocking,
    Dead
}

public enum FighterTeam
{
    Player,
    Enemy
}

public enum CoreHitResult
{
    Damaged,
    Blocked,
    Killed,
    Ignored
}

public struct CoreHitInfo
{
    public SimpleFighter attacker;
    public SimpleFighter target;
    public float damage;
    public Vector3 direction;
    public Vector3 point;
    public CoreHitResult result;
}

/// <summary>Глобальные события боя для HUD, звука или эффектов. Все события статические.</summary>
public static class CoreCombatEvents
{
    public static event Action<SimpleFighter> AttackStarted;
    public static event Action<CoreHitInfo> Hit;
    public static event Action<SimpleFighter> BlockChanged;
    public static event Action<SimpleFighter> Died;
    public static event Action<SimpleFighter> StaminaFailed;

    internal static void RaiseAttackStarted(SimpleFighter fighter) => AttackStarted?.Invoke(fighter);
    internal static void RaiseHit(CoreHitInfo hit) => Hit?.Invoke(hit);
    internal static void RaiseBlockChanged(SimpleFighter fighter) => BlockChanged?.Invoke(fighter);
    internal static void RaiseDied(SimpleFighter fighter) => Died?.Invoke(fighter);
    internal static void RaiseStaminaFailed(SimpleFighter fighter) => StaminaFailed?.Invoke(fighter);

    /// <summary>Сброс статических подписок при перезагрузке домена/сцены.</summary>
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetEvents()
    {
        AttackStarted = null;
        Hit = null;
        BlockChanged = null;
        Died = null;
        StaminaFailed = null;
    }
}
