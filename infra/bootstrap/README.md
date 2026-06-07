# EC2 Deployment Bootstrap

This directory contains a small server bootstrap foundation for preparing an Ubuntu EC2 instance for Docker-based deployment.

It installs Docker from the official Docker apt repository, installs the Docker Compose plugin, starts Docker, grants the SSH user Docker access, and creates a deployment directory.

This prepares the server only. It does not deploy the application yet.

## Prerequisites

- An Ubuntu EC2 instance
- SSH access to the instance
- A user with `sudo` privileges
- Outbound internet access from the instance

## SSH Into EC2

Use the SSH command from the IaC output and replace `<ssh-user>` with the Ubuntu image user, commonly `ubuntu`:

```sh
ssh -i /path/to/private-key.pem <ssh-user>@<public-ip>
```

## Run Bootstrap

Copy `bootstrap.sh` to the instance, then run:

```sh
chmod +x bootstrap.sh
./bootstrap.sh
```

The script creates `/opt/backoffice-service-portal` by default. To use a different deployment directory:

```sh
DEPLOY_DIR=/opt/your-app ./bootstrap.sh
```

## Refresh Docker Group Access

After the script adds your user to the `docker` group, log out and log back in before running Docker commands without `sudo`.

## Verify

Copy `verify.sh` to the instance, then run:

```sh
chmod +x verify.sh
./verify.sh
```

If you used a custom deployment directory:

```sh
DEPLOY_DIR=/opt/your-app ./verify.sh
```

## Cost And Security Notes

Running EC2 instances, Elastic IPs, storage, and data transfer can create AWS charges. Stop or remove temporary infrastructure when it is no longer needed.

Restrict SSH access to a trusted CIDR, keep the instance patched, avoid placing secrets in shell history or scripts, and review security group rules before exposing services publicly.
