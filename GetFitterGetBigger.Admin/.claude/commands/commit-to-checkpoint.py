#!/usr/bin/env python3
"""
Commit to Checkpoint Command Implementation

This script:
1. Creates a git commit with analyzed changes
2. Finds the current checkpoint based on task completion
3. Adds the commit hash to the checkpoint
"""

import os
import re
import subprocess
import sys
from datetime import datetime
from pathlib import Path


def run_command(cmd):
    """Execute a shell command and return output."""
    try:
        result = subprocess.run(cmd, shell=True, capture_output=True, text=True)
        return result.stdout.strip(), result.returncode
    except Exception as e:
        return str(e), 1


def get_git_status():
    """Get current git status."""
    output, code = run_command("git status --porcelain")
    if code != 0:
        return None
    return output


def get_git_diff():
    """Get detailed diff of changes."""
    staged, _ = run_command("git diff --cached --stat")
    unstaged, _ = run_command("git diff --stat")
    return staged, unstaged


def find_current_checkpoint(feature_tasks_path):
    """
    Find the current checkpoint based on task completion status.
    Returns: (checkpoint_line_number, phase_name, is_complete)
    """
    if not os.path.exists(feature_tasks_path):
        return None, None, False
    
    with open(feature_tasks_path, 'r') as f:
        lines = f.readlines()
    
    # Find all checkpoints and tasks
    checkpoints = []
    current_phase = None
    last_completed_task_phase = None
    
    for i, line in enumerate(lines):
        # Track checkpoints
        if line.startswith("## CHECKPOINT:"):
            match = re.search(r"Phase (\d+)", line)
            if match:
                phase_num = int(match.group(1))
                # Check if checkpoint is complete or pending
                is_complete = False
                if i + 1 < len(lines):
                    next_line = lines[i + 1]
                    is_complete = "`[COMPLETE]`" in next_line
                checkpoints.append((i, phase_num, line.strip(), is_complete))
        
        # Track phase headers
        if line.startswith("## Phase "):
            match = re.search(r"Phase (\d+)", line)
            if match:
                current_phase = int(match.group(1))
        
        # Track completed tasks
        if line.startswith("### Task ") and current_phase:
            # Check if task is marked complete
            task_match = re.search(r"Task (\d+\.\d+):", line)
            if task_match:
                # Look for COMPLETE marker in next few lines
                for j in range(i, min(i + 5, len(lines))):
                    line_lower = lines[j].lower()
                    if "✅ complete" in line_lower or "`[complete]`" in line_lower:
                        last_completed_task_phase = current_phase
                        break
    
    # Determine which checkpoint to use
    if not last_completed_task_phase:
        # No completed tasks, don't update any checkpoint
        return None, None, False
    
    # Find the checkpoint for the phase of the last completed task
    for line_num, phase_num, checkpoint_text, is_complete in checkpoints:
        if phase_num == last_completed_task_phase:
            return line_num, f"Phase {phase_num}", is_complete
    
    return None, None, False


