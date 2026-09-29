using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using shComponent.Navigation;

namespace shComponent.Internal
{
    /// <summary>
    /// 메뉴 버튼 목록 생성과 "단일 선택 상태"만 관리하는 비시각(non-visual) 헬퍼.
    /// WHY: CustomMenu와 CustomSubMenu는 모양만 다르고 선택 규칙은 같다.
    ///      XAML UserControl은 서로 상속할 수 없으므로, 로직을 이 클래스로 분리해 조합(Composition)으로 공유한다.
    ///      또한 ItemsSource 바인딩 대신 버튼을 코드로 직접 만들어, 디버거로 한 줄씩 따라갈 수 있게 한다.
    /// </summary>
    internal sealed class NavButtonHost
    {
        private readonly Panel _panel;
        private readonly Func<INavItem, ToggleButton> _createButton;
        private readonly List<INavItem> _items = new List<INavItem>();
        private readonly List<ToggleButton> _buttons = new List<ToggleButton>();

        public NavButtonHost(Panel panel, Func<INavItem, ToggleButton> createButton)
        {
            _panel = panel;
            _createButton = createButton;
        }

        /// <summary>사용자 클릭 또는 notify=true 인 Select로 선택이 바뀌었을 때 발생.</summary>
        public event EventHandler<NavItemEventArgs>? SelectionChanged;

        public IReadOnlyList<INavItem> Items => _items;
        public INavItem? SelectedItem { get; private set; }

        public void SetItems(IEnumerable<INavItem>? items)
        {
            _items.Clear();
            if (items != null) _items.AddRange(items);
            SelectedItem = null;
            Rebuild();
        }

        /// <summary>같은 항목으로 버튼만 다시 만든다(크기/폰트/컴팩트 모드 변경 시). 선택은 유지, 이벤트는 발생하지 않음.</summary>
        public void Rebuild()
        {
            foreach (var old in _buttons) old.Click -= OnButtonClick;
            _buttons.Clear();
            _panel.Children.Clear();

            foreach (var item in _items)
            {
                var button = _createButton(item);
                button.Tag = item;
                button.IsEnabled = item.IsEnabled;
                button.IsChecked = ReferenceEquals(item, SelectedItem);
                button.Click += OnButtonClick;
                _buttons.Add(button);
                _panel.Children.Add(button);
            }
        }

        public void Select(INavItem? item, bool notify)
        {
            if (item != null && !_items.Contains(item)) item = null;

            var previous = SelectedItem;
            SelectedItem = item;
            SyncCheckedState();

            if (notify && item != null && !ReferenceEquals(previous, item))
                SelectionChanged?.Invoke(this, new NavItemEventArgs(item, previous));
        }

        public INavItem? FindByKey(string key)
            => _items.FirstOrDefault(i => string.Equals(i.Key, key, StringComparison.OrdinalIgnoreCase));

        private void OnButtonClick(object sender, RoutedEventArgs e)
        {
            var item = (INavItem)((FrameworkElement)sender).Tag;

            // WHY: ToggleButton은 클릭할 때마다 스스로 IsChecked를 뒤집는다.
            //      이미 선택된 항목을 다시 누르면 "선택 해제" 상태가 되지 않도록 체크 상태를 다시 확정한다.
            if (ReferenceEquals(item, SelectedItem))
            {
                SyncCheckedState();
                return;
            }
            Select(item, notify: true);
        }

        private void SyncCheckedState()
        {
            foreach (var button in _buttons)
                button.IsChecked = ReferenceEquals(button.Tag, SelectedItem);
        }
    }
}
