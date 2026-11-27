namespace TowerDefense.Enemies
{
    /// <summary>웨이브 배율과 이번 판의 효과를 적용한 적 한 마리의 수치.</summary>
    public readonly struct EnemyStats
    {
        public EnemyStats(float maxHealth, float moveSpeed, int attackDamage, float rewardScale)
        {
            MaxHealth = maxHealth;
            MoveSpeed = moveSpeed;
            AttackDamage = attackDamage;
            RewardScale = rewardScale;
        }

        public float MaxHealth { get; }
        public float MoveSpeed { get; }
        public int AttackDamage { get; }
        /// <summary>처치 보상에 곱하는 웨이브 배율.</summary>
        public float RewardScale { get; }
    }
}
