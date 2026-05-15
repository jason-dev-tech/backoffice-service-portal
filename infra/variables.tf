variable "aws_region" {
  description = "AWS region where resources will be created."
  type        = string
}

variable "instance_type" {
  description = "EC2 instance type."
  type        = string
  default     = "t3.micro"
}

variable "ami_id" {
  description = "AMI ID for the EC2 instance."
  type        = string
}

variable "key_pair_name" {
  description = "Name of an existing AWS EC2 key pair used for SSH access."
  type        = string
}

variable "allowed_ssh_cidr" {
  description = "CIDR range allowed to connect over SSH."
  type        = string
}

variable "project_name" {
  description = "Project name used for resource naming and tags."
  type        = string
  default     = "backoffice-service-portal"
}

variable "environment_name" {
  description = "Environment name used for resource naming and tags."
  type        = string
  default     = "dev"
}

variable "enable_elastic_ip" {
  description = "Whether to allocate and associate an Elastic IP for stable public access."
  type        = bool
  default     = false
}
