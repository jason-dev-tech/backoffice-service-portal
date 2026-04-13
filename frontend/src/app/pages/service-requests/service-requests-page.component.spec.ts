import { TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { AuthService } from '../../services/auth.service';
import {
  PagedServiceRequestsResponse,
  ServiceRequestService,
} from '../../services/service-request.service';
import { ServiceRequestsPageComponent } from './service-requests-page.component';

describe('ServiceRequestsPageComponent role-aware actions', () => {
  async function renderWithRole(role: 'Viewer' | 'Operator' | 'Admin') {
    const authServiceStub = {
      canCreateServiceRequest: () => role === 'Operator' || role === 'Admin',
      canEditServiceRequest: () => role === 'Operator' || role === 'Admin',
      canDeleteServiceRequest: () => role === 'Admin',
    };

    const serviceRequestServiceStub = {
      getServiceRequests: () => of(createResponse()),
      createServiceRequest: () => of(null),
      updateServiceRequest: () => of(null),
      deleteServiceRequest: () => of(undefined),
    };

    await TestBed.configureTestingModule({
      imports: [ServiceRequestsPageComponent],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              queryParamMap: convertToParamMap({}),
            },
          },
        },
        { provide: AuthService, useValue: authServiceStub },
        { provide: ServiceRequestService, useValue: serviceRequestServiceStub },
      ],
    }).compileComponents();

    const fixture = TestBed.createComponent(ServiceRequestsPageComponent);
    fixture.detectChanges();
    await fixture.whenStable();
    fixture.detectChanges();

    return fixture.nativeElement as HTMLElement;
  }

  function createResponse(): PagedServiceRequestsResponse {
    return {
      items: [
        {
          id: 101,
          title: 'Sales report discrepancy',
          description: 'Review a mismatch in the weekly sales report totals.',
          requesterName: 'Bianca',
          status: 'Open',
          createdAt: '2026-04-01T00:00:00Z',
          updatedAt: null,
        },
      ],
      page: 1,
      pageSize: 10,
      totalCount: 1,
      totalPages: 1,
    };
  }

  function getVisiblePageActionLabels(element: HTMLElement): string[] {
    const labels: string[] = [];
    const createButton = element.querySelector('.table-header-actions > button');

    if (createButton?.textContent?.trim()) {
      labels.push(createButton.textContent.trim());
    }

    const rowActionButtons = element.querySelectorAll('.row-actions button');
    for (const button of rowActionButtons) {
      const label = button.textContent?.trim();

      if (label) {
        labels.push(label);
      }
    }

    return labels;
  }

  it('hides create, edit, and delete actions for viewers', async () => {
    const element = await renderWithRole('Viewer');
    const buttonLabels = getVisiblePageActionLabels(element);

    expect(buttonLabels).not.toContain('Create Service Request');
    expect(buttonLabels).not.toContain('Edit');
    expect(buttonLabels).not.toContain('Delete');
  });

  it('shows create and edit actions but hides delete for operators', async () => {
    const element = await renderWithRole('Operator');
    const buttonLabels = getVisiblePageActionLabels(element);

    expect(buttonLabels).toContain('Create Service Request');
    expect(buttonLabels).toContain('Edit');
    expect(buttonLabels).not.toContain('Delete');
  });

  it('shows create, edit, and delete actions for admins', async () => {
    const element = await renderWithRole('Admin');
    const buttonLabels = getVisiblePageActionLabels(element);

    expect(buttonLabels).toContain('Create Service Request');
    expect(buttonLabels).toContain('Edit');
    expect(buttonLabels).toContain('Delete');
  });
});
