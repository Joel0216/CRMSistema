using System;
using System.Collections.Generic;

namespace CRMSistema.Models.Dashboard
{
    /// <summary>
    /// Respuesta agregada del dashboard para el frontend.
    /// </summary>
    public class DashboardKpiDto
    {
        public KpiResumen kpis { get; set; }
        public List<TendenciaMesDto> tendencia { get; set; }
        public List<OrigenDto> origenes { get; set; }
        public List<TipoInmuebleDto> tiposInmueble { get; set; }
        public List<EstatusDistribucionDto> estatusDistrib { get; set; }
        public List<PipelineDto> pipeline { get; set; }
        public List<CotizacionDetalleDto> cotizacionesDetalle { get; set; }
    }

    public class KpiResumen
    {
        public KpiValor ingresosMes { get; set; }
        public KpiValor prospectosMes { get; set; }
        public KpiCotizaciones cotizaciones { get; set; }
        public KpiTasaConversion tasaConversion { get; set; }
        public int deudores { get; set; }
        public int alCorriente { get; set; }
        public KpiSucursales sucursales { get; set; }
    }

    public class KpiValor
    {
        public double valor { get; set; }
        public double cambio { get; set; }
    }

    public class KpiCotizaciones
    {
        public int servicios { get; set; }
        public int borradores { get; set; }
        public int total { get; set; }
    }

    public class KpiTasaConversion
    {
        public double valor { get; set; }
        public int convertidos { get; set; }
        public int totalProspectos { get; set; }
    }

    public class KpiSucursales
    {
        public int prospectosConSuc { get; set; }
        public int totalSuc { get; set; }
    }

    public class TendenciaMesDto
    {
        public string mes { get; set; }
        public int prospectos { get; set; }
        public double ingresos { get; set; }
        public int tratos { get; set; }
    }

    public class OrigenDto
    {
        public string nombre { get; set; }
        public int cantidad { get; set; }
    }

    public class TipoInmuebleDto
    {
        public string tipo { get; set; }
        public int cantidad { get; set; }
    }

    public class EstatusDistribucionDto
    {
        public string estatus { get; set; }
        public int cantidad { get; set; }
    }

    public class PipelineDto
    {
        public string empresa { get; set; }
        public string contacto { get; set; }
        public string estatus { get; set; }
        public string tipoInmueble { get; set; }
        public string fuente { get; set; }
        public string trato { get; set; }
        public decimal monto { get; set; }
        public string fase { get; set; }
        public string tieneSucursales { get; set; }
        public string vendedorNombre { get; set; }
        public object fecha { get; set; }
    }

    public class CotizacionDetalleDto
    {
        public string tipo_residuo { get; set; }
        public string frecuencia { get; set; }
        public string periodicidad_pago { get; set; }
        public decimal volumen_estimado { get; set; }
        public decimal precio_unitario { get; set; }
        public string trato { get; set; }
        public string empresa { get; set; }
    }
}
