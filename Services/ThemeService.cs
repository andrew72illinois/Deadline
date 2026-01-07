using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Media;

namespace Deadline.Services
{
    public enum Theme
    {
        Light,
        Dark
    }

    public class ThemeService
    {
        private static ThemeService? _instance;
        private readonly string _settingsFilePath;
        private Theme _currentTheme;

        public static ThemeService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ThemeService();
                }
                return _instance;
            }
        }

        public Theme CurrentTheme
        {
            get => _currentTheme;
            set
            {
                if (_currentTheme != value)
                {
                    _currentTheme = value;
                    ApplyTheme(value);
                    SaveTheme();
                    ThemeChanged?.Invoke();
                }
            }
        }

        public event Action? ThemeChanged;

        private ThemeService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "YearDeadline"
            );
            
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _settingsFilePath = Path.Combine(appDataPath, "theme.json");
            LoadTheme();
        }

        private void LoadTheme()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    string json;
                    using (var fileStream = new FileStream(_settingsFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                    using (var reader = new StreamReader(fileStream))
                    {
                        json = reader.ReadToEnd();
                    }
                    
                    var settings = JsonSerializer.Deserialize<ThemeSettings>(json);
                    if (settings != null && Enum.TryParse<Theme>(settings.Theme, out var theme))
                    {
                        _currentTheme = theme;
                    }
                    else
                    {
                        _currentTheme = Theme.Light;
                    }
                }
                else
                {
                    _currentTheme = Theme.Light;
                }
            }
            catch
            {
                _currentTheme = Theme.Light;
            }
        }

        private void SaveTheme()
        {
            try
            {
                var settings = new ThemeSettings { Theme = _currentTheme.ToString() };
                var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
                
                // Use async-safe file writing
                using (var fileStream = new FileStream(_settingsFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(fileStream))
                {
                    writer.Write(json);
                }
            }
            catch
            {
                // Ignore save errors
            }
        }

        public void ApplyTheme(Theme theme)
        {
            var app = Application.Current;
            if (app == null) return;

            if (theme == Theme.Dark)
            {
                // Dark theme colors
                app.Resources["BackgroundColor"] = new SolidColorBrush(Color.FromRgb(30, 30, 30));
                app.Resources["SurfaceColor"] = new SolidColorBrush(Color.FromRgb(45, 45, 45));
                app.Resources["BorderColor"] = new SolidColorBrush(Color.FromRgb(60, 60, 60));
                app.Resources["TextColor"] = new SolidColorBrush(Colors.White);
                app.Resources["TextSecondaryColor"] = new SolidColorBrush(Color.FromRgb(200, 200, 200));
                app.Resources["HeaderBackgroundColor"] = new SolidColorBrush(Color.FromRgb(25, 25, 25));
                app.Resources["InputBackgroundColor"] = new SolidColorBrush(Color.FromRgb(40, 40, 40));
                app.Resources["NoteBackgroundColor"] = new SolidColorBrush(Color.FromRgb(50, 50, 50));
            }
            else
            {
                // Light theme colors
                app.Resources["BackgroundColor"] = new SolidColorBrush(Colors.White);
                app.Resources["SurfaceColor"] = new SolidColorBrush(Color.FromRgb(245, 245, 245));
                app.Resources["BorderColor"] = new SolidColorBrush(Color.FromRgb(224, 224, 224));
                app.Resources["TextColor"] = new SolidColorBrush(Colors.Black);
                app.Resources["TextSecondaryColor"] = new SolidColorBrush(Color.FromRgb(100, 100, 100));
                app.Resources["HeaderBackgroundColor"] = new SolidColorBrush(Color.FromRgb(45, 45, 48));
                app.Resources["InputBackgroundColor"] = new SolidColorBrush(Colors.White);
                app.Resources["NoteBackgroundColor"] = new SolidColorBrush(Color.FromRgb(249, 249, 249));
            }
        }

        private class ThemeSettings
        {
            public string Theme { get; set; } = "Light";
        }
    }
}

