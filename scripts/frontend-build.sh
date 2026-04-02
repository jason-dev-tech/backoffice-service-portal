#!/usr/bin/env bash

set -euo pipefail

repo_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
frontend_dir="$repo_root/frontend"

if [[ ! -d "$frontend_dir" ]]; then
  echo "Frontend directory not found: $frontend_dir" >&2
  exit 1
fi

if [[ -z "${NVM_DIR:-}" ]]; then
  NVM_DIR="$HOME/.nvm"
fi

if [[ -s "$NVM_DIR/nvm.sh" ]]; then
  # shellcheck disable=SC1090
  . "$NVM_DIR/nvm.sh"

  if [[ -f "$repo_root/.nvmrc" ]]; then
    nvm use >/dev/null
  fi
fi

cd "$frontend_dir"

echo "Working directory: $(pwd)"
echo "Node: $(command -v node)"
node -v
echo "npm: $(command -v npm)"
npm -v
echo
npx ng version
echo
npm run build
