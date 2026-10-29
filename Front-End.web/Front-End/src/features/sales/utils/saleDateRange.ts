export function lastSevenSaleDays(now = new Date()) {
  const parts = new Intl.DateTimeFormat('en-US', { timeZone: 'America/Guatemala', year: 'numeric', month: '2-digit', day: '2-digit' }).formatToParts(now)
  const part = (type: string) => parts.find(p => p.type === type)!.value
  const hasta = part('year') + '-' + part('month') + '-' + part('day')
  const start = new Date(hasta + 'T00:00:00Z')
  start.setUTCDate(start.getUTCDate() - 6)
  return { desde: start.toISOString().slice(0,10), hasta }
}
