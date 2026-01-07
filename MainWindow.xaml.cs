using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;
using Deadline.Models;
using Deadline.Services;

namespace Deadline
{
    public partial class MainWindow : Window
    {
        private readonly GoalDataService _dataService;
        private ObservableCollection<GoalViewModel> _goals;
        private ObservableCollection<GoalViewModel> _allGoals; // Store all goals
        private string? _editingGoalId;
        private DispatcherTimer _updateTimer;
        private bool _isViewingArchived = false;

        public MainWindow()
        {
            InitializeComponent();
            
            _dataService = new GoalDataService();
            _goals = new ObservableCollection<GoalViewModel>();
            _allGoals = new ObservableCollection<GoalViewModel>();
            GoalsItemsControl.ItemsSource = _goals;
            
            // Timer to update progress every minute
            _updateTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMinutes(1)
            };
            _updateTimer.Tick += UpdateTimer_Tick;
            _updateTimer.Start();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                // Initialize theme after window is loaded
                var themeService = ThemeService.Instance;
                themeService.ThemeChanged += ThemeService_ThemeChanged;
                
                // Apply theme on UI thread after window is fully loaded
                Dispatcher.BeginInvoke(new Action(() =>
                {
                    themeService.ApplyTheme(themeService.CurrentTheme);
                    UpdateThemeUI();
                }), DispatcherPriority.Loaded);
                
