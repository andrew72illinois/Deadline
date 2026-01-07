# Deadline

A Windows desktop application for tracking goals with deadlines, featuring circular progress meters, notes, and automatic persistence. Supports both quantitative and qualitative goal tracking with customizable progress colors and theme support.

## Features

- ✅ **Goal Types**: 
  - **Qualitative Goals**: Track completion with an accomplished checkbox
  - **Quantitative Goals**: Track progress with target and current amounts
- 📊 **Circular Progress Meters**: Visual progress indicators showing time-based and target-based completion
- ⏰ **Days Remaining**: Real-time countdown to each deadline (updates every minute)
- 📝 **Notes System**: Add notes to goals with optional progress amounts for quantitative goals
- 🎨 **Custom Progress Colors**: Choose custom colors for each goal's progress indicator
- 🌓 **Theme Support**: Light and dark themes with persistent theme preference
- 📅 **Start Dates & Deadlines**: Track goals with both start dates and deadlines
- 📦 **Archived Goals View**: View and manage completed/archived goals separately
- 💾 **Data Persistence**: Goals, notes, and settings are automatically saved and loaded from local storage
- ✏️ **Edit Goals**: Update goal names, types, dates, targets, and colors
- 🗑️ **Delete Goals**: Remove goals you no longer need

## Getting Started

### Prerequisites

1. **Install .NET SDK 8.0** (required)
   - Download from: https://dotnet.microsoft.com/download
   - Version 8.0 or later required
   - Verify installation: `dotnet --version`

2. **Install Visual Studio** (recommended) or Visual Studio Code
   - Visual Studio Community (free): https://visualstudio.microsoft.com/
   - Include ".NET desktop development" workload with WPF support
   - Or use VS Code with C# extension
   - **Note**: This is a WPF application and requires Windows

### Running the Application

```bash
dotnet run
```

Or open in Visual Studio and press F5.

### Data Storage

Application data is automatically saved to:
```
%LocalAppData%\YearDeadline\
├── goals.json      # Goals and notes data
└── theme.json      # Theme preference (light/dark)
```

Your goals, notes, and theme preference persist between application sessions - no need to re-enter them!

## Usage

1. **Add a Goal**: 
   - Enter a goal name in the text box
   - Select a start date and deadline using the date pickers
   - Choose goal type (Qualitative or Quantitative)
   - For quantitative goals: set target and current amounts
   - Optionally choose a custom progress color
   - Click "Add Goal"

2. **View Progress**: 
   - Each goal displays a circular progress meter
   - The number of days remaining is shown in the center
   - For qualitative goals: progress is based on time elapsed or accomplishment status
   - For quantitative goals: progress shows both time-based and target-based percentages
   - Goals automatically archive after the deadline passes

3. **Add Notes**: 
   - Expand the "Notes" section on any goal
   - Click "Add Note" to add a note with optional progress amount
   - Notes can include progress amounts for quantitative goals
   - Delete notes using the × button

4. **Edit a Goal**: 
   - Click the "Edit" button on any goal (not available for archived goals)
   - Modify the name, type, dates, targets, or color
   - Click "Add Goal" to save changes (button text changes during edit)

5. **View Archived Goals**: 
   - Click the "Archived" button in the header
   - View all goals that have passed their deadline
   - Archived goals show 100% progress

6. **Change Theme**: 
   - Click the settings (⚙) button in the header
   - Toggle between light and dark themes
   - Theme preference is saved automatically

7. **Delete a Goal**: 
   - Click the "Delete" button on any goal
   - The goal is immediately removed

## Project Structure

```
Deadline/
├── Models/
│   ├── Goal.cs              # Goal data model with progress calculations
│   └── Note.cs              # Note data model
├── Services/
│   ├── GoalDataService.cs   # Data persistence service (JSON)
│   └── ThemeService.cs      # Theme management service
├── Controls/
│   └── CircularProgress.xaml # Custom circular progress meter control
├── Converters/
│   └── BoolToVisibilityConverter.cs # Value converters for UI
├── MainWindow.xaml          # Main application window UI
├── MainWindow.xaml.cs       # Main window logic and event handlers
├── AddNoteDialog.xaml       # Note creation dialog
├── App.xaml                 # Application resources and styling
└── Deadline.csproj          # Project configuration
```

## Technology Stack

- **.NET 8.0** - Runtime framework (targeting `net8.0-windows`)
- **WPF (Windows Presentation Foundation)** - UI framework (Windows-only)
- **System.Text.Json** - JSON serialization for data persistence
- **C#** - Programming language with nullable reference types enabled
- **XAML** - UI markup language for WPF

## Deployment

The application can be published as a self-contained executable for Windows. See [DEPLOYMENT.md](DEPLOYMENT.md) for detailed deployment instructions.

**Quick publish command:**
```powershell
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:EnableCompressionInSingleFile=true -o ./publish/win-x64
```

Or use the provided script:
```powershell
.\publish-windows.ps1
```

**Note**: This is a Windows-only application. WPF does not support Linux or macOS deployment.
