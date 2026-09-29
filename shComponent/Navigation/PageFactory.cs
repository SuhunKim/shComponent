using System;
using System.Collections.Generic;
using System.Windows;

namespace shComponent.Navigation
{
    /// <summary>
    /// Key → 페이지 생성 함수를 등록하는 기본 <see cref="IPageFactory"/> 구현.
    /// <code>
    /// var pages = new PageFactory()
    ///     .Register&lt;HomePage&gt;("home", cache: true)
    ///     .Register("display", () => new DisplayPage(displayService));
    /// </code>
    /// </summary>
    public class PageFactory : IPageFactory
    {
        private readonly Dictionary<string, Registration> _registrations =
            new Dictionary<string, Registration>(StringComparer.OrdinalIgnoreCase);

        /// <summary>생성 함수로 등록. 서비스 주입이 필요한 페이지에 사용.</summary>
        /// <param name="cache">true면 처음 만든 인스턴스를 재사용(입력 상태 유지, 생성 비용 절약).</param>
        public PageFactory Register(string key, Func<FrameworkElement> create, bool cache = false)
        {
            if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key는 비어 있을 수 없습니다.", nameof(key));
            _registrations[key] = new Registration(create ?? throw new ArgumentNullException(nameof(create)), cache);
            return this;
        }

        /// <summary>기본 생성자가 있는 페이지 형식으로 등록.</summary>
        public PageFactory Register<TPage>(string key, bool cache = false) where TPage : FrameworkElement, new()
            => Register(key, () => new TPage(), cache);

        public bool IsRegistered(string key) => _registrations.ContainsKey(key);

        public FrameworkElement? CreatePage(INavItem item)
        {
            if (item == null || !_registrations.TryGetValue(item.Key, out var reg)) return null;
            if (!reg.Cache) return reg.Create();
            return reg.Instance ??= reg.Create();
        }

        /// <summary>캐시된 페이지 인스턴스를 모두 버린다(로그아웃 등).</summary>
        public void ClearCache()
        {
            foreach (var reg in _registrations.Values) reg.Instance = null;
        }

        private sealed class Registration
        {
            public Registration(Func<FrameworkElement> create, bool cache)
            {
                Create = create;
                Cache = cache;
            }

            public Func<FrameworkElement> Create { get; }
            public bool Cache { get; }
            public FrameworkElement? Instance { get; set; }
        }
    }
}
