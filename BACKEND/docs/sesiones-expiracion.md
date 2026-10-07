# Seguimiento de sesiones internas

Requiere sesiones_usuario y auditoria_evento ya creadas, y la columna nullable
motivo_cierre varchar(20) con CHECK VOLUNTARIO/EXPIRACION. No se ejecuta DDL al iniciar.

Cada login crea un GUID de sesión que se incluye en el JWT como id_sesion. Guarda
SHA-256 del JWT (no un refresh token), fecha UTC de creación y vencimiento exacto del
JWT. Sesión y SESION_INICIADA se insertan en el mismo SaveChanges transaccional.
La respuesta del login conserva su contrato; no cambia el frontend.

UsuarioTokenValidator exige una sesión perteneciente al usuario, sin cierre y no
vencida. Tokens anteriores sin id_sesion requieren nuevo login. El flujo de clientes
no se modifica. Cerrar sesión voluntariamente afecta únicamente la sesión actual.

SesionExpiracionWorker procesa al arrancar y luego cada 30 segundos lotes de hasta
100 sesiones pendientes. Si el backend estuvo apagado procesa vencimientos atrasados
al volver; para continuar procesando necesita estar ejecutándose. Con una cola grande
la detección puede tardar varios ciclos. La validez del JWT no depende del worker.

SESION_EXPIRADA usa origen SISTEMA, responsable propietario de la sesión, entidad
sesiones_usuario, IdRegistro=GUID y FechaUtc igual al vencimiento, no al procesamiento.
No inventa IP o TraceId para un evento automático. No procesa sesiones ya cerradas.

El cierre usa UPDATE condicional dentro de una transacción junto con la auditoría:
solo una instancia puede confirmar cada cierre. Si falla la auditoría se revierte
el cierre y se reintenta después. No depende solo de row_version para evitar duplicados.

Pruebas Auth.Checks verifican login, invalidación tras cierre, expiración sin navegador,
fecha real y procesamiento repetido. Usan SQLite con adaptación de rowversion y tipos
específicos; se necesita validar el despliegue y concurrencia en SQL Server real.
