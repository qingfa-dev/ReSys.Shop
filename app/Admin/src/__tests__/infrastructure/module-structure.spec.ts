import { describe, it, expect } from 'vitest'
import fs from 'node:fs'
import path from 'node:path'

const modules = ['catalog', 'inventory', 'ordering', 'payment', 'shipping', 'location', 'users', 'profile', 'reports']
const subdirs = ['api', 'components', 'composables', 'models', 'store', 'utils']

const featuresDir = path.resolve(__dirname, '../../features')

describe('module directory structure', () => {
  it.each(modules)('%s has all standard subdirectories', (mod) => {
    const base = path.join(featuresDir, mod)
    for (const dir of subdirs) {
      const dirPath = path.join(base, dir)
      expect(fs.existsSync(dirPath), `${mod}/${dir} should exist`).toBe(true)
    }
  })

  it.each(modules)('%s has a pages directory maintained', (mod) => {
    const pagesPath = path.join(featuresDir, mod, 'pages')
    expect(fs.existsSync(pagesPath), `${mod}/pages should exist`).toBe(true)
  })
})
