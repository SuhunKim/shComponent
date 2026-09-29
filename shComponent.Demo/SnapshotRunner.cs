using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace shComponent.Demo
{
    /// <summary>
    /// 개발 검증용: "--snapshot 폴더" 인자로 실행하면 주요 화면을 PNG로 저장하고 종료한다.
    /// (일반 실행에는 영향 없음)
    /// </summary>
    internal static class SnapshotRunner
    {
        public static void AttachIfRequested(Window window, CustomMain shell)
        {
            var args = Environment.GetCommandLineArgs();
            var index = Array.IndexOf(args, "--snapshot");
            if (index < 0 || index + 1 >= args.Length) return;

            var folder = args[index + 1];
            Directory.CreateDirectory(folder);
            window.Loaded += async (_, __) => await RunAsync(window, shell, folder);
        }

        private static async Task RunAsync(Window window, CustomMain shell, string folder)
        {
            // 홈: 로딩 중(스켈레톤) → 로딩 완료
            await Task.Delay(500);
            Save(window, Path.Combine(folder, "01_home_loading.png"));
            await Task.Delay(1800);
            Save(window, Path.Combine(folder, "02_home.png"));

            foreach (var key in new[] { "menu1.sub1", "menu1.sub2", "menu1.sub3", "menu2.sub1", "menu3", "menu4" })
            {
                shell.Navigate(key);
                await Task.Delay(key == "menu3" || key == "menu2.sub1" ? 1600 : 700);
                Save(window, Path.Combine(folder, $"03_{key}.png"));
            }

            shell.IsPaneCompact = true;
            shell.Navigate("menu1.sub3");
            await Task.Delay(700);
            Save(window, Path.Combine(folder, "04_compact.png"));

            shell.IsPaneCompact = false;
            shell.MenuPlacement = MenuPlacement.Top;
            shell.Navigate("menu1.sub1");
            await Task.Delay(700);
            Save(window, Path.Combine(folder, "05_top_menu.png"));

            Application.Current.Shutdown();
        }

        private static void Save(Window window, string path)
        {
            var element = (FrameworkElement)window.Content;
            var dpi = VisualTreeHelper.GetDpi(element);
            var bitmap = new RenderTargetBitmap(
                (int)(element.ActualWidth * dpi.DpiScaleX), (int)(element.ActualHeight * dpi.DpiScaleY),
                dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Pbgra32);
            bitmap.Render(element);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using var stream = File.Create(path);
            encoder.Save(stream);
        }
    }
}
