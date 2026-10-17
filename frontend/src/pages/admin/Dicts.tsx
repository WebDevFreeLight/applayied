import { useEffect, useState } from 'react'
import { useTranslation } from 'react-i18next'
import * as sys from '../../api/system'
import type { DictType, DictData } from '../../api/system'
import { useAuth } from '../../auth/auth'
import { Empty, ErrorBox, Loading } from '../../components/ui'

// Read-only dictionary browser: pick a type on the left, see its entries on the right.
export default function Dicts() {
  const { t } = useTranslation()
  const { hasPerm } = useAuth()
  const [types, setTypes] = useState<DictType[]>([])
  const [active, setActive] = useState<DictType | null>(null)
  const [data, setData] = useState<DictData[]>([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    sys.listDictType({ pageNum: 1, pageSize: 200 }).then((p) => setTypes(p.result || []))
      .catch((e) => setError((e as Error).message)).finally(() => setLoading(false))
  }, [])
  const open = async (dt: DictType) => {
    setActive(dt)
    try { setData(await sys.listDictData(dt.dictType)) } catch (e) { setError((e as Error).message) }
  }

  if (!hasPerm('system:dict:list')) return <div className="panel">{t('admin.noAccess')}</div>
  return (
    <div>
      <h2 style={{ margin: 0 }}>{t('sys.dicts')}</h2>
      {error && <ErrorBox message={error} />}
      {loading ? <Loading /> : (
        <div className="grid2" style={{ marginTop: 12 }}>
          <div className="panel" style={{ maxHeight: '70vh', overflow: 'auto' }}>
            <h3>{t('sys.dictType')}</h3>
            {types.length === 0 ? <Empty /> : types.map((dt) => (
              <div key={dt.dictId} className={`navlink ${active?.dictId === dt.dictId ? 'active' : ''}`}
                style={{ cursor: 'pointer' }} onClick={() => open(dt)}>
                {dt.dictName} <span className="muted">· {dt.dictType}</span>
              </div>))}
          </div>
          <div className="panel" style={{ maxHeight: '70vh', overflow: 'auto' }}>
            <h3>{active ? active.dictName : t('sys.dictLabel')}</h3>
            {!active ? <Empty /> : (
              <table><thead><tr><th>{t('sys.dictLabel')}</th><th>{t('sys.dictValue')}</th><th>{t('admin.orderNum')}</th></tr></thead>
                <tbody>{data.map((d) => (
                  <tr key={d.dictCode}><td>{d.dictLabel}</td><td><code>{d.dictValue}</code></td><td>{d.dictSort}</td></tr>))}</tbody>
              </table>)}
          </div>
        </div>
      )}
    </div>
  )
}
