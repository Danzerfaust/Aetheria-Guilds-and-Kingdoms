using System;
using System.Collections.Generic;
using System.Linq;
using Vintagestory.API.Server;
using Vintagestory.API.Config;

namespace SOAGuildsAndKingdomsPVP.src.pvp
{
    /// <summary>
    /// Manages duel challenges and active duels between players
    /// </summary>
    public class DuelManager
    {
        private readonly ICoreServerAPI api;
        private readonly Dictionary<string, DuelChallenge> pendingChallenges;
        private readonly Dictionary<string, ActiveDuel> activeDuels;
        private const int CHALLENGE_TIMEOUT_SECONDS = 60;

        public DuelManager(ICoreServerAPI api)
        {
            this.api = api;
            this.pendingChallenges = new Dictionary<string, DuelChallenge>();
            this.activeDuels = new Dictionary<string, ActiveDuel>();
        }

        /// <summary>
        /// Challenge another player to a duel
        /// </summary>
        public bool CreateChallenge(string challengerUid, string targetUid, out string message)
        {
            // Players cannot duel themselves
            if (challengerUid == targetUid)
            {
                message = "You cannot duel yourself!";
                return false;
            }

            // Check if challenger is already in a duel
            if (IsPlayerInDuel(challengerUid))
            {
                message = "You are already in a duel!";
                return false;
            }

            // Check if target is already in a duel
            if (IsPlayerInDuel(targetUid))
            {
                var targetPlayer = api.World.PlayerByUid(targetUid);
                message = $"{targetPlayer?.PlayerName ?? "That player"} is already in a duel!";
                return false;
            }

            // Check if there's already a pending challenge between these players
            string challengeKey = GetChallengeKey(challengerUid, targetUid);
            if (pendingChallenges.ContainsKey(challengeKey))
            {
                message = "You already have a pending challenge with this player!";
                return false;
            }

            // Check if target has a pending challenge TO the challenger (they can accept instead)
            string reverseChallengeKey = GetChallengeKey(targetUid, challengerUid);
            if (pendingChallenges.ContainsKey(reverseChallengeKey))
            {
                message = "This player has already challenged you! Use /duel accept to accept their challenge.";
                return false;
            }

            // Get player names
            var challenger = api.World.PlayerByUid(challengerUid);
            var target = api.World.PlayerByUid(targetUid);

            if (target == null)
            {
                message = "Target player not found!";
                return false;
            }

            // Create challenge
            var challenge = new DuelChallenge
            {
                ChallengerUid = challengerUid,
                ChallengerName = challenger?.PlayerName ?? "Unknown",
                TargetUid = targetUid,
                TargetName = target.PlayerName,
                ChallengeTime = api.World.Calendar.TotalHours
            };

            pendingChallenges[challengeKey] = challenge;

            message = $"Duel challenge sent to {target.PlayerName}! They have {CHALLENGE_TIMEOUT_SECONDS} seconds to accept.";
            return true;
        }

        /// <summary>
        /// Accept a duel challenge
        /// </summary>
        public bool AcceptChallenge(string accepterUid, out string message, out string challengerUid)
        {
            challengerUid = string.Empty;

            // Find pending challenges where this player is the target
            var challenge = pendingChallenges.Values.FirstOrDefault(c => c.TargetUid == accepterUid);

            if (challenge == null)
            {
                message = "You don't have any pending duel challenges!";
                return false;
            }

            // Check if challenge has expired
            if (HasChallengeExpired(challenge))
            {
                string key = GetChallengeKey(challenge.ChallengerUid, challenge.TargetUid);
                pendingChallenges.Remove(key);
                message = "That duel challenge has expired!";
                return false;
            }

            // Check if either player is already in a duel
            if (IsPlayerInDuel(challenge.ChallengerUid))
            {
                message = $"{challenge.ChallengerName} is already in another duel!";
                return false;
            }

            if (IsPlayerInDuel(accepterUid))
            {
                message = "You are already in a duel!";
                return false;
            }

            // Create active duel
            var duel = new ActiveDuel
            {
                Player1Uid = challenge.ChallengerUid,
                Player1Name = challenge.ChallengerName,
                Player2Uid = challenge.TargetUid,
                Player2Name = challenge.TargetName,
                StartTime = api.World.Calendar.TotalHours
            };

            // Add duel for both players
            activeDuels[challenge.ChallengerUid] = duel;
            activeDuels[challenge.TargetUid] = duel;

            // Remove pending challenge
            string challengeKey = GetChallengeKey(challenge.ChallengerUid, challenge.TargetUid);
            pendingChallenges.Remove(challengeKey);

            challengerUid = challenge.ChallengerUid;
            message = $"Duel accepted! You are now fighting {challenge.ChallengerName}!";
            return true;
        }

