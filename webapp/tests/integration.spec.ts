import { test, expect } from '@playwright/test';

const sampleImage = '../docs/dataset_samples/93_HONOR-7X.png';

async function uploadAndAnalyze(page, heatmaps=false) {
  await page.goto('/');
  const fileInput = page.locator('input[type="file"]');
  await fileInput.setInputFiles(sampleImage);
  if (heatmaps) await page.getByRole('checkbox', { name: /heatmaps/i }).check();
  await page.getByText('Analizza').click();
  await expect(page.locator('pre')).toBeVisible();
  const json = await page.locator('pre').textContent();
  return JSON.parse(json!);
}

test.describe('Frontend/Backend Integration', () => {
  test('default configuration returns valid response', async ({ page }) => {
    const data = await uploadAndAnalyze(page);
    expect(data.isValidDocument).toBeDefined();
  });

  test('heatmap generation works', async ({ page }) => {
    const data = await uploadAndAnalyze(page, true);
    expect(data.blurHeatmap).not.toBeNull();
    expect(data.glareHeatmap).not.toBeNull();
  });

  test('heatmaps not requested are omitted', async ({ page }) => {
    const data = await uploadAndAnalyze(page, false);
    expect(data.blurHeatmap).toBeNull();
    expect(data.glareHeatmap).toBeNull();
  });

  test('custom blur threshold modifies result', async ({ page }) => {
    await page.goto('/');
    const fileInput = page.locator('input[type="file"]');
    await fileInput.setInputFiles(sampleImage);
    await page.locator('text=Soglia Blur').press('ArrowRight');
    await page.getByText('Analizza').click();
    const json = await page.locator('pre').textContent();
    const data = JSON.parse(json!);
    expect(data.results.BlurScore).toBeDefined();
  });
});
