# Plan: Mejorar el resumen de cotización en "Cotizaciones por Aprobar"

## Objetivo
Mejorar la presentación visual del apartado **"Ver resumen del contrato"** (modal de detalle) en `Views/ValidacionCotizaciones/Index.cshtml` para que:
1. El modal sea más grande y los botones **Rechazar / Autorizar** no queden al borde ni se salgan del recuadro.
2. La sección **"Resumen Consolidado - Todas las Sucursales"** y las tarjetas de **Matriz / Sucursales** se vean más organizadas, espaciosas y profesionales.

## Problemas detectados en la vista actual
- El modal tiene `max-height: 95vh` y `height: auto`, pero la distribución interna (`detalle-body` con `flex-grow: 1`) puede dejar poco espacio al footer, haciendo que los botones queden pegados al borde.
- Las tarjetas de sucursal usan estilos inline dispersos, bordes poco consistentes y no resaltan bien la sucursal activa.
- El cuadro de totales (`summary-responsive`) tiene márgenes negativos que lo hacen pegarse a los bordes.
- Falta una separación clara entre la tabla de servicios de la sucursal seleccionada y el resumen consolidado.

## Enfoque de la mejora
Ajustar solo el archivo afectado, manteniendo la lógica JavaScript actual y usando los colores y variables CSS ya definidos en `Site.css`.

## Cambios propuestos

### 1. Modal más grande y robusto
- Aumentar el ancho máximo del modal en escritorio (`1350px` → `1500px`) y mejorar el uso del alto.
- Asegurar que el footer tenga altura suficiente y padding para que los botones no toquen los bordes.
- Mejorar el cierre con `Esc` y scroll del body cuando el modal está abierto.

### 2. Reorganización del contenido del modal
- Dividir la parte inferior en dos columnas en pantallas grandes:
  - Izquierda: tabla de servicios de la sucursal seleccionada.
  - Derecha: cuadro de resumen consolidado (subtotal, IVA, descuento, total).
- En pantallas medianas/pequeñas mantener el orden vertical actual pero con mejor espaciado.

### 3. Mejorar tarjetas de Matriz / Sucursales
- Usar grid responsive (`grid-template-columns: repeat(auto-fit, minmax(220px, 1fr))`).
- Unificar estilos: sombra, bordes redondeados, padding consistente, icono de edificio.
- Resaltar la tarjeta activa con borde grueso del color primario y fondo ligeramente diferente.
- Mostrar mejor la información: nombre, subtotal, cantidad de servicios.

### 4. Mejorar cuadro de totales
- Eliminar márgenes negativos que rompen el layout.
- Alinear montos a la derecha con fuente monoespaciada para que los números queden alineados.
- Destacar el total mensual con el color primario de la marca.
- Agregar etiqueta de "Total mensual aproximado" para claridad.

### 5. Ajustes menores
- Usar las variables CSS del proyecto (`--brown-dark`, `--brown-pale`, `--bg2`, etc.) en lugar de colores hardcodeados donde sea posible.
- Mantener todos los datos y cálculos actuales; solo se cambia presentación.

## Archivos a modificar
- `Views/ValidacionCotizaciones/Index.cshtml` (CSS embebido en `<style>` y estructura HTML del modal).

## No se modificarán
- Controladores ni lógica de negocio.
- Base de datos ni modelos.
- Otras vistas o archivos CSS globales.

## Criterios de aceptación
- Al abrir el resumen de una cotización, el modal ocupa más espacio de pantalla y los botones del footer tienen padding adecuado.
- Las tarjetas de Matriz y Sucursales se ven alineadas y la activa se distingue claramente.
- La tabla de servicios de la sucursal activa y el cuadro de totales están bien separados y legibles.
- El total mensual se destaca visualmente.
- Las funciones de rechazar/autorizar siguen funcionando igual.
