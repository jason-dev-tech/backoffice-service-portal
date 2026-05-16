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
```

Do not place secrets, credentials, or private values in these scripts.

## Deploy

Run from your workstation:

```sh
./deploy.sh
```

The script creates the remote deployment directory if it is missing, includes a placeholder for copying deployment artifacts, then runs:

```sh
docker compose pull
docker compose up -d
```

The remote deployment directory must already contain the required Docker Compose files and runtime configuration. Deployment will fail until artifact syncing is implemented if those files are missing.

## Verify

Run:

```sh
./verify-deployment.sh
```

The verification script checks SSH connectivity, remote Docker availability, and the remote deployment directory.

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
