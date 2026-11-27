namespace TowerDefense.Combat
{
    /// <summary>소속. 서로 다른 소속끼리만 피해를 준다.</summary>
    public enum Team
    {
        Defenders, // 플레이어, 타워, 기지, 마을 건물
        Invaders   // 적
    }

    /// <summary>피해를 받을 수 있는 대상.</summary>
    public interface IDamageable
    {
        Team Team { get; }
        bool IsAlive { get; }
        void TakeDamage(float amount);
    }
}
