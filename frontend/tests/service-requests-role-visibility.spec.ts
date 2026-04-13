/// <reference types="node" />
import { test, expect } from '@playwright/test';

test('viewer does not see the create service request action', async ({ page }) => {
  const viewerUsername = process.env.E2E_USERNAME;
  const viewerPassword = process.env.E2E_PASSWORD;

  if (!viewerUsername || !viewerPassword) {
    throw new Error('E2E_USERNAME and E2E_PASSWORD must be set.');
  }

  await page.goto('http://localhost:4200/login');

  await page.getByLabel('Username').fill(viewerUsername);
  await page.getByLabel('Password').fill(viewerPassword);

  await Promise.all([
    page.waitForURL(/\/dashboard$/),
    page.getByRole('button', { name: 'Sign in' }).click(),
  ]);

  await page.getByRole('link', { name: 'Service Requests' }).click();
  await page.waitForURL(/\/service-requests$/);

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

  await page.getByLabel('Username').fill(operatorUsername);
  await page.getByLabel('Password').fill(operatorPassword);

  await Promise.all([
    page.waitForURL(/\/dashboard$/),
    page.getByRole('button', { name: 'Sign in' }).click(),
  ]);

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

  await page.getByLabel('Username').fill(adminUsername);
  await page.getByLabel('Password').fill(adminPassword);

  await Promise.all([
    page.waitForURL(/\/dashboard$/),
    page.getByRole('button', { name: 'Sign in' }).click(),
  ]);

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

  await page.getByLabel('Username').fill(operatorUsername);
  await page.getByLabel('Password').fill(operatorPassword);

  await Promise.all([
    page.waitForURL(/\/dashboard$/),
    page.getByRole('button', { name: 'Sign in' }).click(),
  ]);

  await page.getByRole('link', { name: 'Service Requests' }).click();
  await page.waitForURL(/\/service-requests$/);

  await expect(page.getByRole('heading', { name: 'Service Requests' })).toBeVisible();
  await page.getByRole('button', { name: 'Create Service Request' }).click();

  await expect(page.getByRole('dialog', { name: 'Create Service Request' })).toBeVisible();
  await page.getByLabel('Title').fill(uniqueTitle);
  await page.getByLabel('Description').fill('Created by the operator E2E test.');
  await page.getByLabel('Requester name').fill('Operator Test User');

  await page.getByRole('dialog', { name: 'Create Service Request' }).getByRole('button', { name: 'Create Service Request' }).click();

  await expect(page.getByRole('dialog', { name: 'Create Service Request' })).toBeHidden();
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

  await page.getByLabel('Username').fill(adminUsername);
  await page.getByLabel('Password').fill(adminPassword);

  await Promise.all([
    page.waitForURL(/\/dashboard$/),
    page.getByRole('button', { name: 'Sign in' }).click(),
  ]);

  await page.getByRole('link', { name: 'Service Requests' }).click();
  await page.waitForURL(/\/service-requests$/);

  await expect(page.getByRole('heading', { name: 'Service Requests' })).toBeVisible();
  await page.getByRole('button', { name: 'Create Service Request' }).click();

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
