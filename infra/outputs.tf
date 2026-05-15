locals {
  public_ip = var.enable_elastic_ip ? aws_eip.app[0].public_ip : aws_instance.app.public_ip
}

output "public_ip" {
  description = "Public IP address for the EC2 instance."
  value       = local.public_ip
}

output "instance_id" {
  description = "EC2 instance ID."
  value       = aws_instance.app.id
}

output "deployment_url" {
  description = "HTTPS URL for the deployed application."
  value       = "https://${local.public_ip}"
}

output "ssh_command" {
  description = "Example SSH command for connecting to the EC2 instance."
  value       = "ssh -i /path/to/private-key.pem <ssh-user>@${local.public_ip}"
}
