# Plan de correcciones al CRM

## Decisiones confirmadas con el usuario

| Tema | Decisión |
|------|----------|
| Status duplicado en detalle de prospecto | Mostrar solo la sección **ESTATUS** del prospecto; ocultar la sección **COTIZACIÓN**. |
| Texto del botón de borrador no autorizado | Cambiar **"Continuar Editando"** → **"Corregir y reenviar"**. |
| RME – Servicio único / un solo viaje | Agregar opción **"Único"** en el select de tipo de servicio; al elegirla se ocultan los días de la semana y aparece un campo de **fecha puntual**. |
| Unidades de medida adicionales (m²/m³) en RME | **No modificar** en este ciclo. |

> **Pendiente de confirmar:** Rutas Cotizadas y Manifiestos "no sirven aún". Se dejan fuera del plan hasta que confirmes si los revisamos en este mismo trabajo.

---

## 1. Prospectos – Estatus duplicado en detalle

**Archivo:** `Views/Prospectos/_FormularioProspecto.cshtml`

- Ocultar la sección `seccionCotizacionDetalle` y dejar visible solo `seccionEstadoDetalle`.
- Si el prospecto tiene un motivo de rechazo de cotización, mostrarlo dentro de la sección **ESTATUS** como un campo adicional (no como bloque separado de "COTIZACIÓN").

## 2. Prospectos – Teléfono de contacto exactamente 10 dígitos

**Archivos:**
- `Views/Prospectos/_FormularioProspecto.cshtml` (teléfono matriz)
- `Views/Prospectos/_ModalContacto.cshtml` (teléfono de contacto)
- `Views/Prospectos/_ModalSucursal.cshtml` (teléfono de sucursal)
- `Scripts/prospectos.js`

- Agregar `minlength="10"` en los inputs de teléfono.
- En la validación JS (`obtenerErroresTabProspecto`, `obtenerErroresTabSuc`, `guardarContactoConValidacion`) verificar que la longitud sea exactamente 10.
- Mensaje: "El teléfono debe tener 10 dígitos".

## 3. Prospectos – RFC: validación reforzada

**Archivo:** `Scripts/prospectos.js`

- Ya existe validación de longitud 12/13 según tipo de persona en `obtenerErroresTabProspecto`.
- Se asegurará que el mensaje de error sea claro y que se marque el campo al fallar.

## 4. Prospectos – Quitar "Búsqueda rápida en mapa"

**Archivos:**
- `Views/Prospectos/_FormularioProspecto.cshtml` (matriz)
- `Views/Prospectos/_ModalSucursal.cshtml` (sucursal)

- Eliminar el bloque de input/botón de "Búsqueda rápida en mapa".
- Dejar intactos los campos de dirección, el botón "Verificar Dirección" y el mapa.

## 5. Prospectos – Fotografías con nombres diferentes

**Archivos:**
- `Views/Prospectos/_FormularioProspecto.cshtml`
- `Views/Prospectos/_ModalSucursal.cshtml`
- `Scripts/prospectos.js`

- Agregar hidden inputs para guardar el nombre original/prefijado de cada foto.
- En `previewFoto`, asignar nombres distintos automáticamente usando prefijo + timestamp:
  - Fachada: `Fachada_<timestamp>.jpg`
  - Acceso: `Acceso_<timestamp>.jpg`
  - Referencia: `Referencia_<timestamp>.jpg`
- Incluir esos nombres en el modelo que se envía al servidor.

## 6. Prospectos – Folio y documento catastral opcionales

**Archivos:**
- `Models/ViewModels/ProspectoViewModel.cs`
- `Views/Prospectos/_FormularioProspecto.cshtml`
- `Views/Prospectos/_ModalSucursal.cshtml`
- `Scripts/prospectos.js`

- Quitar `[Required]` de `FolioCatastral` y `DocumentoCatastral` en el ViewModel.
- En JS, quitar la validación obligatoria de folio y documento catastral.
- Agregar validación opcional: si se captura folio, debe tener entre **16 y 31 caracteres alfanuméricos**.

## 7. Prospectos – Botón GUARDAR de sucursal solo en "Fotos y Archivos"

**Archivo:** `Scripts/prospectos.js`

- En `actualizarBotonesSuc`, mostrar `btnGuardarSucursal` solo cuando `tabIdxSuc === 2` (FOTOS Y ARCHIVOS), en lugar de `tabIdxSuc === 0` (DATOS DE CONTACTO).

## 8. Prospectos – Sucursal: Estado, Concesionaria y colores

**Archivos:**
- `Views/Prospectos/_ModalSucursal.cshtml`
- `Scripts/prospectos.js`

- Corregir el valor por defecto de `sucEstado` para que sea `"Yucatán"` (no `"Yucat&aacute;n"` ni entidades HTML escapadas).
- Quitar el campo **Concesionaria** de la sucursal.
- Unificar estilos de los botones **+ Agregar Contacto** y **+ Agregar Sucursal** para que usen fondo marrón oscuro y texto blanco, consistente con el resto de la app.

## 9. Cotizador – Cambiar "Continuar Editando" → "Corregir y reenviar"

