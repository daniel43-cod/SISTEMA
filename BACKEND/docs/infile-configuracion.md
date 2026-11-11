# Configuración privada de INFILE

El backend lee la sección `Infile` de la configuración de ASP.NET Core.
No guardar credenciales en SQL, archivos versionados ni en el frontend.
El proyecto ya dispone de UserSecretsId. En desarrollo se puede usar **Administrar secretos de usuario**
en Visual Studio y agregar esta sección al JSON privado, conservando las demás secciones existentes:

```json
{
  "Infile": {
    "Habilitado": true,
    "Ambiente": "PRUEBAS",
    "UsuarioApi": "REEMPLAZAR",
    "LlaveApi": "REEMPLAZAR",
    "UsuarioFirma": "REEMPLAZAR",
    "LlaveFirma": "REEMPLAZAR"
  }
}
```

Los valores REEMPLAZAR son marcadores, no credenciales válidas.
User Secrets se carga por defecto en el ambiente Development; es almacenamiento local sin cifrar.
No confundir ASPNETCORE_ENVIRONMENT con el estado de implementación del emisor en INFILE.

Para variables de entorno los nombres equivalentes son:

| Variable | Valor |
| --- | --- |
| `Infile__Habilitado` | `true` |
| `Infile__Ambiente` | `PRUEBAS` o `PRODUCCION` |
| `Infile__UsuarioApi` | Usuario API asignado |
| `Infile__LlaveApi` | Llave API asignada |
| `Infile__UsuarioFirma` | Usuario de firma asignado |
| `Infile__LlaveFirma` | Llave de firma asignada |

Las variables de entorno tienen prioridad sobre User Secrets. Reiniciar el backend después de cambiarlas.
Sin configuración, FEL queda deshabilitado. Al habilitarlo, se valida que existan las cuatro credenciales;
esta validación no comprueba su autenticidad con INFILE.

**Ambiente no cambia el endpoint ni convierte al emisor en Sandbox.** INFILE debe confirmar que
el emisor se encuentra en estado Implementación antes de enviar documentos de prueba.
Esta etapa únicamente prepara la configuración; todavía no certifica ventas ni realiza llamadas externas.
