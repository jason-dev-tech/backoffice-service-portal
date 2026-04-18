/// <reference types="node" />
import { test, expect, type Page } from '@playwright/test';

test.describe.configure({ mode: 'serial' });

async function signInAndWaitForAuthenticatedApp(page: Page, username: string, password: string) {
  const usernameInput = page.getByLabel('Username');
  const passwordInput = page.getByLabel('Password');
  const signInButton = page.getByRole('button', { name: 'Sign in' });

  await usernameInput.fill(username);
  await passwordInput.fill(password);

  await expect(usernameInput).toHaveValue(username);
  await expect(passwordInput).toHaveValue(password);

  await signInButton.click({ delay: 100 });

  await waitForAuthenticatedAppReady(page);
}

async function waitForAuthenticatedAppReady(page: Page) {
  const serviceRequestsLink = page.getByRole('link', { name: 'Service Requests' });
  await expect(serviceRequestsLink).toBeVisible({ timeout: 15000 });
  await expect(serviceRequestsLink).toHaveAttribute('href', /service-requests/);
}

async function waitForServiceRequestsPageReady(page: Page) {
  await page.waitForURL(/\/service-requests$/);
  await expect(page.getByRole('heading', { name: 'Service Requests' })).toBeVisible();
  await expect(page.getByText('Loading service requests...')).toHaveCount(0);
}

test('viewer does not see the create service request action', async ({ page }) => {
  const viewerUsername = process.env.E2E_USERNAME;
  const viewerPassword = process.env.E2E_PASSWORD;

  if (!viewerUsername || !viewerPassword) {
    throw new Error('E2E_USERNAME and E2E_PASSWORD must be set.');
  }

  await page.goto('http://localhost:4200/login');

  await signInAndWaitForAuthenticatedApp(page, viewerUsername, viewerPassword);

  await page.getByRole('link', { name: 'Service Requests' }).click();
  await waitForServiceRequestsPageReady(page);

  await expect(page.getByRole('heading', { name: 'Service Requests' })).toBeVisible();
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

  await page.getByRole('link', { name: 'Service Requests' }).click();
  await page.waitForURL(/\/service-requests$/);

  await expect(page.getByRole('heading', { name: 'Service Requests' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Create Service Request' })).toBeVisible();
});

test('admin sees create and delete service request actions', async ({ page }) => {
  const adminUsername = process.env.E2E_ADMIN_USERNAME;
  const adminPassword = process.env.E2E_ADMIN_PASSWORD;

  if (!adminUsername || !adminPassword) {
    throw new Error('E2E_ADMIN_USERNAME and E2E_ADMIN_PASSWORD must be set.');
  }

  await page.goto('http://localhost:4200/login');

  await signInAndWaitForAuthenticatedApp(page, adminUsername, adminPassword);

  await page.getByRole('link', { name: 'Service Requests' }).click();
  await page.waitForURL(/\/service-requests$/);

  await expect(page.getByRole('heading', { name: 'Service Requests' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Create Service Request' })).toBeVisible();
  await expect(page.getByRole('button', { name: 'Delete' }).first()).toBeVisible();
});

test('operator can create a service request', async ({ page }) => {
  const operatorUsername = process.env.E2E_OPERATOR_USERNAME;
  const operatorPassword = process.env.E2E_OPERATOR_PASSWORD;

  if (!operatorUsername || !operatorPassword) {
    throw new Error('E2E_OPERATOR_USERNAME and E2E_OPERATOR_PASSWORD must be set.');
  }

  const uniqueTitle = `Operator E2E ${Date.now()}`;

  await page.goto('http://localhost:4200/login');

  await signInAndWaitForAuthenticatedApp(page, operatorUsername, operatorPassword);

  await page.getByRole('link', { name: 'Service Requests' }).click();
  await waitForServiceRequestsPageReady(page);

  await expect(page.getByRole('heading', { name: 'Service Requests' })).toBeVisible();
  await page.getByRole('button', { name: 'Create Service Request' }).click();

  await expect(page.getByRole('dialog', { name: 'Create Service Request' })).toBeVisible();
  await page.getByLabel('Title').fill(uniqueTitle);
  await page.getByLabel('Description').fill('Created by the operator E2E test.');
  await page.getByLabel('Requester name').fill('Operator Test User');

  await page
    .getByRole('dialog', { name: 'Create Service Request' })
    .getByRole('button', { name: 'Create Service Request' })
    .click();

  await expect(page.getByRole('dialog', { name: 'Create Service Request' })).toBeHidden();
  await expect(page.getByText('Loading service requests...')).toHaveCount(0);
  await expect(page.getByRole('cell', { name: uniqueTitle })).toBeVisible();
});

test('admin can delete a service request', async ({ page }) => {
  const adminUsername = process.env.E2E_ADMIN_USERNAME;
  const adminPassword = process.env.E2E_ADMIN_PASSWORD;

  if (!adminUsername || !adminPassword) {
    throw new Error('E2E_ADMIN_USERNAME and E2E_ADMIN_PASSWORD must be set.');
  }

  const uniqueTitle = `Admin E2E ${Date.now()}`;

  await page.goto('http://localhost:4200/login');

  await signInAndWaitForAuthenticatedApp(page, adminUsername, adminPassword);

  await page.getByRole('link', { name: 'Service Requests' }).click();
  await waitForServiceRequestsPageReady(page);
  await page.getByRole('button', { name: 'Create Service Request' }).click({ delay: 100 });

  await expect(page.getByRole('dialog', { name: 'Create Service Request' })).toBeVisible();
  await page.getByLabel('Title').fill(uniqueTitle);
  await page.getByLabel('Description').fill('Created by the admin delete E2E test.');
  await page.getByLabel('Requester name').fill('Admin Test User');

  await page
    .getByRole('dialog', { name: 'Create Service Request' })
    .getByRole('button', { name: 'Create Service Request' })
    .click();

  await expect(page.getByRole('dialog', { name: 'Create Service Request' })).toBeHidden();

  const createdRow = page.getByRole('row', { name: new RegExp(uniqueTitle) });
  await expect(createdRow).toBeVisible();

  page.once('dialog', (dialog) => dialog.accept());
  await createdRow.getByRole('button', { name: 'Delete' }).click();

  await expect(page.getByRole('cell', { name: uniqueTitle })).toHaveCount(0);
});
