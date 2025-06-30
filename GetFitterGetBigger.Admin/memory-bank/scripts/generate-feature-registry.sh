#!/bin/bash
echo "# Feature Registry (Generated)"
echo "Generated on: $(date)"
echo ""
cd "$(dirname "$0")/../features" || exit 1
for stage_dir in 0-SUBMITTED 1-READY_TO_DEVELOP 2-IN_PROGRESS 3-COMPLETED 4-BLOCKED 5-SKIPPED; do
  if [ -d "$stage_dir" ] && ls "$stage_dir"/FEAT-* >/dev/null 2>&1; then
    echo "## ${stage_dir#*-}"
    for feat in "$stage_dir"/FEAT-*; do
      [ -d "$feat" ] && echo "- $(basename "$feat")"
    done
    echo ""
  fi
done
[ -f "NEXT_FEATURE_ID.txt" ] && echo "Next ID: FEAT-$(cat NEXT_FEATURE_ID.txt)"