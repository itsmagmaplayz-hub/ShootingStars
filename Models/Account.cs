using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;

namespace ShootingStars.Models
{
    public class Quest
    {
        public string Description { get; set; } = "";
        public string Type { get; set; } = ""; // e.g., "WinMatch", "Kills", "Survive"
        public int Target { get; set; } // e.g., 1 for win, 3 for kills
        public int Current { get; set; } = 0;
        public int RewardXP { get; set; }

        public bool IsCompleted => Current >= Target;
    }

    public class MatchResult
    {
        public string GameMode { get; set; } = "";
        public int Position { get; set; } // 1st, 2nd, etc
        public int TrophyChange { get; set; }
        public DateTime Date { get; set; }

        public override string ToString()
        {
            string result = Position == 1 ? "🥇 1st" : Position == 2 ? "🥈 2nd" : $"{Position}th";
            return $"{result} - {GameMode} ({(TrophyChange >= 0 ? "+" : "")}{TrophyChange}) on {Date:MMM d, HH:mm}";
        }
    }

    public class Account
    {
        public string Username { get; set; } = "";
        public int Trophies { get; set; }
        public int Gems { get; set; }
        public int ShootPassXP { get; set; }
        public int PassProgress { get; set; } = 0; // 0 to 50000
        public List<int> ClaimedMilestones { get; set; } = new List<int>(); // e.g., 2000, 4000, etc.
        public List<string> OwnedShooters { get; set; } = new List<string> { "Shelly" };
        public List<MatchResult> MatchHistory { get; set; } = new List<MatchResult>();
        public List<Quest> ActiveQuests { get; set; } = new List<Quest>();

        private static string AccountsFolder => Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "accounts");

        public static Account Load(string username)
        {
            if (!Directory.Exists(AccountsFolder))
                Directory.CreateDirectory(AccountsFolder);

            string path = Path.Combine(AccountsFolder, username + ".json");
            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var acc = JsonSerializer.Deserialize<Account>(json);
                if (acc != null)
                {
                    // ensure active quests if empty
                    if (acc.ActiveQuests.Count == 0)
                        acc.GenerateNewQuests();
                    return acc;
                }
                return new Account { Username = username, Trophies = 1000 };
            }

            // create new
            var newAcc = new Account { Username = username, Trophies = 0, Gems = 0, ShootPassXP = 0 };
            newAcc.OwnedShooters = new List<string> { "Shelly" };
            newAcc.GenerateNewQuests();
            Save(newAcc);
            return newAcc;
        }

        public void GenerateNewQuests()
        {
            var questTemplates = new[]
            {
                new { Desc = "Win a match", Type = "WinMatch", Target = 1, XP = 250 },
                new { Desc = "Get 3 kills in a match", Type = "Kills", Target = 3, XP = 500 },
                new { Desc = "Survive 60 seconds", Type = "Survive", Target = 60, XP = 1000 },
                new { Desc = "Deal 100 damage", Type = "Damage", Target = 100, XP = 750 },
                new { Desc = "Play 5 matches", Type = "Matches", Target = 5, XP = 300 }
            };
            var rand = new Random();
            ActiveQuests = questTemplates.OrderBy(x => rand.Next()).Take(3).Select(t => new Quest
            {
                Description = t.Desc,
                Type = t.Type,
                Target = t.Target,
                RewardXP = t.XP
            }).ToList();
        }

        public void UpdateQuestProgress(string type, int amount = 1)
        {
            foreach (var quest in ActiveQuests.Where(q => q.Type == type && !q.IsCompleted))
            {
                quest.Current += amount;
            }
        }

        public void ClaimQuest(Quest quest)
        {
            if (quest.IsCompleted)
            {
                AddShootPassXP(quest.RewardXP);
                ActiveQuests.Remove(quest);
                // add a new quest
                var newQuest = GenerateRandomQuest();
                ActiveQuests.Add(newQuest);
            }
        }

        private Quest GenerateRandomQuest()
        {
            var templates = new[]
            {
                new { Desc = "Win a match", Type = "WinMatch", Target = 1, XP = 250 },
                new { Desc = "Get 3 kills in a match", Type = "Kills", Target = 3, XP = 500 },
                new { Desc = "Survive 60 seconds", Type = "Survive", Target = 60, XP = 1000 },
                new { Desc = "Deal 100 damage", Type = "Damage", Target = 100, XP = 750 },
                new { Desc = "Play 5 matches", Type = "Matches", Target = 5, XP = 300 }
            };
            var rand = new Random();
            var t = templates[rand.Next(templates.Length)];
            return new Quest { Description = t.Desc, Type = t.Type, Target = t.Target, RewardXP = t.XP };
        }

        public static void Save(Account account)
        {
            if (!Directory.Exists(AccountsFolder))
                Directory.CreateDirectory(AccountsFolder);

            string path = Path.Combine(AccountsFolder, account.Username + ".json");
            var json = JsonSerializer.Serialize(account, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(path, json);
        }

        public static List<Account> GetAllAccounts()
        {
            if (!Directory.Exists(AccountsFolder))
                Directory.CreateDirectory(AccountsFolder);

            var accounts = new List<Account>();
            foreach (var file in Directory.GetFiles(AccountsFolder, "*.json"))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    var acc = JsonSerializer.Deserialize<Account>(json);
                    if (acc != null)
                        accounts.Add(acc);
                }
                catch { /* skip corrupted files */ }
            }
            return accounts.OrderByDescending(a => a.Trophies).ToList();
        }

        public static List<Account> GetLeaderboard(int limit = 10)
        {
            return GetAllAccounts().Take(limit).ToList();
        }

        public int GetWinCount() => MatchHistory.Count(m => m.Position == 1);
        public int GetTopFourCount() => MatchHistory.Count(m => m.Position <= 4);
        public double GetWinRate() => MatchHistory.Count == 0 ? 0.0 : (GetWinCount() * 100.0) / MatchHistory.Count;

        // shootpass xp adds and convert to gems
        public void AddShootPassXP(int xp)
        {
            ShootPassXP += xp;
            while (ShootPassXP >= 2000)
            {
                ShootPassXP -= 2000;
                Gems += 10;
            }
        }

        // purchase shooter if affordable and not already owned
        public bool PurchaseShooter(string shooterName, int cost)
        {
            if (Gems >= cost && !OwnedShooters.Contains(shooterName))
            {
                Gems -= cost;
                OwnedShooters.Add(shooterName);
                return true;
            }
            return false;
        }
    }
}
