using System.Collections.Generic;
using CRMSistema.Models.Dashboard;

namespace CRMSistema.Models.ViewModels
{
    public class DashboardViewModel
    {
        public KpiResumen Kpis { get; set; } = new KpiResumen();
        public List<TendenciaMesDto> Tendencia { get; set; } = new List<TendenciaMesDto>();
        public List<OrigenDto> Origenes { get; set; } = new List<OrigenDto>();
        public List<TipoInmuebleDto> TiposInmueble { get; set; } = new List<TipoInmuebleDto>();
        public List<EstatusDistribucionDto> EstatusDistribucion { get; set; } = new List<EstatusDistribucionDto>();
        public List<PipelineDto> Pipeline { get; set; } = new List<PipelineDto>();
        public List<CotizacionDetalleDto> CotizacionesDetalle { get; set; } = new List<CotizacionDetalleDto>();
        public string Error { get; set; }
    }
}
