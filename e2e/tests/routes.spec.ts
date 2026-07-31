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

test('article filters support keyboard navigation and selection', async ({ page }) => {
  test.skip(isRemote, 'Published production filters are not deterministic fixtures.')

  for (const filter of [
    { name: 'tag', ariaLabel: 'Tag filter', value: 'Engineering' },
    { name: 'year', ariaLabel: 'Published year filter', value: '2026' },
  ]) {
    await page.goto('/articles', { waitUntil: 'domcontentloaded' })
    await page.getByText('+ Add filter', { exact: true }).click()

    const select = page.locator('[data-select-root]').filter({ has: page.locator(`input[name="${filter.name}"]`) })
    const button = select.getByRole('combobox', { name: filter.ariaLabel })
    const options = select.getByRole('listbox')
    const selectedOption = select.locator(`[role="option"][value="${filter.value}"]`)

    await button.focus()
    await button.press('ArrowDown')
    await expect(options).toBeVisible()
    await expect(select.getByRole('option').first()).toBeFocused()

    await page.keyboard.press('Escape')
    await expect(options).toBeHidden()
    await expect(button).toBeFocused()

    await button.press('ArrowDown')
    await expect(select.getByRole('option').first()).toBeFocused()
    await page.keyboard.press('ArrowDown')
    await expect(selectedOption).toBeFocused()
    await page.keyboard.press('Enter')

    await expect(page).toHaveURL(new RegExp(`[?&]${filter.name}=${filter.value}(?:&|$)`))
  }
})

test('navigation menus work by keyboard without Tailwind Elements', async ({ page }) => {
  await page.goto('/', { waitUntil: 'domcontentloaded' })

  await expect(page.locator('el-dropdown, el-menu, el-select, el-options, el-option')).toHaveCount(0)

  const themeButton = page.getByRole('button', { name: 'Choose theme' })
  const themeMenu = page.getByRole('menu', { name: 'Theme' })
  await themeButton.focus()
  await themeButton.press('ArrowDown')
  await expect(themeMenu).toBeVisible()
  await expect(page.getByRole('menuitemradio', { name: 'Light' })).toBeFocused()
  await page.keyboard.press('ArrowDown')
  await expect(page.getByRole('menuitemradio', { name: 'Dark' })).toBeFocused()
  await page.keyboard.press('Enter')
  await expect(themeMenu).toBeHidden()
  await expect(themeButton).toBeFocused()
  await expect(page.locator('html')).toHaveClass(/dark/)

  await page.setViewportSize({ width: 390, height: 844 })
  const navigationButton = page.getByRole('button', { name: 'Open navigation' })
  const navigationMenu = page.getByRole('menu', { name: 'Navigation' })
  await navigationButton.focus()
  await navigationButton.press('ArrowDown')
  await expect(navigationMenu).toBeVisible()
  await expect(navigationMenu.getByRole('menuitem').first()).toBeFocused()
  await page.keyboard.press('Escape')
  await expect(navigationMenu).toBeHidden()
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
