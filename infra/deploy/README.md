# EC2 Docker Deployment

This directory contains a lightweight deployment automation foundation for running Docker-based releases on an EC2 instance.

It assumes:

- Docker is already installed on the instance.
- The deployment bootstrap has already completed.
- The deployment directory exists or can be created by the deployment user.
- SSH access to the instance is available.

This is a simple operational foundation, not a full CI/CD platform yet.

See `RUNBOOK.md` for the end-to-end deployment checklist and troubleshooting commands.

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
export ALLOW_SELF_SIGNED_CERT=false
export DEPLOY_IMAGE_REPOSITORY="ghcr.io/<owner>/<image>"
export DEPLOY_IMAGE_TAG="<commit-sha-or-latest>"
```

Do not place secrets, credentials, or private values in these scripts.

`HEALTHCHECK_URL` is optional. It can point to a backend readiness endpoint such as `/health/ready`.

Set `ALLOW_SELF_SIGNED_CERT=true` only for development or self-signed certificate checks. Production should use a trusted CA certificate and should not rely on insecure TLS verification.

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
- `.env.example`

Use the tracked `docker-compose.yml` as the base deployment definition. For EC2-specific runtime changes, create untracked override files such as `docker-compose.override.yml` directly in `DEPLOY_DIR`. Do not edit the tracked `docker-compose.yml` on EC2 because `deploy.sh` will overwrite it on the next deployment.

The server must still provide runtime configuration required by the compose file, such as `.env` values, image repository/tag overrides, certificates, and any host-mounted files. Manage secrets and runtime environment values outside tracked files.

For HTTPS certificates, `HTTPS_CERT_HOST_PATH` is the source file on the EC2 host and `HTTPS_CERT_PATH` is the path inside the backend container that Kestrel reads. The certificate file must exist on the EC2 host before deployment. If the source path is wrong or missing, Docker can create a directory instead of mounting the intended certificate file.

On the EC2 instance, copy the example file once and fill in real runtime values on the server only:

```sh
cd /opt/backoffice-service-portal
cp .env.example .env
chmod 600 .env
```

Never commit `.env`, real secrets, certificates, or host-specific private values. Required runtime categories are PostgreSQL, JWT signing, HTTPS certificate host path/container path/password, and bootstrap admin settings. The deploy script checks that `.env` exists, includes the required variable names, and that `HTTPS_CERT_HOST_PATH` points to an existing certificate file before running `docker compose up -d`, but it does not validate secret strength.

## Verify

Run:

```sh
./verify-deployment.sh
```

The verification script checks SSH connectivity, remote Docker availability, the remote deployment directory, and `docker compose ps`. If `HEALTHCHECK_URL` is set, it also checks the application health URL.

For read-only runtime inspection, run:

```sh
./inspect-runtime.sh
```

The inspection helper prints Compose status, backend log tail, service image summary, and disk usage without printing `.env` contents.

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
