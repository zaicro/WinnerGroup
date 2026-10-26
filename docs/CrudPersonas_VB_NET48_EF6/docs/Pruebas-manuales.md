# Pruebas del formulario

1. Abrir por primera vez: aparece la lista vacía; existe el MDF en la ruta documentada.
2. Crear `Ana`: aparece una fila con Id generado.
3. Crear `  Andrés Sierra  `: aparece sin espacios exteriores y conserva la tilde.
4. Intentar crear con nombre vacío y luego solo espacios: muestra validación, no agrega filas.
5. Seleccionar `Ana`, cambiar a `Ana María`, guardar: conserva el Id.
6. Pulsar Nuevo / cancelar después de seleccionar una fila: limpia el nombre y muestra Crear.
7. Seleccionar una persona, pulsar Eliminar y responder No: conserva el registro.
8. Eliminar y responder Sí: desaparece el registro.
9. Cerrar y abrir: conserva los datos y no vuelve a crear la tabla ni a aplicar Inicial.
10. Intentar abrir una segunda instancia en la misma sesión: informa que ya está abierta.
11. Compilar en Release y ejecutar desde su carpeta: usa el mismo catálogo y conserva los datos.
12. Desconectar la red con los paquetes ya restaurados: la aplicación y el CRUD siguen funcionando.

Para el nombre largo, el TextBox impide superar 150 caracteres; el script de verificación además prueba el rechazo en la capa de negocio.

Si falla el arranque, consulta el registro documentado. No borres la base para ocultar una incompatibilidad del modelo o un fallo de migración.
