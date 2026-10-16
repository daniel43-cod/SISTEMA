$ErrorActionPreference = 'Stop'
$frontendRoot = 'C:\Users\coy81\Desktop\SISTEMA\SISTEMA\Front-End.web\Front-End'
$pagePath = Join-Path $frontendRoot 'src/features/cash/pages/CashPage.tsx'
$cssPath = Join-Path $frontendRoot 'src/features/cash/pages/CashPage.css'
$page = [System.IO.File]::ReadAllText($pagePath).Replace("`r`n", "`n")
$hint = '              <p className="cash-hint">Totales de todo el turno al consultar. Mostrando {summary.movimientos.length} de {summary.total_movimientos} movimientos, del más reciente al más antiguo.</p>'
$start = $page.IndexOf('                <ul className="cash-movement-list">')
$end = $page.IndexOf('                </ul>}', $start)
if (-not $page.Contains($hint) -or $start -lt 0 -or $end -lt 0) { throw 'El listado cambió; revisar antes de editar.' }
$table = @'
                <div className="cash-movement-table-scroll">
                  <table className="cash-movement-table">
                    <thead><tr>
                      <th scope="col">Movimiento</th>
                      <th scope="col">Encargado</th>
                      <th scope="col">Fecha y hora</th>
                      <th scope="col">Tipo</th>
                      <th scope="col">Monto</th>
                    </tr></thead>
                    <tbody>{summary.movimientos.map(movement => {
                      const incoming = ['ENTRADA', 'INGRESO'].includes(movement.naturaleza.trim().toUpperCase())
                      const showDescription = movement.descripcion &&
                        !/^pago\s+(?:de\s+)?(?:compras?|ventas?)\s*#/i.test(movement.descripcion.trim())
                      return <tr key={movement.id_movimiento_caja}>
                        <td><strong>{movement.tipo_movimiento}</strong>
                          {showDescription && <small>{movement.descripcion}</small>}
                        </td>
                        <td>{movement.usuario}</td>
                        <td><time dateTime={movement.fecha_movimiento}>
                          {new Date(movement.fecha_movimiento).toLocaleString('es-GT')}
                        </time></td>
                        <td className={incoming ? 'cash-incoming' : 'cash-outgoing'}>{incoming ? 'Entrada' : 'Salida'}</td>
                        <td className={incoming ? 'cash-incoming' : 'cash-outgoing'}>
                          <strong>{incoming ? '+' : '−'}{money(movement.monto)}</strong>
                        </td>
                      </tr>
                    })}</tbody>
                  </table>
                </div>}
'@
$page = $page.Remove($start, $end + '                </ul>}'.Length - $start).Insert($start, $table)
$page = $page.Replace($hint + "`n", '')
$styles = @'

.cash-movement-table-scroll { max-height: 480px; overflow: auto; }
.cash-movement-table { width: 100%; min-width: 600px; border-collapse: collapse; text-align: left; }
.cash-movement-table th, .cash-movement-table td { padding: 12px 10px; border-bottom: 1px solid #e2e8f0; vertical-align: top; font-size: .875rem; }
.cash-movement-table th { position: sticky; top: 0; background: #f8fafc; color: #475569; font-weight: 600; }
.cash-movement-table td:first-child { min-width: 140px; overflow-wrap: anywhere; }
.cash-movement-table td small { display: block; margin-top: 5px; color: #64748b; }
.cash-movement-table td:nth-child(2) { overflow-wrap: anywhere; }
.cash-movement-table td:nth-child(3) { min-width: 150px; }
.cash-movement-table th:last-child, .cash-movement-table td:last-child { text-align: right; white-space: nowrap; }
'@
$utf8 = New-Object System.Text.UTF8Encoding($false)
[System.IO.File]::WriteAllText($pagePath, $page, $utf8)
[System.IO.File]::AppendAllText($cssPath, $styles, $utf8)
