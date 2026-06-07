locals {
  name_prefix = "${var.project_name}-${var.environment_name}"

  common_tags = {
    Project     = var.project_name
    Environment = var.environment_name
    ManagedBy   = "OpenTofu"
  }
}

resource "aws_security_group" "app" {
  name        = "${local.name_prefix}-sg"
  description = "Security group for ${local.name_prefix} EC2 instance"

  ingress {
    description = "HTTPS"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "SSH from allowed CIDR"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = [var.allowed_ssh_cidr]
  }

  egress {
    description = "Outbound internet access"
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = merge(local.common_tags, {
    Name = "${local.name_prefix}-sg"
  })
}

resource "aws_eip" "app" {
  count = var.enable_elastic_ip ? 1 : 0

  domain = "vpc"

  tags = merge(local.common_tags, {
    Name = "${local.name_prefix}-eip"
  })
}

resource "aws_eip_association" "app" {
  count = var.enable_elastic_ip ? 1 : 0

  instance_id   = aws_instance.app.id
  allocation_id = aws_eip.app[0].id
}
