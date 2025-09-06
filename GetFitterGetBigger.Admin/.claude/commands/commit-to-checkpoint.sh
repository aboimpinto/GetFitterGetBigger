#!/bin/bash

# Commit to Checkpoint - Implementation Script
# This script is executed by Claude when the /commit-to-checkpoint command is run

echo "🔍 Checking for IN_PROGRESS feature..."

# Find the IN_PROGRESS feature
FEATURE_DIR="/home/esqueleto/myWork/GetFitterGetBigger/GetFitterGetBigger.Admin/memory-bank/features/2-IN_PROGRESS"
if [ ! -d "$FEATURE_DIR" ]; then
    echo "❌ No IN_PROGRESS features directory found"
    exit 1
fi

# Get the first (and should be only) IN_PROGRESS feature
FEATURE=$(ls -d "$FEATURE_DIR"/FEAT-* 2>/dev/null | head -n 1)
if [ -z "$FEATURE" ]; then
    echo "❌ No IN_PROGRESS feature found. Please use standard git commit command."
    exit 1
fi

FEATURE_NAME=$(basename "$FEATURE")
FEATURE_TASKS="$FEATURE/feature-tasks.md"

echo "✅ Found feature: $FEATURE_NAME"

# Check for uncommitted changes
if [ -z "$(git status --porcelain)" ]; then
    echo "❌ No uncommitted changes to commit"
    exit 1
fi

echo "📝 Uncommitted changes detected"

# The actual implementation will be handled by Claude
# This script just sets up the environment and validates prerequisites

echo "FEATURE_PATH=$FEATURE"
echo "FEATURE_TASKS_PATH=$FEATURE_TASKS"
echo "READY_FOR_COMMIT=true"