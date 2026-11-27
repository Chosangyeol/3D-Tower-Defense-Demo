using UnityEngine;

namespace TowerDefense.Effects
{
    /// <summary>
    /// 연구, 제약, 증강 등이 바꿀 수 있는 수치. 번호는 데이터에 저장되므로 바꾸지 않고 새 번호로 추가한다.
    /// </summary>
    public enum Stat
    {
        [InspectorName("타워/공격력")] TowerDamage = 0,
        [InspectorName("타워/사거리")] TowerRange = 1,
        [InspectorName("타워/발사 간격")] TowerFireInterval = 2,
        [InspectorName("타워/투사체 속도")] TowerProjectileSpeed = 3,
        [InspectorName("타워/설치 비용")] TowerCost = 4,

        [InspectorName("플레이어/이동 속도")] PlayerMoveSpeed = 10,
        [InspectorName("플레이어/최대 마나")] PlayerMaxMana = 11,
        [InspectorName("플레이어/마나 회복")] PlayerManaRegen = 12,

        [InspectorName("마법/피해")] SpellDamage = 20,
        [InspectorName("마법/마나 소모")] SpellManaCost = 21,
        [InspectorName("마법/시전 간격")] SpellCastInterval = 22,

        [InspectorName("적/수 (보스 제외)")] EnemyCount = 30,
        [InspectorName("적/체력")] EnemyHealth = 31,
        [InspectorName("적/공격력")] EnemyAttackDamage = 32,
        [InspectorName("적/이동 속도")] EnemyMoveSpeed = 33,

        [InspectorName("보상/처치")] KillReward = 40,
        [InspectorName("보상/웨이브 완료")] WaveReward = 41,
        [InspectorName("보상/채집량")] GatherAmount = 42,
        [InspectorName("보상/채집 시간")] GatherSeconds = 43,

        [InspectorName("게임/준비 시간")] PrepSeconds = 50,
        [InspectorName("게임/시작 자원")] StartResources = 51,
        [InspectorName("게임/기지 체력")] BaseHealth = 52
    }
}
