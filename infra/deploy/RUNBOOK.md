# EC2 Docker Deployment Runbook

This runbook covers the lightweight EC2 Docker deployment workflow.

## End-To-End Flow

1. CI builds the application image and publishes it to GHCR with `latest` and commit SHA tags.
2. The EC2 host pulls the selected image from GHCR.
3. `deploy.sh` syncs deployment artifacts to `DEPLOY_DIR`, validates runtime configuration, then runs `docker compose pull` and `docker compose up -d`.
4. `verify-deployment.sh` checks SSH, Docker availability, remote Compose status, and the optional health endpoint.

## Prerequisites

- EC2 instance is provisioned and reachable over SSH.
- Docker bootstrap has completed.
- GHCR login is completed on EC2 if the package is private.
- `.env` exists in `DEPLOY_DIR`, copied from `.env.example` and filled on the server.
- HTTPS certificate file exists on EC2 at `HTTPS_CERT_HOST_PATH`.
- Any environment-specific Compose overrides are kept in untracked files on EC2.

## Compose Overrides

Use the tracked `docker-compose.yml` as the base deployment definition. If an environment needs local runtime changes, create an untracked `docker-compose.override.yml` in `DEPLOY_DIR` using `docker-compose.override.yml.example` as a starting point.

Do not commit real `.env` files, secrets, certificates, or host-specific private values. Avoid editing the tracked `docker-compose.yml` directly on EC2 because `deploy.sh` overwrites it during deployment.

## Standard Commands

Run from the deployment directory on EC2:

```sh
docker compose ps
docker compose logs backend --tail=100
docker compose config
```

For development checks with a self-signed certificate:

```sh
curl -k https://<host>/health/ready
```

Production should use a trusted CA certificate instead of `-k`.

## Troubleshooting

`docker not found`

Docker is not installed or the current user cannot access it. Run the bootstrap process, then log out and back in so group membership refreshes.

`GHCR unauthorized`

The EC2 host cannot pull the image. Confirm the package visibility or complete `docker login ghcr.io` on the host with appropriate read access.

`missing .env`

Create runtime config on EC2:

```sh
cd /opt/backoffice-service-portal
cp .env.example .env
chmod 600 .env
```

Then fill real values on the server only.

`missing runtime variables`

Compare `.env` with `.env.example` and add the missing variable names. The deploy script checks presence, not secret strength.

`cert path is directory instead of file`

Docker may create a directory when the bind mount source path is wrong or missing. Remove the incorrect directory and place the certificate file at `HTTPS_CERT_HOST_PATH`.

`self-signed certificate curl failure`

For development verification only, use:

```sh
ALLOW_SELF_SIGNED_CERT=true ./verify-deployment.sh
```

Production should use a trusted CA certificate.

`backend restarting`

Check service status and recent logs:

```sh
docker compose ps
docker compose logs backend --tail=100
docker compose config
```

Common causes include invalid `.env` values, missing certificate files, database readiness issues, or an image that does not match the expected runtime configuration.
