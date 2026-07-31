import { expect, test } from '@playwright/test'

const isRemote = process.env.E2E_START_LOCAL === '0'

test.beforeEach(async ({ page }) => {
  await page.addInitScript(() => localStorage.setItem('analytics-consent', 'declined'))
})

test('homepage renders recent articles', async ({ page }) => {
  const response = await page.goto('/', { waitUntil: 'domcontentloaded' })

  expect(response?.status()).toBe(200)
  await expect(page.getByRole('heading', { name: 'Andy Meier', exact: true })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Recent articles', exact: true })).toBeVisible()
})

test('articles index renders and opens a published article when available', async ({ page }) => {
  const response = await page.goto('/articles', { waitUntil: 'domcontentloaded' })

  expect(response?.status()).toBe(200)
  await expect(page.getByRole('heading', { name: 'Articles', exact: true })).toBeVisible()

  const articleLinks = page.locator('#page a[href^="/articles/"]')
  if (await articleLinks.count()) {
    const href = await articleLinks.first().getAttribute('href')
    expect(href).toBeTruthy()
    const articleResponse = await page.goto(href!, { waitUntil: 'domcontentloaded' })
    expect(articleResponse?.status()).toBe(200)
    await expect(page.locator('article')).toBeVisible()
  } else {
    await expect(page.getByRole('heading', { name: 'No articles found', exact: true })).toBeVisible()
  }
})

test('local article search and detail routes use MockNotion fixtures', async ({ page }) => {
  test.skip(isRemote, 'Published production content is not a deterministic fixture.')

  await page.goto('/articles?search=engineering', { waitUntil: 'domcontentloaded' })
  await expect(page.getByRole('heading', { name: 'Mock engineering notes', exact: true })).toBeVisible()
  await expect(page.getByRole('link', { name: 'Clear search' })).toBeVisible()

  const response = await page.goto('/articles/mock-finance-systems', { waitUntil: 'domcontentloaded' })
  expect(response?.status()).toBe(200)
  await expect(page.getByRole('heading', { name: 'Mock finance systems', exact: true }).first()).toBeVisible()
  await expect(page.getByText('Finance', { exact: true }).first()).toBeVisible()
})

test('legacy company paths permanently redirect to Meier Made', async ({ request }) => {
  for (const path of ['/services', '/projects']) {
    const response = await request.get(path, { maxRedirects: 0 })
    expect([301, 308], `${path} permanent redirect status`).toContain(response.status())
    expect(response.headers().location).toBe(`https://meiermade.com${path}`)
  }
})

test('articles remain usable at a mobile viewport', async ({ page }) => {
  await page.setViewportSize({ width: 390, height: 844 })
  await page.goto('/articles', { waitUntil: 'domcontentloaded' })

  await expect(page.getByRole('heading', { name: 'Articles', exact: true })).toBeVisible()
  expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true)
})
