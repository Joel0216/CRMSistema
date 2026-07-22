# Plan: Replicar el módulo de Contratos de CRMCiclo en CRMSistema

## Objetivo
Hacer funcionales en CRMSistema los apartados **Contratos** (`/Contratos`) y **Contratos Autorizados** (`/ContratosAutorizados`), replicando la funcionalidad visual y de flujo que tiene CRMCiclo, pero **respetando las reglas y arquitectura de CRMSistema**:

- ASP.NET MVC 5 (.NET Framework 4.8)
- Razor + jQuery + Bootstrap 5 (sin React)
- ADO.NET puro con Stored Procedures
- Reutilizar tablas y DALs existentes
- No crear tablas/código innecesarios

---

## Estado actual en CRMSistema

| Apartado | Estado |
|---|---|
| `/ValidacionCotizaciones` | Funcional. Muestra cotizaciones pendientes de aprobar. Al autorizar crea un registro en `crm_contratos_autorizados` con estatus **Activo** y cambia el prospecto a **Autorizado**. |
| `/ContratosAutorizados` | Lista contratos autorizados (tabla `crm_contratos_autorizados`) con modal de detalle básico. |
| `/Contratos` | **Placeholder**. Solo tiene una tabla con datos estáticos y un botón "Nuevo contrato" que no hace nada. |

Tablas y SPs ya existentes relevantes:
- `crm_contratos_autorizados` (Contrato_ID, Prospecto_ID, Validacion_ID, Folio, Monto_Mensual, Estatus, Fecha_Autorizacion, Autorizado_Por)
- `crm_prospectos`, `crm_tratos`, `crm_servicios_cotizados`, `crm_prospecto_sucursales`
- SPs: `SP_ContratosAutorizados_GetAll`, `SP_ContratosAutorizados_Insert`, `SP_Prospectos_GetAll`, `SP_Tratos_GetByProspecto`, `SP_ServiciosCotizados_GetByTrato`, `SP_ProspectoSucursales_GetByProspecto`

---

## Propuesta de diseño

### 1. `/Contratos` — Contratos por firmar / enviar

Mostrará los contratos recién autorizados que aún no han sido firmados/enviados al cliente. Fuente: `crm_contratos_autorizados` filtrados por `Estatus = 'Activo'` (o `'Por firmar'`, según se defina).

#### Funcionalidades:
1. Tabla con: Folio, Fecha inicio, Cliente, Vigencia, Servicios, Estatus, Acciones.
2. Filtro por texto y estatus.
3. Modal "Ver contrato" con:
   - Datos del prestador de servicios (SANA) — se mantendrán como constante en el controlador/vista, igual que CRMCiclo, pero centralizados.
   - Datos del cliente (razón social, RFC, domicilio, representante, etc.).
   - Tabla de **Servicios Cotizados** (igual que en el cotizador / validación), agrupados por MATRIZ/Sucursales.
   - Condiciones del servicio.
   - Sección de firmas.
   - Botones: **Enviar Pre-Contrato**, **Re-enviar**, **Enviar Contrato**.
4. Al enviar, actualizar el estatus del contrato a `'Enviado'` y/o del prospecto a `'Firmado'` según corresponda.

### 2. `/ContratosAutorizados` — Clientes con contrato activo

Mejorar el modal de detalle existente para incluir la información tipo CRMCiclo:

1. Banner de estado (Activo / Moroso / Inactivo).
2. Datos del contrato (razón social, RFC, domicilios de servicio/fiscal/cobro, vigencia, pago mensual).
3. Servicios contratados y días asignados.
4. Ruta y horario.
5. **Documentos del expediente** (INE, comprobante fiscal, carta de responsabilidad, etc.) — por ahora checks informativos, como en CRMCiclo.
6. **Timeline del contrato** (cotización aceptada, programación, documentación, envío, firma, pago, factura, primer servicio).
7. **Sección de suspensión de servicio** — UI funcional con selects y botón que dispara SweetAlert, como en CRMCiclo.

### 3. Backend (C#)

#### Cambios en `Controllers/Contratos/ContratosController.cs`
- Mantener `Index()` para renderizar la vista.
- Agregar `GetContratosPorFirmar()` → JSON con contratos activos/por firmar.
- Agregar `GetDetalle(int id)` → JSON con: contrato, prospecto, trato, servicios cotizados, sucursales.
- Agregar `EnviarContrato(int id)` → actualiza estatus a `'Enviado'`.
- Agregar `MarcarFirmado(int id)` → actualiza estatus a `'Firmado'`.

#### Cambios en `Controllers/ContratosAutorizados/ContratosAutorizadosController.cs`
- Mantener `Index()` y `GetContratos()`.
- Agregar `GetDetalle(int id)` → mismo detalle que `/Contratos`.

