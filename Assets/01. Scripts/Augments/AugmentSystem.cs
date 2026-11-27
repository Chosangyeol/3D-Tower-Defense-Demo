using System;
using System.Collections.Generic;
using TowerDefense.Economy;
using TowerDefense.Effects;
using TowerDefense.Enemies;
using TowerDefense.Game;
using TowerDefense.Waves;
using UnityEngine;
using Random = UnityEngine.Random;

namespace TowerDefense.Augments
{
    /// <summary>
    /// 이번 판의 증강. 획득 방식이 요청하면 후보를 뽑아 게임을 멈추고, 고른 증강의 효과를 RunModifiers에 더한다.
    /// 요청이 여러 개면 하나씩 이어서 처리한다. 화면 표시와 입력은 다루지 않는다.
    /// </summary>
    public class AugmentSystem : MonoBehaviour
    {
        [SerializeField] private GameSetup setup;
        [SerializeField] private RunModifiers modifiers;
        [SerializeField] private ResourceWallet wallet;
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private MatchController match;
        [SerializeField] private GamePause pause;

        private readonly List<AugmentSourceRuntime> sources = new List<AugmentSourceRuntime>();
        private readonly Queue<AugmentTier> pending = new Queue<AugmentTier>();
        private readonly List<AugmentDefinition> choices = new List<AugmentDefinition>();
        private readonly List<AugmentDefinition> acquired = new List<AugmentDefinition>();
        private readonly Dictionary<AugmentDefinition, int> stacks = new Dictionary<AugmentDefinition, int>();

        /// <summary>새 후보가 나올 때(다시 뽑기 포함) 호출된다.</summary>
        public event Action OfferShown;

        public ResourceWallet Wallet => wallet;
        /// <summary>지금 웨이브가 마지막 웨이브인지 여부.</summary>
        public bool IsFinalWave => waveSpawner.StartedWaves >= waveSpawner.WaveCount;

        /// <summary>지금 고를 수 있는 후보. 고르는 중이 아니면 비어 있다.</summary>
        public IReadOnlyList<AugmentDefinition> Choices => choices;
        public bool IsOffering => choices.Count > 0;
        /// <summary>고르는 중이거나 순서를 기다리는 선택이 있는지 여부.</summary>
        public bool HasWaitingOffer => IsOffering || pending.Count > 0;
        public AugmentTier OfferTier { get; private set; }
        public int RerollsLeft { get; private set; }
        /// <summary>다시 뽑기가 남아 있고, 지금 후보와 다른 증강이 있어야 다시 뽑을 수 있다.</summary>
        public bool CanReroll => IsOffering && RerollsLeft > 0 && GetEligible(OfferTier).Count > choices.Count;

        /// <summary>얻은 증강. 처음 고른 순서이며 중복은 없다.</summary>
        public IReadOnlyList<AugmentDefinition> Acquired => acquired;

        public int GetStacks(AugmentDefinition augment)
        {
            return augment != null && stacks.TryGetValue(augment, out int count) ? count : 0;
        }

        /// <summary>켜져 있는 획득 방식 중에서 해당 종류의 상태를 찾는다. 없으면 null.</summary>
        public T FindSource<T>() where T : AugmentSourceRuntime
        {
            foreach (AugmentSourceRuntime source in sources)
            {
                if (source is T found) return found;
            }
            return null;
        }

        public bool HasEligible(AugmentTier tier)
        {
            return GetEligible(tier).Count > 0;
        }

        /// <summary>증강 선택을 요청한다. 다음 LateUpdate에 화면이 열린다. 남은 증강이 없으면 false.</summary>
        public bool RequestOffer(AugmentTier tier)
        {
            if (match.Result != MatchResult.InProgress || !HasEligible(tier)) return false;

            pending.Enqueue(tier);
            return true;
        }

        /// <summary>후보 중 하나를 고른다. 기다리는 선택이 없으면 게임을 다시 진행한다.</summary>
        public bool Choose(int index)
        {
            if (index < 0 || index >= choices.Count) return false;

            AugmentDefinition augment = choices[index];
            choices.Clear();
            if (!stacks.ContainsKey(augment)) acquired.Add(augment);
            stacks[augment] = GetStacks(augment) + 1;
            modifiers.Apply(augment.Effects);

            ShowNext();
            if (!IsOffering) pause.SetPaused(false);
            return true;
        }

        /// <summary>후보를 모두 바꾼다. 다른 증강이 모자라면 남는 자리만 이전 후보로 채운다.</summary>
        public bool Reroll()
        {
            if (!CanReroll) return false;

            var previous = new List<AugmentDefinition>(choices);
            RerollsLeft--;
            Roll(OfferTier, previous);
            OfferShown?.Invoke();
            return true;
        }

        private void Awake()
        {
            RerollsLeft = setup.Campaign.AugmentRerolls;
            foreach (AugmentSource source in setup.Campaign.AugmentSources)
            {
                if (source != null) sources.Add(source.CreateRuntime(this));
            }
        }

        private void OnEnable()
        {
            waveSpawner.EnemyKilled += OnEnemyKilled;
            waveSpawner.WaveCleared += OnWaveCleared;
        }

        private void OnDisable()
        {
            waveSpawner.EnemyKilled -= OnEnemyKilled;
            waveSpawner.WaveCleared -= OnWaveCleared;
        }

        // 요청은 피해를 처리하는 도중에도 들어오므로, 그 프레임의 처리가 끝난 뒤에 연다
        private void LateUpdate()
        {
            if (IsOffering || pending.Count == 0 || match.Result != MatchResult.InProgress) return;

            ShowNext();
        }

        private void ShowNext()
        {
            while (!IsOffering && pending.Count > 0)
            {
                OfferTier = pending.Dequeue();
                Roll(OfferTier, null);
            }
            if (!IsOffering) return;

            pause.SetPaused(true);
            OfferShown?.Invoke();
        }

        private void OnEnemyKilled(Enemy enemy)
        {
            foreach (AugmentSourceRuntime source in sources) source.OnEnemyKilled(enemy);
        }

        private void OnWaveCleared(int wave)
        {
            foreach (AugmentSourceRuntime source in sources) source.OnWaveCleared(wave);
        }

        // 등급이 맞고 중첩이 가득 차지 않은 증강
        private List<AugmentDefinition> GetEligible(AugmentTier tier)
        {
            var result = new List<AugmentDefinition>();
            foreach (AugmentDefinition augment in setup.Campaign.Augments)
            {
                if (augment == null || augment.Tier != tier || GetStacks(augment) >= augment.MaxStacks) continue;
                if (!result.Contains(augment)) result.Add(augment);
            }
            return result;
        }

        // 피하고 싶은 증강은 다른 후보가 모자랄 때만 넣는다
        private void Roll(AugmentTier tier, List<AugmentDefinition> avoid)
        {
            List<AugmentDefinition> fresh = GetEligible(tier);
            var fallback = new List<AugmentDefinition>();
            if (avoid != null)
            {
                fallback = fresh.FindAll(avoid.Contains);
                fresh.RemoveAll(avoid.Contains);
            }

            choices.Clear();
            int count = setup.Campaign.AugmentChoices;
            TakeRandom(fresh, count);
            TakeRandom(fallback, count);
        }

        private void TakeRandom(List<AugmentDefinition> from, int count)
        {
            while (choices.Count < count && from.Count > 0)
            {
                int index = Random.Range(0, from.Count);
                choices.Add(from[index]);
                from.RemoveAt(index);
            }
        }
    }
}
