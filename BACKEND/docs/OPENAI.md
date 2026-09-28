# Asistente de productos con OpenAI

`POST /api/Mensajes` requiere el JWT de una cuenta cliente. El backend comprueba
la propiedad de la conversación, guarda la pregunta, envía hasta diez turnos
completados de esa conversación y la pregunta actual a Responses API, guarda
la respuesta y devuelve los identificadores. Las preguntas fallidas quedan con
respuesta nula y no se incluyen en el contexto de los siguientes turnos.

## Configuración

Configurar `OPENAI_API_KEY` y `OPENAI_MODEL` como variables de entorno del proceso
que ejecuta la API. El modelo debe estar disponible en tu cuenta y admitir
Responses y function calling. No hay un modelo predeterminado. No guardar la
clave en Git, Swagger, frontend ni appsettings. Reiniciar la API después del cambio.

En PowerShell puedes introducir la clave sin mostrarla en pantalla ni escribirla
literalmente en el historial:

```powershell
$claveIA = Read-Host 'Clave de OpenAI' -AsSecureString
$env:OPENAI_API_KEY = [System.Net.NetworkCredential]::new('', $claveIA).Password
$env:OPENAI_MODEL = Read-Host 'Identificador del modelo habilitado en tu cuenta'
dotnet run --project API_SISTEMA/API_SISTEMA.csproj --launch-profile http
```

La integración usa HttpClient, sin paquetes nuevos. Referencia:
https://developers.openai.com/api/docs/guides/function-calling

## Prueba manual

Iniciar sesión en `/api/CuentaCliente/login`, autorizar Swagger con el token y enviar:

```json
{ "idConversacion": null, "mensaje": "¿Qué productos Coca-Cola tienen?" }
```

Reutilizar el ID devuelto para seguir el chat. OpenAI solicita `BuscarProductos`;
el backend ejecuta una consulta EF de solo lectura y devuelve como máximo diez
presentaciones activas, sus precios registrados y disponibilidad calculada por
presentación. No envía costos, contraseñas, tokens, correos ni otras tablas.
El texto escrito por el cliente sí se envía: no debe incluir información sensible.
`store:false` evita almacenar la respuesta para recuperación en Responses; no
es una garantía de retención cero del proveedor.

La búsqueda es textual por nombre/presentación; no busca por marca en una tabla
separada ni garantiza reconocer todos los sinónimos. La herramienta no vende ni
reserva stock. Las respuestas de IA pueden contener errores y requieren evaluación.

## Errores y límites

- 503: variables ausentes o acceso del proveedor rechazado.
- 429: límite/cuota del proveedor.
- 502: fallo de red/proveedor, salida inválida o incompleta.
- 504: tiempo total superior a 60 segundos.

Los errores del proveedor posteriores al guardado incluyen `idMensaje` e
`idConversacion`. Conservar el ID de conversación; enviar otro mensaje crea otro
registro. No se reintenta automáticamente ni existe aún un endpoint de reintento
del mismo mensaje. Si falla la persistencia después de obtener la respuesta,
se devuelve 500. Cancelar una petición no garantiza que el proveedor no la facture.

Entrada máxima: 2000 caracteres; salida aceptada máxima: 8000. Columnas verificadas
como `nvarchar(max)` y modelo alineado. Hasta tres solicitudes a OpenAI por mensaje,
dos rondas de herramientas. Enviar mensajes secuencialmente desde el frontend:
todavía no hay cola ni serialización distribuida por conversación. El límite de
llamadas por mensaje no sustituye un límite de solicitudes por cliente para producción.

## Comprobaciones automáticas sin consumo de API

```powershell
dotnet run --project tests/OpenAIService.Checks/OpenAIService.Checks.csproj
```

Simulan el intercambio de herramientas, autorización del proveedor, cuota,
fallos de red, JSON inválido, respuesta incompleta, timeout y cancelación.
No sustituyen una prueba real con tu modelo y SQL Server.
