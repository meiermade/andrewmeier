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

test('article filter disclosures support native keyboard selection', async ({ page }) => {
  test.skip(isRemote, 'Published production filters are not deterministic fixtures.')

  for (const filter of [
    { name: 'tag', ariaLabel: 'Tag filter', value: 'Engineering' },
    { name: 'year', ariaLabel: 'Published year filter', value: '2026' },
  ]) {
    await page.goto('/articles', { waitUntil: 'domcontentloaded' })
    await page.getByText('+ Add filter', { exact: true }).click()

    const disclosure = page.locator(`[data-filter-control="${filter.name}"]`)
    const button = disclosure.getByRole('button', { name: filter.ariaLabel })
    const panel = disclosure.locator('[data-disclosure-panel]')
    const option = disclosure.getByRole('link', { name: filter.value, exact: true })

    await button.focus()
    await button.press('Enter')
    await expect(panel).toBeVisible()
    await expect(button).toHaveAttribute('aria-expanded', 'true')
    await page.keyboard.press('Tab')
    await expect(option).toBeFocused()

    await page.keyboard.press('Escape')
    await expect(panel).toBeHidden()
    await expect(button).toBeFocused()

    await button.press('Space')
    await expect(panel).toBeVisible()
    await page.keyboard.press('Tab')
    await expect(option).toBeFocused()
    await page.keyboard.press('Enter')

    await expect(page).toHaveURL(new RegExp(`[?&]${filter.name}=${filter.value}(?:&|$)`))
  }
})

test('navigation disclosures use native keyboard behavior without Tailwind Elements', async ({ page }) => {
  await page.goto('/', { waitUntil: 'domcontentloaded' })

  await expect(page.locator('el-dropdown, el-menu, el-select, el-options, el-option')).toHaveCount(0)
  await expect(page.locator('[role="menu"], [role="menuitem"], [role="menuitemradio"]')).toHaveCount(0)

  const themeButton = page.getByRole('button', { name: 'Choose theme' })
  const themePanel = page.locator('#theme-panel')
  await themeButton.focus()
  await themeButton.press('Enter')
  await expect(themePanel).toBeVisible()
  await page.keyboard.press('Tab')
  await expect(page.getByRole('button', { name: 'Light' })).toBeFocused()
  await page.keyboard.press('Tab')
  await expect(page.getByRole('button', { name: 'Dark' })).toBeFocused()
  await page.keyboard.press('Enter')
  await expect(themePanel).toBeHidden()
  await expect(themeButton).toBeFocused()
  await expect(page.locator('html')).toHaveClass(/dark/)

  await page.setViewportSize({ width: 390, height: 844 })
  const navigationButton = page.getByRole('button', { name: 'Open navigation' })
  const navigationPanel = page.locator('#navigation-panel')
  await navigationButton.focus()
  await navigationButton.press('Space')
  await expect(navigationPanel).toBeVisible()
  await page.keyboard.press('Tab')
  await expect(navigationPanel.getByRole('link', { name: 'Articles' })).toBeFocused()
  await page.keyboard.press('Escape')
  await expect(navigationPanel).toBeHidden()
  await expect(navigationButton).toBeFocused()
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
