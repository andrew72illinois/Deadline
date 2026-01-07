using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Deadline.Controls
{
    public partial class CircularProgress : UserControl
    {
        private bool _isUpdating = false;
        private bool _isLayoutComplete = false;

        public static readonly DependencyProperty ProgressPercentageProperty =
            DependencyProperty.Register(nameof(ProgressPercentage), typeof(double), typeof(CircularProgress),
                new PropertyMetadata(0.0, OnProgressChanged));

        public static readonly DependencyProperty ProgressTextProperty =
            DependencyProperty.Register(nameof(ProgressText), typeof(string), typeof(CircularProgress),
                new PropertyMetadata("0%"));

        public static readonly DependencyProperty ProgressColorProperty =
            DependencyProperty.Register(nameof(ProgressColor), typeof(Brush), typeof(CircularProgress),
                new PropertyMetadata(new SolidColorBrush(Colors.Blue)));

        public static readonly DependencyProperty TextColorProperty =
            DependencyProperty.Register(nameof(TextColor), typeof(Brush), typeof(CircularProgress),
                new PropertyMetadata(new SolidColorBrush(Colors.Black)));

        public double ProgressPercentage
        {
            get => (double)GetValue(ProgressPercentageProperty);
            set => SetValue(ProgressPercentageProperty, value);
        }

        public string ProgressText
        {
            get => (string)GetValue(ProgressTextProperty);
            set => SetValue(ProgressTextProperty, value);
        }

        public Brush ProgressColor
        {
            get => (Brush)GetValue(ProgressColorProperty);
            set => SetValue(ProgressColorProperty, value);
        }

        public Brush TextColor
        {
            get => (Brush)GetValue(TextColorProperty);
            set => SetValue(TextColorProperty, value);
        }

        public CircularProgress()
        {
            InitializeComponent();
            Loaded += CircularProgress_Loaded;
            SizeChanged += CircularProgress_SizeChanged;
        }

        private void CircularProgress_Loaded(object sender, RoutedEventArgs e)
        {
            _isLayoutComplete = true;
            UpdateProgress();
        }

        private void CircularProgress_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (e.NewSize.Width > 0 && e.NewSize.Height > 0)
            {
                _isLayoutComplete = true;
            }
            UpdateProgress();
        }

        private static void OnProgressChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is CircularProgress control)
            {
                control.UpdateProgress();
            }
        }

        private void UpdateProgress()
        {
            // Prevent recursive calls and infinite loops
            if (_isUpdating) return;
            
            if (ProgressPath == null || BackgroundCircle == null) return;
            
            // Wait for layout to complete to get accurate size
            if (!_isLayoutComplete || BackgroundCircle.ActualWidth <= 0 || BackgroundCircle.ActualHeight <= 0)
            {
                // If not laid out yet, schedule update for after layout (only once)
                if (!_isUpdating)
                {
                    _isUpdating = true;
                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        _isUpdating = false;
                        UpdateProgress();
                    }), System.Windows.Threading.DispatcherPriority.Loaded);
                }
                return;
            }
            
            _isUpdating = true;
            try
            {
            
            // Use the actual rendered size
            var size = Math.Min(BackgroundCircle.ActualWidth, BackgroundCircle.ActualHeight);
            
            // Account for stroke thickness - the radius is measured to the center of the stroke
            var strokeThickness = 8.0; // Match the StrokeThickness in XAML
            var radius = (size - strokeThickness) / 2.0;
            var centerX = size / 2.0;
            var centerY = size / 2.0;
            
            // Clamp progress between 0 and 100
            var progress = Math.Min(100, Math.Max(0, ProgressPercentage));
            
            // Create a path geometry for the progress arc
            var pathGeometry = new PathGeometry();
            
            if (progress > 0)
            {
                // Special case: 100% should draw a complete circle
                if (progress >= 100)
                {
                    // Draw a complete circle using two 180-degree arcs
                    var startAngle = -90.0; // Start at top
                    var startAngleRad = startAngle * Math.PI / 180.0;
                    var midAngleRad = (startAngle + 180) * Math.PI / 180.0;
                    var endAngleRad = (startAngle + 360) * Math.PI / 180.0;
                    
                    // Calculate points
                    var startX = centerX + radius * Math.Cos(startAngleRad);
                    var startY = centerY + radius * Math.Sin(startAngleRad);
                    var midX = centerX + radius * Math.Cos(midAngleRad);
                    var midY = centerY + radius * Math.Sin(midAngleRad);
                    var endX = centerX + radius * Math.Cos(endAngleRad);
                    var endY = centerY + radius * Math.Sin(endAngleRad);
                    
                    // Create path with two arcs to complete the circle
                    var pathFigure = new PathFigure
                    {
                        StartPoint = new Point(startX, startY),
                        IsClosed = false
                    };
                    
                    // First half of the circle
                    pathFigure.Segments.Add(new ArcSegment
                    {
                        Point = new Point(midX, midY),
                        Size = new Size(radius, radius),
                        SweepDirection = SweepDirection.Clockwise,
                        IsLargeArc = false
                    });
                    
                    // Second half of the circle
                    pathFigure.Segments.Add(new ArcSegment
                    {
                        Point = new Point(endX, endY),
                        Size = new Size(radius, radius),
                        SweepDirection = SweepDirection.Clockwise,
                        IsLargeArc = false
                    });
                    
                    pathGeometry.Figures.Add(pathFigure);
                }
                else
                {
                    // Calculate the angle in degrees (0% = -90 degrees, 100% = 270 degrees)
                    // We start at -90 degrees (top) and go clockwise
                    var startAngle = -90.0; // Start at top
                    var sweepAngle = (progress / 100.0) * 360.0; // Sweep angle based on progress
                    
                    // Convert angles to radians
                    var startAngleRad = startAngle * Math.PI / 180.0;
                    var endAngleRad = (startAngle + sweepAngle) * Math.PI / 180.0;
                    
                    // Calculate start and end points on the circle
                    var startX = centerX + radius * Math.Cos(startAngleRad);
                    var startY = centerY + radius * Math.Sin(startAngleRad);
                    var endX = centerX + radius * Math.Cos(endAngleRad);
                    var endY = centerY + radius * Math.Sin(endAngleRad);
                    
                    // Create the path figure
                    var pathFigure = new PathFigure
                    {
                        StartPoint = new Point(startX, startY),
                        IsClosed = false
                    };
                    
                    // Create the arc segment
                    var arcSegment = new ArcSegment
                    {
                        Point = new Point(endX, endY),
                        Size = new Size(radius, radius),
                        SweepDirection = SweepDirection.Clockwise,
                        IsLargeArc = sweepAngle > 180
                    };
                    
                    pathFigure.Segments.Add(arcSegment);
                    pathGeometry.Figures.Add(pathFigure);
                }
            }
            
            // Set the path data
            ProgressPath.Data = pathGeometry;
            }
            finally
            {
                _isUpdating = false;
            }
        }
    }
}

