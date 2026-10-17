import { useEffect, useState } from 'react'
import * as pmo from '../api/pmo'
import type { LookupItem } from '../api/types'

// Every list is the same unified { id, name } lookup shape for consistent rendering.
export interface Lookups {
  departments: LookupItem[]
  users: LookupItem[]
  managers: LookupItem[]
  projects: LookupItem[]
  categories: LookupItem[]
}

// Loads the reference lists once for filters and form dropdowns. Failures degrade to empty
// lists (the page still renders) rather than blocking the whole screen.
export function useLookups(): Lookups {
  const [state, setState] = useState<Lookups>({ departments: [], users: [], managers: [], projects: [], categories: [] })
  useEffect(() => {
    let alive = true
    Promise.allSettled([
      pmo.getDepartments(), pmo.getUsers(), pmo.getManagers(), pmo.getProjectOptions(), pmo.getCategoryOptions(),
    ]).then(([d, u, m, p, c]) => {
      if (!alive) return
      setState({
        departments: d.status === 'fulfilled' ? d.value : [],
        users: u.status === 'fulfilled' ? u.value : [],
        managers: m.status === 'fulfilled' ? m.value : [],
        projects: p.status === 'fulfilled' ? p.value : [],
        categories: c.status === 'fulfilled' ? c.value : [],
      })
    })
    return () => { alive = false }
  }, [])
  return state
}
