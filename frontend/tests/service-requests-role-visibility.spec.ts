/// <reference types="node" />
import { test, expect, type Locator, type Page, type TestInfo } from '@playwright/test';

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

async function signInAndWaitForAuthenticatedApp(page: Page, username: string, password: string) {
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

  await waitForAuthenticatedAppReady(page);
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
  await page.goto('http://localhost:4200/service-requests');
  await waitForServiceRequestsPageReady(page);
}

async function openCreateServiceRequestDialog(page: Page) {
  let openButton = page.getByRole('button', { name: 'Create Service Request' });
  const dialog = page.getByRole('dialog', { name: 'Create Service Request' });
  const titleInput = dialog.getByLabel('Title');
  const descriptionInput = dialog.getByLabel('Description');
  const requesterNameInput = dialog.getByLabel('Requester name');

  await expect(openButton).toBeVisible();
  await expect(openButton).toBeEnabled();
  openButton = page.getByRole('button', { name: 'Create Service Request' });
  await openButton.evaluate((button: HTMLButtonElement) => button.click());
  await expect(dialog).toBeVisible();

  await expect(titleInput).toBeVisible();
  await expect(descriptionInput).toBeVisible();
  await expect(requesterNameInput).toBeVisible();

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
  await expect(input).toBeVisible();
  input = dialog.getByLabel(label);
  await expect(input).toBeEditable();
  await input.fill(value);
  await expect(input).toHaveValue(value);
}

async function filterServiceRequestsByTitle(page: Page, title: string) {
  await fillField(page.getByLabel('Search service requests'), title);
  await expect(page.getByText('Loading service requests...')).toHaveCount(0);
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

  await signInAndWaitForAuthenticatedApp(page, viewerUsername, viewerPassword);

  await openServiceRequestsPage(page);

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

  await openServiceRequestsPage(page);

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

  await expect(page.getByRole('heading', { name: 'Service Requests' })).toBeVisible();
  const dialog = await openCreateServiceRequestDialog(page);
  await fillCreateServiceRequestDialogField(dialog, 'Title', uniqueTitle);
  await fillCreateServiceRequestDialogField(dialog, 'Description', 'Created by the operator E2E test.');
  await fillCreateServiceRequestDialogField(dialog, 'Requester name', 'Operator Test User');

  await submitCreateServiceRequestDialog(dialog);

  await expect(dialog).toBeHidden();
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

  await signInAndWaitForAuthenticatedApp(page, adminUsername, adminPassword);

  await openServiceRequestsPage(page);
  const dialog = await openCreateServiceRequestDialog(page);
  await fillCreateServiceRequestDialogField(dialog, 'Title', uniqueTitle);
  await fillCreateServiceRequestDialogField(dialog, 'Description', 'Created by the admin delete E2E test.');
  await fillCreateServiceRequestDialogField(dialog, 'Requester name', 'Admin Test User');

  await submitCreateServiceRequestDialog(dialog);

  await expect(dialog).toBeHidden();
  await filterServiceRequestsByTitle(page, uniqueTitle);

  const createdRow = page.getByRole('row', { name: new RegExp(uniqueTitle) });
  await expect(createdRow).toBeVisible();

  page.once('dialog', (dialog) => dialog.accept());
  await createdRow.getByRole('button', { name: 'Delete' }).click();

  await expect(page.getByRole('cell', { name: uniqueTitle })).toHaveCount(0);
});
