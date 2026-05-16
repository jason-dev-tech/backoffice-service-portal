# EC2 Docker Deployment

This directory contains a lightweight deployment automation foundation for running Docker-based releases on an EC2 instance.

It assumes:

- Docker is already installed on the instance.
- The deployment bootstrap has already completed.
- The deployment directory exists or can be created by the deployment user.
- SSH access to the instance is available.

This is a simple operational foundation, not a full CI/CD platform yet.

## Workflow

1. SSH to the EC2 instance.
2. Sync deployment files into the deployment directory.
3. Pull or update container images.
4. Restart Docker Compose services.
5. Verify the deployment.

## Configuration

Set these environment variables before running the scripts:

```sh
export DEPLOY_HOST="<public-ip-or-dns-name>"
export DEPLOY_USER="<ssh-user>"
export DEPLOY_DIR="/opt/backoffice-service-portal"
export SSH_KEY_PATH="/path/to/private-key.pem"
export HEALTHCHECK_URL="https://<public-ip-or-dns-name>/health/ready"
export DEPLOY_IMAGE_REPOSITORY="ghcr.io/<owner>/<image>"
export DEPLOY_IMAGE_TAG="<commit-sha-or-latest>"
```

Do not place secrets, credentials, or private values in these scripts.

`HEALTHCHECK_URL` is optional. It can point to a backend readiness endpoint such as `/health/ready`.

`DEPLOY_IMAGE_REPOSITORY` and `DEPLOY_IMAGE_TAG` are consumed by `docker-compose.yml`. The CI workflow publishes both `latest` and the commit SHA; use the SHA tag for immutable deployments, or omit these values to use the compose defaults.

## Deploy

Run from your workstation:

```sh
./deploy.sh
```

The script creates the remote deployment directory if it is missing, syncs the tracked deployment artifact, then runs:

```sh
docker compose pull
docker compose up -d
```

Synced files:

- `docker-compose.yml`

The server must still provide runtime configuration required by the compose file, such as `.env` values, image repository/tag overrides, certificates, and any host-mounted files. Manage secrets and runtime environment values outside tracked files.

## Verify

Run:

```sh
./verify-deployment.sh
```

The verification script checks SSH connectivity, remote Docker availability, the remote deployment directory, and `docker compose ps`. If `HEALTHCHECK_URL` is set, it also checks the application health URL.

## Manual GitHub Actions Deployment

The repository includes a manual `Remote Deploy` workflow that runs the same verification and deployment scripts from GitHub Actions.

Configure these repository variables:

- `DEPLOY_HOST`
- `DEPLOY_USER`
- `DEPLOY_DIR`

Configure this repository secret:

- `SSH_PRIVATE_KEY`

Run the workflow manually from GitHub Actions when you want to verify the EC2 target and restart the remote Docker Compose services. This is a remote deployment foundation, not a full production CI/CD platform yet.

## Operational Notes

Stopping an EC2 instance pauses runtime charges for compute but does not remove all associated resources or costs. Destroying infrastructure removes resources and should be treated as a deliberate infrastructure lifecycle action.

If the instance does not use an Elastic IP, its public IP can change after stop/start. Update `DEPLOY_HOST` after the instance starts again.

Restrict SSH access to trusted CIDR ranges, protect private keys, avoid storing secrets in shell history, and review exposed ports before making services public.
