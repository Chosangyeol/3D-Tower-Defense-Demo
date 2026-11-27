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
    /// 이번 판의 증강. 획득 방식이 요청하면 등급을 무작위로 정해 그 등급의 후보를 뽑아 게임을 멈추고,
    /// 고른 증강의 효과를 RunModifiers에 더한다.
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
        // 기다리는 선택마다 그 요청의 등급 확률을 보관한다
        private readonly Queue<IReadOnlyList<AugmentTierWeight>> pending = new Queue<IReadOnlyList<AugmentTierWeight>>();
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
        /// <summary>
        /// 다시 뽑기가 남아 있고, 지금 후보를 빼고도 같은 등급의 새 후보를 모두 채울 수 있어야 다시 뽑을 수 있다.
        /// </summary>
        public bool CanReroll =>
            IsOffering && RerollsLeft > 0
            && GetEligible(OfferTier).Count - choices.Count >= setup.Campaign.AugmentChoices;

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

        /// <summary>확률이 있는 등급 중에 고를 수 있는 증강이 남아 있는지 여부.</summary>
        public bool HasEligible(IReadOnlyList<AugmentTierWeight> chances)
        {
            foreach (AugmentTierWeight chance in chances)
            {
                if (chance.weight > 0f && GetEligible(chance.tier).Count > 0) return true;
            }
            return false;
        }

        /// <summary>증강 선택을 요청한다. 다음 LateUpdate에 화면이 열린다. 남은 증강이 없으면 false.</summary>
        /// <param name="chances">등급별 확률 비중</param>
        public bool RequestOffer(IReadOnlyList<AugmentTierWeight> chances)
        {
            if (match.Result != MatchResult.InProgress || !HasEligible(chances)) return false;

            pending.Enqueue(chances);
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

        /// <summary>같은 등급에서 지금 후보를 뺀 증강으로 후보를 모두 바꾼다.</summary>
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
                // 등급은 화면을 여는 순간에 남은 증강을 보고 정한다
                if (!TryPickTier(pending.Dequeue(), out AugmentTier tier)) continue;

                OfferTier = tier;
                Roll(tier, null);
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

        // 고를 수 있는 증강이 남은 등급 중에서 비중에 따라 하나를 뽑는다
        private bool TryPickTier(IReadOnlyList<AugmentTierWeight> chances, out AugmentTier picked)
        {
            var open = new List<AugmentTierWeight>();
            float total = 0f;
            foreach (AugmentTierWeight chance in chances)
            {
                if (chance.weight <= 0f || GetEligible(chance.tier).Count == 0) continue;

                open.Add(chance);
                total += chance.weight;
            }

            picked = default;
            float roll = Random.value * total;
            foreach (AugmentTierWeight chance in open)
            {
                picked = chance.tier;
                roll -= chance.weight;
                if (roll < 0f) break;
            }
            return open.Count > 0;
        }

        /// <param name="exclude">이번 후보에서 뺄 증강. 없으면 null</param>
        private void Roll(AugmentTier tier, List<AugmentDefinition> exclude)
        {
            List<AugmentDefinition> from = GetEligible(tier);
            if (exclude != null) from.RemoveAll(exclude.Contains);

            choices.Clear();
            while (choices.Count < setup.Campaign.AugmentChoices && from.Count > 0)
            {
                int index = Random.Range(0, from.Count);
                choices.Add(from[index]);
                from.RemoveAt(index);
            }
        }
    }
}
