import { describe, it, expect } from 'vitest'
import fs from 'node:fs'
import path from 'node:path'

const modules = ['auth', 'catalog', 'inventory', 'ordering', 'payment', 'shipping', 'location', 'users', 'profile', 'reports']
const subdirs = ['api', 'components', 'composables', 'store', 'schemas', 'types']

const featuresDir = path.resolve(__dirname, '../../features')

describe('module directory structure', () => {
  it.each(modules)('%s has all standard subdirectories', (mod) => {
    const base = path.join(featuresDir, mod)
    for (const dir of subdirs) {
      const dirPath = path.join(base, dir)
      expect(fs.existsSync(dirPath), `${mod}/${dir} should exist`).toBe(true)
    }
    const hasTypes = fs.existsSync(path.join(base, 'types'))
    expect(hasTypes, `${mod} should have types/`).toBe(true)
  })

  it.each(modules)('%s has a pages directory maintained', (mod) => {
    const pagesPath = path.join(featuresDir, mod, 'pages')
    expect(fs.existsSync(pagesPath), `${mod}/pages should exist`).toBe(true)
  })
})

const removedFiles = [
  'features/catalog/pages/ProductCreatePage.vue',
  'features/catalog/pages/TaxonListPage.vue',
  'features/catalog/pages/TaxonTreeManagerPage.vue',
  'features/catalog/pages/OptionValueListPage.vue',
  'features/inventory/pages/StockImportPage.vue',
  'features/inventory/pages/UnitListPage.vue',
  'features/ordering/pages/OrderCreatePage.vue',
  'features/users/pages/StaffCreatePage.vue',
]

describe('deprecated page removal', () => {
  it.each(removedFiles)('%s does not exist', (file) => {
    const filePath = path.resolve(__dirname, '../../', file)
    expect(fs.existsSync(filePath), `${file} should be removed`).toBe(false)
  })
})
