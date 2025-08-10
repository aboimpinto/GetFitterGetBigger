# Git Commit and Push

Please use the git-commit-push agent to commit and push my changes to the remote repository.

This command triggers a specialized agent that handles the entire git workflow including:
- Checking current repository status
- Analyzing and categorizing changes
- Creating meaningful commit messages with proper conventions
- Safely pushing changes to the remote repository
- Handling any conflicts or errors that may occur

## Optional Parameters:
- If I specify "all" - stage all modified files
- If I specify "amend" - amend the previous commit
- If I specify a commit message - use it directly
- If I specify "force" - use with caution and confirm first

The agent will automatically:
- Follow the repository's commit message conventions
- Add the required signature from CLAUDE.md
- Ensure no sensitive data is committed
- Handle merge conflicts if they arise
- Provide clear feedback throughout the process