#!/usr/bin/env bash
set -euo pipefail

DEPLOY_DIR="${DEPLOY_DIR:-/opt/backoffice-service-portal}"
TARGET_USER="${SUDO_USER:-$USER}"

log() {
  printf '[bootstrap] %s\n' "$1"
}

require_command() {
  if ! command -v "$1" >/dev/null 2>&1; then
    printf '[bootstrap] missing required command: %s\n' "$1" >&2
    exit 1
  fi
}

if [[ ! -r /etc/os-release ]]; then
  printf '[bootstrap] unable to detect operating system\n' >&2
  exit 1
fi

. /etc/os-release

if [[ "${ID:-}" != "ubuntu" ]]; then
  printf '[bootstrap] this script is intended for Ubuntu EC2 instances\n' >&2
  exit 1
fi

require_command sudo
require_command curl

log "Updating apt package index"
sudo apt-get update

log "Installing required packages"
sudo apt-get install -y ca-certificates curl gnupg

log "Configuring Docker apt repository"
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL "https://download.docker.com/linux/ubuntu/gpg" | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg

ARCH="$(dpkg --print-architecture)"
CODENAME="${VERSION_CODENAME:-}"

if [[ -z "$CODENAME" ]]; then
  printf '[bootstrap] unable to detect Ubuntu version codename\n' >&2
  exit 1
fi

printf 'deb [arch=%s signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu %s stable\n' "$ARCH" "$CODENAME" |
  sudo tee /etc/apt/sources.list.d/docker.list >/dev/null

log "Installing Docker and Docker Compose plugin"
sudo apt-get update
sudo apt-get install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin

log "Enabling and starting Docker"
sudo systemctl enable docker
sudo systemctl start docker

log "Adding ${TARGET_USER} to docker group"
sudo usermod -aG docker "$TARGET_USER"

log "Creating deployment directory at ${DEPLOY_DIR}"
sudo install -d -o "$TARGET_USER" -g docker -m 2775 "$DEPLOY_DIR"

log "Bootstrap complete"
log "Log out and log back in before running Docker commands without sudo"
