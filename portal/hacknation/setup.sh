#!/usr/bin/env bash
# Prepares the local configuration (.env, mcod/settings/local.py) for running the portal on localhost.
# Run from the repository root: bash hacknation/setup.sh
set -euo pipefail
cp hacknation/env.hacknation .env
cat mcod/settings/local.py.sample hacknation/local-overrides.py > mcod/settings/local.py
echo "OK: .env and mcod/settings/local.py"
