export interface ServiceRequestStatusDistribution {
  status: string;
  count: number;
  percentage: number;
}

export interface ServiceRequestDashboard {
  totalRequests: number;
  openRequests: number;
  inProgressRequests: number;
  closedRequests: number;
  oldestOpenRequestCreatedAt: string | null;
  mostRecentRequestCreatedAt: string | null;
  openSharePercentage: number;
  closedSharePercentage: number;
  statusDistribution: ServiceRequestStatusDistribution[];
}
