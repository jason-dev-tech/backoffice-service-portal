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
