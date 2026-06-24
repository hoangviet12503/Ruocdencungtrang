using System;
using System.Collections.Generic;
using UnityEngine;
using MidAutumn.Data;

namespace MidAutumn.Gameplay
{
    /// <summary>
    /// Central singleton: owns the player's economy (Tickets, Lantern Points),
    /// the 5x5 board state, and campaign-day/check-in tracking. UI screens and
    /// the merge controller read/write through here so all 5 tabs stay in sync
    /// off one source of truth. Persists via SaveService on every mutation.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public const int GridWidth = 5;
        public const int GridHeight = 5;
        public const int GridSize = GridWidth * GridHeight;
        public const int CampaignDurationDays = 60;
        public const int StageCount = 15;

        [Header("Chain Root")]
        [SerializeField] private ItemDatabase itemDatabase;

        [Header("Stage Goals")]
        [Tooltip("Ordered by stageNumber. Stage index = currentStageIndex into this array.")]
        [SerializeField] private Data.LevelGoalData[] levelGoals;

        [Header("Spawn Weights (Level 1 / 2 / 3)")]
        [SerializeField] private float spawnWeightLevel1 = 0.70f;
        [SerializeField] private float spawnWeightLevel2 = 0.25f;
        [SerializeField] private float spawnWeightLevel3 = 0.05f;

        [Header("Voucher Reward (independent roll per box-open)")]
        [Tooltip("Chance per box-open that a partner voucher is won, on top of the item spawn.")]
        [SerializeField] private float voucherWinChance = 0.05f;
        [SerializeField] private Data.VoucherData[] voucherPool;

        [Header("Economy (runtime, mirrors save data)")]
        [SerializeField] private int tickets;
        [SerializeField] private int lanternPoints;

        private SaveData _save;

        public GridSlot[] Slots { get; private set; }
        public int Tickets => tickets;
        public int LanternPoints => lanternPoints;
        public int LoginStreak => _save.loginStreak;
        public ItemDatabase ItemDatabase => itemDatabase;

        public int CurrentStageIndex => _save.currentStageIndex;
        public Data.LevelGoalData CurrentLevelGoal =>
            levelGoals != null && CurrentStageIndex < levelGoals.Length ? levelGoals[CurrentStageIndex] : null;
        public bool IsCampaignComplete => levelGoals == null || CurrentStageIndex >= levelGoals.Length;

        /// <summary>Map's current stop, always mirroring CurrentStageIndex (no independent counter).</summary>
        public int MapStage => levelGoals == null || levelGoals.Length == 0
            ? 0
            : Mathf.Min(CurrentStageIndex, levelGoals.Length - 1);

        /// <summary>True once stageIndex has been cleared at least once via TryCollectStage.</summary>
        public bool IsStageCleared(int stageIndex) =>
            stageIndex >= 0 && stageIndex < _save.clearedStages.Length && _save.clearedStages[stageIndex];

        /// <summary>True once the board holds enough items at the goal's target level to collect — does not auto-clear; player must call TryCollectStage.</summary>
        public bool IsGoalReady =>
            CurrentLevelGoal != null && CountItemsAtLevel(CurrentLevelGoal.targetLevel) >= CurrentLevelGoal.targetCount;

        public event Action<Data.LevelGoalData> OnStageCleared;
        public event Action OnCampaignComplete;
        public event Action<Data.VoucherData> OnVoucherWon;

        public DateTime CampaignStartDate => string.IsNullOrEmpty(_save.campaignStartDateIso)
            ? DateTime.UtcNow.Date
            : DateTime.Parse(_save.campaignStartDateIso).ToUniversalTime();
        public int CampaignDayIndex => Mathf.Clamp((DateTime.UtcNow.Date - CampaignStartDate.Date).Days, 0, CampaignDurationDays - 1);
        public int CampaignDaysRemaining => CampaignDurationDays - CampaignDayIndex;
        public bool HasCheckedInToday => _save.lastCheckInDateIso == DateTime.UtcNow.Date.ToString("o");

