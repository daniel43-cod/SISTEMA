# Esquemas de referencia FEL

Descargados el 10 de noviembre de 2026:

- SAT: https://github.com/notificacioneselectfel/Catalogo-FEL/blob/main/XSD/GT_Documento-0.2.1.xsd
- Firma XML: https://www.w3.org/TR/2002/REC-xmldsig-core-20020212/xmldsig-core-schema.xsd

El repositorio Catalogo-FEL se identifica como mantenido por la Gerencia de Informática de SAT.
Los archivos XSD se conservan sin modificaciones. La licencia SAT se incluye en SAT-LICENSE.txt;
el archivo de W3C contiene su aviso de derechos y licencia.

FacturaXsdValidator genera una proyección EN MEMORIA sin los dos elementos Signature de la raíz,
porque el endpoint unificado de INFILE recibe el documento antes de firmarlo.
Esta proyección solo valida datos y estructura PREVIOS a firma. No valida firmas electrónicas,
autorización fiscal, vigencia del NIT ni reglas de negocio de SAT.
No se consultan esquemas ni DTD por Internet al validar una factura.
Actualizar estas copias requiere revisar cambios y volver a ejecutar tests/Fel.Checks.
