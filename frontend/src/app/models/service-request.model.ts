export interface ServiceRequest {
  id: number;
  title: string;
  description: string;
  requesterName: string;
  status: string;
  createdAt: string;
  updatedAt: string | null;
}