using System.ComponentModel.Design.Serialization;
using System.Reflection.Metadata;
using Godot;
using wizardballz.spells;

namespace wizardballz.world.Network;

public partial class NetworkSpellManager : Node3D
{
    [Export] private SpellManager SpellManager = null!;
    [Export] private GameMatch GameMatch = null!;

    public void TellSpellCast(SpellRecord record, Vector3 targetPosition)
    {
        if (record.Target == SpellRecord.TargetType.Ball) {
            targetPosition = GameMatch.Ball.GlobalPosition;
        }

        var spellId = (ulong)GD.Randi();
        var resourcePath = record.ResourcePath;
        Rpc(MethodName.SpellcastRequested, resourcePath, spellId, targetPosition);
    }
    
    [Rpc(MultiplayerApi.RpcMode.AnyPeer, CallLocal = true, TransferMode = MultiplayerPeer.TransferModeEnum.Reliable)]
    private void SpellcastRequested(string spellResourcePath, ulong spellId, Vector3 position)
    {
        var senderId = (ulong)Multiplayer.GetRemoteSenderId();
        var player = GameMatch.PlayerFromClientId(senderId);

        GD.Print($"RPC: Spell cast from {senderId}");
        
        var spellRecord = GD.Load<SpellRecord>(spellResourcePath);
        var spell = SpellManager.BeginSpellCharge(spellRecord, player, position, spellId);
    }
}