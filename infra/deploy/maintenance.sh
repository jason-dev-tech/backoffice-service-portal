#!/usr/bin/env bash
set -euo pipefail

log() {
  printf '[maintenance] %s\n' "$1"
}

fail() {
  printf '[maintenance] ERROR: %s\n' "$1" >&2
  exit 1
}

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    fail "missing required command: $1"
  fi
}

require_env() {
  if [[ -z "${!1:-}" ]]; then
    fail "required environment variable is not set: $1"
  fi
}

require_command ssh

require_env DEPLOY_HOST
require_env DEPLOY_USER
require_env DEPLOY_DIR
require_env SSH_KEY_PATH

SSH_TARGET="${DEPLOY_USER}@${DEPLOY_HOST}"
SSH_OPTS=(-i "$SSH_KEY_PATH" -o IdentitiesOnly=yes)

log "Running Docker maintenance on ${SSH_TARGET}"

ssh "${SSH_OPTS[@]}" "$SSH_TARGET" "DEPLOY_DIR='$DEPLOY_DIR' bash -s" <<'REMOTE_MAINTENANCE'
set -euo pipefail

cd "$DEPLOY_DIR"

printf '\n== disk usage before cleanup ==\n'
df -h /

printf '\n== docker disk usage before cleanup ==\n'
docker system df

printf '\n== pruning unused Docker images ==\n'
docker image prune -f

printf '\n== pruning stopped Docker containers ==\n'
docker container prune -f

printf '\n== pruning unused Docker builder cache ==\n'
docker builder prune -f

printf '\n== disk usage after cleanup ==\n'
df -h /

printf '\n== docker disk usage after cleanup ==\n'
docker system df
REMOTE_MAINTENANCE

log "Docker maintenance complete"