def update_checkpoint_with_commit(feature_tasks_path, commit_hash, message, checkpoint_line, phase_name):
    """Add or update Git Commits section in the checkpoint."""
    with open(feature_tasks_path, 'r') as f:
        lines = f.readlines()
    
    # Find where to insert Git Commits
    insert_line = None
    git_commits_line = None
    
    # Search for existing Git Commits section within this checkpoint
    # Look from checkpoint line downward until we hit the next major section
    for i in range(checkpoint_line, len(lines)):
        # Stop at next checkpoint or phase
        if i != checkpoint_line and (lines[i].startswith("## CHECKPOINT:") or 
                                     lines[i].startswith("## Phase ") or
                                     lines[i].startswith("---")):
            break
        
        # Found Git Commits section
        if lines[i].startswith("Git Commit"):
            git_commits_line = i
            break
        
        # Found Status line - insert before it
        if lines[i].startswith("Status:"):
            insert_line = i
    
    # Format the new commit entry
    new_commit_entry = f"- `{commit_hash}` - {message}\n"
    
    if git_commits_line is not None:
        # Git Commits section exists
        # Check if it's single line or multi-line format
        if "Git Commit:" in lines[git_commits_line]:
            # Single commit format - convert to multi-line
            # Check if there's an inline commit
            existing_match = re.search(r"`([a-f0-9]+)`\s*-\s*(.+)", lines[git_commits_line])
            if existing_match:
                # Convert single line with commit to multi-line
                lines[git_commits_line] = "Git Commits:\n"
                lines.insert(git_commits_line + 1, f"- `{existing_match.group(1)}` - {existing_match.group(2).strip()}\n")
                lines.insert(git_commits_line + 2, new_commit_entry)
            else:
                # Just "Git Commit:" with no commit yet
                lines[git_commits_line] = "Git Commits:\n"
                lines.insert(git_commits_line + 1, new_commit_entry)
        else:
            # Multi-line format "Git Commits:" - add to the list
            # Find insertion point (after last commit entry)
            insert_at = git_commits_line + 1
            while insert_at < len(lines):
                # Stop if we hit a non-commit line (not starting with "- `")
                if not lines[insert_at].strip().startswith("- `"):
                    # But skip empty lines between commits
                    if lines[insert_at].strip() == "":
                        insert_at += 1
                        continue
                    break
                insert_at += 1
            
            # Insert the new commit at the right position
            lines.insert(insert_at, new_commit_entry)
    else:
        # No Git Commits section exists, create one
        if insert_line:
            # Insert before Status line with proper spacing
            lines.insert(insert_line, "\nGit Commits:\n")
            lines.insert(insert_line + 2, new_commit_entry)
            lines.insert(insert_line + 3, "\n")
        else:
            # Couldn't find Status line, look for a good place
            # Find where this checkpoint section ends
            end_line = checkpoint_line + 1
            while end_line < len(lines):
                # Stop at next section marker
                if lines[end_line].startswith("---") or \
                   lines[end_line].startswith("## "):
                    break
                # Found Notes section - insert before it
                if lines[end_line].startswith("Notes:"):
                    lines.insert(end_line, "\nGit Commits:\n")
                    lines.insert(end_line + 2, new_commit_entry)
                    lines.insert(end_line + 3, "\n")
                    break
                end_line += 1
            else:
                # Fallback: insert at the end of checkpoint
                lines.insert(end_line - 1, "\nGit Commits:\n")
                lines.insert(end_line, new_commit_entry)
                lines.insert(end_line + 1, "\n")
    
    # Write updated content
    with open(feature_tasks_path, 'w') as f:
        f.writelines(lines)
    
    return True


def main():
    """Main execution flow."""
    # Find IN_PROGRESS feature
    base_dir = Path("/home/esqueleto/myWork/GetFitterGetBigger/GetFitterGetBigger.Admin")
    features_dir = base_dir / "memory-bank" / "features" / "2-IN_PROGRESS"
    
    if not features_dir.exists():
        print("❌ No IN_PROGRESS features directory found")
        return 1
    
    # Get the first IN_PROGRESS feature
    features = list(features_dir.glob("FEAT-*"))
    if not features:
        print("❌ No IN_PROGRESS feature found. Please use standard git commit command.")
        return 1
    
    feature_path = features[0]
    feature_name = feature_path.name
    feature_tasks_path = feature_path / "feature-tasks.md"
    
    print(f"✅ Found feature: {feature_name}")
    
    # Check for uncommitted changes
    status = get_git_status()
    if not status:
        print("❌ No uncommitted changes to commit")
        return 1
    
    # Find current checkpoint
    checkpoint_line, phase_name, is_complete = find_current_checkpoint(feature_tasks_path)
    
    if checkpoint_line is None:
        print("❌ Could not find a valid checkpoint to update")
        print("   No completed tasks found in the current feature")
        return 1
    
    if not is_complete:
        print(f"⚠️  {phase_name} checkpoint is still pending")
        print("   Will update once checkpoint is marked complete")
    
    print(f"📍 Current checkpoint: {phase_name}")
    
    # This script just validates - actual implementation by Claude
    print(f"\n✅ Ready to create commit and update checkpoint")
    print(f"   Feature: {feature_name}")
    print(f"   Checkpoint: {phase_name}")
    print(f"   Tasks file: {feature_tasks_path}")
    
    # Output for Claude to use
    print(f"\nFEATURE_PATH={feature_path}")
    print(f"CHECKPOINT_LINE={checkpoint_line}")
    print(f"PHASE_NAME={phase_name}")
    
    return 0


if __name__ == "__main__":
    sys.exit(main())