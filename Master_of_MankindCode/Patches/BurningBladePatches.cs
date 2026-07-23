using System.Reflection;
using HarmonyLib;
using Master_of_Mankind.Master_of_MankindCode.Relics;
using Master_of_Mankind.Master_of_MankindCode.Cards.Rare;
using MegaCrit.Sts2.Core.Commands.Builders;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.ValueProps;

namespace Master_of_Mankind.Master_of_MankindCode.Patches;

internal static class BurningBladeRules
{
    public static bool BypassesSpecialDefense(
        Creature? target,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource)
    {
        return target != null
               && dealer is { IsPlayer: true }
               && target.Side != dealer.Side
               && cardSource?.Type == CardType.Attack
               && props.IsPoweredAttack()
               && dealer.Player is { } player
               && HasEitherBlade(player);
    }

    public static bool IsRadiantAttack(Creature? dealer, CardModel? cardSource)
    {
        return dealer is { IsPlayer: true }
               && cardSource?.Type == CardType.Attack
               && dealer.Player is { } player
               && player.GetRelic<RadiantBurningBlade>() != null;
    }

    private static bool HasEitherBlade(Player player)
    {
        return player.GetRelic<BurningBlade>() != null
               || player.GetRelic<RadiantBurningBlade>() != null;
    }
}

[HarmonyPatch]
internal static class BurningBladeMultiplierPatches
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(FlutterPower), nameof(FlutterPower.ModifyDamageMultiplicative));
        yield return AccessTools.Method(typeof(SoarPower), nameof(SoarPower.ModifyDamageMultiplicative));
        yield return AccessTools.Method(typeof(CoveredPower), nameof(CoveredPower.ModifyDamageMultiplicative));
        yield return AccessTools.Method(typeof(GuardedPower), nameof(GuardedPower.ModifyDamageMultiplicative));
        yield return AccessTools.Method(typeof(ColossusPower), nameof(ColossusPower.ModifyDamageMultiplicative));
    }

    private static bool Prefix(
        Creature? target,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        ref decimal __result)
    {
        if (!BurningBladeRules.BypassesSpecialDefense(target, props, dealer, cardSource))
            return true;

        __result = 1m;
        return false;
    }
}

[HarmonyPatch]
internal static class BurningBladeDamageCapPatches
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(IntangiblePower), nameof(IntangiblePower.ModifyDamageCap));
        yield return AccessTools.Method(typeof(HardToKillPower), nameof(HardToKillPower.ModifyDamageCap));
    }

    private static bool Prefix(
        Creature? target,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        ref decimal __result)
    {
        if (!BurningBladeRules.BypassesSpecialDefense(target, props, dealer, cardSource))
            return true;

        __result = decimal.MaxValue;
        return false;
    }
}

[HarmonyPatch]
internal static class BurningBladeHpLossAfterPatches
{
    private static IEnumerable<MethodBase> TargetMethods()
    {
        yield return AccessTools.Method(typeof(IntangiblePower), nameof(IntangiblePower.ModifyHpLostAfterOsty));
        yield return AccessTools.Method(typeof(SlipperyPower), nameof(SlipperyPower.ModifyHpLostAfterOsty));
        yield return AccessTools.Method(typeof(BufferPower), nameof(BufferPower.ModifyHpLostAfterOstyLate));
    }

    private static bool Prefix(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        ref decimal __result)
    {
        if (!BurningBladeRules.BypassesSpecialDefense(target, props, dealer, cardSource))
            return true;

        __result = amount;
        return false;
    }
}

[HarmonyPatch(typeof(HardenedShellPower), nameof(HardenedShellPower.ModifyHpLostBeforeOstyLate))]
internal static class BurningBladeHardenedShellPatch
{
    private static bool Prefix(
        Creature target,
        decimal amount,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        ref decimal __result)
    {
        if (!BurningBladeRules.BypassesSpecialDefense(target, props, dealer, cardSource))
            return true;

        __result = amount;
        return false;
    }
}

[HarmonyPatch(typeof(WeakPower), nameof(WeakPower.ModifyDamageMultiplicative))]
internal static class RadiantBurningBladeWeakPatch
{
    private static bool Prefix(
        Creature? target,
        ValueProp props,
        Creature? dealer,
        CardModel? cardSource,
        ref decimal __result)
    {
        if (!BurningBladeRules.IsRadiantAttack(dealer, cardSource) || !props.IsPoweredAttack())
            return true;

        __result = 1m;
        return false;
    }
}

[HarmonyPatch(typeof(AttackCommand), nameof(AttackCommand.Execute))]
internal static class RadiantBurningBladeBlockPatch
{
    private static readonly AccessTools.FieldRef<AttackCommand, ValueProp> DamagePropsRef =
        AccessTools.FieldRefAccess<AttackCommand, ValueProp>("<DamageProps>k__BackingField");

    private static void Prefix(AttackCommand __instance)
    {
        if (__instance.ModelSource is not CardModel card
            || (!BurningBladeRules.IsRadiantAttack(__instance.Attacker, card)
                && card is not TheEmperorsSword))
            return;

        DamagePropsRef(__instance) |= ValueProp.Unblockable;
    }
}
