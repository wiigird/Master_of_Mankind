using BaseLib.Patches.Content;
using BaseLib.Utils;
using MegaCrit.Sts2.Core.Entities.Players;

namespace Master_of_Mankind.Master_of_MankindCode.Combat;

internal static class PlayerCombatMemory
{
    private static readonly SavedSpireField<Player, int> RoomKey =
        new(() => int.MinValue, "MasterOfMankind_CombatRoomKey");

    private static readonly SavedSpireField<Player, int> ArmourLastTriggeredTurn =
        new(() => -1, "MasterOfMankind_ArmourLastTriggeredTurn");

    private static readonly SavedSpireField<Player, int[]> GoldenThronePreviousChoices =
        new(() => [], "MasterOfMankind_GoldenThronePreviousChoices")
        {
            Serializer = (choices, writer) =>
            {
                writer.WriteByte(checked((byte)choices.Length), bits: 8);
                foreach (int choice in choices)
                    writer.WriteInt(choice, bits: 32);
            },
            Deserializer = reader =>
            {
                int[] choices = new int[reader.ReadByte(bits: 8)];
                for (int i = 0; i < choices.Length; i++)
                    choices[i] = reader.ReadInt(bits: 32);
                return choices;
            }
        };

    private static readonly SavedSpireField<Player, int> MasterPowerLastTriggeredTurn =
        new(() => -1, "MasterOfMankind_PowerLastTriggeredTurn");

    private static readonly SavedSpireField<Player, int> WrathExecutedAttackTurn =
        new(() => -1, "MasterOfMankind_WrathExecutedAttackTurn");

    private static readonly SavedSpireField<Player, int> WrathExecutedAttacks =
        new(() => 0, "MasterOfMankind_WrathExecutedAttacks");

    public static bool ArmourTriggeredThisTurn(Player player, int turnNumber)
    {
        EnsureCurrentCombat(player);
        return ArmourLastTriggeredTurn.Get(player) == turnNumber;
    }

    public static void MarkArmourTriggered(Player player, int turnNumber)
    {
        EnsureCurrentCombat(player);
        ArmourLastTriggeredTurn.Set(player, turnNumber);
    }

    public static int GetGoldenThronePreviousChoice(Player player, int instanceIndex)
    {
        EnsureCurrentCombat(player);
        int[] choices = GoldenThronePreviousChoices.Get(player) ?? [];
        return instanceIndex >= 0 && instanceIndex < choices.Length ? choices[instanceIndex] : -1;
    }

    public static void SetGoldenThronePreviousChoice(Player player, int instanceIndex, int choice)
    {
        EnsureCurrentCombat(player);
        int[] current = GoldenThronePreviousChoices.Get(player) ?? [];
        int[] updated = current.Length > instanceIndex
            ? current.ToArray()
            : current.Concat(Enumerable.Repeat(-1, instanceIndex + 1 - current.Length)).ToArray();
        updated[instanceIndex] = choice;
        GoldenThronePreviousChoices.Set(player, updated);
    }

    public static bool MasterPowerTriggeredThisTurn(Player player, int turnNumber)
    {
        EnsureCurrentCombat(player);
        return MasterPowerLastTriggeredTurn.Get(player) == turnNumber;
    }

    public static void MarkMasterPowerTriggered(Player player, int turnNumber)
    {
        EnsureCurrentCombat(player);
        MasterPowerLastTriggeredTurn.Set(player, turnNumber);
    }

    public static void RecordExecutedAttack(Player player, int turnNumber)
    {
        EnsureCurrentCombat(player);
        if (WrathExecutedAttackTurn.Get(player) != turnNumber)
        {
            WrathExecutedAttackTurn.Set(player, turnNumber);
            WrathExecutedAttacks.Set(player, 0);
        }

        WrathExecutedAttacks.Set(player, WrathExecutedAttacks.Get(player) + 1);
    }

    public static int GetExecutedAttacks(Player player, int turnNumber)
    {
        EnsureCurrentCombat(player);
        return WrathExecutedAttackTurn.Get(player) == turnNumber
            ? WrathExecutedAttacks.Get(player)
            : 0;
    }

    private static void EnsureCurrentCombat(Player player)
    {
        int currentRoomKey = player.RunState.CurrentRoomCount;
        if (RoomKey.Get(player) == currentRoomKey)
            return;

        RoomKey.Set(player, currentRoomKey);
        ArmourLastTriggeredTurn.Set(player, -1);
        GoldenThronePreviousChoices.Set(player, []);
        MasterPowerLastTriggeredTurn.Set(player, -1);
        WrathExecutedAttackTurn.Set(player, -1);
        WrathExecutedAttacks.Set(player, 0);
    }
}