                LoadGoals();
                StartDatePicker.SelectedDate = DateTime.Today;
                DeadlineDatePicker.SelectedDate = DateTime.Today.AddDays(30);
                GoalTypeComboBox.SelectedIndex = 0; // Default to Qualitative
                UpdateNavigationButtons(); // Initialize navigation buttons
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading goals: {ex.Message}\n\nThe application will continue with an empty list.",
                    "Loading Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                UpdateEmptyState();
            }
        }
        
        private void ThemeService_ThemeChanged()
        {
            UpdateThemeUI();
        }
        
        private void UpdateThemeUI()
        {
            var themeService = ThemeService.Instance;
            if (ThemeToggleButton != null)
            {
                ThemeToggleButton.Content = themeService.CurrentTheme == Theme.Light 
                    ? "Switch to Dark Mode" 
                    : "Switch to Light Mode";
            }
        }
        
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsPopup.IsOpen = !SettingsPopup.IsOpen;
        }
        
        private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
        {
            var themeService = ThemeService.Instance;
            themeService.CurrentTheme = themeService.CurrentTheme == Theme.Light 
                ? Theme.Dark 
                : Theme.Light;
            SettingsPopup.IsOpen = false;
        }

        private void LoadGoals()
        {
            try
            {
                _allGoals.Clear();
                var goals = _dataService.GetAllGoals();
                
                foreach (var goal in goals)
                {
                    if (goal != null)
                    {
                        var goalViewModel = new GoalViewModel(goal);
                        _allGoals.Add(goalViewModel);
                    }
                }

                FilterGoalsByView();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error loading goals: {ex.Message}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                _allGoals.Clear();
                _goals.Clear();
                UpdateEmptyState();
            }
        }
        
        private void FilterGoalsByView()
        {
            _goals.Clear();
            
            foreach (var goalViewModel in _allGoals)
            {
                if (goalViewModel.Goal != null)
                {
                    bool isArchived = goalViewModel.Goal.IsArchived;
                    
                    if (_isViewingArchived && isArchived)
                    {
                        _goals.Add(goalViewModel);
                    }
                    else if (!_isViewingArchived && !isArchived)
                    {
                        _goals.Add(goalViewModel);
                    }
                }
            }
            
            UpdateEmptyState();
            UpdateNavigationButtons();
        }
        
        private void UpdateNavigationButtons()
        {
            if (_isViewingArchived)
            {
                UpcomingButton.Background = new SolidColorBrush(Colors.Transparent);
                ArchivedButton.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // #2196F3
                AddGoalBorder.Visibility = Visibility.Collapsed;
            }
            else
            {
                UpcomingButton.Background = new SolidColorBrush(Color.FromRgb(33, 150, 243)); // #2196F3
                ArchivedButton.Background = new SolidColorBrush(Colors.Transparent);
                AddGoalBorder.Visibility = Visibility.Visible;
            }
        }
        
        private void UpcomingButton_Click(object sender, RoutedEventArgs e)
        {
            _isViewingArchived = false;
            FilterGoalsByView();
        }
        
        private void ArchivedButton_Click(object sender, RoutedEventArgs e)
        {
            _isViewingArchived = true;
            FilterGoalsByView();
        }

        private void UpdateEmptyState()
        {
            EmptyStateText.Visibility = _goals.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            GoalsItemsControl.Visibility = _goals.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
            
            // Update empty state text based on view
            if (_goals.Count == 0)
            {
                EmptyStateText.Text = _isViewingArchived 
                    ? "No archived goals yet." 
                    : "No goals yet. Add your first goal above!";
            }
        }

        private void AddGoalButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var goalName = GoalNameTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(goalName) || goalName == "Enter goal name...")
                {
                    MessageBox.Show("Please enter a goal name.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (StartDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Please select a start date.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (DeadlineDatePicker.SelectedDate == null)
                {
                    MessageBox.Show("Please select a deadline.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var startDate = StartDatePicker.SelectedDate.Value.Date;
                var deadline = DeadlineDatePicker.SelectedDate.Value.Date;

                if (deadline < startDate)
                {
                    MessageBox.Show("Deadline must be after the start date.", "Validation Error", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!string.IsNullOrEmpty(_editingGoalId))
                {
                    // Update existing goal
                    var existingGoal = _goals.FirstOrDefault(g => g.Goal?.Id == _editingGoalId);
                    if (existingGoal != null && existingGoal.Goal != null)
                    {
                        var goalType = GoalTypeComboBox.SelectedItem is ComboBoxItem item && item.Tag?.ToString() == "Quantitative" 
                            ? GoalType.Quantitative 
                            : GoalType.Qualitative;
                        
                        // Get selected color from button tag
                        var selectedColor = ProgressColorButton.Tag as Color? ?? Color.FromRgb(33, 150, 243);
                        
                        existingGoal.Goal.Name = goalName;
                        existingGoal.Goal.Type = goalType;
                        existingGoal.Goal.StartDate = startDate;
                        existingGoal.Goal.Deadline = deadline;
                        existingGoal.Goal.ProgressColorArgb = (int)((selectedColor.A << 24) | (selectedColor.R << 16) | (selectedColor.G << 8) | selectedColor.B);
                        
                        // Treat all goals the same - always update target/current amounts
                        if (double.TryParse(TargetAmountTextBox.Text, out var targetAmount) && targetAmount > 0)
                        {
                            existingGoal.Goal.TargetAmount = targetAmount;
                        }
                        else
                        {
                            existingGoal.Goal.TargetAmount = null;
                        }
                        
                        if (double.TryParse(CurrentAmountTextBox.Text, out var currentAmount))
                        {
                            existingGoal.Goal.CurrentAmount = currentAmount;
                        }
                        
                        _dataService.UpdateGoal(existingGoal.Goal);
                        existingGoal.Refresh();
                        
                        // Re-filter goals in case archive status changed
                        FilterGoalsByView();
                    }
                    CancelEdit();
                }
                else
                {
                    // Add new goal
                    var goalType = GoalTypeComboBox.SelectedItem is ComboBoxItem item && item.Tag?.ToString() == "Quantitative" 
                        ? GoalType.Quantitative 
                        : GoalType.Qualitative;
                    
                    // Get selected color from button tag
                    var selectedColor = ProgressColorButton.Tag as Color? ?? Color.FromRgb(33, 150, 243);
                    
                    var newGoal = new Goal
                    {
                        Name = goalName,
                        Type = goalType,
                        StartDate = startDate,
                        Deadline = deadline,
                        ProgressColorArgb = (int)((selectedColor.A << 24) | (selectedColor.R << 16) | (selectedColor.G << 8) | selectedColor.B)
                    };
                    
                    // Treat all goals the same - always set target/current amounts
                    if (double.TryParse(TargetAmountTextBox.Text, out var targetAmount) && targetAmount > 0)
                    {
                        newGoal.TargetAmount = targetAmount;
                    }
                    
                    if (double.TryParse(CurrentAmountTextBox.Text, out var currentAmount))
                    {
                        newGoal.CurrentAmount = currentAmount;
                    }
                    
                    // Add to UI synchronously - GoalViewModel constructor fires property changes
                    var goalViewModel = new GoalViewModel(newGoal);
                    _allGoals.Add(goalViewModel);
                    
                    // Only add to visible goals if it's not archived
                    if (!newGoal.IsArchived && !_isViewingArchived)
                    {
                        _goals.Add(goalViewModel);
                    }
                    
                    UpdateEmptyState();
                    
                    // Save asynchronously to avoid blocking UI
                    System.Threading.Tasks.Task.Run(() =>
                    {
                        try
                        {
                            _dataService.AddGoal(newGoal);
                        }
                        catch (Exception ex)
                        {
                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                MessageBox.Show(
                                    $"Error saving goal to disk: {ex.Message}",
                                    "Save Error",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Warning);
                            }));
                        }
                    });
                }

                GoalNameTextBox.Text = "Enter goal name...";
                TargetAmountTextBox.Text = string.Empty;
                CurrentAmountTextBox.Text = string.Empty;
                StartDatePicker.SelectedDate = DateTime.Today;
                DeadlineDatePicker.SelectedDate = DateTime.Today.AddDays(30);
                
                // Reset progress color to default
                var defaultColor = Color.FromRgb(33, 150, 243);
                ProgressColorPreview.Background = new SolidColorBrush(defaultColor);
                ProgressColorButton.Tag = defaultColor;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error saving goal: {ex.Message}\n\n{ex.StackTrace}",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void EditGoalButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is GoalViewModel goalViewModel && goalViewModel.Goal != null)
            {
                _editingGoalId = goalViewModel.Goal.Id;
                GoalNameTextBox.Text = goalViewModel.Goal.Name;
                StartDatePicker.SelectedDate = goalViewModel.Goal.StartDate;
                DeadlineDatePicker.SelectedDate = goalViewModel.Goal.Deadline;
                
                // Set goal type
                GoalTypeComboBox.SelectedIndex = goalViewModel.Goal.Type == GoalType.Quantitative ? 1 : 0;
                
                // Set quantitative fields
                TargetAmountTextBox.Text = goalViewModel.Goal.TargetAmount?.ToString() ?? string.Empty;
                CurrentAmountTextBox.Text = goalViewModel.Goal.CurrentAmount.ToString();
                
                // Set progress color
                if (goalViewModel.Goal.ProgressColorArgb.HasValue)
                {
                    var argb = goalViewModel.Goal.ProgressColorArgb.Value;
                    var color = Color.FromArgb(
                        (byte)((argb >> 24) & 0xFF),
                        (byte)((argb >> 16) & 0xFF),
                        (byte)((argb >> 8) & 0xFF),
                        (byte)(argb & 0xFF));
                    ProgressColorPreview.Background = new SolidColorBrush(color);
                    ProgressColorButton.Tag = color;
                }
                else
                {
                    var defaultColor = Color.FromRgb(33, 150, 243);
                    ProgressColorPreview.Background = new SolidColorBrush(defaultColor);
                    ProgressColorButton.Tag = defaultColor;
                }
                
                AddGoalButton.Content = "Update Goal";
                CancelEditButton.Visibility = Visibility.Visible;
                GoalNameTextBox.Focus();
            }
        }

        private void DeleteGoalButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is GoalViewModel goalViewModel && goalViewModel.Goal != null)
            {
                var result = MessageBox.Show(
                    $"Are you sure you want to delete '{goalViewModel.Goal.Name}'?",
                    "Confirm Delete",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                {
                    _dataService.DeleteGoal(goalViewModel.Goal.Id);
                    _allGoals.Remove(goalViewModel);
                    _goals.Remove(goalViewModel);
                    UpdateEmptyState();
                }
            }
        }

        private void CancelEditButton_Click(object sender, RoutedEventArgs e)
        {
            CancelEdit();
        }

        private void CancelEdit()
        {
            _editingGoalId = null;
            GoalNameTextBox.Text = "Enter goal name...";
            GoalTypeComboBox.SelectedIndex = 0;
            TargetAmountTextBox.Text = string.Empty;
            CurrentAmountTextBox.Text = string.Empty;
            StartDatePicker.SelectedDate = DateTime.Today;
            DeadlineDatePicker.SelectedDate = DateTime.Today.AddDays(30);
            
            // Reset progress color to default
            var defaultColor = Color.FromRgb(33, 150, 243);
            ProgressColorPreview.Background = new SolidColorBrush(defaultColor);
            ProgressColorButton.Tag = defaultColor;
            
            AddGoalButton.Content = "Add Goal";
            CancelEditButton.Visibility = Visibility.Collapsed;
        }
        
        private void ProgressColorButton_Click(object sender, RoutedEventArgs e)
        {
            // Use WPF ColorPicker or create a simple color selection dialog
            // For now, let's use a simple approach with predefined colors
            var colorMenu = new ContextMenu();
            
            var colors = new[]
            {
                ("Blue", Color.FromRgb(33, 150, 243)),
                ("Green", Color.FromRgb(76, 175, 80)),
                ("Orange", Color.FromRgb(255, 152, 0)),
                ("Red", Color.FromRgb(244, 67, 54)),
                ("Purple", Color.FromRgb(156, 39, 176)),
                ("Teal", Color.FromRgb(0, 150, 136)),
                ("Pink", Color.FromRgb(233, 30, 99)),
                ("Yellow", Color.FromRgb(255, 235, 59))
            };
            
            foreach (var (name, color) in colors)
            {
                var menuItem = new MenuItem
                {
                    Tag = color
                };
                
                var colorRect = new System.Windows.Shapes.Rectangle
                {
                    Width = 20,
                    Height = 20,
                    Fill = new SolidColorBrush(color),
                    Margin = new Thickness(0, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };
                
                var headerPanel = new StackPanel { Orientation = Orientation.Horizontal };
                headerPanel.Children.Add(colorRect);
                headerPanel.Children.Add(new TextBlock { Text = name });
                menuItem.Header = headerPanel;
                
                menuItem.Click += (s, args) =>
                {
                    ProgressColorPreview.Background = new SolidColorBrush(color);
                    ProgressColorButton.Tag = color;
                    colorMenu.IsOpen = false;
                };
                
                colorMenu.Items.Add(menuItem);
            }
            
            ProgressColorButton.ContextMenu = colorMenu;
            colorMenu.PlacementTarget = ProgressColorButton;
            colorMenu.Placement = PlacementMode.Bottom;
            colorMenu.IsOpen = true;
        }
        
        private void GoalTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GoalTypeComboBox.SelectedItem is ComboBoxItem item)
            {
                bool isQuantitative = item.Tag?.ToString() == "Quantitative";
                QuantitativeFieldsGrid.Visibility = isQuantitative ? Visibility.Visible : Visibility.Collapsed;
            }
        }
        
        private void AccomplishedCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is GoalViewModel goalViewModel && goalViewModel.Goal != null)
            {
                goalViewModel.IsAchieved = true;
                // Save asynchronously to avoid blocking
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        _dataService.UpdateGoal(goalViewModel.Goal!);
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show(
                                $"Error saving goal: {ex.Message}",
                                "Save Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }));
                    }
                });
            }
        }
        
        private void AccomplishedCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox checkBox && checkBox.Tag is GoalViewModel goalViewModel && goalViewModel.Goal != null)
            {
                goalViewModel.IsAchieved = false;
                // Save asynchronously to avoid blocking
                System.Threading.Tasks.Task.Run(() =>
                {
                    try
                    {
                        _dataService.UpdateGoal(goalViewModel.Goal!);
                    }
                    catch (Exception ex)
                    {
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            MessageBox.Show(
                                $"Error saving goal: {ex.Message}",
                                "Save Error",
                                MessageBoxButton.OK,
                                MessageBoxImage.Warning);
                        }));
                    }
                });
            }
        }
        
        private void AddNoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is GoalViewModel goalViewModel && goalViewModel.Goal != null)
            {
                var dialog = new AddNoteDialog();
                if (dialog.ShowDialog() == true)
                {
                    var note = new Note
                    {
                        Content = dialog.NoteContent,
                        ProgressAmount = dialog.ProgressAmount,
                        CreatedDate = DateTime.Now
                    };
                    
                    goalViewModel.Goal.Notes.Add(note);
                    
                    // Update current amount if progress amount was provided
                    if (dialog.ProgressAmount.HasValue && dialog.ProgressAmount.Value > 0)
                    {
                        goalViewModel.Goal.CurrentAmount += dialog.ProgressAmount.Value;
                    }
                    
                    // Clear notes cache so it gets refreshed
                    goalViewModel.ClearNotesCache();
                    
                    _dataService.UpdateGoal(goalViewModel.Goal);
                    goalViewModel.Refresh();
                }
            }
        }
        
        private void DeleteNoteButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is Note note)
            {
                // Find the goal that contains this note
                var goalViewModel = _allGoals.FirstOrDefault(g => g.Goal?.Notes?.Any(n => n.Id == note.Id) == true);
                
                if (goalViewModel?.Goal != null)
                {
                    var result = MessageBox.Show(
                        "Are you sure you want to delete this note?",
                        "Confirm Delete",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
                    
                    if (result == MessageBoxResult.Yes)
                    {
                        // If the note had a progress amount, subtract it from current amount
                        if (note.ProgressAmount.HasValue && note.ProgressAmount.Value > 0)
                        {
                            goalViewModel.Goal.CurrentAmount = Math.Max(0, goalViewModel.Goal.CurrentAmount - note.ProgressAmount.Value);
                        }
                        
                        // Remove the note
                        goalViewModel.Goal.Notes.RemoveAll(n => n.Id == note.Id);
                        
                        // Clear notes cache so it gets refreshed
                        goalViewModel.ClearNotesCache();
                        
                        // Save changes
                        _dataService.UpdateGoal(goalViewModel.Goal);
                        goalViewModel.Refresh();
                    }
                }
            }
        }

        private void GoalNameTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (GoalNameTextBox.Text == "Enter goal name...")
            {
                GoalNameTextBox.Text = string.Empty;
            }
        }

        private void GoalNameTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GoalNameTextBox.Text))
            {
                GoalNameTextBox.Text = "Enter goal name...";
            }
        }

        private void UpdateTimer_Tick(object? sender, EventArgs e)
        {
            // Refresh all goals to update progress and days remaining
            foreach (var goalViewModel in _allGoals.ToList())
            {
                goalViewModel.Refresh();
            }
            
            // Re-filter goals in case any became archived
            FilterGoalsByView();
        }
    }

    public class GoalViewModel : System.ComponentModel.INotifyPropertyChanged
    {
        private Goal? _goal;
        private System.Collections.ObjectModel.ObservableCollection<Note>? _notes;
        private Brush? _cachedProgressColor;
        private double _cachedProgressPercentage = -1;

        public Goal? Goal
        {
            get => _goal;
            set
            {
                if (_goal == value) return;
                
                _goal = value;
                _notes = null; // Reset notes cache when goal changes
                _cachedProgressColor = null; // Reset brush cache
                OnPropertyChanged(nameof(Notes)); // Notify that notes changed
                _cachedProgressPercentage = -1;
                
                // Fire property changes synchronously - they should be fast now
                OnPropertyChanged(nameof(Goal));
                OnPropertyChanged(nameof(Name));
                OnPropertyChanged(nameof(StartDate));
                OnPropertyChanged(nameof(Deadline));
                OnPropertyChanged(nameof(CreatedDate));
                OnPropertyChanged(nameof(TargetAmount));
                OnPropertyChanged(nameof(CurrentAmount));
                OnPropertyChanged(nameof(IsQuantitative));
                OnPropertyChanged(nameof(IsQualitative));
                OnPropertyChanged(nameof(IsAchieved));
                OnPropertyChanged(nameof(IsArchived));
                OnPropertyChanged(nameof(Notes));
                OnPropertyChanged(nameof(TargetProgressPercentage));
                OnPropertyChanged(nameof(TimeProgressPercentage));
                OnPropertyChanged(nameof(MainProgressPercentage));
                OnPropertyChanged(nameof(ProgressPercentage));
                OnPropertyChanged(nameof(DaysRemaining));
                OnPropertyChanged(nameof(DaysRemainingText));
                OnPropertyChanged(nameof(ProgressColor));
                OnPropertyChanged(nameof(TimeProgressColor));
            }
        }

        public string Name => Goal?.Name ?? string.Empty;
        public DateTime StartDate => Goal?.StartDate ?? DateTime.Today;
        public DateTime Deadline => Goal?.Deadline ?? DateTime.Now;
        public DateTime CreatedDate => Goal?.CreatedDate ?? DateTime.Now;
        public double? TargetAmount => Goal?.TargetAmount;
        public double CurrentAmount => Goal?.CurrentAmount ?? 0;
        public bool IsQuantitative => Goal?.IsQuantitative ?? false;
        public bool IsQualitative => Goal?.Type == GoalType.Qualitative;
        public bool IsArchived => Goal?.IsArchived ?? false;
        public bool IsAchieved
        {
            get => Goal?.IsAchieved ?? false;
            set
            {
                if (Goal != null)
                {
                    Goal.IsAchieved = value;
                    OnPropertyChanged(nameof(IsAchieved));
                    OnPropertyChanged(nameof(MainProgressPercentage));
                }
            }
        }
        public System.Collections.ObjectModel.ObservableCollection<Note> Notes
        {
            get
            {
                if (_notes == null && Goal?.Notes != null)
                {
                    _notes = new System.Collections.ObjectModel.ObservableCollection<Note>(Goal.Notes.OrderByDescending(n => n.CreatedDate));
                }
                return _notes ?? new System.Collections.ObjectModel.ObservableCollection<Note>();
            }
        }
        
        public void ClearNotesCache()
        {
            _notes = null;
            OnPropertyChanged(nameof(Notes));
        }
        public double TargetProgressPercentage => Goal?.TargetProgressPercentage ?? 0;
        public double TimeProgressPercentage => Goal?.TimeProgressPercentage ?? 0;
        public double MainProgressPercentage => Goal?.TargetProgressPercentage ?? 0;
        public double ProgressPercentage => Goal?.ProgressPercentage ?? 0;
        public int DaysRemaining => Goal?.DaysRemaining ?? 0;
        public Brush TimeProgressColor
        {
            get
            {
                if (Goal == null) return new SolidColorBrush(Colors.Gray);
                
                // Use stored color if available, otherwise default to blue
                if (Goal.ProgressColorArgb.HasValue)
                {
                    var argb = Goal.ProgressColorArgb.Value;
                    var color = Color.FromArgb(
                        (byte)((argb >> 24) & 0xFF),
                        (byte)((argb >> 16) & 0xFF),
                        (byte)((argb >> 8) & 0xFF),
                        (byte)(argb & 0xFF));
                    return new SolidColorBrush(color);
                }
                
                // Default color if none set
                return new SolidColorBrush(Color.FromRgb(33, 150, 243)); // #2196F3
            }
        }
        public string DaysRemainingText
        {
            get
            {
                if (Goal == null) return "0 days";
                var days = Goal.DaysRemaining;
                var label = Goal.DaysRemainingLabel;
                return $"{days} {label}";
            }
        }
        public Brush ProgressColor
        {
            get
            {
                if (Goal == null) return new SolidColorBrush(Colors.Gray);
                
                var progress = ProgressPercentage;
                var isOverdue = Goal.IsOverdue;
                
                // Cache brush if progress hasn't changed
                if (_cachedProgressColor != null && Math.Abs(_cachedProgressPercentage - progress) < 0.1)
                {
                    return _cachedProgressColor;
                }
                
                _cachedProgressPercentage = progress;
                if (isOverdue) 
                    _cachedProgressColor = new SolidColorBrush(Colors.Red);
                else if (progress > 75) 
                    _cachedProgressColor = new SolidColorBrush(Colors.Green);
                else if (progress > 50) 
                    _cachedProgressColor = new SolidColorBrush(Colors.Orange);
                else 
                    _cachedProgressColor = new SolidColorBrush(Colors.Blue);
                
                return _cachedProgressColor;
            }
        }

        public GoalViewModel(Goal goal)
        {
            // Set goal directly
            _goal = goal;
            
            // Fire property changes in batches to avoid blocking
            // First batch: Basic properties
            OnPropertyChanged(nameof(Goal));
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(StartDate));
            OnPropertyChanged(nameof(Deadline));
            OnPropertyChanged(nameof(CreatedDate));
            OnPropertyChanged(nameof(TargetAmount));
            OnPropertyChanged(nameof(CurrentAmount));
            OnPropertyChanged(nameof(IsQuantitative));
            OnPropertyChanged(nameof(IsQualitative));
            OnPropertyChanged(nameof(IsAchieved));
            OnPropertyChanged(nameof(IsArchived));
            
            // Defer progress calculations and UI-heavy operations
            if (System.Windows.Application.Current?.Dispatcher != null)
            {
                System.Windows.Application.Current.Dispatcher.BeginInvoke(new Action(() =>
                {
                    OnPropertyChanged(nameof(TargetProgressPercentage));
                    OnPropertyChanged(nameof(TimeProgressPercentage));
                    OnPropertyChanged(nameof(MainProgressPercentage));
                    OnPropertyChanged(nameof(ProgressPercentage));
                    OnPropertyChanged(nameof(DaysRemaining));
                    OnPropertyChanged(nameof(DaysRemainingText));
                    OnPropertyChanged(nameof(Notes));
                    OnPropertyChanged(nameof(ProgressColor));
                    OnPropertyChanged(nameof(TimeProgressColor));
                }), System.Windows.Threading.DispatcherPriority.Background);
            }
            else
            {
                // Fallback if dispatcher not available
                OnPropertyChanged(nameof(TargetProgressPercentage));
                OnPropertyChanged(nameof(TimeProgressPercentage));
                OnPropertyChanged(nameof(MainProgressPercentage));
                OnPropertyChanged(nameof(ProgressPercentage));
                OnPropertyChanged(nameof(DaysRemaining));
                OnPropertyChanged(nameof(DaysRemainingText));
            }
        }

        public void Refresh()
        {
            OnPropertyChanged(nameof(Name));
            OnPropertyChanged(nameof(StartDate));
            OnPropertyChanged(nameof(Deadline));
            OnPropertyChanged(nameof(CreatedDate));
            OnPropertyChanged(nameof(TargetAmount));
            OnPropertyChanged(nameof(CurrentAmount));
            OnPropertyChanged(nameof(IsQuantitative));
            OnPropertyChanged(nameof(IsQualitative));
            OnPropertyChanged(nameof(IsAchieved));
            OnPropertyChanged(nameof(IsArchived));
            OnPropertyChanged(nameof(Notes));
            OnPropertyChanged(nameof(TargetProgressPercentage));
            OnPropertyChanged(nameof(TimeProgressPercentage));
            OnPropertyChanged(nameof(MainProgressPercentage));
            OnPropertyChanged(nameof(ProgressPercentage));
            OnPropertyChanged(nameof(DaysRemaining));
            OnPropertyChanged(nameof(DaysRemainingText));
            OnPropertyChanged(nameof(ProgressColor));
            OnPropertyChanged(nameof(TimeProgressColor));
        }

        public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
}
