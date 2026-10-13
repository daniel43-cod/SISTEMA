const fs = require('fs');
const path = require('path');
const root = process.cwd();
function walk(dir) { return fs.readdirSync(dir, {withFileTypes:true}).flatMap(e =>
  ['bin','obj','.git','.artifacts','node_modules'].includes(e.name) ? [] :
  e.isDirectory() ? walk(path.join(dir,e.name)) : [path.join(dir,e.name)]); }
const files = [...walk(path.join(root,'API_SISTEMA')), ...walk(path.join(root,'tests'))].filter(f=>/\.cs$/i.test(f));
const names = new Map(Object.entries({
  productocrear:'CrearProductoDto', DetalleVenta_Controller:'DetalleVentaController',
  DetalleVenta_Service:'DetalleVentaService', Detalle_venta:'DetalleVenta',
  Inventario_movimiento:'InventarioMovimiento', Producto_Presentacion:'ProductoPresentacion',
  Producto_precio:'ProductoPrecio', Rol_permisocs:'RolPermiso', Tabla_permiso:'TablaPermiso',
  usuario_permiso:'UsuarioPermiso', CrearPresentacionServices:'CrearPresentacionService',
  ListarPresentacionServices:'ListarPresentacionService', BuscarVentaServices:'BuscarVentaService',
  AbonarSaldoVentaServices:'AbonarSaldoVentaService', Pago:'PagoCompraService',
  conexion:'ConexionDbContext'
}));
for (const f of files) for (const m of fs.readFileSync(f,'utf8').matchAll(/\b(?:class|record|interface|enum)\s+(\w+)/g)) {
  if (/DTOs?$/.test(m[1])) names.set(m[1],m[1].replace(/DTOs?$/,'Dto'));
}
const namespaceReplacements = [
 ['API_SISTEMA.controllers','API_SISTEMA.Controllers'], ['API_SISTEMA.models','API_SISTEMA.Models'],
 ['API_SISTEMA.data','API_SISTEMA.Data'], ['API_SISTEMA.services','API_SISTEMA.Services'],
 ['API_SISTEMA.Securyti','API_SISTEMA.Security'], ['API_SISTEMA.DTOs','API_SISTEMA.Dtos'],
 ['API_SISTEMA.Services.Prestacion','API_SISTEMA.Services.Presentaciones'],
 ['API_SISTEMA.Services.ProductoS','API_SISTEMA.Services.Productos'],
 ['API_SISTEMA.Services.CompraS','API_SISTEMA.Services.Compras'],
 ['API_SISTEMA.Dtos.ProductosD','API_SISTEMA.Dtos.Productos'],
 ['API_SISTEMA.Dtos.EmpresaDTOs','API_SISTEMA.Dtos.Empresa'],
 ['API_SISTEMA.Dtos.RegistrCliente','API_SISTEMA.Dtos.RegistroCliente']
];
// Match strings/comments first so renaming types never changes JSON, SQL, routes or user messages.
const tokens = /\/\/[^\r\n]*|\/\*[\s\S]*?\*\/|@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'|\b[A-Za-z_]\w*\b/g;
const moves=[];
for (const f of files) {
 let s=fs.readFileSync(f,'utf8');
 for(const [a,b] of namespaceReplacements) s=s.split(a).join(b);
 s=s.replace(tokens, token=>names.get(token)??token);
 // Lowercase model names also occur as navigation properties/local variables: change only type positions.
 s=s.replace(/\bclass caja\b/g,'class Caja').replace(/<caja>/g,'<API_SISTEMA.Models.Caja>')
   .replace(/typeof\(caja\)/g,'typeof(API_SISTEMA.Models.Caja)').replace(/\bnew caja\b/g,'new API_SISTEMA.Models.Caja')
   .replace(/\bpublic caja caja\b/g,'public Caja caja');
 s=s.replace(/\bclass permisos\b/g,'class Permisos').replace(/\bpermisos\.Vender\b/g,'Permisos.Vender');
 if(f.endsWith('DetalleVenta_Controller.cs')) s=s.replace('[Route("api/[controller]")]','[Route("api/DetalleVenta_")]');
 fs.writeFileSync(f,s);
 if(f.startsWith(path.join(root,'API_SISTEMA')+path.sep)) {
   const first = [...s.matchAll(/\b(?:public|internal)\s+(?:(?:sealed|static|abstract|partial)\s+)*(?:class|record|interface|enum)\s+(\w+)/g)][0];
   // Multi-type files keep their primary type's name, not an unrelated response type.
   const oldBase=path.basename(f).replace(/\.cs$/i,'');
   let base = names.get(oldBase)??oldBase;
   if(oldBase==='caja')base='Caja'; if(oldBase==='permisos')base='Permisos';
   if(oldBase==='VentasService')base='VentaService';
   if(oldBase==='RegistroCuentaService')base='RegistroCuentaClienteService';
   if(first && !s.includes('class '+base) && !s.includes('record '+base) && oldBase==='Rol_permisocs')base=first[1];
   const dest=path.join(path.dirname(f),base+'.cs');
   if(dest!==f)moves.push([f,dest]);
 }
}
// Windows needs an intermediate name for renames that change only casing.
function move(a,b) { if(fs.existsSync(b)&&a.toLowerCase()!==b.toLowerCase())throw Error('Target exists: '+b);
 const tmp=a+'.naming-tmp';fs.renameSync(a,tmp);fs.renameSync(tmp,b); }
for(const [a,b] of moves)move(a,b);
const dirs=[['services/marca','services/Marca'],['services/Prestacion','services/Presentaciones'],
 ['services/ProductoS','services/Productos'],['DTOs/Producto','DTOs/Productos'],['DTOs/Compra','DTOs/Compras'],
 ['DTOs/RegistrCliente','DTOs/RegistroCliente'],['Securyti','Security'],['controllers','Controllers'],
 ['models','Models'],['data','Data'],['services','Services'],['DTOs','Dtos']];
for(const [a,b] of dirs) { const from=path.join(root,'API_SISTEMA',a);if(fs.existsSync(from))move(from,path.join(root,'API_SISTEMA',b)); }
fs.writeFileSync(path.join(root,'.artifacts/naming-map.json'),JSON.stringify({types:Object.fromEntries(names),files:moves.map(([a,b])=>[path.relative(root,a),path.relative(root,b)]),directories:dirs},null,2));
console.log('Updated '+files.length+' C# files; '+moves.length+' file renames.');
