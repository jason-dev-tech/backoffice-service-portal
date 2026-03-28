import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ServiceRequest } from './models/service-request.model';
import { ServiceRequestService } from './services/service-request.service';

@Component({
  selector: 'app-root',
  imports: [CommonModule],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App implements OnInit {
  private serviceRequestService = inject(ServiceRequestService);

  serviceRequests: ServiceRequest[] = [];
  errorMessage = '';
  isLoading = false;

  ngOnInit(): void {
    this.loadServiceRequests();
  }

  loadServiceRequests(): void {
    this.isLoading = true;
    this.errorMessage = '';

    this.serviceRequestService.getServiceRequests().subscribe({
      next: (data) => {
        this.serviceRequests = data;
        this.isLoading = false;
      },
      error: (error) => {
        console.error('Failed to load service requests', error);
        this.errorMessage = 'Failed to load service requests.';
        this.isLoading = false;
      }
    });
  }
}