#!/usr/bin/env bash
set -euo pipefail

log() {
  printf '[deploy] %s\n' "$1"
}

fail() {
  printf '[deploy] ERROR: %s\n' "$1" >&2
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
require_command scp

require_env DEPLOY_HOST
require_env DEPLOY_USER
require_env DEPLOY_DIR
require_env SSH_KEY_PATH

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/../.." && pwd)"
DEPLOY_ARTIFACTS=("docker-compose.yml")

SSH_TARGET="${DEPLOY_USER}@${DEPLOY_HOST}"
SSH_OPTS=(-i "$SSH_KEY_PATH" -o IdentitiesOnly=yes)

for artifact in "${DEPLOY_ARTIFACTS[@]}"; do
  if [[ ! -f "${REPO_ROOT}/${artifact}" ]]; then
    fail "required deployment artifact is missing: ${artifact}"
  fi
done

log "Starting deployment to ${SSH_TARGET}"

log "Ensuring remote deployment directory exists at ${DEPLOY_DIR}"
ssh "${SSH_OPTS[@]}" "$SSH_TARGET" "mkdir -p '$DEPLOY_DIR'"

log "Syncing deployment artifacts"
for artifact in "${DEPLOY_ARTIFACTS[@]}"; do
  log "Copying ${artifact}"
  scp "${SSH_OPTS[@]}" "${REPO_ROOT}/${artifact}" "$SSH_TARGET:$DEPLOY_DIR/${artifact}"
done

log "Pulling container images"
ssh "${SSH_OPTS[@]}" "$SSH_TARGET" "cd '$DEPLOY_DIR' && docker compose pull"

log "Restarting Docker Compose services"
ssh "${SSH_OPTS[@]}" "$SSH_TARGET" "cd '$DEPLOY_DIR' && docker compose up -d"

log "Deployment command sequence complete"
log "Run ./verify-deployment.sh to verify remote readiness"
