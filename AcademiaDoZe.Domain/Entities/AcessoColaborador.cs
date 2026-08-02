//Pablo Valente Neto

using System;
using System.Collections.Generic;
using System.Text;

namespace AcademiaDoZe.Domain.Entities
{
    public sealed class AcessoColaborador : Entity
    {
        public Colaborador Colaborador { get; private set; }
        public DateTime DataHoraEntrada { get; private set; }
        public DateTime? DataHoraSaida { get; private set; }

        private AcessoColaborador(
            int id,
            Colaborador colaborador,
            DateTime dataHoraEntrada,
            DateTime? dataHoraSaida)
            : base(id)
        {
            Colaborador = colaborador;
            DataHoraEntrada = dataHoraEntrada;
            DataHoraSaida = dataHoraSaida;
        }
    }
}