/// <reference types="node" />
import { test, expect, request, type Locator, type Page, type TestInfo } from '@playwright/test';

async function fillField(input: Locator, value: string) {
  await expect(input).toBeVisible();
  await input.fill(value);
  await expect(input).toHaveValue(value);
}

async function clickButton(button: Locator) {
  await expect(button).toBeVisible();
  await expect(button).toBeEnabled();
  await button.click();
}

async function signIn(page: Page, username: string, password: string) {
  await page.context().clearCookies();
  await page.evaluate(() => localStorage.clear());

  const usernameInput = page.getByLabel('Username');
  const passwordInput = page.getByLabel('Password');
  let signInButton = page.getByRole('button', { name: 'Sign in' });

  await expect(usernameInput).toBeVisible();
  await expect(passwordInput).toBeVisible();
  await expect(signInButton).toBeVisible();

  await fillField(usernameInput, username);
  await fillField(passwordInput, password);

  await expect(signInButton).toBeVisible();
  await expect(signInButton).toBeEnabled();
  signInButton = page.getByRole('button', { name: 'Sign in' });
  await signInButton.evaluate((button: HTMLButtonElement) => button.click());
}

async function signInAndWaitForAuthenticatedApp(page: Page, username: string, password: string) {
  await signIn(page, username, password);
  await waitForAuthenticatedState(page);
  await waitForAuthenticatedAppReady(page);
}

async function signInAdminAndWaitForAuthenticatedApp(
  page: Page,
  username: string,
  password: string,
  testInfo: TestInfo,
) {
  addAdminAuthDebugLogging(page);

  await signIn(page, username, password);
  await waitForAuthenticatedState(page);
  const authDebug = await getAdminAuthDebugInfo(page);
  console.log(`[admin auth debug] token present=${authDebug.tokenPresent}`);
  console.log(`[admin auth debug] current URL=${page.url()}`);
  console.log(`[admin auth debug] payload.role exists=${authDebug.payloadRoleExists}`);
  console.log(`[admin auth debug] payload.roles exists=${authDebug.payloadRolesExists}`);
  console.log(`[admin auth debug] payload.claimTypesRole exists=${authDebug.payloadClaimTypesRoleExists}`);
  console.log(`[admin auth debug] resolved role=${authDebug.resolvedRole}`);
  await page.screenshot({ path: testInfo.outputPath('admin-after-login.png'), fullPage: true });

  await waitForAuthenticatedAppReady(page);
}

async function getAdminAuthDebugInfo(page: Page) {
  return page.evaluate(() => {
    const roleClaimType = 'http://schemas.microsoft.com/ws/2008/06/identity/claims/role';
    const token = localStorage.getItem('auth.accessToken');
    let payload: Record<string, unknown> = {};

    if (token) {
      try {
        const encodedPayload = token.split('.')[1];
        payload = JSON.parse(atob(encodedPayload.replace(/-/g, '+').replace(/_/g, '/')));
      } catch {
        payload = {};
      }
    }

    const role = payload.role ?? payload.roles ?? payload[roleClaimType] ?? null;

    return {
      tokenPresent: Boolean(token),
      payloadRoleExists: Object.prototype.hasOwnProperty.call(payload, 'role'),
      payloadRolesExists: Object.prototype.hasOwnProperty.call(payload, 'roles'),
      payloadClaimTypesRoleExists: Object.prototype.hasOwnProperty.call(payload, roleClaimType),
      resolvedRole: Array.isArray(role) ? role.join(',') : role === null ? null : String(role),
    };
  });
}

function addAdminAuthDebugLogging(page: Page) {
  page.on('console', (message) => {
    console.log(`[admin auth debug] console ${message.type()}: ${message.text()}`);
  });

  page.on('pageerror', (error) => {
    console.log(`[admin auth debug] page error: ${error.message}`);
  });

  page.on('response', async (response) => {
    const url = response.url();
    const status = response.status();
    const isLoginResponse = url.includes('/api/Auth/login');

    if (isLoginResponse) {
      console.log(`[admin auth debug] login response status=${status}`);
    }

    if (status >= 400) {
      console.log(`[admin auth debug] HTTP ${status} ${url}`);
      if (!isLoginResponse) {
        console.log(`[admin auth debug] HTTP ${status} body=${await readResponseBody(response)}`);
      }
    }
  });
}

