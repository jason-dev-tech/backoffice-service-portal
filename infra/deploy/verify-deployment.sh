#!/usr/bin/env bash
set -euo pipefail

log() {
  printf '[verify-deployment] %s\n' "$1"
}

fail() {
  printf '[verify-deployment] FAIL: %s\n' "$1" >&2
  exit 1
}

pass() {
  printf '[verify-deployment] OK: %s\n' "$1"
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

if [[ -n "${HEALTHCHECK_URL:-}" ]]; then
  require_command curl
fi

SSH_TARGET="${DEPLOY_USER}@${DEPLOY_HOST}"
SSH_OPTS=(-i "$SSH_KEY_PATH" -o IdentitiesOnly=yes)

log "Checking SSH connectivity to ${SSH_TARGET}"
ssh "${SSH_OPTS[@]}" "$SSH_TARGET" "true" >/dev/null
pass "SSH connectivity is available"

log "Checking Docker availability on remote host"
ssh "${SSH_OPTS[@]}" "$SSH_TARGET" "docker --version >/dev/null"
pass "Docker is available remotely"

log "Checking deployment directory at ${DEPLOY_DIR}"
ssh "${SSH_OPTS[@]}" "$SSH_TARGET" "test -d '$DEPLOY_DIR'"
pass "Deployment directory exists remotely"

log "Checking remote Docker Compose services"
ssh "${SSH_OPTS[@]}" "$SSH_TARGET" "cd '$DEPLOY_DIR' && docker compose ps"
pass "Remote Docker Compose status command completed"

if [[ -n "${HEALTHCHECK_URL:-}" ]]; then
  log "Checking application health at ${HEALTHCHECK_URL}"
  curl --fail --silent --show-error --location --max-time 15 "$HEALTHCHECK_URL" >/dev/null
  pass "Application health check succeeded"
else
  log "Skipping HTTP health check because HEALTHCHECK_URL is not set"
fi

pass "Deployment verification complete"
