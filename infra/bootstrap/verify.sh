#!/usr/bin/env bash
set -euo pipefail

DEPLOY_DIR="${DEPLOY_DIR:-/opt/backoffice-service-portal}"

pass() {
  printf '[verify] OK: %s\n' "$1"
}

fail() {
  printf '[verify] FAIL: %s\n' "$1" >&2
  exit 1
}

if command -v docker >/dev/null 2>&1; then
  pass "Docker is installed"
else
  fail "Docker is not installed"
fi

if docker compose version >/dev/null 2>&1; then
  pass "Docker Compose plugin is available"
else
  fail "Docker Compose plugin is not available"
fi

if systemctl is-active --quiet docker; then
  pass "Docker service is active"
else
  fail "Docker service is not active"
fi

if [[ -d "$DEPLOY_DIR" ]]; then
  pass "Deployment directory exists at ${DEPLOY_DIR}"
else
  fail "Deployment directory does not exist at ${DEPLOY_DIR}"
fi

pass "Server bootstrap verification complete"
