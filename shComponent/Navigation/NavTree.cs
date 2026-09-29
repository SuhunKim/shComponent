using System;
using System.Collections.Generic;
using System.Linq;

namespace shComponent.Navigation
{
    /// <summary>메뉴 트리 탐색 유틸리티(2단계: 1차 메뉴 → 서브메뉴).</summary>
    public static class NavTree
    {
        /// <summary>Key로 항목을 찾는다. 서브메뉴 항목이면 parent에 1차 메뉴 항목을 돌려준다.</summary>
        public static INavItem? Find(IEnumerable<INavItem>? roots, string key, out INavItem? parent)
        {
            parent = null;
            if (roots == null || string.IsNullOrEmpty(key)) return null;

            foreach (var root in roots)
            {
                if (KeyEquals(root, key)) return root;
                foreach (var child in root.Children)
                {
                    if (!KeyEquals(child, key)) continue;
                    parent = root;
                    return child;
                }
            }
            return null;
        }

        /// <summary>1차 메뉴 항목이 선택됐을 때 실제로 열 페이지(자식이 있으면 첫 번째 활성 자식)를 결정한다.</summary>
        public static INavItem ResolveLeaf(INavItem root)
        {
            return root.Children.FirstOrDefault(c => c.IsEnabled) ?? root;
        }

        private static bool KeyEquals(INavItem item, string key)
            => string.Equals(item.Key, key, StringComparison.OrdinalIgnoreCase);
    }
}
