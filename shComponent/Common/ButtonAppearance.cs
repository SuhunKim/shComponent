namespace shComponent
{
    /// <summary>CustomButton의 시각적 종류.</summary>
    public enum ButtonAppearance
    {
        /// <summary>일반 버튼: Background / HoverBackground / PressedBackground 사용.</summary>
        Standard,

        /// <summary>강조 버튼(주요 동작): AccentBrush 배경 + AccentForeground 글자.</summary>
        Accent,

        /// <summary>배경 없는 버튼(툴바/아이콘 버튼): 마우스를 올렸을 때만 배경 표시.</summary>
        Subtle
    }
}
