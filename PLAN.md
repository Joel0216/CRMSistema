# Plan: Roles y visibilidad por vendedor en el CRM

## Objetivo
Aplicar las reglas de negocio:

- **Vendedor**:
  - Ve solo sus propios datos en: Dashboard, Prospectos, Cotizador, Contratos, Contratos Autorizados.
  - Al crear un prospecto, se asigna automáticamente a sí mismo.
- **Supervisor / Superadmin**:
  - Ven todo.
  - Pueden asignar un vendedor a un prospecto.

## Estado actual
- El login ya guarda `Session["UsuarioId"]`, `Session["UsuarioNombre"]` y `Session["Rol"]`.
- Algunos controladores ya tienen filtros en memoria (`PuedeVerProspecto`, `PuedeVerContrato`, `FiltrarPorRol`), pero no están en todos lados o usan solo nombre de vendedor.
- El menú ya oculta "Cotizaciones por Aprobar", "Contratos por Autorizar" y "Rutas Cotizadas" a los vendedores, pero **deja visible "Manifiestos"** aunque su controlador solo permite Supervisor/Superadmin.
- Los SP de dashboard agregan datos de **todos** los usuarios; un vendedor actualmente ve los KPIs globales.

## Enfoque
Híbrido para minimizar cambios y mantener el rendimiento:

1. **Listados y detalles** (Prospectos, Contratos, Cotizador): filtrado en memoria con helper centralizado, usando `VendedorId`/`PropietarioId` y respaldo por nombre.
2. **Dashboard KPIs y tendencias**: modificar los stored procedures para que acepten un `@VendedorId` opcional; si viene, filtran por `crm_prospectos.Vendedor_ID` / `Propietario_ID`. De este modo los totales mostrados al vendedor son reales.
3. **Creación de prospectos**: ajustar `ProspectosController.Nuevo` para auto-asignar al vendedor actual; para supervisores/superadmins agregar un dropdown de vendedor en el formulario.
4. **Menú**: ocultar "Manifiestos" a los vendedores.
5. **Limpieza opcional**: eliminar los usuarios de prueba `Prueba` y `Prueba1` (sin registros asociados).

## Archivos a modificar

### C# / ASP.NET MVC
1. `Models/Usuarios/RolHelper.cs` — agregar helper `UsuarioIdActual()` ya existe; agregar helper para obtener rol y verificar visibilidad.
2. `Controllers/Base/BaseController.cs` — agregar métodos reutilizables:
   - `bool EsSupervisorOAdmin()`
   - `bool PuedeVerProspecto(dynamic r)`
   - `bool PuedeVerContrato(ContratoAutorizadoModel c)`
   - `int? UsuarioIdActual()`
   - `string UsuarioNombreActual()`
3. `Controllers/Prospectos/ProspectosController.cs`:
   - Reutilizar el helper de `BaseController` para `PuedeVerProspecto`.
   - En `Nuevo` POST, auto-asignar `VendedorId` si el rol es Vendedor.
   - Si es Supervisor/Superadmin y el modelo trae `VendedorId`, usarlo; si no, asignar al creador.
   - Reforzar permisos en `Editar`, `Eliminar`, `CambiarEstatus`.
4. `Controllers/Cotizador/CotizadorController.cs`:
   - Reemplazar `PuedeVerProspecto` local por el helper centralizado.
5. `Controllers/Contratos/ContratosController.cs`, `ContratosAutorizadosController.cs`, `ContratosPorAutorizarController.cs`:
   - Reemplazar `PuedeVerContrato` local por el helper centralizado.
   - Asegurar que `ContratoAutorizadoModel` exponga `VendedorId` (si los SPs lo devuelven) además de `VendedorNombre`.
6. `Controllers/Dashboard/DashboardController.cs`:
   - Pasar `usuarioId` y `rol` a `DashboardDAL`.
   - Aplicar `FiltrarPorRol` a todos los listados ya filtrados (pipeline ya lo hace; cotizaciones detalle necesita filtro).
7. `DAL/Dashboard/DashboardDAL.cs`:
   - Agregar sobrecargas/métodos que acepten `int? vendedorId` y lo pasen a los SPs.
8. `Views/Prospectos/_FormularioProspecto.cshtml`:
   - Mostrar dropdown de vendedor solo para Supervisor/Superadmin.
9. `Views/Shared/_LayoutAdmin.cshtml`:
   - Ocultar "Manifiestos" a los vendedores.

### SQL Server (stored procedures)
Modificar estos SPs para aceptar `@VendedorId INT = NULL` y, cuando no sea NULL, filtrar por `Vendedor_ID` / `Propietario_ID`:

1. `SP_Dashboard_GetKPIs`
2. `SP_Dashboard_GetTendenciaProspectos`
3. `SP_Dashboard_GetTendenciaVentas`
4. `SP_Dashboard_GetOrigenes`
5. `SP_Dashboard_GetTiposInmueble`
6. `SP_Dashboard_GetEstatusDistribucionPorMes`
7. `SP_Dashboard_GetPipeline`
8. `SP_Dashboard_GetCotizacionesDetalle`
9. `SP_Prospectos_GetAll` (opcional, ya se filtra en memoria; se deja como está o se agrega parámetro para eficiencia).
10. `SP_ContratosAutorizados_GetAll` / `SP_ContratosAutorizados_GetPending` (opcional; se filtra en memoria por ahora).

**Nota:** si los SPs no se modifican, el dashboard seguirá mostrando totales globales a los vendedores. Por eso se propone la modificación SQL como paso esencial.

## Pasos de implementación

1. **Crear helper centralizado** en `BaseController`.
2. **Actualizar controladores** para usar el helper.
3. **Ajustar creación de prospectos** y formulario.
4. **Modificar SPs del dashboard** para aceptar `@VendedorId`.
5. **Actualizar `DashboardDAL`** y `DashboardController` para pasar el filtro.
6. **Ajustar menú** (`_LayoutAdmin.cshtml`).
7. **Verificar** con usuarios de cada rol.
8. **Eliminar** usuarios `Prueba` y `Prueba1` si el usuario lo confirma.

## Cómo probar

1. Iniciar sesión como vendedor, crear un prospecto y verificar que:
   - El prospecto aparece con `Vendedor_ID` = usuario actual.
   - En Dashboard, Prospectos, Cotizador, Contratos y Contratos Autorizados solo se ve ese prospecto/contrato.
2. Iniciar sesión como supervisor/superadmin y verificar que:
   - Se ven todos los prospectos/contratos.
   - Se puede asignar un prospecto a otro vendedor.
3. Confirmar que `Prueba` y `Prueba1` ya no aparecen en la lista de usuarios.

## Decisión pendiente

¿Procedo con esta propuesta, incluyendo la modificación de los stored procedures del dashboard?  
Si prefieres evitar cambios SQL, puedo alternativamente calcular los KPIs del dashboard desde los datos filtrados en C# (más código, menos rendimiento con muchos datos).