        /// <summary>
        /// Decline a duel challenge
        /// </summary>
        public bool DeclineChallenge(string declinerUid, out string message, out string challengerUid)
        {
            challengerUid = string.Empty;

            // Find pending challenges where this player is the target
            var challenge = pendingChallenges.Values.FirstOrDefault(c => c.TargetUid == declinerUid);

            if (challenge == null)
            {
                message = "You don't have any pending duel challenges!";
                return false;
            }

            // Remove pending challenge
            string challengeKey = GetChallengeKey(challenge.ChallengerUid, challenge.TargetUid);
            pendingChallenges.Remove(challengeKey);

            challengerUid = challenge.ChallengerUid;
            message = $"You declined the duel challenge from {challenge.ChallengerName}.";
            return true;
        }

        /// <summary>
        /// End a duel (called when a player dies or surrenders)
        /// </summary>
        public void EndDuel(string playerUid, out string opponentUid, out string opponentName)
        {
            opponentUid = string.Empty;
            opponentName = string.Empty;

            if (!activeDuels.TryGetValue(playerUid, out var duel))
                return;

            // Get opponent info
            if (duel.Player1Uid == playerUid)
            {
                opponentUid = duel.Player2Uid;
                opponentName = duel.Player2Name;
            }
            else
            {
                opponentUid = duel.Player1Uid;
                opponentName = duel.Player1Name;
            }

            // Remove duel for both players
            activeDuels.Remove(duel.Player1Uid);
            activeDuels.Remove(duel.Player2Uid);
        }

        /// <summary>
        /// Check if a player is currently in a duel
        /// </summary>
        public bool IsPlayerInDuel(string playerUid)
        {
            return activeDuels.ContainsKey(playerUid);
        }

        /// <summary>
        /// Check if two players are dueling each other
        /// </summary>
        public bool ArePlayersDueling(string player1Uid, string player2Uid)
        {
            if (!activeDuels.TryGetValue(player1Uid, out var duel))
                return false;

            return (duel.Player1Uid == player1Uid && duel.Player2Uid == player2Uid) ||
                   (duel.Player1Uid == player2Uid && duel.Player2Uid == player1Uid);
        }

        /// <summary>
        /// Get pending challenge for a player (as target)
        /// </summary>
        public DuelChallenge? GetPendingChallenge(string targetUid)
        {
            return pendingChallenges.Values.FirstOrDefault(c => c.TargetUid == targetUid);
        }

        /// <summary>
        /// Clean up expired challenges (should be called periodically)
        /// </summary>
        public void CleanupExpiredChallenges()
        {
            var expiredKeys = pendingChallenges
                .Where(kvp => HasChallengeExpired(kvp.Value))
                .Select(kvp => kvp.Key)
                .ToList();

            foreach (var key in expiredKeys)
            {
                pendingChallenges.Remove(key);
            }
        }

        private bool HasChallengeExpired(DuelChallenge challenge)
        {
            double hoursSinceChallenge = api.World.Calendar.TotalHours - challenge.ChallengeTime;
            double secondsSinceChallenge = hoursSinceChallenge * 3600; // Convert to real seconds (1 game hour = 3600 real seconds)
            return secondsSinceChallenge > CHALLENGE_TIMEOUT_SECONDS;
        }

        private string GetChallengeKey(string challengerUid, string targetUid)
        {
            return $"{challengerUid}:{targetUid}";
        }
    }

    /// <summary>
    /// Represents a pending duel challenge
    /// </summary>
    public class DuelChallenge
    {
        public string ChallengerUid { get; set; } = string.Empty;
        public string ChallengerName { get; set; } = string.Empty;
        public string TargetUid { get; set; } = string.Empty;
        public string TargetName { get; set; } = string.Empty;
        public double ChallengeTime { get; set; }
    }

    /// <summary>
    /// Represents an active duel between two players
    /// </summary>
    public class ActiveDuel
    {
        public string Player1Uid { get; set; } = string.Empty;
        public string Player1Name { get; set; } = string.Empty;
        public string Player2Uid { get; set; } = string.Empty;
        public string Player2Name { get; set; } = string.Empty;
        public double StartTime { get; set; }
    }
}