**Archivo:** `Views/Cotizador/Index.cshtml`

- En la lista de borradores, cambiar el texto del botón de borradores no autorizados a **"Corregir y reenviar"**.
- El icono puede seguir siendo el lápiz o cambiarse a un icono de envío.

## 10. Cotizador – Al enviar a validación, actualizar el borrador primero

**Archivo:** `Views/Cotizador/Generar.cshtml`

- En `solicitarValidacion`, si `borradorGuardadoId` existe, llamar primero a `UpdateBorrador` con los datos actuales antes de crear/enviar la validación.
- Si no existe borrador, mantener el flujo actual (crear borrador y luego validación).
- Esto garantiza que, si el supervisor rechaza y el vendedor vuelve a entrar, el borrador contenga lo último que se envió.

## 11. Cotizador – No mostrar la contraseña al enviar al cliente

**Archivo:** `Views/Cotizador/Generar.cshtml`

- En `enviarACliente`, quitar la línea que muestra `emailRes.password_temporal` en el mensaje de éxito.
- El backend seguirá generando y enviando la contraseña por correo, pero no se mostrará en pantalla.

## 12. Prospectos – Notificación "Finalizar datos de registro"

**Archivo:** `Scripts/prospectos.js`

- En el modal de confirmación de **"Finalizar datos de registro"**, **no mostrar la contraseña temporal**.
- Mostrar solo: "Se enviará un correo a [correo] con las instrucciones para finalizar el registro.".
- Agregar comentario/documentación en el código indicando que la contraseña se genera en el backend (`SP_Notificaciones_Insert`) y se envía únicamente al correo del cliente.

## 13. Usuarios – Correo electrónico válido y sin duplicados

**Archivos:**
- `DAL/Usuarios/UsuariosDAL.cs`
- `Controllers/Usuarios/UsuariosController.cs`

- Agregar método `ExisteCorreo(string correo, int? excluirId)` en `UsuariosDAL`.
- En `UsuariosController.Guardar`:
  - Validar formato de correo con `EmailAddressAttribute` o regex.
  - Si el correo ya existe, devolver mensaje amigable: **"Correo ya utilizado"**.
  - Si no se captura correo, permitir guardar sin validar duplicado (evita el error de UNIQUE KEY con valor vacío).
- El error técnico de SQL ya no debe llegar al usuario.

## 14. Usuarios – Formulario de información personal

**Archivo:** `Views/Usuarios/Index.cshtml`

- Cambiar labels:
  - "Nombre" → **"Nombres"**
  - "Apellidos" → **"Apellido Paterno"** y **"Apellido Materno"**
- Mantener por ahora un solo campo de apellidos en el request (`UsuarioCrudRequest.apellidos`), concatenando en el controlador si el usuario escribe ambos apellidos en campos separados. Si la base de datos ya tiene columnas separadas, se ajustará en el SP/DAL correspondiente.

## 15. RME – Servicio único / un solo viaje

**Archivos:**
- `Views/Cotizador/Generar.cshtml`
- `Models/Cotizador/ServicioCotizadoModel.cs` (solo si se requiere almacenar fecha puntual)
- `DAL/Cotizador/CotizacionesDAL.cs` (solo si se requiere persistir nuevos campos)

- Agregar opción **"Único"** al select de tipo de servicio (`mdlTipoCobro` o un nuevo select de periodicidad).
- Al seleccionar **"Único"**:
  - Ocultar los checkboxes de días de la semana.
  - Mostrar un campo de fecha puntual (`mdlFechaUnica`).
- En el cálculo de subtotal (`calcularSubtotalItem`):
  - Si es servicio único, cobrar **un solo viaje** (no multiplicar por 4 semanas).
- Guardar la fecha puntual en el objeto del item.

---

## Archivos que se modificarán

1. `Views/Prospectos/_FormularioProspecto.cshtml`
2. `Views/Prospectos/_ModalContacto.cshtml`
3. `Views/Prospectos/_ModalSucursal.cshtml`
4. `Scripts/prospectos.js`
5. `Models/ViewModels/ProspectoViewModel.cs`
6. `Views/Cotizador/Index.cshtml`
7. `Views/Cotizador/Generar.cshtml`
8. `Models/Cotizador/ServicioCotizadoModel.cs`
9. `DAL/Cotizador/CotizacionesDAL.cs`
10. `DAL/Usuarios/UsuariosDAL.cs`
11. `Controllers/Usuarios/UsuariosController.cs`
12. `Views/Usuarios/Index.cshtml`

---

## Notas / riesgos

- **Stored procedures:** No se modificarán salvo que sea estrictamente necesario. El envío de correo de "Finalizar datos de registro" se asume que ya lo hace `SP_Notificaciones_Insert`; solo se oculta la contraseña en la UI.
- **Unidades de medida RME (m²/m³):** Se dejan sin cambios según lo indicado.
- **Manifiestos y Rutas Cotizadas:** Se dejan fuera del plan hasta confirmación.
- **Testing:** Se recomienda probar flujos completos de creación de prospecto, cotización RME única, rechazo/reenvío de validación y creación de usuario con correo duplicado.
