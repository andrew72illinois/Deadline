using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;

namespace Deadline.Models
{
    public enum GoalType
    {
        Quantitative,
        Qualitative
    }

    public class Goal
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = string.Empty;
        public GoalType Type { get; set; } = GoalType.Qualitative;
        public DateTime StartDate { get; set; } = DateTime.Today;
        public DateTime Deadline { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        
        // Quantitative progress tracking
        public double? TargetAmount { get; set; }
        public double CurrentAmount { get; set; }
        
        // Qualitative progress tracking
        public bool IsAchieved { get; set; }
        
        // Notes
        public List<Note> Notes { get; set; } = new List<Note>();
        
        // Progress color (stored as ARGB integer)
        public int? ProgressColorArgb { get; set; }
        
        [JsonIgnore]
        public bool IsQuantitative => Type == GoalType.Quantitative;

        [JsonIgnore]
        public bool HasStarted => DateTime.Now >= StartDate;

        [JsonIgnore]
        public int DaysRemaining
        {
            get
            {
                if (!HasStarted)
                {
                    // Days until start date
                    var daysUntilStart = (StartDate - DateTime.Now).Days;
                    return daysUntilStart > 0 ? daysUntilStart : 0;
                }
                else
                {
                    // Days until deadline
                    var remaining = (Deadline - DateTime.Now).Days;
                    return remaining > 0 ? remaining : 0;
                }
            }
        }

        [JsonIgnore]
        public string DaysRemainingLabel
        {
            get
            {
                if (!HasStarted)
                {
                    return "days until start";
                }
                else
                {
                    return "days remaining";
                }
            }
        }

        [JsonIgnore]
        public double TargetProgressPercentage
        {
            get
            {
                // Only for quantitative goals
                if (!IsQuantitative || !TargetAmount.HasValue || TargetAmount.Value <= 0) return 0;
                var amountPercentage = (CurrentAmount / TargetAmount.Value) * 100;
                return Math.Min(100, Math.Max(0, amountPercentage));
            }
        }
        
        [JsonIgnore]
        public double TimeProgressPercentage
        {
            get
            {
                // If archived, always show 100%
                if (IsArchived)
                {
                    return 100;
                }
                
                if (!HasStarted)
                {
                    return 0;
                }

                var totalDays = (Deadline - StartDate).TotalDays;
                if (totalDays <= 0) return 100;

                var elapsedDays = (DateTime.Now - StartDate).TotalDays;
                var timePercentage = (elapsedDays / totalDays) * 100;
                return Math.Min(100, Math.Max(0, timePercentage));
            }
        }
        
        [JsonIgnore]
        public double ProgressPercentage => IsQuantitative ? TargetProgressPercentage : (IsAchieved ? 100 : TimeProgressPercentage);
        
        [JsonIgnore]
        public double TotalProgressFromNotes => Notes?.Where(n => n.ProgressAmount.HasValue).Sum(n => n.ProgressAmount!.Value) ?? 0;

        [JsonIgnore]
        public bool IsOverdue => DateTime.Now > Deadline && HasStarted && DaysRemaining == 0;
        
        [JsonIgnore]
        public bool IsArchived => DateTime.Now.Date > Deadline.Date;
    }
}

