namespace Stellantis.ProjectName.Domain.Entities
{
    public class Audit : EntityBase
    {
        public string Tabela { get; set; } = null!;
        public int RegistroId { get; set; }
        public string Campo { get; set; } = null!;
        public string ValorAntigo { get; set; } = null!;
        public string ValorNovo { get; set; } = null!;
        public int UsuarioId { get; set; }
        public DateTime DataHora { get; set; }
        public string Operacao { get; set; } = null!;
        public string IpUsuario { get; set; } = null!;
        public string Motivo { get; set; } = null!;
    }

}