        // --- Events: UI subscribes, never polls ---
        public event Action<int> OnTicketsChanged;
        public event Action<int> OnLanternPointsChanged;
        public event Action<int, ItemData> OnSlotChanged;          // slotIndex, new item (null = cleared)
        public event Action<int> OnItemSpawned;                    // slotIndex of a freshly spawned item (distinct from merge results)
        public event Action<int, int, ItemData> OnMergeSuccess;    // sourceIndex, targetIndex, resultItem
        public event Action<int> OnCheckedIn;                      // new streak count

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            Slots = new GridSlot[GridSize];
            for (int i = 0; i < GridSize; i++)
                Slots[i] = new GridSlot(i);

            LoadFromDisk();
        }

        private void LoadFromDisk()
        {
            _save = SaveService.Load();
            tickets = _save.tickets;
            lanternPoints = _save.lanternPoints;

            // Older saves may predate a grid-size change (e.g. 4x4 -> 5x5); resize
            // rather than crash, treating any newly-added slots as empty.
            if (_save.slotLevels == null || _save.slotLevels.Length != GridSize)
            {
                var resized = new int[GridSize];
                if (_save.slotLevels != null)
                    Array.Copy(_save.slotLevels, resized, Mathf.Min(_save.slotLevels.Length, GridSize));
                _save.slotLevels = resized;
            }

            // Older saves may predate clearedStages (or the 7->15 stage count change); resize
            // rather than crash, treating any newly-added stages as not-yet-cleared.
            if (_save.clearedStages == null || _save.clearedStages.Length != StageCount)
            {
                var resizedCleared = new bool[StageCount];
                if (_save.clearedStages != null)
                    Array.Copy(_save.clearedStages, resizedCleared, Mathf.Min(_save.clearedStages.Length, StageCount));
                _save.clearedStages = resizedCleared;
            }

            // Older saves may predate wonVoucherIds; JsonUtility leaves new List<T> fields null.
            if (_save.wonVoucherIds == null)
                _save.wonVoucherIds = new System.Collections.Generic.List<string>();

            for (int i = 0; i < GridSize; i++)
            {
                int level = _save.slotLevels[i];
                Slots[i].SetItem(level > 0 ? itemDatabase.GetByLevel(level) : null);
            }
        }

        private void Persist()
        {
            _save.tickets = tickets;
            _save.lanternPoints = lanternPoints;
            for (int i = 0; i < GridSize; i++)
                _save.slotLevels[i] = Slots[i].IsEmpty ? 0 : Slots[i].item.level;

            SaveService.Save(_save);
        }

        private void OnApplicationPause(bool paused)
        {
            if (paused) Persist();
        }

        private void OnApplicationQuit() => Persist();

        // ---------- Daily Check-in ----------

        /// <summary>
        /// Grants 20 Tickets for the daily check-in mission. Streak increments if the
        /// previous check-in was exactly yesterday; otherwise it resets to 1 (broken streak).
        /// Returns false if already checked in today.
        /// </summary>
        public bool TryDailyCheckIn(int ticketReward = 20)
        {
            if (HasCheckedInToday) return false;

            DateTime today = DateTime.UtcNow.Date;
            bool hasPriorCheckIn = !string.IsNullOrEmpty(_save.lastCheckInDateIso);
            DateTime previous = hasPriorCheckIn ? DateTime.Parse(_save.lastCheckInDateIso).ToUniversalTime().Date : today;

            _save.loginStreak = (hasPriorCheckIn && (today - previous).Days == 1) ? _save.loginStreak + 1 : 1;
            _save.lastCheckInDateIso = today.ToString("o");

            AddTickets(ticketReward);
            Persist();
            OnCheckedIn?.Invoke(_save.loginStreak);
            return true;
        }

        // ---------- Mission completion (persisted set, see MissionManager) ----------

        public bool IsMissionCompleted(string missionId) => _save.completedMissionIds.Contains(missionId);
        public bool IsMissionClaimed(string missionId) => _save.claimedMissionIds.Contains(missionId);

        public void MarkMissionCompleted(string missionId)
        {
            if (!_save.completedMissionIds.Contains(missionId))
                _save.completedMissionIds.Add(missionId);
            Persist();
        }

        public void MarkMissionClaimed(string missionId)
        {
            if (!_save.claimedMissionIds.Contains(missionId))
                _save.claimedMissionIds.Add(missionId);
            Persist();
        }

        // ---------- Economy ----------

        public void AddTickets(int amount)
        {
            if (amount == 0) return;
            tickets = Mathf.Max(0, tickets + amount);
            OnTicketsChanged?.Invoke(tickets);
            Persist();
        }

        public bool TrySpendTicket(int cost = 1)
        {
            if (tickets < cost) return false;
            tickets -= cost;
            OnTicketsChanged?.Invoke(tickets);
            Persist();
            return true;
        }

        public void AddLanternPoints(int amount)
        {
            if (amount == 0) return;
            lanternPoints = Mathf.Max(0, lanternPoints + amount);
            OnLanternPointsChanged?.Invoke(lanternPoints);
            Persist();
        }

        public bool TrySpendLanternPoints(int amount)
        {
            if (lanternPoints < amount) return false;
            lanternPoints -= amount;
            OnLanternPointsChanged?.Invoke(lanternPoints);
            Persist();
            return true;
        }

        // ---------- Board ----------

        /// <summary>
        /// Spends 1 ticket and spawns one item into a random empty slot. Level is
        /// randomized by weight (70% Lvl1 / 25% Lvl2 / 5% Lvl3 by default) rather
        /// than always Level 1, matching the reference "open gift box" loot table.
        /// </summary>
        public bool TrySpawnRandomWeighted(out int spawnedIndex)
        {
            spawnedIndex = -1;
            int emptyIndex = FindRandomEmptySlot();
            if (emptyIndex < 0) return false; // board full

            if (!TrySpendTicket(1)) return false;

            ItemData item = RollSpawnItem();
            Slots[emptyIndex].SetItem(item);
            spawnedIndex = emptyIndex;
            // Fired before OnSlotChanged so listeners can flag "this slot's next
            // refresh is a fresh spawn" before Refresh() actually runs.
            OnItemSpawned?.Invoke(emptyIndex);
            OnSlotChanged?.Invoke(emptyIndex, item);
            Persist();

            // Independent roll: a voucher can be won on top of the item spawn,
            // it never occupies a grid slot.
            RollVoucherReward();

            return true;
        }

        private void RollVoucherReward()
        {
            if (voucherPool == null || voucherPool.Length == 0) return;
            if (UnityEngine.Random.value >= voucherWinChance) return;

            Data.VoucherData won = voucherPool[UnityEngine.Random.Range(0, voucherPool.Length)];
            _save.wonVoucherIds.Add(won.id);
            Persist();
            OnVoucherWon?.Invoke(won);
        }

        /// <summary>Every VoucherData.id won via the box-open roll, in win order. Backs "Quà tặng của bạn".</summary>
        public IReadOnlyList<string> WonVoucherIds => _save.wonVoucherIds;

        private ItemData RollSpawnItem()
        {
            float total = spawnWeightLevel1 + spawnWeightLevel2 + spawnWeightLevel3;
            float roll = UnityEngine.Random.value * total;

            if (roll < spawnWeightLevel1) return itemDatabase.GetByLevel(1);
            if (roll < spawnWeightLevel1 + spawnWeightLevel2) return itemDatabase.GetByLevel(2);
            return itemDatabase.GetByLevel(3);
        }

        public int FindRandomEmptySlot()
        {
            var emptyIndices = new System.Collections.Generic.List<int>();
            for (int i = 0; i < GridSize; i++)
                if (Slots[i].IsEmpty) emptyIndices.Add(i);

            if (emptyIndices.Count == 0) return -1;
            return emptyIndices[UnityEngine.Random.Range(0, emptyIndices.Count)];
        }

        public int FindFirstEmptySlot()
        {
            for (int i = 0; i < GridSize; i++)
                if (Slots[i].IsEmpty) return i;
            return -1;
        }

        /// <summary>
        /// Attempts to merge the item at sourceIndex into targetIndex.
        /// Both must hold the same item level, and that level must not be MAX.
        /// On success: target gets the leveled-up item, source is cleared.
        /// </summary>
        public bool TryMerge(int sourceIndex, int targetIndex)
        {
            if (sourceIndex == targetIndex) return false;
            if (!IsValidIndex(sourceIndex) || !IsValidIndex(targetIndex)) return false;

            GridSlot source = Slots[sourceIndex];
            GridSlot target = Slots[targetIndex];

            if (source.IsEmpty || target.IsEmpty) return false;
            if (source.item.level != target.item.level) return false;
            if (source.item.IsMaxLevel) return false; // level 7, nowhere to go

            ItemData resultItem = target.item.nextLevelData;

            source.Clear();
            target.SetItem(resultItem);

            OnSlotChanged?.Invoke(sourceIndex, null);
            OnSlotChanged?.Invoke(targetIndex, resultItem);
            OnMergeSuccess?.Invoke(sourceIndex, targetIndex, resultItem);
            Persist();

            // Reaching max level is a good moment to award Lantern Points; tune per design.
            if (resultItem.IsMaxLevel)
                AddLanternPoints(50);

            return true;
        }

        // ---------- Stage / Level Goal ----------

        /// <summary>How many slots currently hold an item at the goal's target level.</summary>
        public int CountItemsAtLevel(int level)
        {
            int count = 0;
            for (int i = 0; i < GridSize; i++)
                if (!Slots[i].IsEmpty && Slots[i].item.level == level)
                    count++;
            return count;
        }

        /// <summary>
        /// Player-initiated stage clear: only succeeds once IsGoalReady is true.
        /// Unlike the old auto-clear, the board is NOT touched until the player
        /// explicitly collects — merging/playing on can continue freely beforehand.
        /// </summary>
        public bool TryCollectStage()
        {
            var goal = CurrentLevelGoal;
            if (goal == null) return false; // campaign already complete
            if (!IsGoalReady) return false;

            int consumed = 0;
            for (int i = 0; i < GridSize && consumed < goal.targetCount; i++)
            {
                if (!Slots[i].IsEmpty && Slots[i].item.level == goal.targetLevel)
                {
                    Slots[i].Clear();
                    OnSlotChanged?.Invoke(i, null);
                    consumed++;
                }
            }

            if (CurrentStageIndex >= 0 && CurrentStageIndex < _save.clearedStages.Length)
                _save.clearedStages[CurrentStageIndex] = true;

            AddTickets(goal.clearTicketReward);
            _save.currentStageIndex++;
            Persist();

            OnStageCleared?.Invoke(goal);
            if (IsCampaignComplete)
                OnCampaignComplete?.Invoke();

            return true;
        }

        /// <summary>
        /// Prototype map navigation: every stage is unlocked, so tapping any province
        /// on the map jumps straight to it. The board resets empty (no per-stage save
        /// slots exist) — simplest behavior for a prototype, per design direction.
        /// </summary>
        public void JumpToStage(int stageIndex)
        {
            if (levelGoals == null || stageIndex < 0 || stageIndex >= levelGoals.Length) return;

            for (int i = 0; i < GridSize; i++)
            {
                if (Slots[i].IsEmpty) continue;
                Slots[i].Clear();
                OnSlotChanged?.Invoke(i, null);
            }

            _save.currentStageIndex = stageIndex;
            Persist();
        }

        private bool IsValidIndex(int index) => index >= 0 && index < GridSize;
    }
}
