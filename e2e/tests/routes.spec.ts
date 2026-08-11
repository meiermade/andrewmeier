import { expect, test } from '@playwright/test'

const isRemote = process.env.E2E_START_LOCAL === '0'
const testImage = Buffer.from('iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mNk+A8AAQUBAScY42YAAAAASUVORK5CYII=', 'base64')

test.beforeEach(async ({ page }) => {
  if (!isRemote) {
    await page.route('https://assets.meiermade.com/andymeier/articles/**', route =>
      route.fulfill({ contentType: 'image/png', body: testImage }),
    )
  }

  await page.addInitScript(() => localStorage.setItem('analytics-consent', 'declined'))
})

test('homepage renders recent articles', async ({ page }) => {
  const response = await page.goto('/', { waitUntil: 'domcontentloaded' })

  expect(response?.status()).toBe(200)
  expect(response?.headers()['strict-transport-security']).toContain('max-age=')
  expect(response?.headers()['x-content-type-options']).toBe('nosniff')
  expect(response?.headers()['referrer-policy']).toBe('strict-origin-when-cross-origin')
  expect(response?.headers()['content-security-policy']).toContain("frame-ancestors 'none'")
  await expect(page.getByRole('heading', { name: 'Andy Meier', exact: true })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Recent articles', exact: true })).toBeVisible()
})

test('articles index renders and opens a source-controlled article', async ({ page }) => {
  const response = await page.goto('/articles', { waitUntil: 'domcontentloaded' })

  expect(response?.status()).toBe(200)
  await expect(page.getByRole('heading', { name: 'Articles', exact: true })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'F# Semantic Kernel', exact: true })).toBeVisible()

  const articleResponse = await page.goto('/articles/fsharp-semantic-kernel', { waitUntil: 'domcontentloaded' })
  expect(articleResponse?.status()).toBe(200)
  await expect(page.locator('article')).toBeVisible()
  await expect(page.getByRole('heading', { name: 'F# Semantic Kernel', exact: true })).toBeVisible()
  await expect(page.getByText('Microsoft’s Semantic Kernel SDK', { exact: false }).first()).toBeVisible()
})

test('development environment article presents the current setup', async ({ page }) => {
  const response = await page.goto('/articles/dev-env', { waitUntil: 'domcontentloaded' })

  expect(response?.status()).toBe(200)
  await expect(page.getByRole('heading', { name: 'Development Environment', exact: true })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'Pi coding agent', exact: true })).toBeVisible()
  await expect(page.getByRole('heading', { name: 'JetBrains IDEs', exact: true })).toBeVisible()
  await expect(page.getByText('Windows Subsystem for Linux', { exact: false })).toHaveCount(0)
  await expect(page.locator('article > div')).toHaveCSS('padding-bottom', '32px')
})

test('article search, detail content, and GCS images are deterministic', async ({ page }) => {
  await page.goto('/articles?search=semantic', { waitUntil: 'domcontentloaded' })
  await expect(page.getByRole('heading', { name: 'F# Semantic Kernel', exact: true })).toBeVisible()
  await expect(page.getByRole('link', { name: 'Clear search' })).toBeVisible()

  const response = await page.goto('/articles/personal-infrastructure', { waitUntil: 'domcontentloaded' })
  expect(response?.status()).toBe(200)
  await expect(page.getByRole('heading', { name: 'Personal Infrastructure', exact: true }).first()).toBeVisible()
  await expect(page.getByText('DevOps', { exact: true }).first()).toBeVisible()

  const clusterImage = page.getByRole('img', { name: 'Three Raspberry Pi computers mounted in a home server rack' })
  await expect(clusterImage).toHaveAttribute('src', /^https:\/\/assets\.meiermade\.com\/andymeier\/articles\//)
  await expect.poll(() => clusterImage.evaluate(image => (image as HTMLImageElement).naturalWidth)).toBeGreaterThan(0)
})

test('article filter disclosures support native keyboard selection', async ({ page }) => {
  for (const filter of [
    { name: 'tag', ariaLabel: 'Tag filter', value: '.NET' },
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
    if (await panel.evaluate(element => element === document.activeElement)) {
      await page.keyboard.press('Tab')
    }
    await expect(option).toBeFocused()

    await page.keyboard.press('Escape')
    await expect(panel).toBeHidden()
    await expect(button).toBeFocused()

    await button.press('Space')
    await expect(panel).toBeVisible()
    await page.keyboard.press('Tab')
    if (await panel.evaluate(element => element === document.activeElement)) {
      await page.keyboard.press('Tab')
    }
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

  for (const article of [
    { path: '/articles/personal-infrastructure', title: 'Personal Infrastructure' },
    { path: '/articles/dev-env', title: 'Development Environment' },
  ]) {
    await page.goto(article.path, { waitUntil: 'domcontentloaded' })
    await expect(page.getByRole('heading', { name: article.title, exact: true })).toBeVisible()
    expect(await page.evaluate(() => document.documentElement.scrollWidth <= window.innerWidth)).toBe(true)
  }
})
