#!/bin/bash

# Monitor File Watchers Script for BUG-004
# This script monitors inotify usage during test execution to help diagnose file watcher exhaustion

echo "=== File Watcher Monitoring Script for BUG-004 ==="
echo "Monitoring inotify usage during test execution..."
echo ""

# Function to get current inotify usage
get_inotify_stats() {
    echo "Current inotify statistics:"
    echo "  Max user instances: $(cat /proc/sys/fs/inotify/max_user_instances)"
    echo "  Max user watches: $(cat /proc/sys/fs/inotify/max_user_watches)"
    echo "  Max queued events: $(cat /proc/sys/fs/inotify/max_queued_events)"
    
    # Count current instances (requires lsof to be installed)
    if command -v lsof >/dev/null 2>&1; then
        local current_instances=$(lsof | grep inotify | wc -l)
        echo "  Current instances in use: $current_instances"
        
        # Calculate percentage
        local max_instances=$(cat /proc/sys/fs/inotify/max_user_instances)
        local percentage=$(( (current_instances * 100) / max_instances ))
        echo "  Usage percentage: ${percentage}%"
        
        if [ $current_instances -gt $((max_instances * 80 / 100)) ]; then
            echo "  ⚠️  WARNING: Close to limit!"
        fi
    else
        echo "  (lsof not available - cannot count current usage)"
    fi
    echo ""
}

# Function to monitor during test execution
monitor_tests() {
    echo "Starting test execution monitoring..."
    echo "Press Ctrl+C to stop monitoring"
    echo ""
    
    while true; do
        echo "$(date '+%H:%M:%S') - $(get_inotify_stats | grep 'Current instances')"
        sleep 2
    done
}

# Function to suggest fixes
suggest_fixes() {
    echo "=== Suggested Fixes for File Watcher Exhaustion ==="
    echo ""
    echo "1. TEMPORARY FIX - Increase system limits:"
    echo "   sudo sysctl fs.inotify.max_user_instances=1024"
    echo "   echo 'fs.inotify.max_user_instances = 1024' | sudo tee -a /etc/sysctl.conf"
    echo ""
    echo "2. PROPER FIX - Disable file watching in tests:"
    echo "   Modify test configuration to disable file providers"
    echo "   Tests don't need configuration hot reload"
    echo ""
    echo "3. OPTIMIZATION - Use shared test hosts:"
    echo "   Create one WebApplicationFactory shared across tests"
    echo "   Reduces resource usage significantly"
    echo ""
}

# Function to test system limits
test_limits() {
    echo "=== Testing System Limits ==="
    local max_instances=$(cat /proc/sys/fs/inotify/max_user_instances)
    echo "Your system can handle $max_instances inotify instances"
    echo ""
    
    if [ $max_instances -lt 256 ]; then
        echo "⚠️  Your limit ($max_instances) is quite low for development work."
        echo "Consider increasing it to at least 512 or 1024."
    elif [ $max_instances -lt 512 ]; then
        echo "ℹ️  Your limit ($max_instances) might be sufficient but could be increased."
    else
        echo "✅ Your limit ($max_instances) should be sufficient for most development work."
    fi
    echo ""
}

# Function to run a quick test
quick_test() {
    echo "=== Quick Test Run with Monitoring ==="
    get_inotify_stats
    
    echo "Running a small subset of tests to demonstrate the issue..."
    echo "dotnet test --filter 'Category=Controller' --logger 'console;verbosity=minimal'"
    echo ""
    
    # Store initial count
    if command -v lsof >/dev/null 2>&1; then
        local initial_count=$(lsof | grep inotify | wc -l)
        echo "Initial inotify instances: $initial_count"
        
        # Run tests (comment out if you don't want to actually run them)
        # dotnet test --filter "Category=Controller" --logger "console;verbosity=minimal"
        
        local final_count=$(lsof | grep inotify | wc -l)
        echo "Final inotify instances: $final_count"
        echo "Instances created during test: $((final_count - initial_count))"
    fi
}

# Main menu
case "${1:-menu}" in
    "stats")
        get_inotify_stats
        ;;
    "monitor")
        monitor_tests
        ;;
    "test")
        quick_test
        ;;
    "limits")
        test_limits
        ;;
    "fix")
        suggest_fixes
        ;;
    "menu"|*)
        echo "Usage: $0 [command]"
        echo ""
        echo "Commands:"
        echo "  stats   - Show current inotify statistics"
        echo "  monitor - Continuously monitor inotify usage"
        echo "  test    - Run a quick test with monitoring"
        echo "  limits  - Test and evaluate system limits"
        echo "  fix     - Show suggested fixes"
        echo ""
        echo "Example: $0 stats"
        echo ""
        get_inotify_stats
        test_limits
        ;;
esac