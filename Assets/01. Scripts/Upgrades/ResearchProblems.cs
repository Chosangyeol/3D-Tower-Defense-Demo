using System.Collections.Generic;

namespace TowerDefense.Upgrades
{
    /// <summary>
    /// 연구 목록에서 완료할 수 없거나 화면에 나오지 않는 항목을 찾는다.
    /// </summary>
    public static class ResearchProblems
    {
        public static List<string> Find(IReadOnlyList<UpgradeDefinition> upgrades)
        {
            var problems = new List<string>();
            var listed = new HashSet<UpgradeDefinition>();
            foreach (UpgradeDefinition upgrade in upgrades)
            {
                if (upgrade != null) listed.Add(upgrade);
            }

            foreach (UpgradeDefinition upgrade in upgrades)
            {
                if (upgrade == null) continue;

                string name = $"{upgrade.DisplayName} ({upgrade.name})";
                if (upgrade.Branch == null)
                {
                    problems.Add($"{name}: 갈래가 없어 연구 화면에 나오지 않습니다");
                }
                if (upgrade.Target != null && upgrade.Unlocks(upgrade.Target))
                {
                    problems.Add($"{name}: 해금하려는 타워의 갈래에 있어 열 수 없습니다");
                }
                foreach (UpgradeDefinition required in upgrade.Prerequisites)
                {
                    if (required == null) continue;

                    if (!listed.Contains(required))
                    {
                        problems.Add($"{name}: 선행 연구 {required.DisplayName}이(가) 맵의 연구 목록에 없습니다");
                    }
                    foreach (UpgradeDefinition other in upgrade.Prerequisites)
                    {
                        // 같은 쌍을 두 번 알리지 않는다
                        if (required.ConflictsWith(other) && string.CompareOrdinal(required.name, other.name) < 0)
                        {
                            problems.Add($"{name}: 선행 연구 {required.DisplayName}과(와) {other.DisplayName}은(는) "
                                         + "택 1이라 함께 완료할 수 없습니다");
                        }
                    }
                }
                if (Requires(upgrade, upgrade, new HashSet<UpgradeDefinition>()))
                {
                    problems.Add($"{name}: 선행 연구를 따라가면 자기 자신이 나와 열 수 없습니다");
                }
            }
            return problems;
        }

        // from의 선행을 따라가면 target이 나오는지 확인한다
        private static bool Requires(UpgradeDefinition from, UpgradeDefinition target, HashSet<UpgradeDefinition> visited)
        {
            foreach (UpgradeDefinition required in from.Prerequisites)
            {
                if (required == null) continue;
                if (required == target) return true;
                if (visited.Add(required) && Requires(required, target, visited)) return true;
            }
            return false;
        }
    }
}
