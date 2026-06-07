# Infrastructure as Code (IaC)

This directory contains a lightweight Infrastructure as Code foundation for provisioning AWS resources in a reproducible, version-controlled way.

Some file names, such as `terraform.tfvars.example` and `terraform.tfvars`, are retained because they are standard IaC tool conventions.

It provisions:

- One EC2 instance
- One security group
- HTTPS access on port 443 from anywhere
- SSH access on port 22 from a configurable CIDR only
- Optional Elastic IP association for stable public IP access

## Prerequisites

- OpenTofu installed locally
- AWS CLI installed locally
- An AWS account with permission to provision EC2, security group, and Elastic IP resources
- An existing EC2 key pair in the target AWS region
- A suitable AMI ID for the target AWS region

## AWS CLI Credentials

Configure AWS credentials before running IaC commands:

```sh
aws configure
```

Or use an existing AWS CLI profile:

```sh
export AWS_PROFILE=your-profile-name
```

Do not commit credentials, private keys, account IDs, or secrets to this repository.

## Configuration

Create a local variable file from the example:

```sh
cp terraform.tfvars.example terraform.tfvars
```

Edit `terraform.tfvars` with your AWS region, AMI ID, EC2 key pair name, and allowed SSH CIDR.

The `allowed_ssh_cidr` value should usually be your current public IP as a `/32`, for example:

```hcl
allowed_ssh_cidr = "203.0.113.10/32"
```

## Usage

Initialize:

```sh
tofu init
```

Preview the planned changes:

```sh
tofu plan
```

Apply the AWS infrastructure:

```sh
tofu apply
```

View outputs:

```sh
tofu output
```

Destroy the infrastructure:

```sh
tofu destroy
```

## Elastic IP

By default, `enable_elastic_ip` is `false`, so AWS assigns a normal public IP to the EC2 instance. That public IP can change when the instance is stopped and started.

Set `enable_elastic_ip = true` to allocate and associate an Elastic IP. This gives the instance a stable public IP address, which is useful for portfolio demos or DNS records.

Elastic IPs can incur AWS charges, especially when allocated but not associated with a running resource. Review AWS pricing before enabling this option.

## Cost Warning

These AWS resources may create charges. Instance runtime, Elastic IP usage, storage, and data transfer can all cost money. Run `tofu destroy` when you no longer need temporary development infrastructure.

## Production Warning

This setup is intentionally simple and portfolio-friendly. It does not include production features such as load balancers, managed databases, backups, private networking, autoscaling, centralized logging, or remote state.

Do not use `tofu destroy` casually in production or shared environments. Destroying infrastructure can permanently remove running resources and disrupt users.
