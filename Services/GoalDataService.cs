using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Deadline.Models;

namespace Deadline.Services
{
    public class GoalDataService
    {
        private readonly string _dataFilePath;
        private List<Goal> _goals;

        public GoalDataService()
        {
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "YearDeadline"
            );
            
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _dataFilePath = Path.Combine(appDataPath, "goals.json");
            _goals = LoadGoals();
        }

        public List<Goal> GetAllGoals()
        {
            return _goals.OrderBy(g => g.Deadline).ToList();
        }

        public void AddGoal(Goal goal)
        {
            _goals.Add(goal);
            SaveGoals();
        }

        public void UpdateGoal(Goal goal)
        {
            var existingGoal = _goals.FirstOrDefault(g => g.Id == goal.Id);
            if (existingGoal != null)
            {
                existingGoal.Name = goal.Name;
                existingGoal.Type = goal.Type;
                existingGoal.StartDate = goal.StartDate;
                existingGoal.Deadline = goal.Deadline;
                existingGoal.TargetAmount = goal.TargetAmount;
                existingGoal.CurrentAmount = goal.CurrentAmount;
                existingGoal.IsAchieved = goal.IsAchieved;
                existingGoal.Notes = goal.Notes ?? new List<Note>();
                existingGoal.ProgressColorArgb = goal.ProgressColorArgb;
                SaveGoals();
            }
        }

        public void DeleteGoal(string goalId)
        {
            _goals.RemoveAll(g => g.Id == goalId);
            SaveGoals();
        }

        private List<Goal> LoadGoals()
        {
            if (!File.Exists(_dataFilePath))
            {
                return new List<Goal>();
            }

            try
            {
                // Use async-safe file reading
                string json;
                using (var fileStream = new FileStream(_dataFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var reader = new StreamReader(fileStream))
                {
                    json = reader.ReadToEnd();
                }
                
                if (string.IsNullOrWhiteSpace(json))
                {
                    return new List<Goal>();
                }

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    ReadCommentHandling = JsonCommentHandling.Skip,
                    AllowTrailingCommas = true
                };

                var goals = JsonSerializer.Deserialize<List<Goal>>(json, options);
                
                // Filter out any null goals and validate data
                if (goals != null)
                {
                    goals = goals.Where(g => g != null && !string.IsNullOrWhiteSpace(g.Id)).ToList();
                    foreach (var goal in goals)
                    {
                    // Ensure CreatedDate is set if missing
                    if (goal.CreatedDate == default)
                    {
                        goal.CreatedDate = DateTime.Now;
                    }
                    // Ensure StartDate is set if missing (for backward compatibility)
                    if (goal.StartDate == default)
                    {
                        goal.StartDate = goal.CreatedDate.Date;
                    }
                    // Set default goal type for old goals (backward compatibility)
                    if (goal.Type == default(GoalType))
                    {
                        goal.Type = goal.TargetAmount.HasValue && goal.TargetAmount.Value > 0 
                            ? GoalType.Quantitative 
                            : GoalType.Qualitative;
                    }
                    }
                }
                
                return goals ?? new List<Goal>();
            }
            catch (Exception)
            {
                // If file is corrupted, try to backup and start fresh
                try
                {
                    var backupPath = _dataFilePath + ".backup." + DateTime.Now.ToString("yyyyMMddHHmmss");
                    File.Copy(_dataFilePath, backupPath, true);
                    File.Delete(_dataFilePath);
                }
                catch
                {
                    // Ignore backup errors
                }
                
                return new List<Goal>();
            }
        }

        private void SaveGoals()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                var json = JsonSerializer.Serialize(_goals, options);
                
                // Use async-safe file writing
                using (var fileStream = new FileStream(_dataFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
                using (var writer = new StreamWriter(fileStream))
                {
                    writer.Write(json);
                }
            }
            catch
            {
                // Handle save errors silently or log them
            }
        }
    }
}

