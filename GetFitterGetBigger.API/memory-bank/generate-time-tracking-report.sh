#!/bin/bash

# Generate Time Tracking Report for AI Assistance Impact
# This script analyzes completed features to calculate AI impact on development time

echo "═══════════════════════════════════════════════════════════════════"
echo "                    AI Assistance Impact Report                     "
echo "═══════════════════════════════════════════════════════════════════"
echo ""
echo "Generated: $(date '+%Y-%m-%d %H:%M:%S')"
echo ""

# Function to extract time in minutes from duration string (e.g., "2h 30m" -> 150)
extract_minutes() {
    local duration="$1"
    local hours=$(echo "$duration" | grep -oP '\d+(?=h)' || echo "0")
    local minutes=$(echo "$duration" | grep -oP '\d+(?=m)' || echo "0")
    echo $((hours * 60 + minutes))
}

# Initialize counters
total_features=0
total_estimated_minutes=0
total_actual_minutes=0
features_with_time_data=0

echo "COMPLETED FEATURES ANALYSIS:"
echo "───────────────────────────────────────────────────────────────────"

# Process each completed feature
for feature_dir in features/3-COMPLETED/*/; do
    if [ -d "$feature_dir" ]; then
        feature_name=$(basename "$feature_dir")
        tasks_file="$feature_dir/feature-tasks.md"
        
        if [ -f "$tasks_file" ]; then
            total_features=$((total_features + 1))
            
            # Extract time summary if available
            estimated_time=$(grep -oP 'Total Estimated Time:.*?(\d+)\s*hours?' "$tasks_file" | grep -oP '\d+' | tail -1)
            actual_time=$(grep -oP 'Total Actual Time:.*?(\d+\.?\d*)\s*hours?' "$tasks_file" | grep -oP '\d+\.?\d*' | tail -1)
            ai_impact=$(grep -oP 'AI Assistance Impact:.*?(\d+\.?\d*)%' "$tasks_file" | grep -oP '\d+\.?\d*' | tail -1)
            
            if [ ! -z "$estimated_time" ] && [ ! -z "$actual_time" ]; then
                features_with_time_data=$((features_with_time_data + 1))
                
                # Convert hours to minutes
                est_minutes=$((estimated_time * 60))
                act_minutes=$(echo "$actual_time * 60" | bc | cut -d. -f1)
                
                total_estimated_minutes=$((total_estimated_minutes + est_minutes))
                total_actual_minutes=$((total_actual_minutes + act_minutes))
                
                # Calculate savings
                saved_minutes=$((est_minutes - act_minutes))
                saved_percentage=$(echo "scale=1; ($saved_minutes * 100) / $est_minutes" | bc)
                
                echo "Feature: $feature_name"
                echo "  Estimated: ${estimated_time}h | Actual: ${actual_time}h | Saved: $(echo "scale=1; $saved_minutes / 60" | bc)h (${saved_percentage}%)"
                
                # Show task-level details if available
                task_count=$(grep -c '\[Implemented:.*Duration:' "$tasks_file" || echo "0")
                if [ "$task_count" -gt 0 ]; then
                    echo "  Completed Tasks: $task_count"
                fi
                echo ""
            fi
        fi
    fi
done

echo "───────────────────────────────────────────────────────────────────"
echo ""

# Calculate overall statistics
if [ "$features_with_time_data" -gt 0 ]; then
    total_saved_minutes=$((total_estimated_minutes - total_actual_minutes))
    overall_percentage=$(echo "scale=1; ($total_saved_minutes * 100) / $total_estimated_minutes" | bc)
    
    echo "SUMMARY STATISTICS:"
    echo "───────────────────────────────────────────────────────────────────"
    echo "Total Completed Features: $total_features"
    echo "Features with Time Data: $features_with_time_data"
    echo ""
    echo "Total Estimated Time: $(echo "scale=1; $total_estimated_minutes / 60" | bc) hours"
    echo "Total Actual Time: $(echo "scale=1; $total_actual_minutes / 60" | bc) hours"
    echo "Total Time Saved: $(echo "scale=1; $total_saved_minutes / 60" | bc) hours"
    echo ""
    echo "Overall AI Impact: ${overall_percentage}% time reduction"
    echo ""
    
    # Calculate productivity multiplier
    productivity_multiplier=$(echo "scale=2; $total_estimated_minutes / $total_actual_minutes" | bc)
    echo "Productivity Multiplier: ${productivity_multiplier}x"
    echo "(Work that would take 1 day now takes $(echo "scale=1; 8 / $productivity_multiplier" | bc) hours)"
else
    echo "No completed features with time tracking data found."
    echo "Time tracking will be available as features are completed with the new process."
fi

echo ""
echo "═══════════════════════════════════════════════════════════════════"

# Additional insights
echo ""
echo "INSIGHTS & RECOMMENDATIONS:"
echo "───────────────────────────────────────────────────────────────────"

if [ "$features_with_time_data" -gt 0 ] && [ "$overall_percentage" ]; then
    if (( $(echo "$overall_percentage > 50" | bc -l) )); then
        echo "✓ Excellent AI utilization - achieving over 50% time reduction"
    elif (( $(echo "$overall_percentage > 30" | bc -l) )); then
        echo "✓ Good AI utilization - achieving 30-50% time reduction"
    else
        echo "⚠ Consider optimizing AI usage for better time savings"
    fi
    
    echo ""
    echo "Tips for maximizing AI assistance:"
    echo "- Use AI for boilerplate code generation"
    echo "- Let AI write comprehensive test suites"
    echo "- Leverage AI for documentation creation"
    echo "- Use AI to identify edge cases and potential bugs"
fi

echo ""
echo "Note: Time estimates are based on manual implementation without AI assistance."
echo "Actual times reflect implementation with AI support."