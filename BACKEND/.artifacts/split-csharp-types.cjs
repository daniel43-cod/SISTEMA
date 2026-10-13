const fs=require('fs'),path=require('path');
function walk(dir){return fs.readdirSync(dir,{withFileTypes:true}).flatMap(e=>['bin','obj'].includes(e.name)?[]:e.isDirectory()?walk(path.join(dir,e.name)):[path.join(dir,e.name)])}
let count=0;
for(const f of walk('API_SISTEMA').filter(f=>/\.cs$/i.test(f))){
 const source=fs.readFileSync(f,'utf8');
 // Preserve offsets while hiding braces in comments and literals.
 const masked=source.replace(/\/\/[^\r\n]*|\/\*[\s\S]*?\*\/|@"(?:""|[^"])*"|"(?:\\.|[^"\\])*"|'(?:\\.|[^'\\])*'/g,s=>s.replace(/[^\r\n]/g,' '));
 const ns=/namespace\s+([\w.]+)\s*([;{])/.exec(masked);if(!ns)continue;
 const depthAt=new Int32Array(masked.length+1);let depth=0;
 for(let i=0;i<masked.length;i++){depthAt[i]=depth;if(masked[i]==='{')depth++;if(masked[i]==='}')depth--;}
 const types=[...masked.matchAll(/\b(?:public|internal)\s+(?:(?:sealed|static|abstract|partial)\s+)*(?:class|record|interface|enum)\s+(\w+)/g)].filter(m=>depthAt[m.index]===(ns[2]===';'?0:1));
 if(types.length<2)continue;
 const primary=path.basename(f,'.cs');
 for(const m of types){if(m[1]===primary)continue;
   let end=masked.indexOf('{',m.index),semicolon=masked.indexOf(';',m.index);
   if(semicolon!==-1&&(end===-1||semicolon<end))end=semicolon+1;
   else {const level=depthAt[end];end++;while(end<masked.length&&!(masked[end]==='}'&&depthAt[end]===level+1))end++;end++;}
   const destination=path.join(path.dirname(f),m[1]+'.cs');if(fs.existsSync(destination))throw Error(destination);
   fs.writeFileSync(destination,source.slice(0,ns.index).replace(/\uFEFF/g,'')+'namespace '+ns[1]+';\n\n'+source.slice(m.index,end)+'\n');
   m.end=end;count++;
 }
 let remaining=source;
 for(const m of types.filter(m=>m.end).reverse())remaining=remaining.slice(0,m.index)+remaining.slice(m.end);
 fs.writeFileSync(f,remaining);
}
console.log(count+' types moved into matching files.');
