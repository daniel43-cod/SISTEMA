# Auditoría de presentaciones

Sin cambios en el frontend ni en el contrato HTTP.
Los servicios agregan eventos mediante PresentacionAuditoriaService:
- PRESENTACION_CREADA: descripción y estado inicial.
- PRESENTACION_EDITADA: descripción anterior y nueva.
- PRESENTACION_ACTIVADA y PRESENTACION_DESACTIVADA: estado anterior y nuevo.

Entidad presentaciones, IdRegistro=ID de presentación, responsable consultado en BD
con el ID autenticado, fecha UTC, origen API, IP y TraceId. Solo se serializan
campos modificados: descripción al editar y estado al activar/desactivar. La creación
conserva descripción y estado inicial. No se incluyen credenciales ni entidades completas.
Los registros históricos existentes no se modifican.

Cambio y evento se confirman en la misma transacción. La creación guarda primero
la presentación para obtener su ID, después la auditoría, y al final confirma ambos.
Guardar sin cambios no crea evento de edición/estado. Rechazos no se registran
como cambios exitosos. Si falla la auditoría se revierte la operación.

Requiere auditoria_evento existente. Precio y equivalencia pertenecen a la asociación
producto-presentación y quedan fuera de esta integración.