#### Cambios en `DAL/Contratos/ContratosDAL.cs`
- Agregar `ObtenerContratosPorFirmar()`.
- Agregar `ObtenerContratoPorId(id)`.
- Agregar `ActualizarEstatus(id, estatus)`.
- Reutilizar `ApiProspectosDAL`, `TratosDAL` y `CotizacionesDAL` para traer prospecto, tratos, servicios y sucursales desde los controllers.

#### Cambios en SQL
- No se crearán tablas nuevas.
- Se agregarán dos SPs simples en `sql/validacion_cotizaciones.sql` (o un archivo nuevo `sql/contratos_update.sql`):
  - `SP_ContratosAutorizados_GetById`
  - `SP_ContratosAutorizados_UpdateEstatus`

### 4. Frontend (Razor + jQuery)

#### Reescribir `Views/Contratos/Index.cshtml`
- Estructura similar a `Views/ContratosAutorizados/Index.cshtml`.
- Tabla dinámica vía `fetch` a `GetContratosPorFirmar`.
- Modal grande con el formato de contrato (header café, secciones de datos, tabla de servicios, condiciones, firmas, botones de envío).
- JavaScript para calcular subtotales/IVA/total con la misma fórmula usada en `ValidacionCotizaciones`.

#### Mejorar `Views/ContratosAutorizados/Index.cshtml`
- Reemplazar el modal de detalle simple por el modal completo de CRMCiclo.
- Reutilizar la función de renderizado de servicios y totales.
- Agregar timeline, documentos y sección de suspensión.

### 5. Consideraciones de seguridad y reglas

- Todos los controladores ya tienen `[Authorize]`.
- Se mantendrá `ValidateAntiForgeryToken` donde aplique.
- Los endpoints de solo lectura usarán `JsonRequestBehavior.AllowGet`.
- Las acciones de cambio de estatus serán `POST`.
- No se modificará el flujo de `ValidacionCotizaciones`; solo se consumirán sus resultados.

---

## Archivos que se modificarán

| Archivo | Cambio |
|---|---|
| `Controllers/Contratos/ContratosController.cs` | Agregar endpoints reales. |
| `Controllers/ContratosAutorizados/ContratosAutorizadosController.cs` | Agregar endpoint de detalle. |
| `DAL/Contratos/ContratosDAL.cs` | Agregar métodos de consulta y actualización de estatus. |
| `Views/Contratos/Index.cshtml` | Reescribir vista con tabla dinámica y modal de contrato. |
| `Views/ContratosAutorizados/Index.cshtml` | Mejorar modal de detalle (timeline, documentos, suspensión). |
| `sql/validacion_cotizaciones.sql` (o nuevo `sql/contratos_update.sql`) | SPs `GetById` y `UpdateEstatus`. |
| `Models/Contratos/ContratoAutorizadoModel.cs` | Posiblemente extender con campos de vigencia/estatus extendido (opcional). |

## Archivos que NO se modificarán

- `Controllers/ValidacionCotizaciones/ValidacionCotizacionesController.cs` (flujo de autorización intacto)
- `Controllers/Cotizador/CotizadorController.cs`
- `DAL/Cotizador/CotizacionesDAL.cs`
- `DAL/Cotizador/TratosDAL.cs`
- `DAL/Prospectos/ApiProspectosDAL.cs`
- Tablas principales del CRM

---

## Posibles riesgos / decisiones pendientes

1. **Estatus del contrato vs estatus del prospecto**: CRMSistema ya cambia el prospecto a **Autorizado**. Se propone que `/Contratos` trabaje sobre `crm_contratos_autorizados.Estatus`, no sobre `crm_prospectos.Estatus`, para no romper el flujo existente.
2. **Datos de SANA**: En CRMCiclo están hardcodeados en React. Se propone ponerlos como constantes en el controlador o ViewBag para mantener consistencia.
3. **Cálculo de totales**: Se reutilizará la misma fórmula de `ValidacionCotizaciones` para evitar inconsistencias.
4. **PDF real**: Queda fuera de este alcance (según elección del usuario).

---

## Criterios de aceptación

- `/Contratos` carga y muestra contratos autorizados reales.
- El modal de contrato muestra datos del prestador, cliente, servicios cotizados, totales, condiciones y firmas.
- Los botones de enviar pre-contrato/enviar contrato actualizan el estatus y muestran confirmación.
- `/ContratosAutorizados` lista contratos y su modal de detalle incluye timeline, documentos y suspensión.
- La compilación del proyecto no se rompe.
- No se crean tablas nuevas ni se altera el flujo de validación/cotización.
