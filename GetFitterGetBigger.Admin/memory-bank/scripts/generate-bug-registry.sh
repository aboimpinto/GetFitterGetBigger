#!/bin/bash
echo "# Bug Registry (Generated)"
echo "Generated on: $(date)"
echo ""
cd "$(dirname "$0")/../bugs" || exit 1
for stage_dir in 1-OPEN 1-TODO 2-IN_PROGRESS 4-BLOCKED 3-FIXED 5-WONT_FIX; do
  if [ -d "$stage_dir" ] && ls "$stage_dir"/BUG-* >/dev/null 2>&1; then
    echo "## ${stage_dir#*-}"
    for bug in "$stage_dir"/BUG-*; do
      [ -d "$bug" ] && echo "- $(basename "$bug")"
    done
    echo ""
  fi
done
[ -f "NEXT_BUG_ID.txt" ] && echo "Next ID: BUG-$(cat NEXT_BUG_ID.txt)"