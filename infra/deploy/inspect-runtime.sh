#!/usr/bin/env bash
set -euo pipefail

log() {
  printf '[inspect-runtime] %s\n' "$1"
}

fail() {
  printf '[inspect-runtime] ERROR: %s\n' "$1" >&2
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

log "Inspecting runtime on ${SSH_TARGET}"

ssh "${SSH_OPTS[@]}" "$SSH_TARGET" "DEPLOY_DIR='$DEPLOY_DIR' bash -s" <<'REMOTE_INSPECT'
set -euo pipefail

cd "$DEPLOY_DIR"

printf '\n== docker compose ps ==\n'
docker compose ps

printf '\n== backend logs tail ==\n'
docker compose logs backend --tail=100

printf '\n== compose service images ==\n'
docker compose config --services | while IFS= read -r service_name; do
  image_name="$(docker compose config | awk -v service="$service_name" '
    $1 == service ":" { in_service = 1; next }
    in_service && $1 == "image:" { print $2; exit }
    in_service && /^[[:alnum:]_-]+:$/ { exit }
  ')"
  printf '%s %s\n' "$service_name" "${image_name:-<no image>}"
done

printf '\n== disk usage ==\n'
df -h /

printf '\n== docker disk usage ==\n'
docker system df
REMOTE_INSPECT

log "Runtime inspection complete"
