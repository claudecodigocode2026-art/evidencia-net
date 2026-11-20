namespace eVidencia.Entidades;

// Nombres de clase y de propiedad iguales a los del modelo original (eVidencias.DAL/eVidencias.edmx)
// para mantener la trazabilidad con el código de origen.

public class TIPO_DE_PROCESO
{
    public int IdTipoDeProceso { get; set; }
    public string Nombre { get; set; } = "";
}

public class PROCESO
{
    public int IdProceso { get; set; }
    public string? Codigo { get; set; }
    public string? Nombre { get; set; }
    public int IdTipoDeProceso { get; set; }
    public bool Borrado { get; set; }
    public TIPO_DE_PROCESO? TIPO_DE_PROCESO { get; set; }
}

public class SUBPROCESO
{
    public int IdSubproceso { get; set; }
    public string? Codigo { get; set; }
    public string? Nombre { get; set; }
    public int IdProceso { get; set; }
    public bool Borrado { get; set; }
    public bool TipoOtros { get; set; }
    public PROCESO? PROCESO { get; set; }
}

/// <summary>Solo las columnas que usan las pantallas migradas.</summary>
public class PROCESO_ASOCIADO
{
    public int IdProcesoAsociado { get; set; }
    public int IdProceso { get; set; }
    public int? IdSubproceso { get; set; }
    public bool Borrado { get; set; }
}

public class PERFIL_DE_USUARIO
{
    public int IdPerfilDeUsuario { get; set; }
    public string Nombre { get; set; } = "";
}

public class USUARIO
{
    public int IdUsuario { get; set; }
    public string Nombre { get; set; } = "";
    public string Apellidos { get; set; } = "";
    public string? UID { get; set; }
    public int IdPerfilDeUsuario { get; set; }
    public bool Borrado { get; set; }
    public PERFIL_DE_USUARIO? PERFIL_DE_USUARIO { get; set; }
}

public class LOG_DE_ENTIDADES
{
    public int IdLogDeEntidades { get; set; }
    public DateTime Fecha { get; set; }
    public int IdUsuario { get; set; }
    public int? IdTipoDeAccion { get; set; }
    public int? IdSociedad { get; set; }
    public int? IdPeriodo { get; set; }
    public string Descripcion { get; set; } = "";
    public int? IdProceso { get; set; }
    public int? IdPlanificadorDeEvidencias { get; set; }
    public int? IdActividadDeControl { get; set; }
    public int? IdSubproceso { get; set; }
    public int? IdAreaResponsable { get; set; }
}