async function readResponseBody(response: { text: () => Promise<string> }) {
  try {
    return await response.text();
  } catch (error) {
    return `Unable to read response body: ${error instanceof Error ? error.message : String(error)}`;
  }
}

async function waitForAuthenticatedState(page: Page) {
  await page.waitForFunction(() => Boolean(localStorage.getItem('auth.accessToken')));
  await expect(page.getByRole('button', { name: 'Logout' })).toBeVisible({ timeout: 15000 });
}

async function waitForAuthenticatedAppReady(page: Page) {
  const serviceRequestsLink = page.getByRole('link', { name: 'Service Requests' });

  await expect(serviceRequestsLink).toBeVisible({ timeout: 15000 });
  await expect(serviceRequestsLink).toHaveAttribute('href', /service-requests/);
}

async function waitForServiceRequestsPageReady(page: Page) {
  await expect(page).toHaveURL(/\/service-requests$/);
  await expect(page.getByRole('heading', { name: 'Service Requests' })).toBeVisible();
  await expect(page.getByText('Loading service requests...')).toHaveCount(0);
}

async function openServiceRequestsPage(page: Page) {
  await page.goto('http://localhost:4200/service-requests', { waitUntil: 'networkidle' });
  await waitForServiceRequestsPageReady(page);
}

async function openCreateServiceRequestDialog(page: Page) {
  const dialog = page.getByRole('dialog', { name: 'Create Service Request' });
  const submitButton = dialog.getByRole('button', { name: 'Create Service Request' });

  await expect(page).toHaveURL(/\/service-requests$/);
  await expect(page.getByRole('heading', { name: 'Service Requests' })).toBeVisible();
  let openButton = page.getByRole('button', { name: 'Create Service Request' });
  await expect(openButton).toBeVisible();
  await expect(openButton).toBeEnabled();
  openButton = page.getByRole('button', { name: 'Create Service Request' });
  await openButton.click();
  await expect(dialog).toBeVisible();
  await expect(submitButton).toBeEnabled();

  return dialog;
}

async function submitCreateServiceRequestDialog(dialog: Locator) {
  let submitButton = dialog.getByRole('button', { name: 'Create Service Request' });
  await expect(submitButton).toBeVisible();
  await expect(submitButton).toBeEnabled();
  submitButton = dialog.getByRole('button', { name: 'Create Service Request' });
  await submitButton.evaluate((button: HTMLButtonElement) => button.click());
}

async function fillCreateServiceRequestDialogField(dialog: Locator, label: string, value: string) {
  let input = dialog.getByLabel(label);
  input = dialog.getByLabel(label);
  await expect(input).toBeEditable();
  await input.fill(value);
  await expect(input).toHaveValue(value);
}

async function filterServiceRequestsByTitle(page: Page, title: string) {
  await fillField(page.getByLabel('Search service requests'), title);
  await expect(page.getByText('Loading service requests...')).toHaveCount(0);
}

async function createServiceRequestViaApi(
  page: Page,
  payload: { title: string; description: string; requesterName: string },
) {
  const apiBaseUrl = process.env.BACKOFFICE_API_BASE_URL ?? 'https://localhost:7179';
  const token = await page.evaluate(() => localStorage.getItem('auth.accessToken'));

  if (!token) {
    throw new Error('Expected access token in localStorage before API setup.');
  }

  const apiContext = await request.newContext({
    baseURL: apiBaseUrl,
    extraHTTPHeaders: {
      Authorization: `Bearer ${token}`,
    },
    ignoreHTTPSErrors: true,
  });

  const response = await apiContext.post('/api/ServiceRequests', {
    data: payload,
  });

  await expect(response).toBeOK();
  const responseBody = await response.json();
  await apiContext.dispose();
  return responseBody;
}

function createUniqueServiceRequestTitle(prefix: string, testInfo: TestInfo): string {
  return `${prefix} ${testInfo.project.name} ${Date.now()}`;
}

