using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using BodagovKR1_.Forms;

namespace BodagovKR1_
{
    internal static class Program
    {
        /// <summary>
        /// Главная точка входа для приложения Windows Forms CorpFleet Analytics.
        /// Поддерживает флаг --capture для автоматического формирования скриншотов всех 4 вкладок в отчет.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ApplicationConfiguration.Initialize();

            if (args.Length > 0 && args[0].Equals("--verify", StringComparison.OrdinalIgnoreCase))
            {
                VerificationRunner.RunAllChecks();
                return;
            }

            if (args.Length > 0 && args[0].Equals("--capture", StringComparison.OrdinalIgnoreCase))
            {
                RunCaptureAutomation();
                return;
            }

            Application.Run(new FormMain());
        }

        private static void RunCaptureAutomation()
        {
            string baseDir = AppContext.BaseDirectory;
            // Целевая папка docs/screenshots в проекте
            string outDir = Path.GetFullPath(Path.Combine(baseDir, "..", "..", "..", "docs", "screenshots"));
            Directory.CreateDirectory(outDir);

            using var form = new FormMain();
            form.StartPosition = FormStartPosition.Manual;
            form.Location = new Point(40, 40);
            form.Size = new Size(1150, 750);
            form.Show();
            Application.DoEvents();
            Thread.Sleep(300);

            for (int i = 0; i < 4; i++)
            {
                form.tabControlMain.SelectedIndex = i;
                Application.DoEvents();
                Thread.Sleep(300);

                string filename = i switch
                {
                    0 => "tab1_oop_registry.png",
                    1 => "tab2_linq_analytics.png",
                    2 => "tab3_aggregates_sets.png",
                    3 => "tab4_pagination_events.png",
                    _ => $"tab_{i + 1}.png"
                };

                string fullPath = Path.Combine(outDir, filename);
                using (var bmp = new Bitmap(form.Width, form.Height))
                {
                    form.DrawToBitmap(bmp, new Rectangle(0, 0, form.Width, form.Height));
                    bmp.Save(fullPath, ImageFormat.Png);
                }
                Console.WriteLine($"[CAPTURED]: {filename} -> {fullPath}");
            }

            form.Close();
        }
    }
}
