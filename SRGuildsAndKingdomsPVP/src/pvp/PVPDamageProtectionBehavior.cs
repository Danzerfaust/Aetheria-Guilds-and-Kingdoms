using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Server;
using Vintagestory.API.Config;

namespace SRGuildsAndKingdomsPVP.src.pvp
{
    /// <summary>
    /// Entity behavior that prevents PVP damage when players don't have PVP enabled
    /// </summary>
    public class PVPDamageProtectionBehavior : EntityBehavior
    {
        private readonly PVPManager pvpManager;
        private readonly ICoreServerAPI sapi;

        public PVPDamageProtectionBehavior(Entity entity, PVPManager pvpManager, ICoreServerAPI sapi) : base(entity)
        {
            this.pvpManager = pvpManager;
            this.sapi = sapi;
        }

        public override string PropertyName() => "pvpdamageprotection";

        public override void OnEntityReceiveDamage(DamageSource damageSource, ref float damage)
        {
            // Only process player entities
            if (entity is not EntityPlayer victim)
            {
                base.OnEntityReceiveDamage(damageSource, ref damage);
                return;
            }

            // Only process damage from other players
            if (damageSource.SourceEntity is not EntityPlayer attacker)
            {
                base.OnEntityReceiveDamage(damageSource, ref damage);
                return;
            }

            // Get player UIDs
            var victimPlayer = victim.Player as IServerPlayer;
            var attackerPlayer = attacker.Player as IServerPlayer;

            if (victimPlayer == null || attackerPlayer == null)
            {
                base.OnEntityReceiveDamage(damageSource, ref damage);
                return;
            }

            // Check if PVP is allowed between these players
            if (!pvpManager.CanPlayersAttackEachOther(attackerPlayer.PlayerUID, victimPlayer.PlayerUID))
            {
                // Cancel damage by setting it to 0
                damage = 0f;

                // Send feedback messages
                SendPVPFeedback(attackerPlayer, victimPlayer);
            }

            base.OnEntityReceiveDamage(damageSource, ref damage);
        }

        private void SendPVPFeedback(IServerPlayer attacker, IServerPlayer victim)
        {
            // Check if players are in a duel
            if (pvpManager.DuelManager != null && pvpManager.DuelManager.IsPlayerInDuel(attacker.PlayerUID))
            {
                sapi.SendMessage(attacker, 0,
                    "You can only attack your duel opponent!",
                    EnumChatType.Notification);
                return;
            }

            var attackerInGuild = pvpManager.IsPlayerInGuild(attacker.PlayerUID);
            var victimInGuild = pvpManager.IsPlayerInGuild(victim.PlayerUID);

            // Determine why PVP was blocked
            if (!attackerInGuild)
            {
                sapi.SendMessage(attacker, 0,
                    "You must be in a guild to engage in PVP combat. Or challenge them to a duel with /duel <playername>",
                    EnumChatType.Notification);
            }
            else if (!victimInGuild)
            {
                sapi.SendMessage(attacker, 0,
                    $"{victim.PlayerName} is not in a guild and cannot be attacked. You can challenge them to a duel with /duel {victim.PlayerName}",
                    EnumChatType.Notification);
            }
            else
            {
                var attackerGuild = pvpManager.GetPlayerGuildName(attacker.PlayerUID);
                var victimGuild = pvpManager.GetPlayerGuildName(victim.PlayerUID);

                if (attackerGuild == victimGuild)
                {
                    sapi.SendMessage(attacker, 0,
                        "You cannot attack members of your own guild! Challenge them to a duel instead with /duel <playername>",
                        EnumChatType.Notification);
                }
                else if (!pvpManager.IsPVPEnabled(attacker.PlayerUID))
                {
                    sapi.SendMessage(attacker, 0,
                        "You must enable PVP to attack other players. Use /pvp to enable it.",
                        EnumChatType.Notification);
                }
                else if (!pvpManager.IsPVPEnabled(victim.PlayerUID))
                {
                    sapi.SendMessage(attacker, 0,
                        $"{victim.PlayerName} has not enabled PVP and cannot be attacked.",
                        EnumChatType.Notification);
                }
            }
        }
    }
}
