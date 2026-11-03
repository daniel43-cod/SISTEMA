$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$outputDir = Join-Path $PSScriptRoot 'documentos'
New-Item -ItemType Directory -Path $outputDir -Force | Out-Null
$output = Join-Path $outputDir 'AS2_PresentacionFinal_Dany_Javier_Iqui_Coy.pptx'
Copy-Item -LiteralPath 'C:\Users\coy81\Downloads\AS2_ProyectoFinal_PlantillaPresentacionFinal.pptx' -Destination $output
$map = @{
'[Nombre de tu proyecto]'='Sistema de gestión con asistente de IA';
'[Nombre del proyecto]'='Sistema con asistente de IA';
'[Nombre 1] · [Nombre 2] · [Nombre 3]'='Dany Javier Iqui Coy';
'[dd/mm/aaaa]'='Pendiente de confirmar';
'[pega aquí el enlace, con acceso habilitado para el docente]'='Pendiente de proporcionar';
'[Nombre integrante 1]'='Dany Javier Iqui Coy';
'[Nombre integrante 2]'=''; '[Nombre integrante 3]'='';
'[Explica en 2-3 líneas el problema real que resuelve tu proyecto y para quién es útil.]'="Problema que resuelve`nFacilita a los clientes consultar productos, precios y disponibilidad sin recorrer manualmente el catálogo. El asistente responde en español con datos consultados desde el sistema.";
'[Resume el alcance: módulos, funcionalidades principales que implementaste.]'="Alcance final`nBackend en ASP.NET Core con catálogo y conversaciones. El cliente autenticado envía su consulta; el backend llama a OpenAI, ejecuta búsquedas de solo lectura y guarda la respuesta. El asistente no crea ventas ni modifica inventario.";
'[Herramienta o motor principal]'="Herramienta o motor principal`nASP.NET Core (.NET 10), Entity Framework Core y SQL Server.";
'[Herramienta de diseño/modelado]'="Herramienta de diseño/modelado`nNo se identificó una herramienta de modelado en los archivos revisados.";
'[Lenguaje o entorno adicional]'="Lenguaje o entorno adicional`nC#, LINQ y JSON. HttpClient conecta el backend con la API de OpenAI.";
'[Recursos de apoyo: cursos, documentación]'="Recursos de apoyo`nDocumentación oficial de OpenAI: Responses API y llamadas a funciones. No se verificaron cursos utilizados.";
'[Control de versiones o colaboración]'="Control de versiones o colaboración`nNo se encontró una carpeta .git en el backend revisado. Colaboración no confirmada.";
'[Otros recursos: IA, plantillas, librerías]'="Otros recursos: IA y librerías`nOpenAI Responses API; modelo configurable con OPENAI_MODEL. JWT para acceso y Swagger/OpenAPI para documentar la API.";
'[Cuenta cómo fue el proceso: organización, retos y cómo los resolvieron, y qué tan distinto fue el resultado de lo que imaginaron al inicio.]'="Integración implementada`n1. MensajesController recibe la consulta de un cliente autenticado con JWT.`n2. ConversacionService valida la cuenta y la conversación, guarda la pregunta y recupera hasta 10 intercambios anteriores.`n3. OpenAIService envía el contexto a /v1/responses con autenticación Bearer.`n4. La IA puede solicitar BuscarProductos o BuscarMarcasPorCategoria; CatalogoIAService consulta SQL y devuelve JSON.`n5. El modelo redacta la respuesta y el backend la guarda.`nOrganización, tiempos y expectativas iniciales: pendientes de confirmar con el autor.";
'[Un concepto técnico que ahora dominas mejor]'="Concepto técnico aplicado`nLlamadas a funciones: el modelo solicita una búsqueda y el backend ejecuta la consulta autorizada.";
'[Algo sobre organización o manejo del tiempo]'="Organización del código`nSeparación de responsabilidades entre controlador, servicio de conversación y servicios de IA. Experiencia personal pendiente.";
'[Una habilidad blanda: equipo, comunicación]'="Comunicación y equipo`nPendiente de completar con la experiencia personal de Dany; el código no permite comprobarla.";
'[Algo que harías diferente si empezaras de nuevo]'="Mejora propuesta`nDefinir desde el inicio casos de evaluación para consultas ambiguas, productos agotados y fallos del proveedor.";
'[Justifica brevemente tu nota.]'="Justificación propuesta`nLa integración contempla consultas al catálogo, historial, validaciones y manejo de errores. La nota debe considerar la contribución personal y las evidencias de funcionamiento.";
'[Comparte 2-3 aspectos que disfrutaste del proyecto o del proceso.]'="Aspectos destacados de la implementación`n• El chat combina lenguaje natural con datos del catálogo.`n• Las consultas diferencian productos disponibles y agotados por presentación.`n• La IA consulta funciones de solo lectura sin modificar los datos del negocio.`nPreferencias y experiencias personales: pendientes de confirmar.";
'[Sé autocrítico/a con honestidad: ¿qué parte quedó más débil? ¿qué harías distinto?]'="Oportunidades de mejora`n• Evaluar la búsqueda por coincidencia de texto ante errores de escritura y sinónimos.`n• Añadir métricas de latencia, consumo y calidad de las respuestas.`n• Ampliar la evaluación con conversaciones reales y consultas ambiguas.`nLímites actuales: hasta 10 resultados por búsqueda de productos, hasta 3 rondas de IA y 60 segundos por operación. Estas mejoras son propuestas, no fallos demostrados.";
'[Explica por qué asignas esta nota: qué se cumplió y qué quedó pendiente.]'="Justificación propuesta`nEl código integra OpenAI con consultas reales al catálogo, historial persistente y acceso restringido por cuenta. Incluye validación de herramientas y manejo de errores del proveedor.`nLa revisión del código no demuestra el desempeño en producción ni el cumplimiento total de los requisitos del curso. La calificación queda pendiente de esas evidencias.";
'[Explica tu nota: qué funcionó bien del curso y qué sugerirías mejorar.]'="Justificación propuesta para adaptar`nUn proyecto integrador permite relacionar el análisis de necesidades con una solución concreta y revisar la coherencia entre datos, procesos y funcionalidades. Como sugerencia, podrían incorporarse revisiones parciales con retroalimentación técnica.`nLa nota y la valoración de lo vivido en el curso quedan pendientes de confirmar por el estudiante.";
'[Pega aquí el enlace a la carpeta de Google Drive]'='Enlace a Google Drive: pendiente de proporcionar';
}
$zip = [IO.Compression.ZipFile]::Open($output, [IO.Compression.ZipArchiveMode]::Update)
try {
 foreach ($entry in @($zip.Entries | Where-Object FullName -match '^ppt/slides/slide\d+.xml$')) {
  $name=$entry.FullName
  $reader=[IO.StreamReader]::new($entry.Open()); [xml]$xml=$reader.ReadToEnd(); $reader.Close()
  $ns=[Xml.XmlNamespaceManager]::new($xml.NameTable)
  $ns.AddNamespace('a','http://schemas.openxmlformats.org/drawingml/2006/main')
  $ns.AddNamespace('p','http://schemas.openxmlformats.org/presentationml/2006/main')
  $slide=[int]($entry.Name -replace '\D','')
  foreach($shape in @($xml.SelectNodes('//p:sp[p:txBody]', $ns))) {
   $body=$shape.SelectSingleNode('p:txBody',$ns)
   $nodes=@($body.SelectNodes('.//a:t',$ns))
   $full=($nodes | ForEach-Object InnerText) -join ''
   if($slide -in 2,7 -and ($full -match 'integrante [23]' -or ($slide -eq 2 -and $full -in '2','3'))) { $shape.ParentNode.RemoveChild($shape) | Out-Null; continue }
   $changed=$false
   foreach($t in $nodes){
    $key=$t.InnerText.Trim()
    if($map.ContainsKey($key)){ $t.InnerText=$map[$key]; $changed=$true }
    elseif($key -eq '[Rol en el proyecto]'){ $t.InnerText= if($full -ne ''){'Rol: pendiente de confirmar'}else{''}; $changed=$true }
    elseif($key -eq '[Una breve frase de presentación]'){ $t.InnerText='Presentación personal: pendiente de completar.'; $changed=$true }
    elseif($key -eq '[ ]'){ $t.InnerText='—'; $changed=$true }
   }
   if($slide -eq 2 -and $full -match '\[Rol en el proyecto\]|\[Una breve frase de presentación\]') {
    $off=$shape.SelectSingleNode('p:spPr/a:xfrm/a:off',$ns)
    if([long]$off.GetAttribute('x') -gt 4000000){$shape.ParentNode.RemoveChild($shape)|Out-Null;continue}
   }
   if($slide -eq 7) {
    $off=$shape.SelectSingleNode('p:spPr/a:xfrm/a:off',$ns)
    if($off -and [long]$off.GetAttribute('y') -gt 3500000 -and [long]$off.GetAttribute('y') -lt 6200000){$shape.ParentNode.RemoveChild($shape)|Out-Null;continue}
   }
   if($changed){
    $text=($body.SelectNodes('.//a:t',$ns) | ForEach-Object InnerText) -join ''
    $template=$body.SelectSingleNode('a:p/a:r/a:rPr',$ns)
    if($template){$template=$template.CloneNode($true)}
    foreach($p in @($body.SelectNodes('a:p',$ns))){$body.RemoveChild($p)|Out-Null}
    $lines=$text -split "`n"
    for($i=0;$i -lt $lines.Length;$i++) {
     $p=$xml.CreateElement('a','p',$ns.LookupNamespace('a'));$r=$xml.CreateElement('a','r',$ns.LookupNamespace('a'))
     if($template){$pr=$template.CloneNode($true)}else{$pr=$xml.CreateElement('a','rPr',$ns.LookupNamespace('a'))}
     $pr.SetAttribute('lang','es-GT'); $pr.SetAttribute('i','0')
     if($lines.Length -gt 1){$pr.SetAttribute('b',$(if($i -eq 0){'1'}else{'0'}));$pr.SetAttribute('sz',$(if($slide -eq 4){'1200'}elseif($slide -eq 5){'1600'}else{'1500'}))}
     $r.AppendChild($pr)|Out-Null; $t=$xml.CreateElement('a','t',$ns.LookupNamespace('a'));$t.InnerText=$lines[$i];$r.AppendChild($t)|Out-Null;$p.AppendChild($r)|Out-Null;$body.AppendChild($p)|Out-Null
    }
    $bp=$body.SelectSingleNode('a:bodyPr',$ns)
    foreach($f in @($bp.SelectNodes('a:noAutofit|a:spAutoFit|a:normAutofit',$ns))){$bp.RemoveChild($f)|Out-Null}
    $bp.AppendChild($xml.CreateElement('a','normAutofit',$ns.LookupNamespace('a')))|Out-Null
   }
  }
  if($slide -in 2,7){
   $tree=$xml.SelectSingleNode('//p:spTree',$ns)
   $sample=$xml.SelectSingleNode('//p:sp[p:txBody]',$ns).CloneNode($true)
   foreach($sp in @($tree.SelectNodes('p:sp',$ns))){
    $of=$sp.SelectSingleNode('p:spPr/a:xfrm/a:off',$ns)
    if($of -and [long]$of.GetAttribute('y') -gt 1400000 -and [long]$of.GetAttribute('y') -lt 6200000){$tree.RemoveChild($sp)|Out-Null}
   }
   $sample.SelectSingleNode('p:nvSpPr/p:cNvPr',$ns).SetAttribute('id','100')
   $of=$sample.SelectSingleNode('p:spPr/a:xfrm/a:off',$ns);$of.SetAttribute('x','800000');$of.SetAttribute('y','2000000')
   $ext=$sample.SelectSingleNode('p:spPr/a:xfrm/a:ext',$ns);$ext.SetAttribute('cx','10500000');$ext.SetAttribute('cy','3500000')
   $bd=$sample.SelectSingleNode('p:txBody',$ns)
   foreach($pa in @($bd.SelectNodes('a:p',$ns))){$bd.RemoveChild($pa)|Out-Null}
   $content=if($slide -eq 2){@('Dany Javier Iqui Coy','Rol en el proyecto','Pendiente de confirmar.','Presentación personal','Pendiente de completar por el autor; no se infiere del código.') }else{@('Dany Javier Iqui Coy','Autoevaluación: pendiente / 10','Justificación propuesta','La integración conecta OpenAI con consultas al catálogo y contempla historial, validaciones y manejo de errores. La calificación debe considerar la contribución personal y las evidencias de funcionamiento.')}
   foreach($line in $content){$pa=$xml.CreateElement('a','p',$ns.LookupNamespace('a'));$rr=$xml.CreateElement('a','r',$ns.LookupNamespace('a'));$pr=$xml.CreateElement('a','rPr',$ns.LookupNamespace('a'));$pr.SetAttribute('sz','2000');$pr.SetAttribute('lang','es-GT');$rr.AppendChild($pr)|Out-Null;$tt=$xml.CreateElement('a','t',$ns.LookupNamespace('a'));$tt.InnerText=$line;$rr.AppendChild($tt)|Out-Null;$pa.AppendChild($rr)|Out-Null;$bd.AppendChild($pa)|Out-Null}
   $tree.AppendChild($sample)|Out-Null
  }
  $entry.Delete();$new=$zip.CreateEntry($name);$writer=[IO.StreamWriter]::new($new.Open(),[Text.UTF8Encoding]::new($false));$xml.Save($writer);$writer.Close()
 }
} finally { $zip.Dispose() }
Write-Output $output
