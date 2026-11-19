import test from 'node:test'
import assert from 'node:assert/strict'
import { receiverDocumentInput, saleReceiver } from '../src/features/sales/utils/saleReceiver.ts'

test('receptor fiscal: selección, límite CF y documentos', () => {
  assert.throws(() => saleReceiver('', '', '', 100), /Selecciona/)
  assert.equal(saleReceiver('CF', '', '', 249999).identificacion_receptor, 'CF')
  assert.throws(() => saleReceiver('CF', '', '', 250000), /2,500/)
  assert.throws(() => saleReceiver('CF', '', '', 250001), /2,500/)
  assert.equal(receiverDocumentInput('CUI', '123a456789012345'), '1234567890123')
  assert.throws(() => saleReceiver('CUI', '123', 'Cliente', 250000), /13/)
  assert.equal(saleReceiver('CUI', '2993930620101', 'Cliente', 250000).tipo_identificacion_receptor, 'CUI')
  assert.equal(receiverDocumentInput('NIT', '1170142-k'), '1170142K')
  assert.equal(saleReceiver('NIT', '1170142k', ' Cliente ', 250000).nombre_receptor, 'Cliente')
  assert.throws(() => saleReceiver('NIT', '12K3', 'Cliente', 100), /NIT/)
})
