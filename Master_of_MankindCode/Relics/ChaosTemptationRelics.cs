using BaseLib.Hooks;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Entities.Relics;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models.Powers;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;

namespace Master_of_Mankind.Master_of_MankindCode.Relics;

public abstract class ChaosTemptationRelic : Master_of_MankindRelic
{
    protected abstract int SacredNumber { get; }

    public override RelicRarity Rarity => RelicRarity.Event;
    public override bool HasUponPickupEffect => true;
    public override bool IsAllowed(IRunState runState) => false;

    public override Task AfterObtained() =>
        CreatureCmd.LoseMaxHp(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            SacredNumber,
            isFromCard: false);
}

public sealed class SealOfKhorne : ChaosTemptationRelic
{
    protected override int SacredNumber => 8;

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not CombatRoom)
            return;

        Flash();
        await PowerCmd.Apply<VigorPower>(
            new ThrowingPlayerChoiceContext(),
            Owner.Creature,
            8m,
            Owner.Creature,
            null);
    }
}

public sealed class SealOfTzeentch : ChaosTemptationRelic
{
    protected override int SacredNumber => 9;

    public override decimal ModifyHandDraw(Player player, decimal count)
    {
        if (!ReferenceEquals(player, Owner)
            || player.PlayerCombatState is not { } combatState
            || combatState.TurnNumber > 1)
            return count;

        return Math.Max(count, 9m);
    }
}

public sealed class SealOfNurgle : ChaosTemptationRelic
{
    protected override int SacredNumber => 7;

    public override async Task AfterRoomEntered(AbstractRoom room)
    {
        if (room is not CombatRoom)
            return;

        Flash();
        await CreatureCmd.Heal(Owner.Creature, 7m);
    }
}

public sealed class SealOfSlaanesh : ChaosTemptationRelic, IMaxHandSizeModifier
{
    protected override int SacredNumber => 6;

    public int ModifyMaxHandSize(Player player, int currentMaxHandSize) =>
        ReferenceEquals(player, Owner) ? currentMaxHandSize + 6 : currentMaxHandSize;
}