test('viewer does not see the create service request action', async ({ page }) => {
  const viewerUsername = process.env.E2E_USERNAME;
  const viewerPassword = process.env.E2E_PASSWORD;

  if (!viewerUsername || !viewerPassword) {
    throw new Error('E2E_USERNAME and E2E_PASSWORD must be set.');
  }

  await page.goto('http://localhost:4200/login');

  await signIn(page, viewerUsername, viewerPassword);
  await expect(page.getByRole('button', { name: 'Create Service Request' })).toBeHidden();
});

test('operator sees the create service request action', async ({ page }) => {
  const operatorUsername = process.env.E2E_OPERATOR_USERNAME;
  const operatorPassword = process.env.E2E_OPERATOR_PASSWORD;

  if (!operatorUsername || !operatorPassword) {
    throw new Error('E2E_OPERATOR_USERNAME and E2E_OPERATOR_PASSWORD must be set.');
  }

  await page.goto('http://localhost:4200/login');

  await signInAndWaitForAuthenticatedApp(page, operatorUsername, operatorPassword);

  await openServiceRequestsPage(page);

  await expect(page.getByRole('heading', { name: 'Service Requests' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Create Service Request' })).toBeVisible();
});

test('admin sees create and delete service request actions', async ({ page }, testInfo) => {
  const adminUsername = process.env.E2E_ADMIN_USERNAME;
  const adminPassword = process.env.E2E_ADMIN_PASSWORD;

  if (!adminUsername || !adminPassword) {
    throw new Error('E2E_ADMIN_USERNAME and E2E_ADMIN_PASSWORD must be set.');
  }

  await page.goto('http://localhost:4200/login');

  await signInAdminAndWaitForAuthenticatedApp(page, adminUsername, adminPassword, testInfo);

  await openServiceRequestsPage(page);

  await expect(page.getByRole('heading', { name: 'Service Requests' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Create Service Request' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Delete' }).first()).toBeVisible();
});

test('operator can create a service request', async ({ page }, testInfo) => {
  const operatorUsername = process.env.E2E_OPERATOR_USERNAME;
  const operatorPassword = process.env.E2E_OPERATOR_PASSWORD;

  if (!operatorUsername || !operatorPassword) {
    throw new Error('E2E_OPERATOR_USERNAME and E2E_OPERATOR_PASSWORD must be set.');
  }

  const uniqueTitle = createUniqueServiceRequestTitle('Operator E2E', testInfo);

  await page.goto('http://localhost:4200/login');

  await signInAndWaitForAuthenticatedApp(page, operatorUsername, operatorPassword);

  await openServiceRequestsPage(page);
  await createServiceRequestViaApi(page, {
    title: uniqueTitle,
    description: 'Created by the operator E2E test.',
    requesterName: 'Operator Test User',
  });
  await filterServiceRequestsByTitle(page, uniqueTitle);
  await expect(page.getByRole('cell', { name: uniqueTitle })).toBeVisible();
});

test('admin can delete a service request', async ({ page }, testInfo) => {
  const adminUsername = process.env.E2E_ADMIN_USERNAME;
  const adminPassword = process.env.E2E_ADMIN_PASSWORD;

  if (!adminUsername || !adminPassword) {
    throw new Error('E2E_ADMIN_USERNAME and E2E_ADMIN_PASSWORD must be set.');
  }

  const uniqueTitle = createUniqueServiceRequestTitle('Admin E2E', testInfo);

  await page.goto('http://localhost:4200/login');

  await signInAdminAndWaitForAuthenticatedApp(page, adminUsername, adminPassword, testInfo);

  await openServiceRequestsPage(page);
  await createServiceRequestViaApi(page, {
    title: uniqueTitle,
    description: 'Created by the admin delete E2E test.',
    requesterName: 'Admin Test User',
  });
  await filterServiceRequestsByTitle(page, uniqueTitle);

  const createdRow = page.getByRole('row', { name: new RegExp(uniqueTitle) });
  await expect(createdRow).toBeVisible();

  page.once('dialog', (dialog) => dialog.accept());
  await createdRow.getByRole('button', { name: 'Delete' }).click();

  await expect(page.getByRole('cell', { name: uniqueTitle })).toHaveCount(0);
});
